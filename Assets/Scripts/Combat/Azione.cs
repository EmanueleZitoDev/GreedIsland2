// Rappresenta l'intenzione di un personaggio per uno slot d'azione nel turno.
// StancePianificata è un'intenzione: la stance effettiva al momento dell'esecuzione
// può differire per effetto di buff/debuff che forzano una stance specifica.
public class Azione
{
    public CombatUnit esecutore;
    public CombatUnit bersaglio;
    public AbilitaDato abilitaAttiva;
    public StanceTipo stancePianificata;

    // Priorità dell'azione — valore più basso agisce prima
    public int Priorita => abilitaAttiva != null ? abilitaAttiva.priorita : 3;

    public Azione(CombatUnit esecutore, CombatUnit bersaglio, AbilitaDato abilitaAttiva, StanceTipo stancePianificata = StanceTipo.Ten)
    {
        this.esecutore = esecutore;
        this.bersaglio = bersaglio;
        this.abilitaAttiva = abilitaAttiva;
        this.stancePianificata = stancePianificata;
    }

}
