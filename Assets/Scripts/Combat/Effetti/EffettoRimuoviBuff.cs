using UnityEngine;

[CreateAssetMenu(fileName = "EffettoRimuoviBuff", menuName = "GreedIsland/Effetti/RimuoviBuff")]
public class EffettoRimuoviBuff : EffettoAbilita
{
    public string[] buffDaRimuovere;

    // Rimuove dal contesto tutti i buff elencati in buffDaRimuovere per l'esecutore
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, Azione azione)
    {
        foreach (string buff in buffDaRimuovere)
        {
            esecutore.RimuoviBuffSelf(buff);
            Debug.Log("Buff rimosso: " + buff);
        }
    }
}