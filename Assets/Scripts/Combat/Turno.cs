using System.Collections.Generic;

// Rappresenta un singolo turno del combattimento.
// Contiene N fasi, dove N = maxAzioniPerPersonaggio tra tutti i combattenti.
// Costruito all'inizio di ogni turno dopo che giocatore e AI hanno pianificato le loro azioni.
public class Turno
{
    public int numero;
    public List<Fase> fasi;
    public List<CombatUnit> personaggi;

    public Turno(int numero)
    {
        this.numero = numero;
        this.fasi = new List<Fase>();
        this.personaggi = new List<CombatUnit>();
    }

    // Costruisce le fasi a partire dalle azioni pianificate da tutti i personaggi.
    // personaggiConAzioni: lista di coppie (personaggio, lista azioni pianificate per il turno).
    // Le azioni sono già istanze di Azione con esecutore/bersaglio/abilita/stance.
    public static Turno Costruisci(int numero, List<(CombatUnit personaggio, List<Azione> azioni)> personaggiConAzioni)
    {
        Turno turno = new Turno(numero);

        int maxAzioni = 0;
        foreach (var coppia in personaggiConAzioni)
        {
            turno.personaggi.Add(coppia.personaggio);
            if (coppia.azioni.Count > maxAzioni)
                maxAzioni = coppia.azioni.Count;
        }

        for (int i = 0; i < maxAzioni; i++)
        {
            Fase fase = new Fase(i);
            foreach (var coppia in personaggiConAzioni)
            {
                if (i < coppia.azioni.Count)
                    fase.AggiungiAzione(coppia.azioni[i]);
            }
            turno.fasi.Add(fase);
        }

        return turno;
    }
}
