using UnityEngine;

[CreateAssetMenu(fileName = "EffettoConsumoNen", menuName = "GreedIsland/Effetti/ConsumoNen")]
public class EffettoConsumoNen : EffettoAbilita
{
    public int quantita;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        esecutore.ConsumaNen(quantita);
    }
}
