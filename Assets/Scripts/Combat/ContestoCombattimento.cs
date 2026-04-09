using UnityEngine;
using System.Collections.Generic;

public class ContestoCombattimento
{
    public CombatUnit esecutoreAzioneCorrente;
    public CombatUnit bersaglioAzioneCorrente;
    public TipoAzione tipoAzioneCorrente;
    public bool esecutoreEBersaglio;
    private HashSet<(CombatUnit, string)> buffAggiuntiQuestoTurno = new HashSet<(CombatUnit, string)>();

    // Buff attivi con durata — chiave: (unita, nomeBuff), valore: azioniRimanenti
    private Dictionary<(CombatUnit, string), int> buffAttivi = new Dictionary<(CombatUnit, string), int>();

    public bool HaBuff(CombatUnit unita, string nomeBuff)
    {
        return buffAttivi.ContainsKey((unita, nomeBuff));
    }


    public void RimuoviBuff(CombatUnit unita, string nomeBuff)
    {
        buffAttivi.Remove((unita, nomeBuff));
    }

    public void AggiungiBuff(CombatUnit unita, string nomeBuff, int durataAzioni = -1, bool stack = false)
    {
        var chiave = (unita, nomeBuff);

        if (!stack && buffAttivi.ContainsKey(chiave))
        {
            // Refresh — resetta la durata
            buffAttivi[chiave] = durataAzioni;
            buffAggiuntiQuestoTurno.Add(chiave);
            Debug.Log(unita.nomePersonaggio + " buff refreshato: " + nomeBuff);
            return;
        }

        buffAttivi[chiave] = durataAzioni;
        buffAggiuntiQuestoTurno.Add(chiave);
        Debug.Log(unita.nomePersonaggio + " ottiene buff: " + nomeBuff +
            (durataAzioni > 0 ? " (durata " + durataAzioni + " azioni)" : " (permanente)"));
    }

    public void DecrementaBuffDopoAzione(CombatUnit unita)
    {
        List<(CombatUnit, string)> daRimuovere = new List<(CombatUnit, string)>();
        List<(CombatUnit, string)> daAggiornare = new List<(CombatUnit, string)>();

        foreach (var kvp in buffAttivi)
        {
            if (kvp.Key.Item1 != unita) continue;
            if (kvp.Value == -1) continue;
            // Salta i buff appena aggiunti in questa azione
            if (buffAggiuntiQuestoTurno.Contains(kvp.Key)) continue;

            int nuovaDurata = kvp.Value - 1;
            if (nuovaDurata <= 0)
                daRimuovere.Add(kvp.Key);
            else
                daAggiornare.Add(kvp.Key);
        }

        foreach (var chiave in daAggiornare)
            buffAttivi[chiave] = buffAttivi[chiave] - 1;

        foreach (var chiave in daRimuovere)
        {
            Debug.Log("Buff scaduto: " + chiave.Item2 + " su " + unita.nomePersonaggio);
            buffAttivi.Remove(chiave);
        }

        // Pulisci i buff appena aggiunti dopo il decremento
        buffAggiuntiQuestoTurno.RemoveWhere(k => k.Item1 == unita);
    }
}
