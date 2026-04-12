using UnityEngine;

// Applica un BuffDato al personaggio esecutore con comportamento Refresh o Stack
[CreateAssetMenu(fileName = "EffettoAggiungiBuff", menuName = "GreedIsland/Abilita/Effetti/AggiungiBuff")]
public class EffettoAggiungiBuff : EffettoAbilita
{
    public BuffDato buff;
    public enum ComportamentoBuff { Refresh, Stack }
    public ComportamentoBuff comportamento = ComportamentoBuff.Refresh;
    public TipoScalaturaDurata tipoScalatura = TipoScalaturaDurata.PerAzionePortatore;
    public bool isStackable = false;
    public override void Esegui(CombatUnit esecutore)
    {
        if (buff == null) return;
        foreach (var bersaglio in esecutore.azionePianificata.bersagli)
            esecutore.AggiungiBuffTarget(new BuffAttivo(buff, esecutore, buff.durataAzioni, tipoScalatura,isStackable), bersaglio);
    }
}