using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Combattenti")]
    public CombatUnit giocatore;
    public CombatUnit mostro;

    [Header("Abilità Base")]
    public AbilitaDato attaccoFisicoBase;

    [Header("Stance")]
    public BuffDato buffDatoTen;
    public BuffDato buffDatoRen;

    private int turnoCorrente = 1;
    private bool combattimentoAttivo = false;
    private bool inFaseSelezione = false;
    private bool azioniConfermate = false;
    private InteractableObject oggettoInterazione;

    private Azione[] azioniGiocatore;
    private Azione[] azioniMostro;

    private ContestoCombattimento contesto = new ContestoCombattimento();

    // Inizializza il singleton — distrugge i duplicati
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void IniziaCombattimento(CombatUnit unita1, CombatUnit unita2, InteractableObject oggetto)
    {
        contesto = new ContestoCombattimento();
        giocatore = unita1;
        mostro = unita2;
        oggettoInterazione = oggetto;
        combattimentoAttivo = true;
        turnoCorrente = 1;

        azioniGiocatore = new Azione[giocatore.azioniPerTurno];
        azioniMostro = new Azione[mostro.azioniPerTurno];

        giocatore.stanceCorrente = StanceTipo.Ten;
        mostro.stanceCorrente = StanceTipo.Ten;

        ApplicaPassive(giocatore);
        ApplicaPassive(mostro);

        Debug.Log("════════════════════════════════════════");
        Debug.Log("  COMBATTIMENTO: " + giocatore.nomePersonaggio + " VS " + mostro.nomePersonaggio);
        Debug.Log("════════════════════════════════════════");
        StartCoroutine(GestisciTurno());
        giocatore.MostraUI();
        mostro.MostraUI();
        CombatUI.Instance.MostraCombatUI();
    }

    // Le abilità passive vengono aggiunte come BuffAttivo permanente (durata -1).
    // Vengono valutate ogni azione come qualunque altro buff attivo.
    void ApplicaPassive(CombatUnit unita)
    {
        SkillTreePersonaggio skillTree = unita.GetComponent<SkillTreePersonaggio>();
        if (skillTree == null) return;

        foreach (AbilitaDato abilita in skillTree.abilitaSbloccate)
        {
            if (abilita == null || !abilita.isPassiva || abilita.buffPassivo == null) continue;
            unita.AggiungiBuff(new BuffAttivo(abilita.buffPassivo, null, -1, TipoScalaturaDurata.PerAzionePortatore));
            Debug.Log("[PASSIVA] " + unita.nomePersonaggio + " — " + abilita.nomeAbilita + " attiva come buff permanente");
        }
    }

    // Loop principale dei turni: raccoglie le azioni, costruisce il Turno e lo esegue fase per fase
    IEnumerator GestisciTurno()
    {
        while (combattimentoAttivo)
        {
            Debug.Log("────────────────────────────────────────");
            Debug.Log("  TURNO " + turnoCorrente
                + "  |  " + giocatore.nomePersonaggio + " HP:" + giocatore.GetHP() + "/" + giocatore.GetHPMax() + " Nen:" + giocatore.GetNen() + "/" + giocatore.GetNenMax()
                + "  |  " + mostro.nomePersonaggio + " HP:" + mostro.GetHP() + "/" + mostro.GetHPMax() + " Nen:" + mostro.GetNen() + "/" + mostro.GetNenMax());
            Debug.Log("────────────────────────────────────────");

            for (int i = 0; i < azioniGiocatore.Length; i++)
                azioniGiocatore[i] = null;
            for (int i = 0; i < azioniMostro.Length; i++)
                azioniMostro[i] = null;

            inFaseSelezione = true;
            azioniConfermate = false;
            yield return new WaitUntil(() => azioniConfermate);
            inFaseSelezione = false;
            Debug.Log("[PIANIFICAZIONE] " + giocatore.nomePersonaggio + " ha confermato le azioni");

            ScegliAzioniMostro();

            yield return new WaitForSeconds(0.5f);

            Turno turno = Turno.Costruisci(turnoCorrente, new List<(CombatUnit, List<Azione>)>
            {
                (giocatore, new List<Azione>(azioniGiocatore)),
                (mostro,    new List<Azione>(azioniMostro))
            });

            Debug.Log("[TURNO] Struttura: " + turno.fasi.Count + " fasi, " + turno.personaggi.Count + " personaggi");

            foreach (Fase fase in turno.fasi)
            {
                Debug.Log("  ── Fase " + (fase.indice + 1) + "/" + turno.fasi.Count
                    + " — " + fase.azioni.Count + " azioni ──");
                ApplicaStanceInizioFase(fase, turno.personaggi);

                foreach (Azione azione in fase.azioni)
                {
                    yield return StartCoroutine(EseguiAzione(azione));
                    if (VerificaFine()) yield break;
                    yield return new WaitForSeconds(0.5f);
                }

                // Fine fase — scala i buff PerFase su tutti i personaggi
                giocatore.ScalaBuffPerFase();
                mostro.ScalaBuffPerFase();
            }

            giocatore.RigeneraNen();
            mostro.RigeneraNen();
            Debug.Log("[FINE TURNO " + turnoCorrente + "]"
                + "  " + giocatore.nomePersonaggio + " HP:" + giocatore.GetHP() + " Nen:" + giocatore.GetNen()
                + "  |  " + mostro.nomePersonaggio + " HP:" + mostro.GetHP() + " Nen:" + mostro.GetNen());

            turnoCorrente++;
        }
    }

    // Esegue una singola azione: imposta la stance effettiva, popola il contesto, esegue l'abilità e scala i buff
    IEnumerator EseguiAzione(Azione azione)
    {
        if (azione == null) yield break;

        azione.esecutore.stanceCorrente = azione.stancePianificata;

        if (azione.stancePianificata == StanceTipo.Ren)
        {
            bool renRiuscito = azione.esecutore.ConsumaNenRen();
            if (!renRiuscito)
                azione.esecutore.stanceCorrente = StanceTipo.Ten;
        }

        Debug.Log("[AZIONE] " + azione.esecutore.nomePersonaggio
            + " usa \"" + (azione.abilitaAttiva?.nomeAbilita ?? "—") + "\""
            + " [" + azione.esecutore.stanceCorrente + "]"
            + " → " + azione.bersaglio.nomePersonaggio);

        contesto.esecutore = azione.esecutore;
        contesto.bersaglio = azione.bersaglio;
        contesto.stanceAttiva = azione.esecutore.stanceCorrente;
        contesto.ResetAccumulatori();

        EseguiAbilita(azione);

        // Scala buff per azione sull'esecutore (portatore) e su tutti i personaggi (caster)
        azione.esecutore.ScalaBuffPerAzionePortatore();
        giocatore.ScalaBuffPerAzioneCaster(azione.esecutore);
        mostro.ScalaBuffPerAzioneCaster(azione.esecutore);

        yield return null;
    }

    // Consuma il Nen, esegue gli effetti dei buff attivi compatibili, poi gli effetti dell'abilità e infligge il danno
    void EseguiAbilita(Azione azione)
    {
        if (azione.abilitaAttiva == null) return;

        contesto.abilitaCorrente = azione.abilitaAttiva;

        // Consuma il Nen
        if (azione.abilitaAttiva.costoNen > 0)
        {
            bool ok = azione.esecutore.ConsumaNen(azione.abilitaAttiva.costoNen);
            if (!ok)
            {
                Debug.Log("Nen insufficiente per: " + azione.abilitaAttiva.nomeAbilita);
                return;
            }
        }

        // Fase 1 — Effetti dei buff attivi dell'esecutore
        foreach (BuffAttivo buffAttivo in azione.esecutore.GetBuffAttivi())
        {
            if (buffAttivo == null || buffAttivo.dato == null || buffAttivo.dato.effetti == null) continue;
            bool condizioniSoddisfatte = true;
            foreach (var condizione in buffAttivo.dato.condizioniAttivazione)
            {
                if (!condizione.Valuta(azione.esecutore, azione.bersaglio, contesto))
                { condizioniSoddisfatte = false; break; }
            }
            if (!condizioniSoddisfatte)
            {
                Debug.Log("  [BUFF] " + buffAttivo.dato.nomeBuff + " — condizioni non soddisfatte, saltato");
                continue;
            }
            Debug.Log("  [BUFF] " + buffAttivo.dato.nomeBuff + " — effetti applicati");
            foreach (var effetto in buffAttivo.dato.effetti)
            {
                if (effetto == null || effetto is EffettoDifesa) continue;
                effetto.Esegui(azione.esecutore, azione.bersaglio, contesto);
            }
        }

        // Fase 2 — Effetti dell'abilità attiva
        foreach (var effetto in azione.abilitaAttiva.effetti)
        {
            if (effetto == null) continue;
            if (effetto.CondizioneSoddisfatta(azione.esecutore, azione.bersaglio, contesto))
                effetto.Esegui(azione.esecutore, azione.bersaglio, contesto);
        }

        // Infliggi il danno accumulato direttamente al bersaglio del contesto
        if (contesto.dannoAccumulato > 0)
        {
            Debug.Log("  [DANNO] " + azione.esecutore.nomePersonaggio
                + " → " + contesto.dannoAccumulato + " danno lordo"
                + " | difesa: " + azione.bersaglio.difesaFase
                + " | danno netto: " + Mathf.Max(0, contesto.dannoAccumulato - azione.bersaglio.difesaFase));
            contesto.bersaglio.SubisciDanno(contesto.dannoAccumulato, contesto);
        }
        else
        {
            Debug.Log("  [DANNO] nessun danno accumulato");
        }
    }

    // Riempie le azioni del mostro con attacchi fisici base in Ten (AI semplice)
    void ScegliAzioniMostro()
    {
        for (int i = 0; i < mostro.azioniPerTurno; i++)
            azioniMostro[i] = new Azione(mostro, giocatore, attaccoFisicoBase, StanceTipo.Ten);
    }

    // Controlla se uno dei due combattenti è morto e termina il combattimento di conseguenza
    bool VerificaFine()
    {
        if (mostro.IsMorto())
        {
            TerminaCombattimento(true);
            return true;
        }
        if (giocatore.IsMorto())
        {
            TerminaCombattimento(false);
            return true;
        }
        return false;
    }

    // Chiude il combattimento: in caso di vittoria distrugge il mostro, in caso di sconfitta mostra il game over
    void TerminaCombattimento(bool giocatoreHaVinto)
    {
        combattimentoAttivo = false;

        if (giocatoreHaVinto)
        {
            Debug.Log("=== HAI VINTO ===");
            if (oggettoInterazione != null)
                oggettoInterazione.ForzaUscitaCombattimento();
            if (mostro != null)
                Destroy(mostro.gameObject);
        }
        else
        {
            Debug.Log("=== HAI PERSO ===");
            CombatUI.Instance.NascondiCombatUI();
            GameOverUI.Instance.Mostra();
        }
    }

    // Registra un'abilità del giocatore nello slot indicato con la stance selezionata
    public void AggiungiAbilitaGiocatore(AbilitaDato abilita, StanceTipo stance, int indiceSlot)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        azioniGiocatore[indiceSlot] = new Azione(giocatore, mostro, abilita, stance);
    }

    // Svuota lo slot all'indice indicato rimuovendo l'azione assegnata
    public void RimuoviAzioneAIndice(int index)
    {
        azioniGiocatore[index] = null;
    }

    // Conferma la coda azioni del giocatore se tutti gli slot sono pieni — sblocca l'esecuzione del turno
    public void ConfermaAzioni()
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;

        for (int i = 0; i < azioniGiocatore.Length; i++)
        {
            if (azioniGiocatore[i] == null) return;
        }

        azioniConfermate = true;
        inFaseSelezione = false;
    }

    // Azzera il contesto e ferma tutte le coroutine attive
    public void ResetContesto()
    {
        StopAllCoroutines();
        combattimentoAttivo = false;
        contesto = new ContestoCombattimento();
    }

    // Applica la stance effettiva a inizio fase per ogni personaggio.
    // Usa la stance forzata da buff se presente, altrimenti quella pianificata nell'azione.
    // Sincronizza il buff Ren: lo aggiunge se stance == Ren, lo rimuove altrimenti.
    void ApplicaStanceInizioFase(Fase fase, List<CombatUnit> personaggi)
    {
        foreach (CombatUnit personaggio in personaggi)
        {
            StanceTipo? stanceForzata = GetStanceForzataDaBuff(personaggio);

            StanceTipo stanceEffettiva;
            if (stanceForzata.HasValue)
            {
                stanceEffettiva = stanceForzata.Value;
            }
            else
            {
                Azione azione = fase.azioni.Find(a => a.esecutore == personaggio);
                stanceEffettiva = azione != null ? azione.stancePianificata : personaggio.stanceCorrente;
            }

            if (stanceForzata.HasValue)
                Debug.Log("  [STANCE] " + personaggio.nomePersonaggio + " — stance forzata da buff: " + stanceEffettiva);
            else if (personaggio.stanceCorrente != stanceEffettiva)
                Debug.Log("  [STANCE] " + personaggio.nomePersonaggio + " — " + personaggio.stanceCorrente + " → " + stanceEffettiva);

            personaggio.stanceCorrente = stanceEffettiva;

            if (stanceEffettiva == StanceTipo.Ten && !personaggio.HaBuff("Ten") && buffDatoTen != null)
                personaggio.AggiungiBuff(new BuffAttivo(buffDatoTen, null, -1, TipoScalaturaDurata.PerAzionePortatore));
            else if (stanceEffettiva != StanceTipo.Ten && personaggio.HaBuff("Ten"))
                personaggio.RimuoviBuff("Ten");

            if (stanceEffettiva == StanceTipo.Ren && !personaggio.HaBuff("Ren") && buffDatoRen != null)
                personaggio.AggiungiBuff(new BuffAttivo(buffDatoRen, null, -1, TipoScalaturaDurata.PerAzionePortatore));
            else if (stanceEffettiva != StanceTipo.Ren && personaggio.HaBuff("Ren"))
                personaggio.RimuoviBuff("Ren");

            // Ricalcola difesaFase: reset + ciclo buff attivi, solo EffettoDifesa
            personaggio.ResetDifesaFase();
            foreach (BuffAttivo buff in personaggio.GetBuffAttivi())
            {
                if (buff == null || buff.dato == null || buff.dato.effetti == null) continue;
                bool condizioniSoddisfatte = true;
                foreach (var condizione in buff.dato.condizioniAttivazione)
                {
                    if (!condizione.Valuta(personaggio, null, contesto))
                    { condizioniSoddisfatte = false; break; }
                }
                if (!condizioniSoddisfatte) continue;
                foreach (var effetto in buff.dato.effetti)
                {
                    if (effetto is EffettoDifesa)
                        effetto.Esegui(personaggio, null, contesto);
                }
            }
            Debug.Log("  [DIFESA] " + personaggio.nomePersonaggio + " — difesaFase: " + personaggio.difesaFase);
        }
    }

    // Cerca tra i buff attivi del personaggio uno con tag [forza-stance].
    // Restituisce la StanceTipo corrispondente al nomeBuff, null se non trovato.
    StanceTipo? GetStanceForzataDaBuff(CombatUnit personaggio)
    {
        foreach (BuffAttivo buff in personaggio.GetBuffAttivi())
        {
            if (buff.dato?.tags == null) continue;
            foreach (string tag in buff.dato.tags)
            {
                if (tag.Trim().ToLower() == "forza-stance")
                {
                    if (System.Enum.TryParse(buff.dato.nomeBuff, true, out StanceTipo stance))
                        return stance;
                }
            }
        }
        return null;
    }

    // Restituisce true se il combattimento è in corso
    public bool IsCombattimentoAttivo() { return combattimentoAttivo; }
    // Restituisce true se il giocatore sta ancora selezionando le azioni
    public bool IsInFaseSelezione() { return inFaseSelezione; }
}