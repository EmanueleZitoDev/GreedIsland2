using UnityEngine;

[CreateAssetMenu(fileName = "CondizioneSeiBersaglio", menuName = "GreedIsland/Condizioni/SeiBersaglio")]
public class CondizioneSeiBersaglio : CondizioneAbilita
{
    // Restituisce true se l'esecutore è anche il bersaglio dell'azione (abilità su se stessi)
    public override bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        return contesto.esecutore == contesto.bersaglio;
    }
}