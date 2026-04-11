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

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, LV, Nessuno }

    public int difesaBase = 0;
    public ScalingEntry[] scalings;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        int difesaTotale = difesaBase;
        foreach (var s in scalings)
            difesaTotale += Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);

        contesto.difesaAccumulata += difesaTotale;
        Debug.Log(esecutore.nomePersonaggio + " accumula " + difesaTotale +
            " difesa — totale: " + contesto.difesaAccumulata);
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
            default: return 0;
        }
    }
}