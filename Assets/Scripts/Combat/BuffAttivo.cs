// Scalatura della durata di un buff attivo
public enum TipoScalaturaDurata
{
    PerAzionePortatore,  // decrementato quando il portatore esegue un'azione
    PerAzioneCaster,     // decrementato quando chi ha lanciato il buff esegue un'azione
    PerFase              // decrementato a fine fase su tutti i personaggi
}

// Istanza di un buff applicato a un personaggio.
// Wrappa il BuffDato (dati statici) con lo stato dinamico: caster, durata rimanente e tipo di scalatura.
// Le abilità passive sono buff con durata -1 (infinita).
public class BuffAttivo
{
    public BuffDato dato;
    public CombatUnit caster;
    public int durata;                          // -1 = infinita
    public TipoScalaturaDurata tipoScalatura;

    public bool IsPermanente => durata == -1;

    public BuffAttivo(BuffDato dato, CombatUnit caster, int durata, TipoScalaturaDurata tipoScalatura)
    {
        this.dato = dato;
        this.caster = caster;
        this.durata = durata;
        this.tipoScalatura = tipoScalatura;
    }
}
