using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungTag", menuName = "GreedIsland/Effetti/AggiungTag")]
public class EffettoAggiungTag : EffettoAbilita
{
    public string[] tagDaAggiungere;

    // Aggiunge i tag dinamici al contesto corrente — usato dalle stance per modificare i calcoli delle abilità
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        foreach (string tag in tagDaAggiungere)
            contesto.AggiungiTagDinamico(tag);
    }
}