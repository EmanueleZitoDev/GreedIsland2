using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungTag", menuName = "GreedIsland/Effetti/AggiungTag")]
public class EffettoAggiungTag : EffettoAbilita
{
    public string[] tagDaAggiungere;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (string tag in tagDaAggiungere)
            contesto.AggiungiTagDinamico(tag);
    }
}