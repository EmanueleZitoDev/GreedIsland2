using UnityEngine;

[CreateAssetMenu(fileName = "EffettoInfusioneNen", menuName = "GreedIsland/Effetti/InfusioneNen")]
public class EffettoInfusioneNen : EffettoAbilita
{
    public enum TipoInfusione { ManiNude, Arma }
    public TipoInfusione tipo;
    public float moltiplicatoreFor = 0.5f;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        // Questo è un effetto passivo — viene applicato automaticamente al calcolo del danno
        // Non fa nulla direttamente, viene letto da CombatUnit.CalcolaDannoBase()
        Debug.Log(esecutore.nomePersonaggio + " ha Infusione Nen attiva — +" + moltiplicatoreFor + " FOR ai danni");
    }
}