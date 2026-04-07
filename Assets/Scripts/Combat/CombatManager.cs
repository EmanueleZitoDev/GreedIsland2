using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TipoAzione
{
    AttaccoFisico
    // In futuro: UsaAbilita, UsaCarta, Difesa, ecc.
}

public class AzioneCombattimento
{
    public CombatUnit esecutore;
    public CombatUnit bersaglio;
    public TipoAzione tipo;
    public StanceTipo stance;

    public AzioneCombattimento(CombatUnit esecutore, CombatUnit bersaglio, TipoAzione tipo, StanceTipo stance = StanceTipo.Ten)
    {
        this.esecutore = esecutore;
        this.bersaglio = bersaglio;
        this.tipo = tipo;
        this.stance = stance;
    }
}

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Combattenti")]
    public CombatUnit giocatore;
    public CombatUnit mostro;

    [Header("Impostazioni")]
    public int azioniPerTurno = 3;

    private int turnoCorrente = 1;
    private bool combattimentoAttivo = false;
    private bool inFaseSelezione = false;
    private bool azioniConfermate = false;
    private InteractableObject oggettoInterazione;

    // Code azioni
    private List<AzioneCombattimento> azioniGiocatore = new List<AzioneCombattimento>();
    private List<AzioneCombattimento> azioniMostro = new List<AzioneCombattimento>();

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

        // Reset stance a Ten per entrambi
        giocatore.stanzaCorrente = StanceTipo.Ten;
        mostro.stanzaCorrente = StanceTipo.Ten;

        Debug.Log("=== COMBATTIMENTO INIZIATO ===");
        StartCoroutine(GestisciTurno());
        giocatore.MostraUI();
        mostro.MostraUI();
        CombatUI.Instance.MostraCombatUI();
    }

    IEnumerator GestisciTurno()
    {
        while (combattimentoAttivo)
        {
            Debug.Log("--- Turno " + turnoCorrente + " ---");

            // Pulizia code
            azioniGiocatore.Clear();
            azioniMostro.Clear();

            // Fase selezione
            inFaseSelezione = true;
            Debug.Log("Scegli le tue " + azioniPerTurno + " azioni.");

            azioniConfermate = false;
            yield return new WaitUntil(() => azioniConfermate);
            inFaseSelezione = false;

            // L'IA sceglie le sue azioni
            ScegliAzioniMostro();

            Debug.Log("Azioni confermate. Esecuzione in corso...");
            yield return new WaitForSeconds(0.5f);

            // Determina l'ordine in base alla Destrezza
            bool giocatoreVaPerPrimo = giocatore.GetDestrezza() >= mostro.GetDestrezza();
            Debug.Log(giocatoreVaPerPrimo
                ? "Il giocatore ha più Destrezza — agisce per primo."
                : "Il mostro ha più Destrezza — agisce per primo.");

            // Esegui le azioni alternando i due combattenti
            for (int i = 0; i < azioniPerTurno; i++)
            {
                if (giocatoreVaPerPrimo)
                {
                    yield return StartCoroutine(EseguiAzione(azioniGiocatore[i]));
                    if (VerificaFine()) yield break;
                    yield return new WaitForSeconds(0.5f);

                    yield return StartCoroutine(EseguiAzione(azioniMostro[i]));
                    if (VerificaFine()) yield break;
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    yield return StartCoroutine(EseguiAzione(azioniMostro[i]));
                    if (VerificaFine()) yield break;
                    yield return new WaitForSeconds(0.5f);

                    yield return StartCoroutine(EseguiAzione(azioniGiocatore[i]));
                    if (VerificaFine()) yield break;
                    yield return new WaitForSeconds(0.5f);
                }
            }

            // Fine turno — rigenera Nen
            giocatore.RigeneraNen();
            mostro.RigeneraNen();

            turnoCorrente++;
        }
    }

    IEnumerator EseguiAzione(AzioneCombattimento azione)
    {
        // Aggiorna la stance corrente dell'esecutore
        azione.esecutore.stanzaCorrente = azione.stance;

        // Se stance è Ren, consuma 5 Nen
        // Se non ha abbastanza Nen, l'azione viene eseguita in Ten
        if (azione.stance == StanceTipo.Ren)
        {
            bool renRiuscito = azione.esecutore.ConsumaNenRen();
            if (!renRiuscito)
                azione.esecutore.stanzaCorrente = StanceTipo.Ten;
        }

        switch (azione.tipo)
        {
            case TipoAzione.AttaccoFisico:
                int danno = azione.esecutore.CalcolaDannoBase();
                azione.bersaglio.SubisciDanno(danno);
                Debug.Log(azione.esecutore.nomePersonaggio
                    + " attacca in " + azione.esecutore.stanzaCorrente
                    + " — " + azione.bersaglio.nomePersonaggio
                    + " subisce " + danno + " danni lordi.");
                break;
        }

        yield return null;
    }

    void ScegliAzioniMostro()
    {
        // Il mostro agisce sempre in Ten per ora
        for (int i = 0; i < azioniPerTurno; i++)
        {
            azioniMostro.Add(new AzioneCombattimento(mostro, giocatore, TipoAzione.AttaccoFisico, StanceTipo.Ten));
        }
        Debug.Log("Il mostro ha scelto le sue azioni.");
    }

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
            if (oggettoInterazione != null)
                oggettoInterazione.ForzaUscitaCombattimento();
            //GameOverUI.Instance.Mostra(); — da implementare in futuro
        }
    }

    // Chiamato dalla UI — aggiunge un'azione alla coda del giocatore con la stance selezionata
    public void AggiungiAzioneGiocatore(TipoAzione tipo, StanceTipo stance)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        if (azioniGiocatore.Count >= azioniPerTurno) return;

        azioniGiocatore.Add(new AzioneCombattimento(giocatore, mostro, tipo, stance));
        Debug.Log("Turno: " + turnoCorrente + " - Azione aggiunta: " + tipo + " in " + stance + " — " + azioniGiocatore.Count + "/" + azioniPerTurno);
    }

    // Chiamato dal pulsante Conferma
    public void ConfermaAzioni()
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        if (azioniGiocatore.Count < azioniPerTurno) return;

        azioniConfermate = true;
        inFaseSelezione = false;
    }

    public void RimuoviUltimaAzione()
    {
        if (azioniGiocatore.Count > 0)
        {
            azioniGiocatore.RemoveAt(azioniGiocatore.Count - 1);
            Debug.Log("Azione rimossa. Azioni in coda: " + azioniGiocatore.Count);
        }
    }

    // Getter pubblici
    public bool IsCombattimentoAttivo() { return combattimentoAttivo; }
    public bool IsInFaseSelezione() { return inFaseSelezione; }
    public int GetAzioniInCoda() { return azioniGiocatore.Count; }
}