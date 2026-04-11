# Sistema di Combattimento — Analisi Strutturale

## Struttura delle classi

### Entità principali

| Classe | Tipo | Ruolo |
|---|---|---|
| `CombatManager` | MonoBehaviour (Singleton) | Orchestratore: loop turni, ordine azioni, AI |
| `CombatUnit` | MonoBehaviour | Combattente: stats, HP/Nen, calcoli danno/difesa |
| `ContestoCombattimento` | POCO (no MonoBehaviour) | Stato condiviso per tutta la durata di un'azione |
| `AbilitaDato` | ScriptableObject | Dati di un'abilità (o stance): effetti, costo, tag, priorità |
| `BuffDato` | ScriptableObject | Dati di un buff: durata, condizioni attivazione, effetti |
| `EffettoAbilita` | ScriptableObject (astratto) | Base per tutti gli effetti eseguibili |
| `CondizioneAbilita` | ScriptableObject (astratto) | Base per tutte le condizioni di attivazione |
| `AzioneCombattimento` | POCO | Snapshot di una singola azione: chi, su chi, con quale abilità e stance |

### Gerarchia degli effetti (sottoclassi concrete di `EffettoAbilita`)

```
EffettoAbilita (abstract)
  ├── EffettoDanno                       — calcola danno base + scaling su stat
  ├── EffettoModificatoreDanno           — multiplier su tag (letto dal contesto, non eseguito)
  ├── EffettoModificatoreDannoAccumulato — multiplier finale sul dannoAccumulato
  ├── EffettoModificatoreDifesa          — calcola difesa % Nen (letto, non eseguito direttamente)
  ├── EffettoAggiungiBuff                — aggiunge un BuffDato al personaggio
  ├── EffettoApplicaParata               — aggiunge il buff "Parata"
  ├── EffettoRimuoviBuff                 — rimuove un buff per nome
  ├── EffettoAggiungTag                  — aggiunge tag dinamici al contesto (usato dalle stance)
  ├── EffettoInfusioneNen                — infonde Nen nel personaggio
  ├── EffettoRecuperoHP                  — cura HP
  └── EffettoDifesa                      — difesa generica
```

---

## Flusso del turno

```
IniziaCombattimento()
  └── StartCoroutine(GestisciTurno())   ← loop principale

GestisciTurno() — while (combattimentoAttivo):
  1. Reset slot azioni giocatore e mostro
  2. WaitUntil(azioniConfermate)        ← giocatore seleziona 3 azioni dalla UI
  3. ScegliAzioniMostro()              ← AI: 3× attacco fisico base in Ten
  4. CostruisciListaAzioni()           ← ordine per slot, poi per priorità, poi per DES
  5. foreach azione → EseguiAzione()
     └── VerificaFine()               ← morte? → TerminaCombattimento()
  6. RigeneraNen() su entrambi
  7. turnoCorrente++
```

**Ordine azioni** (`CostruisciListaAzioni`): le azioni vengono confrontate slot per slot. Per ogni coppia di azioni allo stesso slot vince prima chi ha **priorità più bassa** (campo `AbilitaDato.priorita`); a parità vince chi ha **DES più alta**.

---

## Risoluzione di un'azione

```
EseguiAzione(azione):
  1. Imposta stanceCorrente sull'esecutore
  2. Se Ren → ConsumaNenRen() (5 Nen); fallback a Ten se insufficiente
  3. Aggiorna il contesto (esecutore, bersaglio, stanceAttiva)
  4. ResetTagDinamici() + applica EffettoAggiungTag dalla stance offensiva
  5. ResetDifesaAccumulata() + accumula difesa da EffettoModificatoreDifesa della stance del bersaglio
  6. EseguiAbilita(azione)
  7. DecrementaBuffDopoAzione(esecutore)

EseguiAbilita(azione):
  1. ConsumaNen(costoNen) — abort se Nen insufficiente
  2. [Fase 1] Effetti dei buff attivi sull'esecutore (solo se condizioni soddisfatte)
  3. [Fase 2] Effetti dell'abilità selezionata (solo se condizioni soddisfatte)
     → EffettoDanno: accumula danno nel contesto (non infligge subito)
  4. [Fase 3] EffettoModificatoreDannoAccumulato dalla stance offensiva
     → moltiplica il danno accumulato in base ai tag
  5. Infligge danno finale: bersaglio.SubisciDanno(dannoAccumulato, contesto)

SubisciDanno(danno, contesto):
  1. Se ha buff "Parata" → esegui effetti parata (aggiungono a difesaAccumulata) → rimuovi buff
  2. dannoEffettivo = max(0, danno - difesaAccumulata)
  3. Scala HP, aggiorna UI
```

---

## Come funzionano le stance

Le stance (`Ten`, `Ren`) sono **`AbilitaDato` ScriptableObject** configurati nell'Inspector di `CombatManager`. Non sono abilità selezionabili per slot, ma vengono **associate a ogni slot al momento della selezione** nella UI.

**Ten** — stance difensiva:
- Ha un `EffettoModificatoreDifesa` con `percentualeNen = 0.1f` → difesa = 10% Nen attuale
- Questo valore viene letto in `EseguiAzione` e accumulato in `contesto.difesaAccumulata`

**Ren** — stance offensiva:
- Ha un `EffettoModificatoreDannoAccumulato` con moltiplicatore configurabile → bonus % sul danno accumulato
- Applica anche tag dinamici via `EffettoAggiungTag` (es. `[nen-dipendente]`)
- Costa 5 Nen per azione; se insufficiente torna automaticamente a Ten

Il pattern è: le stance non "eseguono" effetti nel senso classico; alcuni loro effetti sono **dati letti** (difesa, moltiplicatore), altri sono **applicati in fasi specifiche** di `EseguiAzione`/`EseguiAbilita`.

---

## Stato del milestone Phase 2

| Feature | Stato |
|---|---|
| Ten (difesa passiva) | Implementata |
| Ren (bonus danno + costo Nen) | Implementata |
| Attacco fisico base | Implementato |
| Parata con buff | Implementata |
| Abilità passive (SkillTree) | Implementate |
| Initiative system (chi conferma prima agisce prima) | Mancante |
| Game-over flow | Chiamata presente (`GameOverUI.Instance.Mostra()`), UI da verificare |
