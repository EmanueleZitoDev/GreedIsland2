# Architettura del Sistema di Combattimento
**Greed Island Online — v1.0 — Aprile 2026**

---

## 1. Overview

Questo documento definisce l'architettura del sistema di combattimento da implementare. L'obiettivo è un sistema scalabile, deterministico e privo di eccezioni hardcoded, in cui ogni meccanica — stance, passive, debuff — passa attraverso lo stesso flusso di risoluzione.

---

## 2. Struttura Gerarchica

```
Combattimento
├── Fazione[N]
│   └── Personaggio[N]
│       └── BuffAttivi[]
│           ├── Passive (durata -1 = infinita)
│           └── Temporanei (durata finita)
└── Turno[N]
    └── Fase[N]  // N = maxAzioni tra tutti i personaggi
        └── Azione[N]  // una per personaggio, null se non disponibile
            ├── AbilitaAttiva
            └── StancePianificata
```

### 2.1 Combattimento e Fazioni
Il combattimento contiene N fazioni. Ogni fazione contiene N personaggi. Supporta qualsiasi configurazione: 1v1, team vs team, multi-fazione.

### 2.2 Turno e Fasi
Ogni turno è composto da N fasi, dove N è il valore massimo di azioni disponibili tra tutti i personaggi. Calcolato all'inizio di ogni turno.

Se un personaggio ha meno azioni degli altri, la sua azione per quella fase è `null` e viene saltata. Gestisce nativamente personaggi con azioni extra (da passive) e boss con più azioni.

| Fase | Gon (3 azioni) | Kurapika (3 azioni) | Boss (4 azioni) |
|------|----------------|---------------------|-----------------|
| 1    | azione[0]      | azione[0]           | azione[0]       |
| 2    | azione[1]      | azione[1]           | azione[1]       |
| 3    | azione[2]      | azione[2]           | azione[2]       |
| 4    | null           | null                | azione[3]       |

### 2.3 Azione
Ogni azione è composta da:
- **AbilitaAttiva**: l'abilità che il personaggio intende eseguire (es. Jan, attacco fisico, Recupero)
- **StancePianificata**: la stance scelta per quella specifica azione (Ten, Ren, Zetsu)

> ⚠️ La stance è un'intenzione, non uno stato garantito. Al momento dell'esecuzione la stance effettiva potrebbe differire da quella pianificata a causa di buff/debuff che forzano una stance specifica.

---

## 3. Sistema delle Stance

### 3.1 Applicazione della Stance
La stance viene applicata all'inizio di ogni fase, non pre-calcolata durante la pianificazione.

```csharp
// Pseudocodice — inizio fase
foreach (var personaggio in fase)
{
    StanceTipo stanceEffettiva = GetStanceForzataDaBuff(personaggio) 
                                 ?? personaggio.StancePianificata[faseCorrente];
    personaggio.StanceAttiva = stanceEffettiva;
}
```

### 3.2 Stance come Stato del Personaggio
La stance non è parte dell'azione pre-composta — è uno stato del personaggio che può cambiare durante l'esecuzione della fase per effetto di azioni esterne.

> **Esempio — Catena del Giudizio:** Gon pianifica Jan (azione[2]) in Ten. Kurapika è più veloce e usa Catena del Giudizio, che applica un debuff Zetsu su Gon. Quando arriva l'azione di Gon, la sua stance effettiva è Zetsu. Jan ha il tag [Nen-dipendente] e fallisce perché Zetsu disabilita tutto ciò che è [Nen-dipendente].

---

## 4. Sistema dei Buff

### 4.1 Struttura di un Buff Attivo
I buff attivi vivono sul **Personaggio**, non sul ContestoCombattimento.

```csharp
public class BuffAttivo
{
    public BuffDato dato;                      // ScriptableObject con effetti e condizioni
    public CombatUnit caster;                  // chi ha lanciato il buff (per PerAzioneCaster)
    public int durata;                         // -1 = infinita (abilità passive)
    public TipoScalaturaDurata tipoScalatura;
}

public enum TipoScalaturaDurata
{
    PerAzionePortatore,   // scala ad ogni azione di chi ha il buff
    PerAzioneCaster,      // scala ad ogni azione di chi lo ha lanciato
    PerFase               // scala a fine fase su tutti i personaggi
}
```

### 4.2 Abilità Passive come Buff Permanenti
Le abilità passive **non sono un caso speciale** — sono buff con durata `-1` applicati al personaggio quando sblocca l'abilità. Vengono valutati esattamente come qualsiasi altro buff attivo.

| Tipo | Esempio | Durata | TipoScalatura |
|------|---------|--------|---------------|
| Passiva permanente | Infusione Nen (Corpo) | -1 (infinita) | N/A |
| Buff temporaneo portatore | Jan (buff Concatenata) | 2 azioni | PerAzionePortatore |
| Debuff forzato da caster | Catena del Giudizio | configurabile | PerAzioneCaster |
| Effetto di fase | En | 5 fasi | PerFase |

### 4.3 Scalatura della Durata
- **PerAzionePortatore**: decrementato quando il personaggio che ha il buff esegue un'azione
- **PerAzioneCaster**: decrementato quando il personaggio che ha lanciato il buff esegue un'azione
- **PerFase**: decrementato a fine fase su tutti i personaggi indipendentemente

---

## 5. Risoluzione di un'Azione

### 5.1 Flusso di Esecuzione
Le azioni vengono eseguite una alla volta. Ogni azione viene risolta immediatamente — lo stato aggiornato è visibile alle azioni successive nella stessa fase.

```csharp
// Pseudocodice — esecuzione singola azione
void EseguiAzione(Azione azione, ContestoCombattimento contesto)
{
    // 1. Costruisci contesto
    contesto.esecutore = azione.esecutore;
    contesto.bersaglio = azione.bersaglio;
    contesto.abilitaCorrente = azione.abilitaAttiva;
    contesto.stanceAttiva = azione.esecutore.StanceAttiva; // già applicata a inizio fase

    // 2. Cicla tutti i buff attivi del personaggio
    foreach (var buff in azione.esecutore.buffAttivi)
    {
        if (buff.dato.ValutaCondizioni(contesto))
            buff.dato.AccumulaEffetti(contesto);
    }

    // 3. Esegui effetti dell'abilità attiva
    azione.abilitaAttiva.Esegui(contesto);

    // 4. Applica risultato finale
    ApplicaRisultato(contesto);

    // 5. Scala durata buff
    ScalaBuffDopoAzione(azione.esecutore, contesto);
}
```

### 5.2 Ordine delle Azioni nella Fase
1. **Priorità** dell'abilità — valore più basso agisce prima (counter: 0, attacco base: 3)
2. **A parità di priorità** — vince chi ha DES più alta

---

## 6. ContestoCombattimento

Snapshot leggero dell'azione corrente. **Nessuno stato persistente** — quello vive sui Personaggi.

```csharp
public class ContestoCombattimento
{
    public CombatUnit esecutore;
    public CombatUnit bersaglio;
    public AbilitaDato abilitaCorrente;
    public StanceTipo stanceAttiva;

    // Accumulatori — resettati ad ogni azione
    public int dannoAccumulato;
    public int difesaAccumulata;
}
```

**Rispetto all'implementazione attuale, rimuovere:**
- I buff attivi (spostati sul Personaggio)
- I tag dinamici (le stance non sono più un caso speciale)
- I metodi `GetModificatoreDanno` e `GetModificatoreDifesa` hardcoded per le stance

---

## 7. Cosa Riscrivere / Cosa Riutilizzare

| Classe | Azione | Note |
|--------|--------|------|
| `EffettoAbilita` (abstract) | ✅ Riutilizzare | Nessuna modifica necessaria |
| `CondizioneAbilita` (abstract) | ✅ Riutilizzare | Nessuna modifica necessaria |
| `ContestoCombattimento` | 🔄 Riscrivere | Vedi sezione 6 |
| `CombatManager` | 🔄 Riscrivere | Implementare struttura Fazione/Turno/Fase |
| `CombatUnit` | ➕ Estendere | Aggiungere `List<BuffAttivo> buffAttivi` |

---

## 8. Note Implementative

- `Fazione`, `Turno`, `Fase`, `Azione` e `BuffAttivo` sono **POCO** (no MonoBehaviour)
- Solo `CombatManager` e `CombatUnit` hanno bisogno di essere MonoBehaviour
- I moltiplicatori numerici (costo Nen stance, percentuali difesa, ecc.) sono parametri configurabili sui ScriptableObject — non hardcoded
