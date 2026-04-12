using System.Collections.Generic;

// Snapshot leggero dell'azione in corso di risoluzione.
// Nessuno stato persistente — quello vive sui Personaggi (CombatUnit.buffAttivi).
// Viene resettato ad ogni nuova azione.
public class ContestoCombattimento
{
    public CombatUnit esecutore;
    public CombatUnit bersaglio;
    public AbilitaDato abilitaCorrente;
    public StanceTipo stanceAttiva;

    // Accumulatori — resettati ad ogni azione tramite ResetAccumulatori()
    public int dannoAccumulato;

    // Tag aggiuntivi iniettati durante l'azione (es. da buff di stance)
    private List<string> tagsAggiuntivi = new List<string>();

    public void AggiungiTag(string tag)
    {
        tagsAggiuntivi.Add(tag.Trim().ToLower());
    }

    // Restituisce l'unione dei tag statici dell'abilità corrente e dei tag aggiuntivi
    public string[] GetTagsEffettivi()
    {
        HashSet<string> tutti = new HashSet<string>();
        if (abilitaCorrente?.tags != null)
            foreach (string t in abilitaCorrente.tags)
                tutti.Add(t.Trim().ToLower());
        foreach (string t in tagsAggiuntivi)
            tutti.Add(t);
        string[] result = new string[tutti.Count];
        tutti.CopyTo(result);
        return result;
    }

    public void ResetAccumulatori()
    {
        dannoAccumulato = 0;
        tagsAggiuntivi.Clear();
    }
}