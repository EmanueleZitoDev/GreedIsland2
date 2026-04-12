using System.Collections.Generic;
using UnityEngine;

public abstract class EffettoAbilita : ScriptableObject
{
    [Header("Condizioni")]
    public CondizioneAbilita[] condizioni;
    public string[] tags; // Tag per identificare il tipo di effetto (es. "Danno", "Buff", "Debuff", "Difesa")

    // Verifica se tutte le condizioni sono soddisfatte
    public bool CondizioneSoddisfatta(CombatUnit esecutore)
    {
        foreach (var condizione in condizioni)
        {
            if (!condizione.Valuta(esecutore))
                return false;
        }
        return true;
    }

    // Override nelle sottoclassi
    public abstract void Esegui(CombatUnit esecutore);
}