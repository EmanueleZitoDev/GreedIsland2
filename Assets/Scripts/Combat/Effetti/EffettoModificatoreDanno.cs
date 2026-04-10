using UnityEngine;

[CreateAssetMenu(fileName = "EffettoModificatoreDanno", menuName = "GreedIsland/Effetti/ModificatoreDanno")]
public class EffettoModificatoreDanno : EffettoAbilita
{
    public float moltiplicatore = 1.1f;
    public string[] tagRichiesti; // es. "[nen-dipendente]"

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        // Questo effetto non fa nulla direttamente
        // Viene letto da EffettoDanno tramite contesto
    }

    public bool ApplicaA(string[] tagsEffettivi)
    {
        if (tagRichiesti == null || tagRichiesti.Length == 0) return true;
        foreach (string tagRichiesto in tagRichiesti)
        {
            foreach (string tag in tagsEffettivi)
            {
                if (tag.Trim().ToLower() == tagRichiesto.Trim().ToLower())
                    return true;
            }
        }
        return false;
    }
}