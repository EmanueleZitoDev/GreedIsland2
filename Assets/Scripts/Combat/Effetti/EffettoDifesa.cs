using UnityEngine;

// Accumula difesa nel contesto — applicata quando il personaggio subisce danno
[CreateAssetMenu(fileName = "EffettoDifesa", menuName = "GreedIsland/Effetti/Difesa")]
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

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, Azione azione)
    {
        esecutore.difesaTotale.Add(new Difesa(difesaBase, azione.abilitaAttiva.tags, azione.abilitaAttiva.name));
        foreach (var s in scalings)
            esecutore.difesaTotale.Add(new Difesa(Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore), azione.abilitaAttiva.tags, azione.abilitaAttiva.name));
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