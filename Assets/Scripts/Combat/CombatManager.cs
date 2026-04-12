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

        ApplicaStance(giocatore);
        ApplicaStance(mostro);


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
            unita.AggiungiBuffSelf(new BuffAttivo(abilita.buffPassivo, null, -1, TipoScalaturaDurata.PerAzionePortatore));
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

            //costruisce il turno con le azioni pianificate da giocatore e mostro, che vengono ordinate in base alla destrezza dell'esecutore e alla priorità dell'abilità
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
                //ApplicaStanceInizioFase(fase, turno.personaggi);

                foreach (Azione azione in fase.azioni)
                {
                    yield return StartCoroutine(azione.esecutore.EseguiAzione(azione));
                    if (VerificaFine()) yield break;
                    
                    azione.esecutore.ScalaBuffPerAzionePortatore();
                    azione.esecutore.ScalaBuffPerAzioneCaster(azione.esecutore);
                
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

    // Restituisce true se il giocatore sta ancora selezionando le azioni
    public bool IsInFaseSelezione() { return inFaseSelezione; }
}