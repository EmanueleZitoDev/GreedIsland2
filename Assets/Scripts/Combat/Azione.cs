// Rappresenta l'intenzione di un personaggio per uno slot d'azione nel turno.
// StancePianificata è un'intenzione: la stance effettiva al momento dell'esecuzione
// può differire per effetto di buff/debuff che forzano una stance specifica.
using System.Collections.Generic;

public class Azione
{
    public CombatUnit esecutore;
    public List<CombatUnit> bersagli;
    public AbilitaDato abilitaAttiva;
    public StanceTipo stancePianificata;

    // Priorità dell'azione — valore più basso agisce prima
    public int Priorita => abilitaAttiva != null ? abilitaAttiva.priorita : 3;

    public Azione(CombatUnit esecutore, List<CombatUnit> bersagli, AbilitaDato abilitaAttiva, StanceTipo stancePianificata = StanceTipo.Ten)
    {
        this.esecutore = esecutore;
        this.bersagli = bersagli;
        this.abilitaAttiva = abilitaAttiva;
        this.stancePianificata = stancePianificata;
    }

}
