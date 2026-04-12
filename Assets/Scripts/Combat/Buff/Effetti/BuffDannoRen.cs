using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffDannoRen", menuName = "GreedIsland/Buff/Effetti/DannoRen")]
public class BuffDannoRen : EffettoBuff
{
    [System.Serializable]
    public class ScalingEntry
    {
        public StatisticaScaling statistica;
        public float moltiplicatore;
    }

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, LV, NEN_ATTUALE, PERCENTUALE, Nessuno }
    //public enum TipoBonusDanno { Flat, Percentuale, Scaling }

    //public int dannoBase = 0;
    //public float percentuale = 0;
    //public TipoBonusDanno tipoBonusDanno = TipoBonusDanno.Flat;
    public ScalingEntry[] scalings;

    public override void Esegui(CombatUnit esecutore)
    {
        List<Danno> nuoviDanni = new List<Danno>();
        foreach (var s in scalings)
        {
            foreach (Danno danno in esecutore.dannoTotale)
            {
                if (!danno.isFromBuff && danno.tags.Contains("[Nen-dipendente]"))
                {
                    nuoviDanni.Add(new Danno((int)((danno.valore * s.moltiplicatore) / 100), danno.tags, danno.fonte + "_BuffRen", esecutore, danno.targets, danno.ignoraTagsScudo));
                    Debug.Log($"BuffDannoRen: Aggiunto danno {nuoviDanni.Last().valore} con moltiplicatore {s.moltiplicatore}% basato su {danno.valore} del danno originale. Fonte: {danno.fonte}");
                }
            }

        }
        esecutore.dannoTotale.AddRange(nuoviDanni);
    }

}