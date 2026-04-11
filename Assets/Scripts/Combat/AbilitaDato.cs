using UnityEngine;

public enum TipoAzione
{
    UsaAbilita
}

[CreateAssetMenu(fileName = "NuovaAbilita", menuName = "GreedIsland/Abilita")]
public class AbilitaDato : ScriptableObject
{
    [Header("Identificazione")]
    public string id;                    // Es. "POTENZIAMENTO_1_1"
    public string nomeAbilita;           // Es. "Infusione Nen (Corpo)"
    [TextArea] public string descrizione;

    [Header("Skill Tree")]
    public TipoHatsu albero;             // A quale albero appartiene
    public int livelloHatsu;             // Livello nell'albero (1, 2, 3...)
    public int costoPA;                  // Costo base in punti abilità

    [Header("Prerequisiti")]
    public AbilitaDato[] prerequisiti;   // Abilità da avere già sbloccate

    [Header("Combattimento")]
    public TipoAzione tipoAzione;
    public bool isPassiva;
    public int costoNen;
    public int priorita = 3;
    public string[] tags;

    [Header("Passiva")]
    public BuffDato buffPassivo;        // Buff permanente applicato all'inizio del combattimento (solo se isPassiva)

    [Header("Effetti")]
    public EffettoAbilita[] effetti;

}