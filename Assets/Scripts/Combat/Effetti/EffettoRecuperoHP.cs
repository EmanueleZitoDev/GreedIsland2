using UnityEngine;

[CreateAssetMenu(fileName = "EffettoRecuperoHP", menuName = "GreedIsland/Effetti/RecuperoHP")]
public class EffettoRecuperoHP : EffettoAbilita
{
    public int baseRecupero = 10;
    public float moltiplicatoreRes = 0.3f;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        int quantita = baseRecupero + Mathf.FloorToInt(esecutore.resistenza * moltiplicatoreRes);
        esecutore.RiceviCura(quantita);
        Debug.Log(esecutore.nomePersonaggio + " recupera " + quantita + " HP.");
    }
}