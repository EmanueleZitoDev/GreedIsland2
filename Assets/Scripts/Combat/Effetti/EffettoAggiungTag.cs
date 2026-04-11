using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungTag", menuName = "GreedIsland/Effetti/AggiungTag")]
public class EffettoAggiungTag : EffettoAbilita
{
    public string[] tagDaAggiungere;

    // I tag dinamici sono stati rimossi dall'architettura v1 — questo effetto è inattivo
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto) { }
}