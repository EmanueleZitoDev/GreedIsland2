using UnityEngine;
using UnityEngine.UI;

public class CombatUnit : MonoBehaviour
{
    [Header("Statistiche Base")]
    public string nomePersonaggio;
    public int livello = 1;
    public int forza = 10;
    public int destrezza = 10;
    public int costituzione = 10;
    public int intelligenza = 10;
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

    void Awake()
    {
        if (canvasUI != null)
            canvasUI.SetActive(false);
        // Calcola HP massimi — formula dal GDD: Base(Hatsu) + COS × LV
        hpMax = hpBase + (costituzione * livello);
        hpAttuali = hpMax;

        // Calcola Nen massimi — formula dal GDD: Base(Hatsu) + floor(1/6 × INT × LV)
        nenMax = nenBase + Mathf.FloorToInt(1f / 6f * intelligenza * livello);
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

    // Subisce danni
    public void SubisciDanno(int danno)
    {
        hpAttuali = Mathf.Max(0, hpAttuali - danno);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " subisce " + danno + " danni. HP: " + hpAttuali + "/" + hpMax);
    }

    // Consuma Nen
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
        int regen = Mathf.FloorToInt(0.1f * livello + 0.1f * intelligenza);
        nenAttuali = Mathf.Min(nenMax, nenAttuali + regen);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " rigenera " + regen + " Nen.");
    }

    // Calcola danno attacco fisico base — formula dal GDD: LV + FOR
    public int CalcolaDannoBase()
    {
        return livello + forza;
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
    }

    // Getter pubblici
    public int GetHP() { return hpAttuali; }
    public int GetHPMax() { return hpMax; }
    public int GetNen() { return nenAttuali; }
    public int GetNenMax() { return nenMax; }
}