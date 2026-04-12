using static EffettoDanno;

public class Difesa
{
    public int valore;
    public string[] tags;
    public string fonte; // per tracciare da dove proviene il danno (es. nome dell'abilità o del buff che lo ha generato)
    public ScalingEntry[] scalings;

    public Difesa(int valore, string[] tags, string fonte)
    {
        this.valore = valore;
        this.tags = tags;
        this.fonte = fonte;
    }
}