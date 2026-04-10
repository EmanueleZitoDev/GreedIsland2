using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TipoAzione
{
    UsaAbilita
}

public class AzioneCombattimento
{
    public CombatUnit esecutore;
    public CombatUnit bersaglio;
    public TipoAzione tipo;
    public StanceTipo stance;
    public AbilitaDato abilita;

    public AzioneCombattimento(CombatUnit esecutore, CombatUnit bersaglio, TipoAzione tipo, StanceTipo stance = StanceTipo.Ten, AbilitaDato abilita = null)
    {
        this.esecutore = esecutore;
        this.bersaglio = bersaglio;
        this.tipo = tipo;
        this.stance = stance;
        this.abilita = abilita;
    }
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Combattenti")]
    public CombatUnit giocatore;
    public CombatUnit mostro;

    [Header("Abilità Base")]
    public AbilitaDato attaccoFisicoBase;

    private int turnoCorrente = 1;
    private bool combattimentoAttivo = false;
    private bool inFaseSelezione = false;
    private bool azioniConfermate = false;
    private InteractableObject oggettoInterazione;

    private AzioneCombattimento[] azioniGiocatore;
    private AzioneCombattimento[] azioniMostro;

    private ContestoCombattimento contesto = new ContestoCombattimento();

    [Header("Stance")]
    public AbilitaDato stanceTen;
    public AbilitaDato stanceRen;

    // Inizializza il singleton — distrugge i duplicati
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Avvia un nuovo combattimento tra giocatore e mostro, resetta lo stato e parte il loop dei turni
    public void IniziaCombattimento(CombatUnit unita1, CombatUnit unita2, InteractableObject oggetto)
    {
        Debug.Log("IniziaCombattimento chiamato — stack: " + System.Environment.StackTrace);
        contesto = new ContestoCombattimento();
        giocatore = unita1;
        mostro = unita2;
        oggettoInterazione = oggetto;
        combattimentoAttivo = true;
        turnoCorrente = 1;

        azioniGiocatore = new AzioneCombattimento[giocatore.azioniPerTurno];
        azioniMostro = new AzioneCombattimento[mostro.azioniPerTurno];

        giocatore.stanceCorrente = StanceTipo.Ten;
        mostro.stanceCorrente = StanceTipo.Ten;

        Debug.Log("=== COMBATTIMENTO INIZIATO ===");
        StartCoroutine(GestisciTurno());
        giocatore.MostraUI();
        mostro.MostraUI();
        CombatUI.Instance.MostraCombatUI();
        //GameOverUI.Instance.Nascondi();
    }

    // Loop principale dei turni: raccoglie le azioni del giocatore, genera quelle del mostro e le esegue in ordine di destrezza
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

            bool giocatoreVaPerPrimo = giocatore.GetDestrezza() >= mostro.GetDestrezza();
            int maxAzioni = Mathf.Max(giocatore.azioniPerTurno, mostro.azioniPerTurno);

            for (int i = 0; i < maxAzioni; i++)
            {
                if (giocatoreVaPerPrimo)
                {
                    if (azioniGiocatore.Length > i)
                    {
                        yield return StartCoroutine(EseguiAzione(azioniGiocatore[i]));
                        if (VerificaFine()) yield break;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (azioniMostro.Length > i)
                    {
                        yield return StartCoroutine(EseguiAzione(azioniMostro[i]));
                        if (VerificaFine()) yield break;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    if (azioniMostro.Length > i)
                    {
                        yield return StartCoroutine(EseguiAzione(azioniMostro[i]));
                        if (VerificaFine()) yield break;
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (azioniGiocatore.Length > i)
                    {
                        yield return StartCoroutine(EseguiAzione(azioniGiocatore[i]));
                        if (VerificaFine()) yield break;
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }

            giocatore.RigeneraNen();
            mostro.RigeneraNen();

            turnoCorrente++;
        }
    }

    // Esegue una singola azione: imposta la stance, applica i tag dinamici, esegue l'abilità e decrementa i buff
    IEnumerator EseguiAzione(AzioneCombattimento azione)
    {
        if (azione == null) yield break;

        azione.esecutore.stanceCorrente = azione.stance;

        if (azione.stance == StanceTipo.Ren)
        {
            bool renRiuscito = azione.esecutore.ConsumaNenRen();
            if (!renRiuscito)
                azione.esecutore.stanceCorrente = StanceTipo.Ten;
        }

        contesto.esecutoreAzioneCorrente = azione.esecutore;
        contesto.bersaglioAzioneCorrente = azione.bersaglio;
        contesto.tipoAzioneCorrente = azione.tipo;
        contesto.stanceAttiva = GetStanceAbilita(azione.esecutore.stanceCorrente);

        // Reset tag dinamici e applica quelli della stance
        contesto.ResetTagDinamici();
        if (contesto.stanceAttiva?.effetti != null)
        {
            foreach (var effetto in contesto.stanceAttiva.effetti)
            {
                if (effetto is EffettoAggiungTag tagEffect)
                    tagEffect.Esegui(azione.esecutore, azione.bersaglio, contesto);
            }
        }

        EseguiAbilita(azione);

        contesto.DecrementaBuffDopoAzione(azione.esecutore);
        yield return null;
    }

    // Consuma il Nen richiesto e applica tutti gli effetti dell'abilità se le condizioni sono soddisfatte
    void EseguiAbilita(AzioneCombattimento azione)
    {
        if (azione.abilita == null) return;

        if (azione.abilita.costoNen > 0)
        {
            bool ok = azione.esecutore.ConsumaNen(azione.abilita.costoNen);
            if (!ok)
            {
                Debug.Log("Nen insufficiente per: " + azione.abilita.nomeAbilita);
                return;
            }
        }

        foreach (var effetto in azione.abilita.effetti)
        {
            if (effetto == null) continue;
            if (effetto.CondizioneSoddisfatta(azione.esecutore, azione.bersaglio, contesto))
                effetto.Esegui(azione.esecutore, azione.bersaglio, contesto);
        }
    }

    // Riempie le azioni del mostro con attacchi fisici base in Ten (AI semplice)
    void ScegliAzioniMostro()
    {
        for (int i = 0; i < mostro.azioniPerTurno; i++)
            azioniMostro[i] = new AzioneCombattimento(mostro, giocatore, TipoAzione.UsaAbilita, StanceTipo.Ten, attaccoFisicoBase);
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
        azioniGiocatore[indiceSlot] = new AzioneCombattimento(giocatore, mostro, TipoAzione.UsaAbilita, stance, abilita);
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

    // Azzera il ContestoCombattimento, rimuovendo tutti i buff e i tag dinamici attivi e ferma tutte le coroutine attive.
    public void ResetContesto()
    {
        StopAllCoroutines();
        combattimentoAttivo = false;
        contesto = new ContestoCombattimento();
    }

    // Restituisce true se il combattimento è in corso
    public bool IsCombattimentoAttivo() { return combattimentoAttivo; }
    // Restituisce true se il giocatore sta ancora selezionando le azioni
    public bool IsInFaseSelezione() { return inFaseSelezione; }
    // Restituisce l'AbilitaDato della stance attiva (Ten o Ren)
    AbilitaDato GetStanceAbilita(StanceTipo stance)
    {
        switch (stance)
        {
            case StanceTipo.Ten: return stanceTen;
            case StanceTipo.Ren: return stanceRen;
            default: return stanceTen;
        }
    }

}