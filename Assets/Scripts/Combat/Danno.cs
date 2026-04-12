using System.Collections.Generic;
using NUnit.Framework;
using static EffettoDanno;

public class Danno
{
    public int valore;
    public string[] tags;
    public string fonte; // per tracciare da dove proviene il danno (es. nome dell'abilità o del buff che lo ha generato)
    public CombatUnit esecutore;
    public List<CombatUnit> targets;
    public ScalingEntry[] scalings;

    public Danno(int valore, string[] tags, string fonte, CombatUnit esecutore, List<CombatUnit> targets)
    {
        this.valore = valore;
        this.tags = tags;
        this.fonte = fonte;
        this.esecutore = esecutore;
        this.targets = targets;
    }
}