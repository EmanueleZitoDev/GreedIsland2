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
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        int dannoTotale = dannoBase;
        foreach (var s in scalings)
            dannoTotale += Mathf.FloorToInt(GetStatistica(esecutore, s.statistica) * s.moltiplicatore);

        // Accumula il danno nel contesto invece di infliggerlo subito
        contesto.dannoAccumulato += dannoTotale;

        //Debug.Log(esecutore.nomePersonaggio + " accumula " + dannoTotale + " danni — totale: " + contesto.dannoAccumulato);

        // Rimuovi buff richiesti dalle condizioni
        foreach (var condizione in condizioni)
        {
            if (condizione is CondizioneBuffAttivi condBuff)
                foreach (string buff in condBuff.buffRichiesti)
                    esecutore.RimuoviBuff(buff);
        }
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