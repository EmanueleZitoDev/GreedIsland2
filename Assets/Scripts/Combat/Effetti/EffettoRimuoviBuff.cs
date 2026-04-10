using UnityEngine;

[CreateAssetMenu(fileName = "EffettoRimuoviBuff", menuName = "GreedIsland/Effetti/RimuoviBuff")]
public class EffettoRimuoviBuff : EffettoAbilita
{
    public string[] buffDaRimuovere;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (string buff in buffDaRimuovere)
        {
            contesto.RimuoviBuff(esecutore, buff);
            Debug.Log("Buff rimosso: " + buff);
        }
    }
}