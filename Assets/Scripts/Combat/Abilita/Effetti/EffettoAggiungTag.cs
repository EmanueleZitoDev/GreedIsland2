using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungTag", menuName = "GreedIsland/Abilita/Effetti/AggiungTag")]
public class EffettoAggiungTag : EffettoAbilita
{
    public string[] tagDaAggiungere;

    public override void Esegui(CombatUnit esecutore)
    {
        foreach (string tag in tagDaAggiungere)
            esecutore.azionePianificata.abilitaAttiva.AggiungiTag(tag);
    }
}