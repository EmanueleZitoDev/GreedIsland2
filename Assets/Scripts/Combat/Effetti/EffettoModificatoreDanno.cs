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

    // Restituisce true se almeno uno dei tag richiesti è presente nei tag effettivi dell'abilità (o se non ci sono tag richiesti)
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