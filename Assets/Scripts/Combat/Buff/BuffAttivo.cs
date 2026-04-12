// Scalatura della durata di un buff attivo
using Unity.VisualScripting;

public enum TipoScalaturaDurata
{
    PerAzionePortatore,  // decrementato quando il portatore esegue un'azione
    PerAzioneCaster,     // decrementato quando chi ha lanciato il buff esegue un'azione
    PerFase              // decrementato a fine fase su tutti i personaggi
}

public enum TipologiaBuff
{
    Positivo,
    Negativo,
    Neutro     
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
    public bool isStackable;                      // se true, più istanze dello stesso buff si sommano (es. +10% danno x3 = +30%)
    public int stacks;
    public int maxStacks;                       // se isStackable, il numero massimo di stacks (es. 5 per un buff che può arrivare al +50%)
    public TipologiaBuff tipologia;
    public bool isStanceBuff;                      // se true, è un buff proveniente dalla stance. Se il personaggio cambia stance, questo buff viene rimosso.
    public bool IsPermanente => durata == -1;

    public BuffAttivo(BuffDato dato, CombatUnit caster, int durata, TipoScalaturaDurata tipoScalatura, bool isStackable)
    {
        this.dato = dato;
        this.caster = caster;
        this.durata = durata;
        this.tipoScalatura = tipoScalatura;
        this.isStackable = isStackable;
        this.stacks = 1;
    }
    public BuffAttivo(BuffDato dato, CombatUnit caster, int durata, TipoScalaturaDurata tipoScalatura)
    {
        this.dato = dato;
        this.caster = caster;
        this.durata = durata;
        this.tipoScalatura = tipoScalatura;
        this.isStackable = false;
        this.stacks = 1;
    }

    public BuffAttivo(BuffDato dato, CombatUnit caster, int durata, bool isStanceBuff, TipoScalaturaDurata tipoScalatura)
    {
        this.dato = dato;
        this.caster = caster;
        this.durata = durata;
        this.tipoScalatura = tipoScalatura;
        this.isStackable = false;
        this.stacks = 1;
        this.isStanceBuff = isStanceBuff;
    }

    public void IncrementaStacks(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (this.stacks < this.maxStacks)
                this.stacks++;
            else return; // se raggiungo il max stack, non incremento più
        }
    }

    public void ImpostaStacks(int n)
    {
        if (n <= this.maxStacks)
            this.stacks = n;
        else
            this.stacks = this.maxStacks;
    }
}
