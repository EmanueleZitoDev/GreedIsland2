using UnityEngine;
using System.Collections.Generic;

// Contiene tutte le informazioni sul contesto dell'azione corrente
// Passato a effetti e condizioni per valutare la situazione
public class ContestoCombattimento
{
    public CombatUnit esecutoreAzioneCorrente;
    public CombatUnit bersaglioAzioneCorrente;
    public TipoAzione tipoAzioneCorrente;
    public bool esecutoreEBersaglio; // true se chi esegue è anche il bersaglio dell'azione avversaria

    // Buff attivi per ogni combattente
    public Dictionary<CombatUnit, List<string>> buffAttivi = new Dictionary<CombatUnit, List<string>>();

    public bool HaBuff(CombatUnit unita, string nomeBuff)
    {
        if (!buffAttivi.ContainsKey(unita)) return false;
        return buffAttivi[unita].Contains(nomeBuff);
    }

    public void AggiungiBuff(CombatUnit unita, string nomeBuff)
    {
        if (!buffAttivi.ContainsKey(unita))
            buffAttivi[unita] = new List<string>();
        buffAttivi[unita].Add(nomeBuff);
    }

    public void RimuoviBuff(CombatUnit unita, string nomeBuff)
    {
        if (!buffAttivi.ContainsKey(unita)) return;
        buffAttivi[unita].Remove(nomeBuff);
    }
}