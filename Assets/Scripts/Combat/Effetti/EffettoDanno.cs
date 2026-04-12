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
}