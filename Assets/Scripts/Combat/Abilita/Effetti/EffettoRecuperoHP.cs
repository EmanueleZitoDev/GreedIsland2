using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EffettoRecuperoHP", menuName = "GreedIsland/Abilita/Effetti/RecuperoHP")]
public class EffettoRecuperoHP : EffettoAbilita
{
    public int baseRecupero = 10;
    public float moltiplicatoreRes = 0.3f;

    // Cura l'esecutore di baseRecupero + (resistenza × moltiplicatoreRes) HP
    public override void Esegui(CombatUnit esecutore)
    {
        int quantita = baseRecupero + Mathf.FloorToInt(esecutore.resistenza * moltiplicatoreRes);
        esecutore.RiceviCura(quantita);
        Debug.Log(esecutore.nomePersonaggio + " recupera " + quantita + " HP.");
    }
}