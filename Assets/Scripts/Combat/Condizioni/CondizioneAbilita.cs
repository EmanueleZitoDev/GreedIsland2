using UnityEngine;

public abstract class CondizioneAbilita : ScriptableObject
{
    public abstract bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto);
}