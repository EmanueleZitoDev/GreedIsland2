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

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        int dannoTotale = dannoBase;
        foreach (var s in scalings)
            dannoTotale += Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);

        bersaglio.SubisciDanno(dannoTotale);
        Debug.Log(esecutore.nomePersonaggio + " infligge " + dannoTotale + " danni a " + bersaglio.nomePersonaggio);

        // Rimuovi buff richiesti dalle condizioni
        foreach (var condizione in condizioni)
        {
            if (condizione is CondizioneBuffAttivi condBuff)
            {
                foreach (string buff in condBuff.buffRichiesti)
                    contesto.RimuoviBuff(esecutore, buff);
            }
        }
    }

    int GetStatistica(CombatUnit unita, StatisticaScaling scaling)
    {
        switch (scaling)
        {
            case StatisticaScaling.FOR: return unita.forza;
            case StatisticaScaling.DES: return unita.destrezza;
            case StatisticaScaling.AUR: return unita.intelligenza;
            case StatisticaScaling.RES: return unita.costituzione;
            case StatisticaScaling.LV: return unita.livello;
            default: return 0;
        }
    }
}