# THE PLAN.

## Phase 1: Core Foundations

-Establish basic world structure.
- Implement core movement and interactions.
- Set up modular data structures (items, world, entities).

### 1. Inventory & Item System

- Establish a data-driven item database (JSON).
- Create a modular Inventory system (drag-and-drop, stacking, UI updates).
- Implement basic item types (blocks, tools, consumables).
- Introduce item dropping & pick-up mechanics.

### 2. World System (Tilemap & Chunks)

- Choose a tilemap structure (Unity Tilemap or custom chunk system).
- Implement chunk loading/unloading (keep memory usage low).
- Set up basic terrain generation (flat land or basic procedural noise).
- Optimize world data storage (JSON, Binary, or SQLite).
- Implement Block interactions (placing, destroying).

### 3. Player Controller & Physics

- Implement basic player movement (jumping, walking, falling).
- Add basic collision detection with tiles
- Implement basic gravity, friction, and physics interactions.
- Implement ‘Use’ (LMB)
- Start designing modular character attributes (health, energy, etc.).
- Equipment Slots

### 4. Player Controller & Physics

- Start adding basic Items to be used
- Pickaxe
- Blocks
- MeleeWeapon
- RangedWeapon

### 5. Multiplayer

- Choose a networking solution (Mirror, FishNet, or Netcode for GameObjects?)
- Sync player movement, animations, inventory and world changes (Prediction?)
- Stat changes, health, and item usage reflect correctly across clients.
- Optimize data packets (don't send every block update?!).
- Player Identity & Ownership
  - Each player gets a unique player ID or network identity.
  - Each client only controls their own character and inventory.
  - Confirm input is local-only, not influencing others.

### 6. AI & NPCs

- Implement basic enemy AI (pathfinding, attacking).
- Introduce friendly NPCs (villagers, merchants).
- Use a Finite State Machine (FSM) or Behavior Tree for AI.

### 7. Advanced Terrain Features

- UImplement background/foreground layers for depth.
- UIntroduce lighting system (2D lights or a tile-based lighting engine).
- UOptimize chunk updates (avoid unnecessary redraws).

### 8. Crafting & Resource System

- UAdd basic crafting system (using JSON recipes or database).
- UImplement resource gathering (mining, chopping trees, collecting items).
- UAllow items to have durability and stats.

### 9. Biomes & Procedural Generation

- UImplement biomes (desert, forest, caves).
- UAdd randomized structures (houses, dungeons).
- UOptimize generation to avoid performance drops.

### 10. UI & UX Improvements

- UImplement a modular UI system (settings, inventory, tooltips).
- UAdd hotbar functionality for quick item switching.
- UImprove accessibility and quality-of-life features.
