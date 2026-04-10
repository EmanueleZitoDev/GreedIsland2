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

    IEnumerator GestisciTurno()
    {
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

    void ScegliAzioniMostro()
    {
        for (int i = 0; i < mostro.azioniPerTurno; i++)
            azioniMostro[i] = new AzioneCombattimento(mostro, giocatore, TipoAzione.UsaAbilita, StanceTipo.Ten, attaccoFisicoBase);
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
            GameOverUI.Instance.Mostra();
        }
    }

    public void AggiungiAbilitaGiocatore(AbilitaDato abilita, StanceTipo stance, int indiceSlot)
    {
        if (!combattimentoAttivo || !inFaseSelezione) return;
        azioniGiocatore[indiceSlot] = new AzioneCombattimento(giocatore, mostro, TipoAzione.UsaAbilita, stance, abilita);
    }

    public void RimuoviAzioneAIndice(int index)
    {
        azioniGiocatore[index] = null;
    }

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

    public bool IsCombattimentoAttivo() { return combattimentoAttivo; }
    public bool IsInFaseSelezione() { return inFaseSelezione; }
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