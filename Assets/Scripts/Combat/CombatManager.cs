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

        Debug.Log("=== COMBATTIMENTO INIZIATO ===");
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
        }
    }

    // Loop principale dei turni: raccoglie le azioni, costruisce il Turno e lo esegue fase per fase
    IEnumerator GestisciTurno()
    {
        Debug.Log("GestisciTurno avviato");
        while (combattimentoAttivo)
        {
            Debug.Log("--- Turno " + turnoCorrente + " ---");

            for (int i = 0; i < azioniGiocatore.Length; i++)
                azioniGiocatore[i] = null;
            for (int i = 0; i < azioniMostro.Length; i++)
                azioniMostro[i] = null;

            inFaseSelezione = true;
            azioniConfermate = false;
            yield return new WaitUntil(() => azioniConfermate);
            inFaseSelezione = false;

            ScegliAzioniMostro();

            yield return new WaitForSeconds(0.5f);

            Turno turno = Turno.Costruisci(turnoCorrente, new List<(CombatUnit, List<Azione>)>
            {
                (giocatore, new List<Azione>(azioniGiocatore)),
                (mostro,    new List<Azione>(azioniMostro))
            });

            foreach (Fase fase in turno.fasi)
            {
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
            if (!condizioniSoddisfatte) continue;
            foreach (var effetto in buffAttivo.dato.effetti)
            {
                if (effetto == null) continue;
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
            contesto.bersaglio.SubisciDanno(contesto.dannoAccumulato, contesto);
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

            personaggio.stanceCorrente = stanceEffettiva;

            if (stanceEffettiva == StanceTipo.Ren && !personaggio.HaBuff("Ren") && buffDatoRen != null)
                personaggio.AggiungiBuff(new BuffAttivo(buffDatoRen, null, -1, TipoScalaturaDurata.PerAzionePortatore));
            else if (stanceEffettiva != StanceTipo.Ren && personaggio.HaBuff("Ren"))
                personaggio.RimuoviBuff("Ren");
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