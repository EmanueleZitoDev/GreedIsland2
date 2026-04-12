using System.Collections.Generic;
using UnityEngine;

// Accumula difesa nel contesto — applicata quando il personaggio subisce danno
[CreateAssetMenu(fileName = "EffettoDifesa", menuName = "GreedIsland/Abilita/Effetti/Difesa")]
public class EffettoDifesa : EffettoAbilita
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
        Azione azione = esecutore.azionePianificata;
        esecutore.difesaTotale.Add(new Difesa(difesaBase, azione.abilitaAttiva.tags, azione.abilitaAttiva.name));
        Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa base {difesaBase} da {azione.abilitaAttiva.name}");
        foreach (var s in scalings)
        {
            int valoreScaling = Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);
            esecutore.difesaTotale.Add(new Difesa(valoreScaling, azione.abilitaAttiva.tags, azione.abilitaAttiva.name));
            Debug.Log($"{esecutore.nomePersonaggio} guadagna difesa da scaling {valoreScaling} ({s.statistica}) da {azione.abilitaAttiva.name}");
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