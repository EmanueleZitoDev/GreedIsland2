using UnityEngine;

[CreateAssetMenu(fileName = "EffettoDanno", menuName = "GreedIsland/Abilita/Effetti/Danno")]
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
    public string[] tagsEffetto;
    public string[] ignoraTagsScudo;

    // Calcola il danno in base a dannoBase + scalings, applica il modificatore della stance e infligge i danni al bersaglio
    public override void Esegui(CombatUnit esecutore)
    {
        esecutore.InfliggiDanno(this );
    }
}