using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Accumula difesa nel contesto — applicata quando il personaggio subisce danno
[CreateAssetMenu(fileName = "BuffDifesa", menuName = "GreedIsland/Buff/Effetti/Difesa")]
public class BuffDifesa : EffettoBuff
{
    [System.Serializable]
    public class ScalingEntry
    {
        public StatisticaScaling statistica;
        public float moltiplicatore;
    }

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, LV, NEN_ATTUALE, Nessuno }

    public int difesaBase = 0;
    public ScalingEntry[] scalings;

    public override void Esegui(CombatUnit esecutore)
    {
        if (difesaBase > 0)
            esecutore.difesaTotale.Add(new Difesa(difesaBase, this.tags, this.name));
        //Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa base {difesaBase} da {this.name}");
        foreach (var s in scalings)
        {
            int valoreScaling = Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);
            esecutore.difesaTotale.Add(new Difesa(valoreScaling, this.tags, this.name));
            Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa da scaling {valoreScaling} ({s.statistica}) da {this.name}");
        }
    }

    int GetStatistica(CombatUnit unita, StatisticaScaling scaling)
    {
        switch (scaling)
        {
            case StatisticaScaling.FOR: return unita.forza;
            case StatisticaScaling.DES: return unita.destrezza;
            case StatisticaScaling.AUR: return unita.aura;
            case StatisticaScaling.RES: return unita.resistenza;
            case StatisticaScaling.LV: return unita.livello;
            case StatisticaScaling.NEN_ATTUALE: return unita.GetNen();
            default: return 0;
        }
    }
}