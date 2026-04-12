using UnityEngine;

// Definisce un buff con condizioni di attivazione ed effetti.
// Usato sia da abilità attive (JAN, KEN, GUU) che da passive (Infusione Nen Corpo).
[CreateAssetMenu(fileName = "NuovoBuff", menuName = "GreedIsland/BuffDato")]
public class BuffDato : ScriptableObject
{
    public string nomeBuff;
    public int durataAzioni; // -1 = permanente
    public CondizioneBuff[] condizioniAttivazione; // condizioni per applicare gli effetti
    public EffettoBuff[] effetti;
    //public string[] tags;
}