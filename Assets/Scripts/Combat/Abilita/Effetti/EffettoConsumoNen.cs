using UnityEngine;

[CreateAssetMenu(fileName = "EffettoConsumoNen", menuName = "GreedIsland/Abilita/Effetti/ConsumoNen")]
public class EffettoConsumoNen : EffettoAbilita
{
    public int quantita;

    public override void Esegui(CombatUnit esecutore)
    {
        esecutore.ConsumaNen(quantita);
    }
}
