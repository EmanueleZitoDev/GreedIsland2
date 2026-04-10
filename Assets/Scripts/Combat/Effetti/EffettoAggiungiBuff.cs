using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungiBuff", menuName = "GreedIsland/Effetti/AggiungiBuff")]
public class EffettoAggiungiBuff : EffettoAbilita
{
    public string nomeBuff;
    public int durataAzioni = 2;
    public enum ComportamentoBuff { Refresh, Stack }
    public ComportamentoBuff comportamento = ComportamentoBuff.Refresh;
    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        bool stack = comportamento == ComportamentoBuff.Stack;
        contesto.AggiungiBuff(esecutore, nomeBuff, durataAzioni, stack);
        //Debug.Log(esecutore.nomePersonaggio + " ottiene buff: " + nomeBuff + " (durata " + durataAzioni + " azioni)");
    }
}