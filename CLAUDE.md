# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Greed Island Online** is a Unity 6 turn-based RPG inspired by Hunter x Hunter's Greed Island arc. The long-term goal is a multiplayer online game; current development is single-player focused, building the combat prototype first.

- **Unity version:** 6000.3.10f1
- **Render pipeline:** Universal Render Pipeline (URP)
- **Input system:** Unity Input System (new)
- **Main scene:** `Assets/Scenes/SampleScene.unity`
- **Code language:** C# — comments and variable names are in Italian

## Developer Profile

Solo developer, <5 hours/week, learning Unity while building. Strong C# background, new to Unity. Approach: learn by building, iterate continuously. Tailor suggestions accordingly — favor clarity and simplicity over clever patterns.

## Development Roadmap

Progress is milestone-gated: **never suggest work from a later phase until the current milestone is complete.**

| Phase | Name | Status |
|-------|------|--------|
| 1 | Learn Unity | ✅ Complete |
| 2 | Combat Prototype | 🔄 In progress |
| 3 | Card system, skill tree, summons | Not started |
| 4 | Open world, exploration, quests | Not started |
| 5 | Multiplayer (PVP + CO-OP) | Not started |

### Phase 2 Milestone (current target)
Two characters fight in a complete turn-based combat: 3 actions/turn, HP and Nen working, physical attack, **Ten and Ren stances operational**, combat ends when a character reaches 0 HP.

**Still needed for Phase 2 milestone:**
- Ten stance (passive Nen defense)
- Ren stance (increased attack/Nen output)
- Initiative system — player who confirms first acts first
- Game-over flow on player defeat

### Phase 3 Preview (do not implement yet)
Card system with tags (`[Danno]`, `[Cura]`, `[Buff]`), partial skill tree (at least one full Hatsu tree), Zetsu stance, Gyo and In techniques, summonable creatures, minimal inventory.

## Build & Run

This is a Unity project — there are no CLI build commands for normal development.

- **Run:** Open the project in Unity Hub (requires Unity 6000.3.10f1), then press Play in the editor
- **Build:** File > Build Settings > select platform > Build
- **Edit C#:** Open `GreedIsland2.slnx` in Visual Studio; Unity auto-recompiles on file save

## Architecture

### Core Game Loop

```
Exploration → Press E near monster (InteractableObject)
  → Player moves to combat position (NavMesh)
  → Camera switches to combat view (CameraFollow)
  → Turn-based combat begins (CombatManager)
  → Action selection UI shown (CombatUI)
  → Execute queued actions per turn (CombatUnit)
  → Victory → monster destroyed, return to exploration
```

### Key Systems

**`CombatManager`** (`Assets/Scripts/Combat/CombatManager.cs`) — Singleton. Orchestrates the entire combat flow. Manages turn order (by Dexterity), runs a coroutine-based turn loop (`GestisciTurno()`), maintains the player's action queue (3 actions/turn), and drives AI for monsters.

**`CombatUnit`** (`Assets/Scripts/Combat/CombatUnit.cs`) — Represents any combatant. Holds stats (Level, STR, DEX, CON, INT), computed HP/Nen pools, and methods for taking damage (`SubisciDanno()`), calculating damage (`CalcolaDannoBase()` = Level + STR), consuming/regenerating Nen. Also manages the in-world HP/Nen bar UI via `BillboardUI`.

**`CombatUI`** (`Assets/Scripts/Combat/CombatUI.cs`) — Singleton. Canvas-based UI for action selection. Manages 3 action slots, confirm/undo buttons, a favorites panel, and a dynamic action list. Controls cursor lock state during combat. Calls `CombatManager.AggiungiAzioneGiocatore()` when player selects an action.

**`InteractableObject`** (`Assets/Scripts/InteractableObject.cs`) — Attached to monsters. Detects player proximity (configurable radius), triggers combat on E press, moves player via NavMesh to combat distance, disables the player controller, and triggers camera transition. Calls `ForzaUscitaCombattimento()` on victory to destroy the monster and restore normal gameplay.

**`CameraFollow`** (`Assets/Scripts/CameraFollow.cs`) — Third-person follow camera with right-click rotation and scroll zoom. Switches to a fixed combat perspective (behind player, angled toward monster) via `ImpostaTargetInterazione()`. Restores normal mode with `RipristinaCamera()`.

**`BillboardUI`** (`Assets/Scripts/Combat/BillboardUI.cs`) — Makes worldspace canvas HP/Nen bars always face the camera.

### Stat Formulas

- `HP_Max = hpBase + (CON × Level)`
- `Nen_Max = nenBase + floor(INT × Level / 6)`
- `DannoBase = Level + Strength`
- `RigenerazioneNen = floor(0.1 × Level + 0.1 × INT)` per turn

### Current State (as of last commits)

- Only `AttaccoFisico` (physical attack) is implemented — stance/ability/card actions are stubs
- Monster AI selects random physical attacks
- No game-over screen on player defeat
- No initiative system (who confirms first acts first)
- No quest system, progression, or save state


## Architettura Combat
Leggi Docs/ArchitetturaCombattimento_v1.md prima di toccare qualsiasi file del sistema di combattimento.