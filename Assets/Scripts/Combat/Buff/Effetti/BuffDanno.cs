using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffDanno", menuName = "GreedIsland/Buff/Effetti/Danno")]
public class BuffDanno : EffettoBuff
{
    [System.Serializable]
    public class ScalingEntry
    {
        public StatisticaScaling statistica;
        public float moltiplicatore;
    }

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, LV, NEN_ATTUALE, Nessuno }
    public enum TipoBonusDanno { Flat, Percentuale, Scaling }

    public int dannoBase = 0;
    public float percentuale = 0;
    public TipoBonusDanno tipoBonusDanno = TipoBonusDanno.Flat;
    public ScalingEntry[] scalings;

    public override void Esegui(CombatUnit esecutore)
    {
        //gestione danno base
        if (tipoBonusDanno == TipoBonusDanno.Flat)
        {
            esecutore.dannoTotale.Add(new Danno(dannoBase, this.tags, this.name, esecutore, esecutore.azionePianificata.bersagli, null));
        }

        //gestione percentuale
        //esecutore.difesaTotale.Add(new Danno(difesaBase, this.tags, this.name));
        //Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa base {difesaBase} da {this.name}");
        //foreach (var s in scalings)
        //{
        //    int valoreScaling = Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);
        //    esecutore.difesaTotale.Add(new Difesa(valoreScaling, this.tags, this.name));
        //    Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa da scaling {valoreScaling} ({s.statistica}) da {this.name}");
        //}
    }

}