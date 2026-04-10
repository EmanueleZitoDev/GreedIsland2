using UnityEngine;

// Definisce un buff con condizioni di attivazione ed effetti.
// Usato sia da abilità attive (JAN, KEN, GUU) che da passive (Infusione Nen Corpo).
[CreateAssetMenu(fileName = "NuovoBuff", menuName = "GreedIsland/Buff")]
public class BuffDato : ScriptableObject
{
    public string nomeBuff;
    public int durataAzioni; // -1 = permanente
    public CondizioneAbilita[] condizioniAttivazione; // condizioni per applicare gli effetti
    public EffettoAbilita[] effetti;
    public string[] tags;
}