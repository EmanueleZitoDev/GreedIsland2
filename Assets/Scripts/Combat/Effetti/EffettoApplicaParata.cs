using UnityEngine;

// Applica il buff Parata al personaggio — intercetta il prossimo attacco fisico
[CreateAssetMenu(fileName = "EffettoApplicaParata", menuName = "GreedIsland/Effetti/ApplicaParata")]
public class EffettoApplicaParata : EffettoAbilita
{
    public BuffDato buffParata;

    public override void Esegui(CombatUnit esecutore, CombatUnit bersaglio, ContestoCombattimento contesto)
    {
        contesto.AggiungiBuff(esecutore, buffParata);
        Debug.Log(esecutore.nomePersonaggio + " si mette in parata.");
    }
}