using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum StanceTipo
{
    Ten,
    Ren,
    Zetsu
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

    // Restituisce il valore di destrezza — usato per determinare l'ordine di iniziativa
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

    // Difesa calcolata a inizio fase dai buff attivi — valida per tutta la durata della fase
    public int difesaFase = 0;

    public void ResetDifesaFase() { difesaFase = 0; }

    // Stance corrente del combattente — aggiornata ad ogni azione eseguita
    [HideInInspector]
    public StanceTipo stanceCorrente = StanceTipo.Ten;

    // Buff attivi sul personaggio — include passive (durata -1) e buff temporanei
    private List<BuffAttivo> buffAttivi = new List<BuffAttivo>();

    // Aggiunge un buff. Se un buff con lo stesso nome è già presente lo sostituisce (refresh).
    public void AggiungiBuff(BuffAttivo buff)
    {
        if (buff == null || buff.dato == null) return;
        RimuoviBuff(buff.dato.nomeBuff);
        buffAttivi.Add(buff);
        Debug.Log(nomePersonaggio + " ottiene buff: " + buff.dato.nomeBuff +
            (buff.IsPermanente ? " (permanente)" : " (durata " + buff.durata + ")"));
    }

    // Rimuove il buff con il nome indicato, se presente.
    public void RimuoviBuff(string nomeBuff)
    {
        buffAttivi.RemoveAll(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
    }

    // Restituisce true se il personaggio ha un buff attivo con il nome indicato.
    public bool HaBuff(string nomeBuff)
    {
        return buffAttivi.Exists(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
    }

    // Restituisce la lista dei buff attivi (copia difensiva per evitare modifiche esterne durante l'iterazione).
    public List<BuffAttivo> GetBuffAttivi()
    {
        return new List<BuffAttivo>(buffAttivi);
    }

    // Nasconde il canvas UI e calcola HP e Nen massimi in base alle statistiche
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

    // Rende visibile il canvas UI con le barre HP e Nen
    public void MostraUI()
    {
        if (canvasUI != null)
            canvasUI.SetActive(true);
    }

    // Nasconde il canvas UI con le barre HP e Nen
    public void NascondiUI()
    {
        if (canvasUI != null)
            canvasUI.SetActive(false);
    }

    // Subisce danni — la difesa viene calcolata dalla stance corrente del difensore
    public int SubisciDanno(int danno, ContestoCombattimento contesto = null)
    {
        int difesa = 0;

        if (contesto != null && this.HaBuff("Parata"))
        {
            // Gli effetti della parata accumulano su difesaFase
            BuffAttivo buffParata = GetBuffAttivi().Find(b => b.dato != null && b.dato.nomeBuff == "Parata");
            if (buffParata?.dato.effetti != null)
                foreach (var effetto in buffParata.dato.effetti)
                    if (effetto != null)
                        effetto.Esegui(this, null, contesto);

            this.RimuoviBuff("Parata");
        }

        // difesaFase include stance (calcolata a inizio fase) + eventuale parata
        difesa += difesaFase;

        int dannoEffettivo = Mathf.Max(0, danno - difesa);
        hpAttuali = Mathf.Max(0, hpAttuali - dannoEffettivo);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " subisce " + dannoEffettivo +
            " danni (difesa totale: " + difesa + "). HP: " + hpAttuali + "/" + hpMax);
        return dannoEffettivo;
    }

    // Restituisce il valore di difesa base — i modificatori stance vengono applicati
    // durante la risoluzione dell'azione tramite il sistema di buff
    public int CalcolaDifesa()
    {
        return 0;
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
    // Recupera HP senza superare il massimo e aggiorna le barre
    public void RiceviCura(int quantita)
    {
        hpAttuali = Mathf.Min(hpMax, hpAttuali + quantita);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " recupera " + quantita + " HP. HP: " + hpAttuali + "/" + hpMax);
    }

    // Calcola danno attacco fisico base — formula dal GDD: LV + FOR
    // I modificatori stance vengono applicati durante la risoluzione dell'azione tramite il sistema di buff
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

        if (testoHP != null)
            testoHP.text = hpAttuali + "/" + hpMax;

        if (testoNen != null)
            testoNen.text = nenAttuali + "/" + nenMax;
    }

    // Ripristina HP, Nen e stance ai valori iniziali. Usato quando il giocatore fugge dal combattimento.
    public void ResetCombattimento()
    {
        hpAttuali = hpMax;
        nenAttuali = nenMax;
        stanceCorrente = StanceTipo.Ten;
        AggiornaBarre();
    }

    // Decrementa i buff PerAzionePortatore dopo ogni azione del portatore e rimuove quelli scaduti.
    public void ScalaBuffPerAzionePortatore()
    {
        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
        foreach (BuffAttivo buff in buffAttivi)
        {
            if (buff.IsPermanente) continue;
            if (buff.tipoScalatura != TipoScalaturaDurata.PerAzionePortatore) continue;
            buff.durata--;
            if (buff.durata <= 0)
                daRimuovere.Add(buff);
        }
        foreach (BuffAttivo buff in daRimuovere)
        {
            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
            buffAttivi.Remove(buff);
        }
    }

    // Decrementa i buff PerAzioneCaster quando il caster specificato esegue un'azione e rimuove quelli scaduti.
    public void ScalaBuffPerAzioneCaster(CombatUnit caster)
    {
        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
        foreach (BuffAttivo buff in buffAttivi)
        {
            if (buff.IsPermanente) continue;
            if (buff.tipoScalatura != TipoScalaturaDurata.PerAzioneCaster) continue;
            if (buff.caster != caster) continue;
            buff.durata--;
            if (buff.durata <= 0)
                daRimuovere.Add(buff);
        }
        foreach (BuffAttivo buff in daRimuovere)
        {
            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
            buffAttivi.Remove(buff);
        }
    }

    // Decrementa i buff PerFase a fine fase e rimuove quelli scaduti.
    public void ScalaBuffPerFase()
    {
        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
        foreach (BuffAttivo buff in buffAttivi)
        {
            if (buff.IsPermanente) continue;
            if (buff.tipoScalatura != TipoScalaturaDurata.PerFase) continue;
            buff.durata--;
            if (buff.durata <= 0)
                daRimuovere.Add(buff);
        }
        foreach (BuffAttivo buff in daRimuovere)
        {
            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
            buffAttivi.Remove(buff);
        }
    }

    // Getter pubblici
    public int GetHP() { return hpAttuali; }
    public int GetHPMax() { return hpMax; }
    public int GetNen() { return nenAttuali; }
    public int GetNenMax() { return nenMax; }
}