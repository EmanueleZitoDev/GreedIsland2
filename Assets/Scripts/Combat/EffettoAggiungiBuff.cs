using UnityEngine;

[CreateAssetMenu(fileName = "EffettoAggiungiBuff", menuName = "GreedIsland/Effetti/AggiungiBuff")]
public class EffettoAggiungiBuff : EffettoAbilita
{
    public string nomeBuff;
    public int durataAzioni = 2;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        contesto.AggiungiBuff(esecutore, nomeBuff);
        Debug.Log(esecutore.nomePersonaggio + " ottiene buff: " + nomeBuff + " (durata " + durataAzioni + " azioni)");
    }
}