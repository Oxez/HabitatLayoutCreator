# Space Habitat Layout Tool (Unity)

An interactive Unity tool to quickly sketch **space habitats**: define shape/volume, paint **functional zones**, place **furniture/equipment**, and get **live metrics & validation** based on mission rules.

---

## ✨ Features

- **Shape & Volume**
  - Cylindrical hull preview, **diameter/length**, **multiple decks**.
  - **Fairing limits** (payload bay diameter/length) checker.

- **Zones**
  - `Sleep`, `Hygiene`, `ECLSS`, `Galley`, `Storage`, `Exercise`, `Medical`, `Maintenance`, `Recreation`, `Corridor`.
  - Paint/erase on a tile grid with instant rebuild of walls & metrics.

- **Auto Walls**
  - Generated along **room↔corridor** and **inside↔outside** borders.
  - Thin shells + **corner posts**; double-sided rendering to avoid z-fighting.

- **Rules & Metrics**
  - ScriptableObjects: **ZoneSpec**, **RuleSet**, **MissionPreset**.
  - Area/volume per zone, **adjacency rules**, **min aisle widths**, path checks.
  - HUD feedback: **Info / Warning / Error** (e.g., “too narrow corridor”, “zone area below required”).

- **Navigation / Access**
  - Corridor **pathfinding** (BFS/A*) for Sleep↔Hygiene / Sleep↔Airlock routes.
  - Minimum traversable width checks.

- **Object Library**
  - Placeable prefabs: **bunk, rack, locker, table, treadmill**.
  - Snap to cell center, rotate by 90°, **continuous stamping** on hold.
  - **Occupancy** blocks cells and affects validation & corridor width.

- **Persistence**
  - Save/Load to JSON; **Export PNG** top-down plan.
  - **Mission presets** (e.g., `Lunar-4-180`, `Transit-4-90`, `Mars-6-500`).

- **Background**
  - Built-in 16:9 starfield UI background (non-blocking for input).

---

## 🧩 Tech

- **Unity 2022.3 LTS (URP)**
- **Input System** (Active Input Handling = **Both**)
- ScriptableObjects + lightweight mesh builders

---

## 🚀 Getting Started

1. Open with **Unity 2022.3 LTS** (or newer LTS).
2. Scene: `Assets/Scenes/Editor.unity`.
3. Press **Play**.

> If you see an error about `UnityEngine.Input`, set  
> **Edit → Project Settings → Player → Active Input Handling = Both**.

---

## 🎮 Controls

- **Camera**: `WASD` move, mouse look, mouse wheel zoom.
- **Zones brush**: pick a zone in UI, **LMB** paint, **Alt** = eraser.
- **Objects**: keys **1–5** select (bunk/rack/locker/table/treadmill);  
  press the same key again to **toggle off**.  
  **LMB** place (hold to **stamp** across cells), **RMB** remove, **R** rotate 90°.
- **Decks**: UI buttons **Deck Prev / Next**.
- **Export**: UI buttons **Save / Load / Export PNG**.

---

## ⚙️ Authoring Mission Rules

- **Create → Habitat → Zone Spec** — define 8–10 base zones (min area per crew, min aisle width, etc.).
- **Create → Habitat → Rule Set** — collect ZoneSpecs + adjacency/separation rules.
- **Create → Habitat → Mission Preset** — e.g., `Lunar-4-180` (crew=4, days=180, fairing limits).
- Assign the preset in **ShapePanelUI** and **DeckManager**.

---

## 🤝 Contributing

PRs welcome: bug fixes, new mission presets, materials/shaders, UI polish.  
Style: Unity C#, null checks, no heavy deps.

---
