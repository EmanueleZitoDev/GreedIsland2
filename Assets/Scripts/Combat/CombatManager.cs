using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum TipoAzione
{
    AttaccoFisico, 
    UsaAbilita
    // In futuro: UsaAbilita, UsaCarta, Difesa, ecc.
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

    [Header("Impostazioni")]
    //public int azioniPerTurno = 3;

    private int turnoCorrente = 1;
    private bool combattimentoAttivo = false;
    private bool inFaseSelezione = false;
    private bool azioniConfermate = false;
    private InteractableObject oggettoInterazione;

    // Code azioni
    private AzioneCombattimento[] azioniGiocatore;
    private AzioneCombattimento[] azioniMostro;

    private ContestoCombattimento contesto = new ContestoCombattimento();

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

        // Inizializza l'array con la dimensione corretta
        azioniGiocatore = new AzioneCombattimento[giocatore.azioniPerTurno];
        azioniMostro = new AzioneCombattimento[mostro.azioniPerTurno];

        // Reset stance a Ten per entrambi
        giocatore.stanceCorrente = StanceTipo.Ten;
        mostro.stanceCorrente = StanceTipo.Ten;

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

            // Pulizia array
            for (int i = 0; i < azioniGiocatore.Length; i++)
            {
                azioniGiocatore[i] = null;
            }
            for (int i = 0; i < azioniMostro.Length; i++)
            {
                azioniMostro[i] = null;
            }

            // Fase selezione
            inFaseSelezione = true;
            //Debug.Log("Scegli le tue " + azioniPerTurno + " azioni.");

            azioniConfermate = false;
            yield return new WaitUntil(() => azioniConfermate);
            inFaseSelezione = false;

            // L'IA sceglie le sue azioni
            ScegliAzioniMostro();

            //Debug.Log("Azioni confermate. Esecuzione in corso...");
            yield return new WaitForSeconds(0.5f);

            // Determina l'ordine in base alla Destrezza
            bool giocatoreVaPerPrimo = giocatore.GetDestrezza() >= mostro.GetDestrezza();
            //Debug.Log(giocatoreVaPerPrimo
            //    ? "Il giocatore ha più Destrezza — agisce per primo."
            //    : "Il mostro ha più Destrezza — agisce per primo.");

            // Esegui le azioni alternando i due combattenti
            int maxAzioni = Mathf.Max(giocatore.azioniPerTurno, mostro.azioniPerTurno);
            Debug.Log("maxAzioni : " + maxAzioni);
            Debug.Log("azioniGiocatore.Length : " + azioniGiocatore.Length);
            Debug.Log("azioniMostro.Length : " + azioniMostro.Length);

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

            // Fine turno — rigenera Nen
            giocatore.RigeneraNen();
            mostro.RigeneraNen();

            turnoCorrente++;
        }
    }

    IEnumerator EseguiAzione(AzioneCombattimento azione)
    {
        // Aggiorna la stance corrente dell'esecutore
        azione.esecutore.stanceCorrente = azione.stance;

        // Se stance è Ren, consuma 5 Nen
        if (azione.stance == StanceTipo.Ren)
        {
            bool renRiuscito = azione.esecutore.ConsumaNenRen();
            if (!renRiuscito)
                azione.esecutore.stanceCorrente = StanceTipo.Ten;
        }

        // Aggiorna il contesto
        contesto.esecutoreAzioneCorrente = azione.esecutore;
        contesto.bersaglioAzioneCorrente = azione.bersaglio;
        contesto.tipoAzioneCorrente = azione.tipo;

        switch (azione.tipo)
        {
            case TipoAzione.AttaccoFisico:
                int danno = azione.esecutore.CalcolaDannoBase();
                azione.bersaglio.SubisciDanno(danno);
                Debug.Log(azione.esecutore.nomePersonaggio
                    + " attacca in " + azione.esecutore.stanceCorrente
                    + " — " + azione.bersaglio.nomePersonaggio
                    + " subisce " + danno + " danni lordi.");
                break;

            case TipoAzione.UsaAbilita:
                EseguiAbilita(azione);
                break;
        }

        yield return null;
    }

    void EseguiAbilita(AzioneCombattimento azione)
    {
        if (azione.abilita == null) return;

        // Consuma il Nen
        if (azione.abilita.costoNen > 0)
        {
            bool ok = azione.esecutore.ConsumaNen(azione.abilita.costoNen);
            if (!ok)
            {
                Debug.Log("Nen insufficiente per: " + azione.abilita.nomeAbilita);
                return;
            }
        }

        // Esegui ogni effetto se le condizioni sono soddisfatte
        foreach (var effetto in azione.abilita.effetti)
        {
            if (effetto == null) continue;
            if (effetto.CondizioneSoddisfatta(azione.esecutore, azione.bersaglio, contesto))
                effetto.Esegui(azione.esecutore, azione.bersaglio, contesto);
        }
    }

    void ScegliAzioniMostro()
    {
        // Il mostro agisce sempre in Ten per ora
        for (int i = 0; i < mostro.azioniPerTurno; i++)
        {
            azioniMostro[i] = new AzioneCombattimento(mostro, giocatore, TipoAzione.AttaccoFisico, StanceTipo.Ten);
        }
        //Debug.Log("Il mostro ha scelto le sue azioni.");
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
            //if (oggettoInterazione != null)
            //    oggettoInterazione.ForzaUscitaCombattimento();
            Debug.Log("GameOverUI.Instance: " + GameOverUI.Instance);
            GameOverUI.Instance.Mostra();
        }
    }

    // Chiamato dalla UI — aggiunge un'azione alla coda del giocatore con la stance selezionata
    //public void AggiungiAzioneGiocatore(TipoAzione tipo, StanceTipo stance, int indiceSlot)
    //{
    //    if (!combattimentoAttivo || !inFaseSelezione) return;
    //    if (azioniGiocatore.Count >= azioniPerTurno) return;

    //    if (indiceSlot <= azioniGiocatore.Count)
    //        azioniGiocatore.Insert(indiceSlot, new AzioneCombattimento(giocatore, mostro, tipo, stance));
    //    else
    //        azioniGiocatore.Add(new AzioneCombattimento(giocatore, mostro, tipo, stance));

    //    string debugMsg = string.Empty;
    //    for (int i = 0; i < azioniGiocatore.Count; i++)
    //    {
    //        debugMsg += "\nAzione " + i + ": " + azioniGiocatore[i].tipo + " in " + azioniGiocatore[i].stance;
    //    }
    //    Debug.Log(debugMsg);
    //}

    public void AggiungiAzioneGiocatore(TipoAzione tipo, StanceTipo stance, int indiceSlot)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        azioniGiocatore[indiceSlot] = new AzioneCombattimento(giocatore, mostro, tipo, stance);
        Debug.Log("Azione aggiunta a slot " + indiceSlot + ": " + tipo + " in " + stance);
    }
    public void AggiungiAbilitaGiocatore(AbilitaDato abilita, StanceTipo stance, int indiceSlot)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        azioniGiocatore[indiceSlot] = new AzioneCombattimento(giocatore, mostro, TipoAzione.UsaAbilita, stance, abilita);
        Debug.Log("Turno: " + turnoCorrente + " - Abilità aggiunta: " + abilita.nomeAbilita + " in " + stance);
    }
    private int GetFirstNullAction(List<AzioneCombattimento> azioniGiocatore)
    {
        for (int i = 0; i < azioniGiocatore.Count; i++)
        {
            if (azioniGiocatore[i] == null)
                return i;
        }
        return 0;
    }

    // Chiamato dal pulsante Conferma
    public void ConfermaAzioni()
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;

        // Verifica che tutti e 3 gli slot siano occupati
        for (int i = 0; i < azioniGiocatore.Length; i++)
        {
            if (azioniGiocatore[i] == null)
            {
                Debug.Log("Slot " + i + " vuoto — conferma non possibile.");
                return;
            }
        }

        azioniConfermate = true;
        inFaseSelezione = false;
    }

    //public void RimuoviUltimaAzione()
    //{
    //    if (azioniGiocatore.Count > 0)
    //    {
    //        azioniGiocatore.RemoveAt(azioniGiocatore.Count - 1);
    //        //Debug.Log("Azione rimossa. Azioni in coda: " + azioniGiocatore.Count);
    //    }
    //}

    //public void RimuoviAzioneAIndice(int index)
    //{
    //    if (index >= 0 && index < azioniGiocatore.Count)
    //    {
    //        azioniGiocatore.RemoveAt(index);
    //        Debug.Log("Azione rimossa all'indice " + index + ". Azioni in coda: " + azioniGiocatore.Count);
    //    }
    //    string debugMsg = string.Empty;
    //    for (int i = 0; i < azioniGiocatore.Count; i++)
    //    {
    //        debugMsg += "Azione " + i + ": " + azioniGiocatore[i].tipo + " in " + azioniGiocatore[i].stance;
    //    }
    //    Debug.Log(debugMsg);
    //}

    public void RimuoviAzioneAIndice(int index)
    {
        azioniGiocatore[index] = null;
        Debug.Log("Azione rimossa allo slot " + index);
    }

    // Getter pubblici
    public bool IsCombattimentoAttivo() { return combattimentoAttivo; }
    public bool IsInFaseSelezione() { return inFaseSelezione; }
    //public int GetAzioniInCoda() { return azioniGiocatore.Count; }

}