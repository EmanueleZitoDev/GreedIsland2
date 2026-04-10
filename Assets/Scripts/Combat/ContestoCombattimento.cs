using UnityEngine;
using System.Collections.Generic;

public class ContestoCombattimento
{
    public CombatUnit esecutoreAzioneCorrente;
    public CombatUnit bersaglioAzioneCorrente;
    public TipoAzione tipoAzioneCorrente;
    public bool esecutoreEBersaglio;
    public AbilitaDato abilitaCorrente;
    public AbilitaDato stanceAttiva;

    // Buff attivi — chiave: (unita, nomeBuff), valore: BuffDato con durata rimanente
    private Dictionary<(CombatUnit, string), (BuffDato buff, int durata)> buffAttivi
        = new Dictionary<(CombatUnit, string), (BuffDato, int)>();

    private HashSet<(CombatUnit, string)> buffAggiuntiQuestoTurno
        = new HashSet<(CombatUnit, string)>();

    private HashSet<string> tagsDinamici = new HashSet<string>();

    // Danno accumulato durante l'esecuzione degli effetti — inflitto alla fine
    public int dannoAccumulato = 0;
    public CombatUnit bersaglioDanno = null;

    // Resetta il danno accumulato dopo ogni azione
    public void ResetDannoAccumulato()
    {
        dannoAccumulato = 0;
        bersaglioDanno = null;
    }

    // Aggiunge un buff al personaggio. Se già presente, fa refresh o stack in base al parametro
    public void AggiungiBuff(CombatUnit unita, BuffDato buff, bool stack = false)
    {
        var chiave = (unita, buff.nomeBuff);

        if (!stack && buffAttivi.ContainsKey(chiave))
        {
            buffAttivi[chiave] = (buff, buff.durataAzioni);
            buffAggiuntiQuestoTurno.Add(chiave);
            Debug.Log(unita.nomePersonaggio + " buff refreshato: " + buff.nomeBuff);
            return;
        }

        buffAttivi[chiave] = (buff, buff.durataAzioni);
        buffAggiuntiQuestoTurno.Add(chiave);
        Debug.Log(unita.nomePersonaggio + " ottiene buff: " + buff.nomeBuff +
            (buff.durataAzioni > 0 ? " (durata " + buff.durataAzioni + " azioni)" : " (permanente)"));
    }

    // Rimuove un buff dal personaggio tramite nome
    public void RimuoviBuff(CombatUnit unita, string nomeBuff)
    {
        buffAttivi.Remove((unita, nomeBuff));
    }

    // Verifica se il personaggio ha un buff attivo tramite nome
    public bool HaBuff(CombatUnit unita, string nomeBuff)
    {
        return buffAttivi.ContainsKey((unita, nomeBuff));
    }

    // Ritorna tutti i buff attivi di un personaggio
    public List<BuffDato> GetBuffAttivi(CombatUnit unita)
    {
        List<BuffDato> risultato = new List<BuffDato>();
        foreach (var kvp in buffAttivi)
            if (kvp.Key.Item1 == unita)
                risultato.Add(kvp.Value.buff);
        return risultato;
    }

    // Decrementa la durata dei buff dopo ogni azione, rimuove quelli scaduti
    public void DecrementaBuffDopoAzione(CombatUnit unita)
    {
        List<(CombatUnit, string)> daRimuovere = new List<(CombatUnit, string)>();
        List<(CombatUnit, string)> daAggiornare = new List<(CombatUnit, string)>();

        foreach (var kvp in buffAttivi)
        {
            if (kvp.Key.Item1 != unita) continue;
            if (kvp.Value.durata == -1) continue;
            if (buffAggiuntiQuestoTurno.Contains(kvp.Key)) continue;

            if (kvp.Value.durata - 1 <= 0)
                daRimuovere.Add(kvp.Key);
            else
                daAggiornare.Add(kvp.Key);
        }

        foreach (var chiave in daAggiornare)
            buffAttivi[chiave] = (buffAttivi[chiave].buff, buffAttivi[chiave].durata - 1);

        foreach (var chiave in daRimuovere)
        {
            Debug.Log("Buff scaduto: " + chiave.Item2 + " su " + unita.nomePersonaggio);
            buffAttivi.Remove(chiave);
        }

        buffAggiuntiQuestoTurno.RemoveWhere(k => k.Item1 == unita);
    }

    // Aggiunge un tag dinamico per l'azione corrente (usato dalle stance)
    public void AggiungiTagDinamico(string tag)
    {
        tagsDinamici.Add(tag.Trim().ToLower());
    }

    // Resetta i tag dinamici prima di ogni azione
    public void ResetTagDinamici()
    {
        tagsDinamici.Clear();
    }

    // Ritorna i tag effettivi dell'abilità corrente uniti ai tag dinamici della stance
    public string[] GetTagsEffettivi(string[] tagsAbilita)
    {
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

    // Ritorna il moltiplicatore danno dalla stance per i tag forniti
    public float GetModificatoreDanno(string[] tagsAbilita)
    {
        if (stanceAttiva == null || stanceAttiva.effetti == null) return 1f;

        string[] tagsEffettivi = GetTagsEffettivi(tagsAbilita);

        foreach (var effetto in stanceAttiva.effetti)
        {
            if (effetto is EffettoModificatoreDanno mod)
                if (mod.ApplicaA(tagsEffettivi))
                    return mod.moltiplicatore;
        }
        return 1f;
    }

    // Ritorna la difesa calcolata dalla stance attiva sul Nen residuo del difensore
    public int GetModificatoreDifesa(CombatUnit difensore)
    {
        if (stanceAttiva == null || stanceAttiva.effetti == null) return 0;
        foreach (var effetto in stanceAttiva.effetti)
            if (effetto is EffettoModificatoreDifesa mod)
                return mod.CalcolaDifesa(difensore);
        return 0;
    }
}