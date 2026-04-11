using System.Collections.Generic;

// Rappresenta un gruppo di combattenti che condividono lo stesso obiettivo nel combattimento.
// Supporta qualsiasi configurazione: 1v1, team vs team, multi-fazione.
public class Fazione
{
    public string nome;
    public List<CombatUnit> personaggi;

    public Fazione(string nome, List<CombatUnit> personaggi)
    {
        this.nome = nome;
        this.personaggi = new List<CombatUnit>(personaggi);
    }

    // Restituisce true se tutti i personaggi della fazione sono morti
    public bool IsEliminata()
    {
        foreach (CombatUnit p in personaggi)
            if (!p.IsMorto()) return false;
        return true;
    }
}
