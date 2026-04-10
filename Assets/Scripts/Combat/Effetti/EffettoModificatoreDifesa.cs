using UnityEngine;

[CreateAssetMenu(fileName = "EffettoModificatoreDifesa", menuName = "GreedIsland/Effetti/ModificatoreDifesa")]
public class EffettoModificatoreDifesa : EffettoAbilita
{
    public float percentualeNen = 0.1f; // 10% per Ten, 20% per Ren

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        // Viene letto da CombatUnit.CalcolaDifesa tramite contesto
    }

    public int CalcolaDifesa(CombatUnit difensore)
    {
        return Mathf.FloorToInt(difensore.GetNen() * percentualeNen);
    }
}