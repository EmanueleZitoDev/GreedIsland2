using System.Collections.Generic;
using NUnit.Framework;

public class Danno
{
    public int valore;
    public string[] tags;
    public string fonte; // per tracciare da dove proviene il danno (es. nome dell'abilità o del buff che lo ha generato)
    public List<CombatUnit> targets;
}