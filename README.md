# Elin's PEAK

## Level Up Everything!

PEAK already gives you a mountain full of terrible decisions, near-death climbs, overloaded backpacks, poison, cold, heat, exhaustion, and the occasional extremely avoidable fall. **Elin's PEAK makes your character actually learn from all of it!**

The idea is simple: **if you keep doing something, you should get better at it.** Carry too much stuff and you slowly become better at handling the weight. Climb constantly and your climbing improves. Run everywhere, jump everything, survive awful falls, push through bad conditions, or keep moving while starving and those experiences start turning into permanent progression.

It is heavily inspired by the use-based progression of **Elin**: there is no single character level deciding what you are good at, and you do not spend a pile of generic points after a run. Your skills grow because you actually used them!

Elin's PEAK currently adds **18 persistent skills**, each with a default maximum level of **999**. Progress belongs to **your player**, is stored locally, and carries between sessions instead of letting the lobby host decide everyone's progression.

## What can I level?

A lot!

### Get stronger, faster, and harder to kill

- **Strength** — Carry weight while moving to train it. Higher Strength reduces the effective Weight affliction from what you are hauling and unlocks additional backpack slots at milestone levels.
- **Endurance** — Spend stamina to train it. Endurance increases your actual maximum stamina and improves stamina regeneration.
- **Athletics** — Walking trains it; sprinting trains it faster. Athletics improves normal ground movement, sprint movement, and sprint stamina efficiency.
- **Agility** — Jump! Agility improves jump impulse, jump stamina efficiency, and gives you a little more control while airborne.
- **Vitality** — Surviving legitimate fall injuries trains it. Higher Vitality reduces future fall Injury.

You are not picking a build from a menu here. If you are the person sprinting across every flat surface, you naturally become the better runner. If you insist on carrying half the mountain in your backpack, your character eventually starts adapting to that too.

### Climbing gets its own progression

Climbing is split up because a wall, a rope, and a vine do not behave the same way!

- **Wall Climbing** — Improves wall-climbing speed and reduces wall-climbing stamina cost.
- **Rope Climbing** — Improves rope-climbing speed and stamina efficiency.
- **Vine Climbing** — Improves vine-climbing speed and stamina efficiency, while also preserving more vine momentum.
- **Wet Grip** — Trains while climbing slippery walls. It reduces slippery downward pull and related climbing drain without simply making you immune to Cold.
- **Climbing Tenacity** — Trains while you continue wall climbing below 20% regular stamina. It reduces the nasty low-stamina climbing penalty, so repeatedly pushing those desperate last few meters actually makes you better at doing it!

### The mountain can train you too

Repeated exposure to conditions creates its own progression instead of being rolled into one generic resistance stat.

- **Poison** — Reduces incoming Poison and speeds its natural recovery.
- **Cold** — Reduces incoming Cold and speeds natural recovery where PEAK marks that recovery path.
- **Heat** — Reduces incoming Heat and speeds its natural recovery.
- **Drowsy** — Reduces incoming Drowsy and speeds its natural recovery.
- **Spores** — Reduces incoming Spores and speeds their natural recovery.
- **Hunger** — Reduces incoming Hunger.
- **Curse** — Reduces incoming Curse.
- **Petrification** — Reduces petrification from the shared amulet and Citadel path.

These eight skills appear together in the blue **Resiliency** panel. They gain XP only when the matching affliction actually increases; natural recovery and cleansing no longer grant XP. Curse and Petrification do not have recovery-speed bonuses because PEAK provides no natural recovery timer for them.

## Strength: yes, your backpack can grow

Strength does more than make the Weight affliction less painful. As it levels, it also unlocks more room in your backpack. The extra space is earned by actually hauling weight around, so the character who insists on carrying half the mountain gradually becomes better equipped to keep doing exactly that.

### Backpack unlocks

| Strength level | Extra backpack slots |
|---:|---:|
| 20 | +1 |
| 40 | +2 |
| 70 | +3 |
| 120 | +4 |
| 200 | +5 |

Your **main inventory remains PEAK's normal vanilla size**. The ordinary Backpack starts with 4 item slots, the Fanny Pack starts with 2, and the Jet Pack starts with 1; all three receive the same +1/+2/+3/+4/+5 Strength milestones. The Jet Pack's separate fuel slot is untouched. Rocket Packs have no item storage and are explicitly excluded. Extra stored items still use PEAK's ordinary Weight system.

> **BackpackCapacity should remain disabled alongside Elin's PEAK** because both mods change the same backpack data. MoreSlots is not required, and compatibility with it is not currently guaranteed.

## Your progression is yours

Your skill progression is **player-owned and stored locally**. Joining somebody else's lobby does not mean the host gets to decide that everyone is suddenly level 4, level 50, or level 999.

That also means your character can continue growing across different sessions and groups. The progression system is intended to feel like *your* long-term character history rather than a temporary server setting.

XP is disabled in the **Airport/lobby**, so standing around before a run does not progress skills. Custom-run XP is also disabled by default, although that can be changed through configuration.

## Skill menu

Elin's PEAK adds skill information directly to the pause menu. Main Skills are on the left, with the blue Resiliency section below them.

You can see your skill levels, progression, and current bonuses, with hover explanations for what the skills actually do. The goal is for you to be able to look at a skill and understand *why* it is leveling and what the next levels are doing without needing to keep this README open on another monitor.

---

# Technical details

Everything below is the more exact implementation/scaling information. You do **not** need to know any of this just to play the mod.

## Progression curve

By default, all skills share a maximum level of **999**.

Experience required for the next level is:

```text
XP(next) = round(100 * level^1.21)
```

The curve intentionally keeps progression going for a very long time. Early levels arrive quickly enough to make a new character feel like they are learning, while later levels become increasingly expensive instead of letting every skill casually hit 999.

The maximum level is configurable locally, but it is part of the player's progression/save configuration rather than something selected by a multiplayer host.

## Default XP sources

The current defaults use the following underlying work values:

| Skill | Default XP source |
|---|---|
| Endurance | 10 XP per normalized point of raw stamina requested |
| Strength | 2 XP per raw Weight × movement meter |
| Wall Climbing | 8 XP per intentional climbing meter |
| Rope Climbing | 8 XP per intentional climbing meter |
| Vine Climbing | 8 XP per intentional climbing meter |
| Athletics | 0.22 XP per qualifying walking meter |
| Athletics while sprinting | 1.05 XP per qualifying sprinting meter |
| Agility | 8 XP per successfully executed local jump |
| Vitality | 100 XP per normalized point of raw fall Injury |
| Resiliency skills | 100 XP per normalized point of actual incoming matching affliction |
| Wet Grip | 20 XP per slippery climbing meter, weighted by slipperiness |
| Climbing Tenacity | 40 XP per intentional wall-climbing meter while regular stamina is below 20% |

These values can be changed in the generated BepInEx configuration.

## Effect scaling

Positive bonuses such as stamina capacity, regeneration, movement speed, jump impulse, air control, vine momentum retention, and natural recovery scale linearly from the configured per-level values.

Default positive scaling includes:

| Effect | Default scaling |
|---|---:|
| Endurance maximum stamina | +0.5% per level |
| Endurance stamina regeneration | +0.1% per level |
| Wall climbing speed | +0.3% per level |
| Rope climbing speed | +0.3% per level |
| Vine climbing speed | +0.3% per level |
| Vine momentum retention | +0.05 percentage points per level, capped at 75% |
| Athletics ground movement force | +0.1% per level |
| Athletics sprint movement force | +0.2% per level |
| Agility jump impulse | +0.15% per level |
| Agility air-control responsiveness | +0.025% per level |
| Matching natural condition recovery | +0.15% per level for Poison/Cold/Heat/Drowsy/Spores |

Reduction-style effects use an **anchored reciprocal curve** rather than simply subtracting a flat percentage forever. This applies to systems such as Strength Weight reduction, climbing/sprint/jump efficiency, Vitality, condition resistance, Wet Grip, and Climbing Tenacity.

Most reduction effects use a default rate of `0.003`; Resiliency uses the halved `0.0015` rate. The curve accelerates after its level-500 anchor and continues toward **99.9% reduction at level 999**. This keeps very high progression meaningful without allowing a basic linear formula to cross zero and turn a penalty into nonsense.

## Stamina implementation

Endurance expands **real base stamina capacity** rather than only drawing a fake larger number on the HUD. The stamina bar backing and outline can extend beyond the vanilla range when the option is enabled.

Endurance currently increases capacity and regeneration. General stamina-cost reduction from Endurance itself is intentionally disabled; activity-specific efficiency comes from the skill related to that activity instead, such as Athletics, Agility, or the individual climbing skills.

## Condition handling

Resiliency is applied before PEAK adds the incoming affliction to the local character, and XP is based on the amount that actually gets through. Poison, Cold, Heat, Drowsy, and Spores also speed PEAK paths marked as natural recovery. Cold remains the odd case: the observed runtime path often subtracts Cold locally without PEAK's natural-recovery flag, so that timer portion still needs a dedicated follow-up; Cold exposure XP and incoming reduction are independent of that issue.

## Save data and backups

Progression is stored at:

```text
%LOCALAPPDATA%\LandCrab\PEAK\PEAKUsageSkills\progression.json
```

The mod uses safe/atomic writes and keeps **five rotating backups** in the adjacent `backups` directory. Progress is not written on every individual XP tick; dirty progression is saved periodically instead.

## Configuration

The generated BepInEx config includes controls for:

- Master enable/disable.
- XP gain enable/disable.
- Gameplay effects enable/disable.
- XP in custom runs.
- Maximum skill level.
- Save interval.
- XP rates for individual skill families.
- Per-level effect scaling.
- Stamina-bar extension.
- Diagnostics and debug options.

The defaults are the intended balance for the current release, but most of the underlying progression and effect values are exposed for people who want a faster, slower, harsher, or completely absurd long-term progression curve.

## Installation

Install through a Thunderstore-compatible mod manager such as Gale or r2modman.

The package declares the following dependencies:

- `BepInEx-BepInExPack_PEAK-5.4.75301`
- `PEAKModding-PEAKLib_Core-1.7.2`
- `PEAKModding-PEAKLib_UI-1.6.1`

## Compatibility and current limitations

- Built for **PEAK `2.1.a`**.
- **BackpackCapacity** should remain disabled because Elin's PEAK expands the same backpack data through Strength. **MoreSlots** is not required, and compatibility with it is not currently guaranteed.
- **PeakStatsEx** may continue showing its own cached/rounded Weight number even though gameplay Weight is recalculated immediately.
- Hunger intentionally keeps PEAK's normal **2.5-point display increments**.
- Multiplayer progression is designed around locally owned player saves, but final multiplayer runtime validation is still pending.
- Broader anti-farming and diminishing-return systems are intentionally deferred for now. Basic action validity is already used in several places so obviously unrelated actions do not simply award the wrong XP.

## Development status and handoff documentation

Source and package version 0.4.1 are current. Multiplayer remains unvalidated.

For development or a new Codex conversation, begin with:

- [`AGENTS.md`](AGENTS.md) — current repository rules and accepted product decisions.
- [`docs/HANDOFF.md`](docs/HANDOFF.md) — current state, history, what was tried, and the immediate continuation prompt.
- [`docs/SKILLS_AND_BALANCE.md`](docs/SKILLS_AND_BALANCE.md) — exact implemented XP/effect defaults.
- [`docs/TESTING_AND_COMPATIBILITY.md`](docs/TESTING_AND_COMPATIBILITY.md) — verified versus unverified behavior and the next test checklist.
- [`docs/ROADMAP.md`](docs/ROADMAP.md) — pending work, deferred systems, and future options.

The full documentation index is in [`docs/README.md`](docs/README.md). The sibling `PEAK_UsageSkills_Spec` folder is historical research and is no longer authoritative where it conflicts with these documents or current source.

## Source and issues

The source is public, and issues/technical reports can be submitted here:

https://github.com/kunrian/Elins-PEAK
