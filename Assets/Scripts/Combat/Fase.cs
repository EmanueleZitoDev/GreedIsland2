using System.Collections.Generic;
using UnityEngine;

// Una fase corrisponde a uno slot d'azione del turno.
// Contiene al massimo un'azione per personaggio, ordinata per priorità e DES.
// Se un personaggio ha meno azioni degli altri la sua azione per questa fase è null e viene saltata.
public class Fase
{
    public int indice;           // indice 0-based all'interno del turno
    public List<Azione> azioni;  // azioni ordinate, pronte per l'esecuzione

    public Fase(int indice)
    {
        this.indice = indice;
        this.azioni = new List<Azione>();
    }

    // Aggiunge un'azione e mantiene la lista ordinata per priorità (ASC), poi DES (DESC) a parità.
    public void AggiungiAzione(Azione azione)
    {
        if (azione == null) return;
        azioni.Add(azione);
        azioni.Sort((a, b) =>
        {
            int cmp = a.Priorita.CompareTo(b.Priorita);
            if (cmp != 0) return cmp;
            return b.esecutore.GetDestrezza().CompareTo(a.esecutore.GetDestrezza());
        });
    }
}
