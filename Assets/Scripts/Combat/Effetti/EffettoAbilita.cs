using UnityEngine;

public abstract class EffettoAbilita : ScriptableObject
{
    [Header("Condizioni")]
    public CondizioneAbilita[] condizioni;

    // Verifica se tutte le condizioni sono soddisfatte
    public bool CondizioneSoddisfatta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (var condizione in condizioni)
        {
            if (!condizione.Valuta(esecutore, bersaglio, contesto))
                return false;
        }
        return true;
    }

    // Override nelle sottoclassi
    public abstract void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto);
}