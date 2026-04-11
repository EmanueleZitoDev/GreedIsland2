using UnityEngine;

// Applica un moltiplicatore al danno accumulato e aggiunge la differenza al totale
[CreateAssetMenu(fileName = "EffettoModificatoreDannoAccumulato", menuName = "GreedIsland/Effetti/ModificatoreDannoAccumulato")]
public class EffettoModificatoreDannoAccumulato : EffettoAbilita
{
    public float moltiplicatore = 1.3f;
    public string[] tagRichiesti;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        if (contesto.dannoAccumulato <= 0) return;

        // Verifica tag se necessario
        if (tagRichiesti != null && tagRichiesti.Length > 0)
        {
            string[] tagsAbilita = contesto.abilitaCorrente?.tags;
            if (tagsAbilita == null) return;
            bool trovato = false;
            foreach (string tagRichiesto in tagRichiesti)
                foreach (string tag in tagsAbilita)
                    if (tag.Trim().ToLower() == tagRichiesto.Trim().ToLower())
                    { trovato = true; break; }
            if (!trovato) return;
        }

        int bonusDanno = Mathf.FloorToInt(contesto.dannoAccumulato * (moltiplicatore - 1f));
        contesto.dannoAccumulato += bonusDanno;
        Debug.Log(esecutore.nomePersonaggio + " accumula " + bonusDanno +
            " danni da modificatore ×" + moltiplicatore + " — totale: " + contesto.dannoAccumulato);
    }
}