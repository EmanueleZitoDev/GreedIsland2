using UnityEngine;

[CreateAssetMenu(fileName = "CondizioneSeiBersaglio", menuName = "GreedIsland/Condizioni/SeiBersaglio")]
public class CondizioneSeiBersaglio : CondizioneAbilita
{
    public override bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        return contesto.esecutoreEBersaglio;
    }
}