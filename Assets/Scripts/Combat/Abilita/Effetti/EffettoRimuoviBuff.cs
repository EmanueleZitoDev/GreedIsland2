using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffettoRimuoviBuff", menuName = "GreedIsland/Abilita/Effetti/RimuoviBuff")]
public class EffettoRimuoviBuff : EffettoAbilita
{
    public string[] buffDaRimuovere;

    // Rimuove dal contesto tutti i buff elencati in buffDaRimuovere per l'esecutore
    public override void Esegui(CombatUnit esecutore)
    {
        foreach (string buff in buffDaRimuovere)
        {
            esecutore.RimuoviBuffSelf(buff);
            Debug.Log("Buff rimosso: " + buff);
        }
    }
}