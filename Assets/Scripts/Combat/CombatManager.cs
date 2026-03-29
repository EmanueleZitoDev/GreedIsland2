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

    public AzioneCombattimento(CombatUnit esecutore, CombatUnit bersaglio, TipoAzione tipo)
    {
        this.esecutore = esecutore;
        this.bersaglio = bersaglio;
        this.tipo = tipo;
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

            // Fase selezione — il giocatore sceglie le sue azioni
            inFaseSelezione = true;
            Debug.Log("Scegli le tue " + azioniPerTurno + " azioni.");

            // Aspetta che il giocatore abbia riempito la coda
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
        switch (azione.tipo)
        {
            case TipoAzione.AttaccoFisico:
                int danno = azione.esecutore.CalcolaDannoBase();
                azione.bersaglio.SubisciDanno(danno);
                Debug.Log(azione.esecutore.nomePersonaggio
                    + " attacca " + azione.bersaglio.nomePersonaggio
                    + " per " + danno + " danni.");
                break;
        }
        yield return null;
    }

    void ScegliAzioniMostro()
    {
        // Per ora il mostro fa sempre attacco fisico
        for (int i = 0; i < azioniPerTurno; i++)
        {
            azioniMostro.Add(new AzioneCombattimento(mostro, giocatore, TipoAzione.AttaccoFisico));
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
            Debug.Log("=== HAI VINTO ===");
        else
            Debug.Log("=== HAI PERSO ===");

        // Prima esci dal combattimento
        if (oggettoInterazione != null)
            oggettoInterazione.ForzaUscitaCombattimento();

        // Poi distruggi il mostro se hai vinto
        if (giocatoreHaVinto && mostro != null)
            Destroy(mostro.gameObject);
    }

    // Chiamato dalla UI — aggiunge un'azione alla coda del giocatore
    public void AggiungiAzioneGiocatore(TipoAzione tipo)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        if (azioniGiocatore.Count >= azioniPerTurno) return;

        azioniGiocatore.Add(new AzioneCombattimento(giocatore, mostro, tipo));
        Debug.Log("Turno: " + turnoCorrente + " - Azione aggiunta: " + tipo + " — " + azioniGiocatore.Count + "/" + azioniPerTurno);

        // Non confermare automaticamente — aspetta il pulsante Conferma
    }

    // Nuovo metodo chiamato dal pulsante Conferma
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