# Shadow Flip

A 2D cel-shaded **Light & Shadow Optics Puzzle Game** built in **Unity 6000.3 (URP 2D)**.

Flip between the **Light Realm** and **Shadow Realm** (`F`) to phase walls in and out, route lasers through mirrors, prisms, and color filters, pull levers, press floor buttons, and unlock dungeon gates.

## Controls
- **`WASD` / Arrow Keys** — Move
- **`F`** — Flip between Light Realm and Shadow Realm
- **`E`** — Rotate Mirrors / Prisms & Pull Levers
- **`R`** — Reset current level

## Core Mechanics
- **Dual-Realm World Flipping (`Switch.cs`, `logic.cs`, `WorldPhaseObject.cs`):** Objects, emitters, sensors, and dungeon walls can belong to `LightOnly`, `ShadowOnly`, or `Both`. Safety checks prevent flipping inside phased obstacles.
- **2D Optics System (`Assets/Scripts/Optics/`):**
  - `LightBeamEmitter`: Dual-layer cel-shaded laser rays with multi-bounce reflection/refraction.
  - `OpticalMirror`: 4-state isometric angled mirrors (`45° / 135° / 225° / 315°`) with optional realm rotation locks.
  - `OpticalPrism`: `Splitter` and `DisperseRGB` (`White -> Red / Green / Blue`) modes.
  - `ColorFilter` & `LightSensor`: Wavelength filtering and floor receiver crystal activation.
- **Puzzle Elements & Audio (`Assets/Scripts/Puzzle/`):**
  - `PuzzleLever`, `PressurePlate` (floor buttons & weight plates), `PushableBlock`, `PuzzleDoor`, `LevelExit`, `TutorialManager`, `PuzzleHUD`.
  - `PuzzleAudioManager`: Automatic runtime `AudioListener` & sound bootstrap, crossfading Light/Shadow Realm ambient music (`bgm_light_realm` / `bgm_shadow_realm`), walking footsteps, and puzzle SFX.

## Levels & Setup in Unity
Open the project in **Unity 6000.3** and use the top menu bar:
- **`Shadow Flip -> Create All Levels (Tutorial + Level 1 + Level 2)`**
  1. `Assets/Scenes/OpticsPuzzle_Tutorial.unity` — 4-lesson interactive tutorial
  2. `Assets/Scenes/OpticsPuzzle.unity` — *Chromatic Vault* (Hard Level 1)
  3. `Assets/Scenes/OpticsPuzzle_Level2.unity` — *Eclipse Temple* (Hard Level 2)

## Project Structure
- `Assets/Scripts/Core/` — Player controllers, realm state (`logic.cs`, `Switch.cs`), and core input/sound hooks
- `Assets/Scripts/Optics/` — Lasers, mirrors, prisms, filters, capacitors, black holes, and split-realm zones
- `Assets/Scripts/Puzzle/` — Doors, pressure plates, levers, blocks, NPCs, sanctuaries, pedestals, and teleporters
- `Assets/Scripts/Enemies/` — Umbra Stalker, Warden Boss, Beam Eater Moth, Mimic Mirror, and Mold
- `Assets/Scripts/UI/` — `PuzzleHUD`, `PixelGuiUtility`, `TutorialManager`, and `PixelSandboxEditor`
- `Assets/Scripts/Systems/` — Audio, VFX, camera, borders, and sprite repair systems
- `Assets/Scripts/Legacy/` — Early prototype scripts archived out of the active folders
- `Assets/Editor/` — Modular `OpticsPuzzleBuilder` partial scripts (`Levels0To6`, `Levels7To13`, `Factory`, `Assets`)
