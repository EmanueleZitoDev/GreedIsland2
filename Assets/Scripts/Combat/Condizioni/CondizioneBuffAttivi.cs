using UnityEngine;

[CreateAssetMenu(fileName = "CondizioneBuffAttivi", menuName = "GreedIsland/Condizioni/BuffAttivi")]
public class CondizioneBuffAttivi : CondizioneAbilita
{
    public string[] buffRichiesti;

    // Restituisce true solo se l'esecutore ha tutti i buff elencati in buffRichiesti
    public override bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (string buff in buffRichiesti)
        {
            if (!esecutore.HaBuff(buff))
                return false;
        }
        return true;
    }
}