using UnityEngine;

[CreateAssetMenu(fileName = "EffettoDanno", menuName = "GreedIsland/Effetti/Danno")]
public class EffettoDanno : EffettoAbilita
{
    public int dannoBase = 5;
    public StatisticaScaling scaling;
    public float moltiplicatoreScaling = 1f;

    public enum StatisticaScaling { FOR, DES, AUR, RES, INF, Nessuno }

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        int statValore = GetStatistica(esecutore);
        int dannoTotale = dannoBase + Mathf.FloorToInt(statValore * moltiplicatoreScaling);
        bersaglio.SubisciDanno(dannoTotale);
        Debug.Log(esecutore.nomePersonaggio + " infligge " + dannoTotale + " danni a " + bersaglio.nomePersonaggio);
    }

    int GetStatistica(CombatUnit unita)
    {
        switch (scaling)
        {
            case StatisticaScaling.FOR: return unita.forza;
            case StatisticaScaling.DES: return unita.destrezza;
            case StatisticaScaling.AUR: return unita.intelligenza; // AUR = intelligenza per ora
            case StatisticaScaling.RES: return unita.costituzione;
            default: return 0;
        }
    }
}