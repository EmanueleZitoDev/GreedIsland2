using System;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BuffApplicaTag", menuName = "GreedIsland/Buff/Effetti/ApplicaTag")]
public class BuffApplicaTagRen : EffettoBuff
{
    public string[] tagDaAggiungere;

    public override void Esegui(CombatUnit esecutore)
    {
        foreach (Danno danno in esecutore.dannoTotale)
        {
            if (danno.tags.Contains("[corpo a corpo]") && danno.tags.Contains("[mani nude]"))
            {
                foreach (string tag in tagDaAggiungere)
                    danno.tags = AggiungiTagAArray(danno.tags, tag);
            }
        }
    }
    private string[] AggiungiTagAArray(string[] tags, string tag)
    {
        if (tags == null)
        {
            return new string[] { tag };
        }

        foreach (string t in tags)
        {
            if (t == tag)
            {
                return tags; // Il tag è già presente, non aggiungerlo
            }
        }

        string[] newTags = new string[tags.Length + 1];
        Array.Copy(tags, newTags, tags.Length);
        newTags[newTags.Length - 1] = tag;
        return newTags;
    }

}
