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
    public int difesaAccumulata;

    public void ResetAccumulatori()
    {
        dannoAccumulato = 0;
        difesaAccumulata = 0;
    }
}