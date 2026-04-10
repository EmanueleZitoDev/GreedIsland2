using UnityEngine;

// Soddisfatta se l'abilità corrente ha i tag richiesti (AND = tutti, OR = almeno uno)
[CreateAssetMenu(fileName = "CondizioneTagAbilita", menuName = "GreedIsland/Condizioni/TagAbilita")]
public class CondizioneTagAbilita : CondizioneAbilita
{
    public string[] tagRichiesti;
    public enum ModalitaControllo { AND, OR }
    public ModalitaControllo modalita = ModalitaControllo.AND;

    public override bool Valuta(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        if (contesto.abilitaCorrente == null) return false;
        if (contesto.abilitaCorrente.tags == null) return false;

        if (modalita == ModalitaControllo.AND)
        {
            // Tutti i tag richiesti devono essere presenti
            foreach (string tagRichiesto in tagRichiesti)
            {
                bool trovato = false;
                foreach (string tag in contesto.abilitaCorrente.tags)
                    if (tag.Trim().ToLower() == tagRichiesto.Trim().ToLower())
                    { trovato = true; break; }
                if (!trovato) return false;
            }
            return true;
        }
        else
        {
            // Almeno uno dei tag richiesti deve essere presente
            foreach (string tagRichiesto in tagRichiesti)
                foreach (string tag in contesto.abilitaCorrente.tags)
                    if (tag.Trim().ToLower() == tagRichiesto.Trim().ToLower())
                        return true;
            return false;
        }
    }
}