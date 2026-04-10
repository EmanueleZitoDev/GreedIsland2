using UnityEngine;

// Applica un BuffDato al personaggio esecutore con comportamento Refresh o Stack
[CreateAssetMenu(fileName = "EffettoAggiungiBuff", menuName = "GreedIsland/Effetti/AggiungiBuff")]
public class EffettoAggiungiBuff : EffettoAbilita
{
    public BuffDato buff;
    public enum ComportamentoBuff { Refresh, Stack }
    public ComportamentoBuff comportamento = ComportamentoBuff.Refresh;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        if (buff == null) return;
        bool stack = comportamento == ComportamentoBuff.Stack;
        contesto.AggiungiBuff(esecutore, buff, stack);
    }
}