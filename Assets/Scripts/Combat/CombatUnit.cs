using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StanceTipo
{
    Ten,
    Ren
}

public class CombatUnit : MonoBehaviour
{
    [Header("Statistiche Base")]
    public string nomePersonaggio;
    public int livello = 1;
    public int forza = 10;
    public int destrezza = 10;
    public int resistenza = 10;
    public int aura = 10;
    public int influenza = 10;
    public GameObject canvasUI;

    public int GetDestrezza() { return destrezza; }

    [Header("HP")]
    public int hpBase = 50;
    private int hpMax;
    private int hpAttuali;

    [Header("Nen")]
    public int nenBase = 30;
    private int nenMax;
    private int nenAttuali;

    [Header("UI")]
    public Slider barraHP;
    public Slider barraNen;

    [Header("UI Testi")]
    public TMP_Text testoHP;
    public TMP_Text testoNen;

    [Header("Combattimento")]
    public int azioniPerTurno = 3;

    // Stance corrente del combattente — aggiornata ad ogni azione eseguita
    [HideInInspector]
    public StanceTipo stanceCorrente = StanceTipo.Ten;

    void Awake()
    {
        if (canvasUI != null)
            canvasUI.SetActive(false);

        // Calcola HP massimi — formula dal GDD: Base(Hatsu) + COS × LV
        hpMax = hpBase + (resistenza * livello);
        hpAttuali = hpMax;

        // Calcola Nen massimi — formula dal GDD: Base(Hatsu) + floor(1/6 × INT × LV)
        nenMax = nenBase + Mathf.FloorToInt(1f / 6f * aura * livello);
        nenAttuali = nenMax;

        AggiornaBarre();
    }

    public void MostraUI()
    {
        if (canvasUI != null)
            canvasUI.SetActive(true);
    }

    public void NascondiUI()
    {
        if (canvasUI != null)
            canvasUI.SetActive(false);
    }

    // Subisce danni — la difesa viene calcolata dalla stance corrente del difensore
    public int SubisciDanno(int danno, ContestoCombattimento contesto = null)
    {
        int difesa = CalcolaDifesa();
        if (contesto != null)
            difesa = contesto.GetModificatoreDifesa(this);

        int dannoEffettivo = Mathf.Max(0, danno - difesa);
        hpAttuali = Mathf.Max(0, hpAttuali - dannoEffettivo);
        AggiornaBarre();
        return dannoEffettivo;
        //Debug.Log(nomePersonaggio + " subisce " + dannoEffettivo + " danni (difesa " + difesa + "). HP: " + hpAttuali + "/" + hpMax);
    }

    // Calcola la difesa in base alla stance corrente
    // Ten: 10% del Nen attuale — Ren: 20% del Nen attuale
    public int CalcolaDifesa()
    {
        float percentuale = stanceCorrente == StanceTipo.Ren ? 0.2f : 0.1f;
        return Mathf.FloorToInt(nenAttuali * percentuale);
    }

    // Consuma Nen per l'attivazione di Ren (5 Nen per azione)
    // Restituisce false se il Nen non è sufficiente
    public bool ConsumaNenRen()
    {
        int costoRen = 5;
        if (nenAttuali < costoRen)
        {
            Debug.Log(nomePersonaggio + " non ha abbastanza Nen per Ren — esegue in Ten.");
            return false;
        }
        nenAttuali -= costoRen;
        //Debug.Log(nomePersonaggio + " consuma " + costoRen + " Nen - Nen attuale: " + nenAttuali);
        AggiornaBarre();
        return true;
    }

    // Consuma Nen generico
    public bool ConsumaNen(int quantita)
    {
        if (nenAttuali < quantita)
        {
            Debug.Log(nomePersonaggio + " non ha abbastanza Nen.");
            return false;
        }
        nenAttuali -= quantita;
        AggiornaBarre();
        return true;
    }

    // Rigenera Nen a fine turno — formula dal GDD: floor(10% × LV + 10% × INT)
    public void RigeneraNen()
    {
        int regen = Mathf.FloorToInt(0.1f * livello + 0.1f * aura);
        nenAttuali = Mathf.Min(nenMax, nenAttuali + regen);
        AggiornaBarre();
        //Debug.Log(nomePersonaggio + " rigenera " + regen + " Nen.");
    }
    public void RiceviCura(int quantita)
    {
        hpAttuali = Mathf.Min(hpMax, hpAttuali + quantita);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " recupera " + quantita + " HP. HP: " + hpAttuali + "/" + hpMax);
    }

    // Calcola danno attacco fisico base — formula dal GDD: LV + FOR
    // Se la stance è Ren, applica +10% ai danni
    public int CalcolaDannoBase()
    {
        int dannoBase = livello + forza;
        if (stanceCorrente == StanceTipo.Ren)
            dannoBase = Mathf.FloorToInt(dannoBase * 1.1f);
        return dannoBase;
    }

    // Controlla se è morto
    public bool IsMorto()
    {
        return hpAttuali <= 0;
    }

    // Aggiorna le barre UI
    void AggiornaBarre()
    {
        if (barraHP != null)
        {
            barraHP.maxValue = hpMax;
            barraHP.value = hpAttuali;
        }

        if (barraNen != null)
        {
            barraNen.maxValue = nenMax;
            barraNen.value = nenAttuali;
        }

        if (testoHP != null)
            testoHP.text = hpAttuali + "/" + hpMax;

        if (testoNen != null)
            testoNen.text = nenAttuali + "/" + nenMax;
    }

    // Getter pubblici
    public int GetHP() { return hpAttuali; }
    public int GetHPMax() { return hpMax; }
    public int GetNen() { return nenAttuali; }
    public int GetNenMax() { return nenMax; }
}