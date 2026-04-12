using UnityEngine;
using System.Collections.Generic;

public class SkillTreePersonaggio : MonoBehaviour
{
    public TipoHatsu hatsu;
    public int puntiAbilitaDisponibili;
    public List<AbilitaDato> abilitaSbloccate = new List<AbilitaDato>();

    // Tutte le abilità disponibili per questo Hatsu
    public AbilitaDato[] tutteLeAbilita;

    // Verifica se il personaggio ha abbastanza punti abilità e ha soddisfatto i prerequisiti
    public bool PuoSbloccare(AbilitaDato abilita)
    {
        // Verifica punti sufficienti
        int costo = CalcolaCosto(abilita);
        if (puntiAbilitaDisponibili < costo) return false;

        // Verifica prerequisiti
        foreach (var prerequisito in abilita.prerequisiti)
        {
            if (!abilitaSbloccate.Contains(prerequisito))
                return false;
        }

        return true;
    }

    // Sblocca l'abilità scalando i punti e aggiungendola alla lista delle sbloccate
    public bool SbloccaAbilita(AbilitaDato abilita)
    {
        if (!PuoSbloccare(abilita)) return false;
        if (abilitaSbloccate.Contains(abilita)) return false;

        int costo = CalcolaCosto(abilita);
        puntiAbilitaDisponibili -= costo;
        abilitaSbloccate.Add(abilita);

        Debug.Log("Abilità sbloccata: " + abilita.nomeAbilita + " — Punti rimanenti: " + puntiAbilitaDisponibili);
        return true;
    }

    // Calcola il costo reale in base all'Hatsu del personaggio
    public int CalcolaCosto(AbilitaDato abilita)
    {
        float moltiplicatore = GetMoltiplicatore(abilita.albero);
        return Mathf.CeilToInt(abilita.costoPA * 100 * moltiplicatore);
    }

    // Restituisce il moltiplicatore costo PA in base alla distanza tra l'Hatsu del personaggio e quello dell'abilità
    float GetMoltiplicatore(TipoHatsu albero)
    {
        // Tabella moltiplicatori dalla stella esagonale
        if (hatsu == albero) return 1.0f;

        switch (hatsu)
        {
            case TipoHatsu.Potenziamento:
                if (albero == TipoHatsu.Emissione || albero == TipoHatsu.Trasformazione) return 1.25f;
                if (albero == TipoHatsu.Manipolazione) return 1.67f;
                if (albero == TipoHatsu.Materializzazione) return 2.5f;
                break;
            case TipoHatsu.Emissione:
                if (albero == TipoHatsu.Potenziamento || albero == TipoHatsu.Manipolazione) return 1.25f;
                if (albero == TipoHatsu.Trasformazione) return 1.67f;
                if (albero == TipoHatsu.Materializzazione) return 2.5f;
                break;
            case TipoHatsu.Trasformazione:
                if (albero == TipoHatsu.Potenziamento) return 1.25f;
                if (albero == TipoHatsu.Materializzazione || albero == TipoHatsu.Manipolazione) return 1.67f;
                if (albero == TipoHatsu.Emissione) return 1.67f;
                break;
            case TipoHatsu.Manipolazione:
                if (albero == TipoHatsu.Emissione) return 1.25f;
                if (albero == TipoHatsu.Potenziamento || albero == TipoHatsu.Materializzazione) return 1.67f;
                if (albero == TipoHatsu.Trasformazione) return 2.5f;
                break;
            case TipoHatsu.Materializzazione:
                if (albero == TipoHatsu.Trasformazione || albero == TipoHatsu.Manipolazione) return 1.67f;
                if (albero == TipoHatsu.Potenziamento || albero == TipoHatsu.Emissione) return 2.5f;
                break;
        }
        return 1.0f;
    }
}