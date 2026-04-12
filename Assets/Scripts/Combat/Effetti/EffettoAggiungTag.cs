using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungTag", menuName = "GreedIsland/Effetti/AggiungTag")]
public class EffettoAggiungTag : EffettoAbilita
{
    public string[] tagDaAggiungere;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, Azione azione)
    {
        foreach (string tag in tagDaAggiungere)
            azione.abilitaAttiva.AggiungiTag(tag);
    }
}