using UnityEngine;

[CreateAssetMenu(fileName = "CondizioneBuffAttivi", menuName = "GreedIsland/Abilita/Condizioni/BuffAttivi")]
public class CondizioneBuffAttivi : CondizioneAbilita
{
    public string[] buffRichiesti;

    // Restituisce true solo se l'esecutore ha tutti i buff elencati in buffRichiesti
    public override bool Valuta(CombatUnit esecutore)
    {
        foreach (string buff in buffRichiesti)
        {
            if (!esecutore.HaBuff(buff))
                return false;
        }
        return true;
    }
}