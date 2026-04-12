using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static EffettoDanno;

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
    public bool canChangeStance = true; // Se false, il personaggio è bloccato nella stance iniziale (usato per Zetsu)

    // Buff attivi sul personaggio — include passive (durata -1) e buff temporanei
    private List<BuffAttivo> buffAttivi = new List<BuffAttivo>();
    //lista di tag che non possono essere utilizzati da questo personaggio. viene manipolata da debuff e controllata dalle abilità prima dell'esecuzione
    private List<string> tagBloccati = new List<string>();

    public List<Difesa> difesaTotale = new List<Difesa>();
    //public int scudoParata = 0;
    // Aggiunge un buff. Se un buff con lo stesso nome è già presente lo sostituisce (refresh).

    public void AggiungiBuffTarget(BuffAttivo buff, CombatUnit target)
    {
        if (buff == null || buff.dato == null) return;
        if (buff.isStackable)
        {
            BuffAttivo buffEsistente = target.buffAttivi.Find(b => b.dato != null && b.dato.nomeBuff == buff.dato.nomeBuff);
            if (buffEsistente != null)
            {
                buffEsistente.IncrementaStacks(1);
                Debug.Log(target.nomePersonaggio + " incrementa stack di buff: " + buff.dato.nomeBuff +
                    " (stack attuali: " + buffEsistente.stacks + ")");
                buffEsistente.durata = Mathf.Max(buffEsistente.durata, buff.durata); // Refresh durata se il nuovo buff ha durata maggiore
                return;
            }
            else
            {
                target.buffAttivi.Add(buff);
            }
        }
        else
        {
            RimuoviBuffTarget(buff.dato.nomeBuff, target);
            target.buffAttivi.Add(buff);
        }
        Debug.Log(target.nomePersonaggio + " ottiene buff: " + buff.dato.nomeBuff +
            (buff.IsPermanente ? " (permanente)" : " (durata " + buff.durata + ")"));
    }

    public void AggiungiBuffSelf(BuffAttivo buff)
    {
        if (buff == null || buff.dato == null) return;
        if (buff.isStackable)
        {
            BuffAttivo buffEsistente = buffAttivi.Find(b => b.dato != null && b.dato.nomeBuff == buff.dato.nomeBuff);
            if (buffEsistente != null)
            {
                buffEsistente.IncrementaStacks(1);
                Debug.Log(nomePersonaggio + " incrementa stack di buff: " + buff.dato.nomeBuff +
                    " (stack attuali: " + buffEsistente.stacks + ")");
                buffEsistente.durata = Mathf.Max(buffEsistente.durata, buff.durata); // Refresh durata se il nuovo buff ha durata maggiore
                return;
            }
            else
            {
                buffAttivi.Add(buff);
            }
        }
        else
        {
            RimuoviBuffSelf(buff.dato.nomeBuff);
            buffAttivi.Add(buff);
        }
        Debug.Log(nomePersonaggio + " ottiene buff: " + buff.dato.nomeBuff +
            (buff.IsPermanente ? " (permanente)" : " (durata " + buff.durata + ")"));
    }

    // Rimuove il buff con il nome indicato, se presente.
    public void RimuoviBuffTarget(string nomeBuff, CombatUnit target)
    {
        target.buffAttivi.RemoveAll(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
    }
    public void RimuoviBuffSelf(string nomeBuff)
    {
        buffAttivi.RemoveAll(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
    }

    // Restituisce true se il personaggio ha un buff attivo con il nome indicato.
    public bool HaBuff(string nomeBuff)
    {
        return buffAttivi.Exists(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
    }

    // Restituisce la lista dei buff attivi (copia difensiva per evitare modifiche esterne durante l'iterazione).
    public List<BuffAttivo> GetBuffAttiviTarget(CombatUnit target)
    {
        return new List<BuffAttivo>(target.buffAttivi);
    }
    public List<BuffAttivo> GetBuffAttiviSelf()
    {
        return new List<BuffAttivo>(buffAttivi);
    }

    public List<BuffAttivo> GetDebuffAttiviSelf()
    {
        return buffAttivi.Where(b => b.tipologia == TipologiaBuff.Negativo).ToList();
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
        if (stanceCorrente == StanceTipo.Zetsu) return false; // Non si può consumare Nen in Zetsu
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


    // Getter pubblici
    public int GetHP() { return hpAttuali; }
    public int GetHPMax() { return hpMax; }
    public int GetNen() { return nenAttuali; }
    public int GetNenMax() { return nenMax; }
    public int GetDestrezza() { return destrezza; }

    public IEnumerator EseguiAzione(Azione azione)
    {
        if (!azione.esecutore.CanPlay()) yield break;

        if (azione == null) yield break;

        if (canChangeStance)
            stanceCorrente = azione.stancePianificata;

        EseguiAbilita(azione);

        // Scala buff per azione sull'esecutore (portatore) e su tutti i personaggi (caster)
        azione.esecutore.ScalaBuffPerAzionePortatore();
        ScalaBuffPerAzioneCaster(azione.esecutore);
        azione.bersaglio.ScalaBuffPerAzioneCaster(azione.esecutore);

        yield return null;
    }

    private bool CanPlay()
    {
        bool canPlay = true;
        if (this.IsMorto())
        {
            Debug.Log(nomePersonaggio + " è morto e non può agire.");
            canPlay = false;
        }
        if (this.HaBuff("Stordito"))
        {
            Debug.Log(nomePersonaggio + " è stordito e non può agire.");
            canPlay = false;
        }

        return canPlay;
    }

    public void EseguiAbilita(Azione azione)
    {
        if (azione.abilitaAttiva == null) return;

        if (!azione.esecutore.CanUseAbility(azione.abilitaAttiva))
        {
            return;
        }
        // Consuma il Nen
        if (azione.abilitaAttiva.costoNen > 0)
        {
            bool ok = ConsumaNen(azione.abilitaAttiva.costoNen);
            if (!ok)
            {
                Debug.Log("Non è possibile consumare Nen per l'abilità: " + azione.abilitaAttiva.nomeAbilita);
                return;
            }
        }
        // Consuma HP
        if (azione.abilitaAttiva.costoHP > 0)
        {
            bool ok = ConsumaHP(azione.abilitaAttiva.costoNen);
            if (!ok)
            {
                Debug.Log("Non è possibile consumare HP per l'abilità" + azione.abilitaAttiva.nomeAbilita);
                return;
            }
        }

        // Fase 1 — Effetti dei buff attivi dell'esecutore
        foreach (BuffAttivo buffAttivo in GetBuffAttiviSelf())
        {
            if (buffAttivo == null || buffAttivo.dato == null || buffAttivo.dato.effetti == null) continue;
            bool condizioniSoddisfatte = true;
            foreach (var condizione in buffAttivo.dato.condizioniAttivazione)
            {
                if (!condizione.Valuta(this, azione.bersaglio, azione))
                { condizioniSoddisfatte = false; break; }
            }
            if (!condizioniSoddisfatte)
            {
                Debug.Log("  [BUFF] " + buffAttivo.dato.nomeBuff + " — condizioni non soddisfatte, saltato");
                continue;
            }
            Debug.Log("  [BUFF] " + buffAttivo.dato.nomeBuff + " — effetti applicati");
            foreach (var effetto in buffAttivo.dato.effetti)
            {
                if (effetto == null || effetto is EffettoDifesa) continue;
                effetto.Esegui(this, azione.bersaglio, azione);
            }
        }

        // Fase 2 — Effetti dell'abilità attiva
        foreach (var effetto in azione.abilitaAttiva.effetti)
        {
            if (effetto == null) continue;
            if (effetto.CondizioneSoddisfatta(this, azione.bersaglio, azione))
                effetto.Esegui(this, azione.bersaglio, azione);
        }
    }

    private bool ConsumaHP(int quantita)
    {
        if (hpAttuali <= quantita)
        {
            Debug.Log(nomePersonaggio + " non ha abbastanza HP.");
            return false;
        }
        hpAttuali -= quantita;
        AggiornaBarre();
        return true;
    }

    private bool CanUseAbility(AbilitaDato abilitaAttiva)
    {
        //controlla nen disponibile
        if (abilitaAttiva.costoNen > 0 && nenAttuali < abilitaAttiva.costoNen)
        {
            Debug.Log(nomePersonaggio + " non ha abbastanza Nen per: " + abilitaAttiva.nomeAbilita);
            return false;
        }
        //controlla hp disponibile
        if (abilitaAttiva.costoHP > 0 && hpAttuali <= abilitaAttiva.costoHP)
        {
            Debug.Log(nomePersonaggio + " non ha abbastanza Hp per: " + abilitaAttiva.nomeAbilita);
            return false;
        }
        //lista di tag non utilizzabili
        foreach (string tag in abilitaAttiva.tags)
        {
            if (tagBloccati.Contains(tag))
            {
                Debug.Log(nomePersonaggio + " non può usare " + abilitaAttiva.nomeAbilita + " a causa del tag bloccato: " + tag);
                return false;
            }
        }

        return true;
    }

    //internal void Para()
    //{

    //    difesaTotale.Add(new Difesa(9999, new string[] { "Parata" }, "Parata"));
    //}

    internal void InfliggiDanno(EffettoDanno effettoDanno, Azione azione, CombatUnit bersaglio)
    {
        foreach (var buff in GetBuffAttiviSelf())
        {
            if (buff == null || buff.dato == null || buff.dato.effetti == null) continue;
            bool condizioniSoddisfatte = true;
            foreach (var condizione in buff.dato.condizioniAttivazione)
            {
                if (!condizione.Valuta(this, bersaglio, azione))
                { condizioniSoddisfatte = false; break; }
            }
            if (!condizioniSoddisfatte)
            {
                Debug.Log("  [BUFF] " + buff.dato.nomeBuff + " — condizioni non soddisfatte, saltato");
                continue;
            }
            Debug.Log("  [BUFF] " + buff.dato.nomeBuff + " — effetti applicati");
            foreach (var effetto in buff.dato.effetti)
            {
                if (effetto == null || !(effetto is EffettoDifesa)) continue;
                effetto.Esegui(this, bersaglio, azione);
            }
        }
        List<Danno> dannoTotale = new List<Danno>();
        dannoTotale.Add(new Danno(effettoDanno.dannoBase, effettoDanno.tagsEffetto, azione.abilitaAttiva.nomeAbilita, this, new List<CombatUnit> { bersaglio }, effettoDanno.ignoraTagsScudo));
        foreach (var s in effettoDanno.scalings)
            dannoTotale.Add(new Danno(Mathf.FloorToInt(GetStatistica(this, s.statistica) * s.moltiplicatore), effettoDanno.tagsEffetto, azione.abilitaAttiva.nomeAbilita, this, new List<CombatUnit> { bersaglio }, effettoDanno.ignoraTagsScudo));

        bersaglio.SubisciDanno(dannoTotale);
    }

    public int SubisciDanno(List<Danno> dannoTotale)
    {
        int difesaTotaleValue = 0;
        List<Difesa> difesaTotaleTemp = new List<Difesa>();
        //copia difesa totale per evitare problemi di modifica durante l'iterazione
        difesaTotaleTemp.AddRange(difesaTotale);

        //rimuove difese bloccate dai tag di danno
        foreach (Danno danno in dannoTotale)
        {
            if (danno == null) continue;
            foreach (string tag in danno.tags)
            {
                difesaTotaleTemp.RemoveAll(d => d.tags.Contains(tag));
            }
        }

        foreach (Difesa def in difesaTotaleTemp)
        {
            if (CanUseDefense(def))
            {
                difesaTotaleValue += def.valore;
            }
        }

        int dannoTotaleValue = dannoTotale.Sum(d => d.valore);

        int dannoEffettivo = Mathf.Max(0, dannoTotaleValue - difesaTotaleValue);
        hpAttuali = Mathf.Max(0, hpAttuali - dannoEffettivo);
        AggiornaBarre();
        Debug.Log(nomePersonaggio + " subisce " + dannoEffettivo +
            " danni (difesa totale: " + difesaTotaleValue + "). HP: " + hpAttuali + "/" + hpMax);
        return dannoEffettivo;
    }

    private bool CanUseDefense(Difesa defense)
    {
        if (defense == null) return false;

        foreach (string tag in defense.tags)
        {
            if (tagBloccati.Contains(tag))
            {
                Debug.Log(nomePersonaggio + " non può utilizzare difesa " + defense.fonte + " a causa del tag bloccato: " + tag);
                return false;
            }
        }
        return true;
    }


    // Restituisce il valore numerico della statistica richiesta dall'unità
    int GetStatistica(CombatUnit unita, StatisticaScaling scaling)
    {
        switch (scaling)
        {
            case StatisticaScaling.FOR: return unita.forza;
            case StatisticaScaling.DES: return unita.destrezza;
            case StatisticaScaling.RES: return unita.resistenza;
            case StatisticaScaling.AUR: return unita.aura;
            case StatisticaScaling.INF: return unita.influenza;
            case StatisticaScaling.LV: return unita.livello;
            default: return 0;
        }
    }
}
#region Gestione vecchia
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections.Generic;

//public enum StanceTipo
//{
//    Ten,
//    Ren,
//    Zetsu
//}

//public class CombatUnit : MonoBehaviour
//{
//    [Header("Statistiche Base")]
//    public string nomePersonaggio;
//    public int livello = 1;
//    public int forza = 10;
//    public int destrezza = 10;
//    public int resistenza = 10;
//    public int aura = 10;
//    public int influenza = 10;
//    public GameObject canvasUI;

//    // Restituisce il valore di destrezza — usato per determinare l'ordine di iniziativa
//    public int GetDestrezza() { return destrezza; }

//    [Header("HP")]
//    public int hpBase = 50;
//    private int hpMax;
//    private int hpAttuali;

//    [Header("Nen")]
//    public int nenBase = 30;
//    private int nenMax;
//    private int nenAttuali;

//    [Header("UI")]
//    public Slider barraHP;
//    public Slider barraNen;

//    [Header("UI Testi")]
//    public TMP_Text testoHP;
//    public TMP_Text testoNen;

//    [Header("Combattimento")]
//    public int azioniPerTurno = 3;

//    // Difesa calcolata a inizio fase dai buff attivi — valida per tutta la durata della fase
//    public int difesaFase = 0;

//    public void ResetDifesaFase() { difesaFase = 0; }

//    // Stance corrente del combattente — aggiornata ad ogni azione eseguita
//    [HideInInspector]
//    public StanceTipo stanceCorrente = StanceTipo.Ten;

//    // Buff attivi sul personaggio — include passive (durata -1) e buff temporanei
//    private List<BuffAttivo> buffAttivi = new List<BuffAttivo>();

//    // Aggiunge un buff. Se un buff con lo stesso nome è già presente lo sostituisce (refresh).
//    public void AggiungiBuff(BuffAttivo buff)
//    {
//        if (buff == null || buff.dato == null) return;
//        RimuoviBuff(buff.dato.nomeBuff);
//        buffAttivi.Add(buff);
//        Debug.Log(nomePersonaggio + " ottiene buff: " + buff.dato.nomeBuff +
//            (buff.IsPermanente ? " (permanente)" : " (durata " + buff.durata + ")"));
//    }

//    // Rimuove il buff con il nome indicato, se presente.
//    public void RimuoviBuff(string nomeBuff)
//    {
//        buffAttivi.RemoveAll(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
//    }

//    // Restituisce true se il personaggio ha un buff attivo con il nome indicato.
//    public bool HaBuff(string nomeBuff)
//    {
//        return buffAttivi.Exists(b => b.dato != null && b.dato.nomeBuff == nomeBuff);
//    }

//    // Restituisce la lista dei buff attivi (copia difensiva per evitare modifiche esterne durante l'iterazione).
//    public List<BuffAttivo> GetBuffAttivi()
//    {
//        return new List<BuffAttivo>(buffAttivi);
//    }

//    // Nasconde il canvas UI e calcola HP e Nen massimi in base alle statistiche
//    void Awake()
//    {
//        if (canvasUI != null)
//            canvasUI.SetActive(false);

//        // Calcola HP massimi — formula dal GDD: Base(Hatsu) + COS × LV
//        hpMax = hpBase + (resistenza * livello);
//        hpAttuali = hpMax;

//        // Calcola Nen massimi — formula dal GDD: Base(Hatsu) + floor(1/6 × INT × LV)
//        nenMax = nenBase + Mathf.FloorToInt(1f / 6f * aura * livello);
//        nenAttuali = nenMax;

//        AggiornaBarre();
//    }

//    // Rende visibile il canvas UI con le barre HP e Nen
//    public void MostraUI()
//    {
//        if (canvasUI != null)
//            canvasUI.SetActive(true);
//    }

//    // Nasconde il canvas UI con le barre HP e Nen
//    public void NascondiUI()
//    {
//        if (canvasUI != null)
//            canvasUI.SetActive(false);
//    }

//    // Subisce danni — la difesa viene calcolata dalla stance corrente del difensore
//    public int SubisciDanno(int danno, ContestoCombattimento contesto = null)
//    {
//        int difesa = 0;

//        if (contesto != null && this.HaBuff("Parata"))
//        {
//            // Gli effetti della parata accumulano su difesaFase
//            BuffAttivo buffParata = GetBuffAttivi().Find(b => b.dato != null && b.dato.nomeBuff == "Parata");
//            if (buffParata?.dato.effetti != null)
//                foreach (var effetto in buffParata.dato.effetti)
//                    if (effetto != null)
//                        effetto.Esegui(this, null, contesto);

//            this.RimuoviBuff("Parata");
//        }

//        // difesaFase include stance (calcolata a inizio fase) + eventuale parata
//        difesa += difesaFase;

//        int dannoEffettivo = Mathf.Max(0, danno - difesa);
//        hpAttuali = Mathf.Max(0, hpAttuali - dannoEffettivo);
//        AggiornaBarre();
//        Debug.Log(nomePersonaggio + " subisce " + dannoEffettivo +
//            " danni (difesa totale: " + difesa + "). HP: " + hpAttuali + "/" + hpMax);
//        return dannoEffettivo;
//    }

//    // Restituisce il valore di difesa base — i modificatori stance vengono applicati
//    // durante la risoluzione dell'azione tramite il sistema di buff
//    public int CalcolaDifesa()
//    {
//        return 0;
//    }

//    // Consuma Nen per l'attivazione di Ren (5 Nen per azione)
//    // Restituisce false se il Nen non è sufficiente
//    public bool ConsumaNenRen()
//    {
//        int costoRen = 5;
//        if (nenAttuali < costoRen)
//        {
//            Debug.Log(nomePersonaggio + " non ha abbastanza Nen per Ren — esegue in Ten.");
//            return false;
//        }
//        nenAttuali -= costoRen;
//        //Debug.Log(nomePersonaggio + " consuma " + costoRen + " Nen - Nen attuale: " + nenAttuali);
//        AggiornaBarre();
//        return true;
//    }

//    // Consuma Nen generico
//    public bool ConsumaNen(int quantita)
//    {
//        if (nenAttuali < quantita)
//        {
//            Debug.Log(nomePersonaggio + " non ha abbastanza Nen.");
//            return false;
//        }
//        nenAttuali -= quantita;
//        AggiornaBarre();
//        return true;
//    }

//    // Rigenera Nen a fine turno — formula dal GDD: floor(10% × LV + 10% × INT)
//    public void RigeneraNen()
//    {
//        int regen = Mathf.FloorToInt(0.1f * livello + 0.1f * aura);
//        nenAttuali = Mathf.Min(nenMax, nenAttuali + regen);
//        AggiornaBarre();
//        //Debug.Log(nomePersonaggio + " rigenera " + regen + " Nen.");
//    }
//    // Recupera HP senza superare il massimo e aggiorna le barre
//    public void RiceviCura(int quantita)
//    {
//        hpAttuali = Mathf.Min(hpMax, hpAttuali + quantita);
//        AggiornaBarre();
//        Debug.Log(nomePersonaggio + " recupera " + quantita + " HP. HP: " + hpAttuali + "/" + hpMax);
//    }

//    // Calcola danno attacco fisico base — formula dal GDD: LV + FOR
//    // I modificatori stance vengono applicati durante la risoluzione dell'azione tramite il sistema di buff
//    public int CalcolaDannoBase()
//    {
//        return livello + forza;
//    }

//    // Controlla se è morto
//    public bool IsMorto()
//    {
//        return hpAttuali <= 0;
//    }

//    // Aggiorna le barre UI
//    void AggiornaBarre()
//    {
//        if (barraHP != null)
//        {
//            barraHP.maxValue = hpMax;
//            barraHP.value = hpAttuali;
//        }

//        if (barraNen != null)
//        {
//            barraNen.maxValue = nenMax;
//            barraNen.value = nenAttuali;
//        }

//        if (testoHP != null)
//            testoHP.text = hpAttuali + "/" + hpMax;

//        if (testoNen != null)
//            testoNen.text = nenAttuali + "/" + nenMax;
//    }

//    // Ripristina HP, Nen e stance ai valori iniziali. Usato quando il giocatore fugge dal combattimento.
//    public void ResetCombattimento()
//    {
//        hpAttuali = hpMax;
//        nenAttuali = nenMax;
//        stanceCorrente = StanceTipo.Ten;
//        AggiornaBarre();
//    }

//    // Decrementa i buff PerAzionePortatore dopo ogni azione del portatore e rimuove quelli scaduti.
//    public void ScalaBuffPerAzionePortatore()
//    {
//        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
//        foreach (BuffAttivo buff in buffAttivi)
//        {
//            if (buff.IsPermanente) continue;
//            if (buff.tipoScalatura != TipoScalaturaDurata.PerAzionePortatore) continue;
//            buff.durata--;
//            if (buff.durata <= 0)
//                daRimuovere.Add(buff);
//        }
//        foreach (BuffAttivo buff in daRimuovere)
//        {
//            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
//            buffAttivi.Remove(buff);
//        }
//    }

//    // Decrementa i buff PerAzioneCaster quando il caster specificato esegue un'azione e rimuove quelli scaduti.
//    public void ScalaBuffPerAzioneCaster(CombatUnit caster)
//    {
//        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
//        foreach (BuffAttivo buff in buffAttivi)
//        {
//            if (buff.IsPermanente) continue;
//            if (buff.tipoScalatura != TipoScalaturaDurata.PerAzioneCaster) continue;
//            if (buff.caster != caster) continue;
//            buff.durata--;
//            if (buff.durata <= 0)
//                daRimuovere.Add(buff);
//        }
//        foreach (BuffAttivo buff in daRimuovere)
//        {
//            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
//            buffAttivi.Remove(buff);
//        }
//    }

//    // Decrementa i buff PerFase a fine fase e rimuove quelli scaduti.
//    public void ScalaBuffPerFase()
//    {
//        List<BuffAttivo> daRimuovere = new List<BuffAttivo>();
//        foreach (BuffAttivo buff in buffAttivi)
//        {
//            if (buff.IsPermanente) continue;
//            if (buff.tipoScalatura != TipoScalaturaDurata.PerFase) continue;
//            buff.durata--;
//            if (buff.durata <= 0)
//                daRimuovere.Add(buff);
//        }
//        foreach (BuffAttivo buff in daRimuovere)
//        {
//            Debug.Log("Buff scaduto: " + buff.dato.nomeBuff + " su " + nomePersonaggio);
//            buffAttivi.Remove(buff);
//        }
//    }

//    // Getter pubblici
//    public int GetHP() { return hpAttuali; }
//    public int GetHPMax() { return hpMax; }
//    public int GetNen() { return nenAttuali; }
//    public int GetNenMax() { return nenMax; }
//}
#endregion