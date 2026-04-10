using UnityEngine;
using System.Collections.Generic;

public class ContestoCombattimento
{
    public CombatUnit esecutoreAzioneCorrente;
    public CombatUnit bersaglioAzioneCorrente;
    public TipoAzione tipoAzioneCorrente;
    public bool esecutoreEBersaglio;
    private HashSet<(CombatUnit, string)> buffAggiuntiQuestoTurno = new HashSet<(CombatUnit, string)>();
    public AbilitaDato stanceAttiva;

    // Buff attivi con durata — chiave: (unita, nomeBuff), valore: azioniRimanenti
    private Dictionary<(CombatUnit, string), int> buffAttivi = new Dictionary<(CombatUnit, string), int>();

    // Controlla se l'unità ha un buff attivo con il nome specificato
    public bool HaBuff(CombatUnit unita, string nomeBuff)
    {
        return buffAttivi.ContainsKey((unita, nomeBuff));
    }


    // Rimuove immediatamente il buff indicato dall'unità
    public void RimuoviBuff(CombatUnit unita, string nomeBuff)
    {
        buffAttivi.Remove((unita, nomeBuff));
    }

    // Aggiunge un buff con durata in azioni — se stack=false fa refresh, se stack=true accumula istanze
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

    // Decrementa la durata dei buff dell'unità dopo ogni azione e rimuove quelli scaduti; salta i buff appena aggiunti
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

    // Restituisce il moltiplicatore danno dalla stance attiva, filtrato per i tag dell'abilità
    public float GetModificatoreDanno(string[] tagsAbilita)
    {
        if (stanceAttiva == null || stanceAttiva.effetti == null) return 1f;

        string[] tagsEffettivi = GetTagsEffettivi(tagsAbilita);

        foreach (var effetto in stanceAttiva.effetti)
        {
            if (effetto is EffettoModificatoreDanno mod)
            {
                if (mod.ApplicaA(tagsEffettivi))
                    return mod.moltiplicatore;
            }
        }
        return 1f;
    }

    // Restituisce il valore di difesa calcolato dalla stance attiva per il difensore
    public int GetModificatoreDifesa(CombatUnit difensore)
    {
        if (stanceAttiva == null || stanceAttiva.effetti == null) return 0;
        foreach (var effetto in stanceAttiva.effetti)
        {
            if (effetto is EffettoModificatoreDifesa mod)
                return mod.CalcolaDifesa(difensore);
        }
        return 0;
    }

    // Tag aggiunti dinamicamente dalla stance all'azione corrente
    private HashSet<string> tagsDinamici = new HashSet<string>();

    // Aggiunge un tag dinamico al set corrente — valido solo per l'azione in corso
    public void AggiungiTagDinamico(string tag)
    {
        tagsDinamici.Add(tag.Trim().ToLower());
    }

    // Pulisce i tag dinamici prima di ogni nuova azione
    public void ResetTagDinamici()
    {
        tagsDinamici.Clear();
    }

    // Unisce i tag fissi dell'abilità con i tag dinamici aggiunti dalla stance, restituisce l'insieme completo
    public string[] GetTagsEffettivi(string[] tagsAbilita)
    {
        // Unisce i tag fissi dell'abilità con quelli dinamici della stance
        HashSet<string> tutti = new HashSet<string>();
        if (tagsAbilita != null)
            foreach (var t in tagsAbilita)
                tutti.Add(t.Trim().ToLower());
        foreach (var t in tagsDinamici)
            tutti.Add(t);

        string[] result = new string[tutti.Count];
        tutti.CopyTo(result);
        return result;
    }
}
