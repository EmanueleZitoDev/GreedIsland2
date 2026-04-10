using UnityEngine;

[CreateAssetMenu(fileName = "CondizioneBuffAttivi", menuName = "GreedIsland/Condizioni/BuffAttivi")]
public class CondizioneBuffAttivi : CondizioneAbilita
{
    public string[] buffRichiesti;

    public override bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (string buff in buffRichiesti)
        {
            if (!contesto.HaBuff(esecutore, buff))
                return false;
        }
        return true;
    }
}