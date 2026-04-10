using UnityEngine;

public abstract class CondizioneAbilita : ScriptableObject
{
    // Valuta se la condizione è soddisfatta nel contesto corrente — implementata dalle sottoclassi
    public abstract bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto);
}