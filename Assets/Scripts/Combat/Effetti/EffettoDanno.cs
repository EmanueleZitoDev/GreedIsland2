using UnityEngine;

[CreateAssetMenu(fileName = "EffettoDanno", menuName = "GreedIsland/Effetti/Danno")]
public class EffettoDanno : EffettoAbilita
{
    [System.Serializable]
    public class ScalingEntry
    {
        public StatisticaScaling statistica;
        public float moltiplicatore;
    }

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, LV, Nessuno }

    public int dannoBase = 0;
    public ScalingEntry[] scalings;
    public string[] tagsAbilita;

    // Calcola il danno in base a dannoBase + scalings, applica il modificatore della stance e infligge i danni al bersaglio
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, Azione azione)
    {
        int dannoTotale = dannoBase;

        esecutore.InfliggiDanno(this, azione, bersaglio);
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