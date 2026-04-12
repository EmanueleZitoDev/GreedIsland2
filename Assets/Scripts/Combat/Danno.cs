using System.Collections.Generic;
using static EffettoDanno;

public class Danno
{
    public int valore;
    public string[] tags;
    public string[] ignoraTagsScudo; //se lo scudo del bersaglio ha uno di questi tag, ignora il suo valore
    public string fonte; // per tracciare da dove proviene il danno (es. nome dell'abilità o del buff che lo ha generato)
    public bool isFromBuff;
    public CombatUnit esecutore;
    public List<CombatUnit> targets;
    public ScalingEntry[] scalings;

    public Danno(int valore, string[] tags, string fonte, CombatUnit esecutore, List<CombatUnit> targets, string[] ignoraTagsScudo)
    {
        this.valore = valore;
        this.tags = tags;
        this.fonte = fonte;
        this.esecutore = esecutore;
        this.targets = targets;
        this.ignoraTagsScudo = ignoraTagsScudo;
    }
}