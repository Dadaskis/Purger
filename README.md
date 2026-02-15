# Purger - First Person Shooter Framework for Unity

A comprehensive FPS framework for Unity featuring modular player controllers, weapon systems, NPC AI, animation instancing, and HUD components.

## Overview

Purger is a fully-featured first-person shooter framework built for Unity. It provides a complete foundation for creating FPS games with advanced systems for player movement, weapon handling, NPC behavior, and UI management. The project emphasizes modularity and reusability, making it easy to extend and customize for your specific game needs. This project is a successor to **Escaper v2.0**, it aims to NOT use Resource API and generally provide cleaner experience in both technical and gaming terms. I really wanted it to be a game, but... it is what it is.

## Features

### Core Systems
- **Modular Player Controller** - Advanced first-person controller with support for:
  - Walking, running, crouching, and jumping
  - Realistic head bobbing and camera effects
  - Ladder climbing mechanics
  - Swimming and underwater mechanics with oxygen system
  - Footstep sounds based on surface materials

- **Weapon System** - Flexible firearm system with:
  - Multiple reload types (basic, shotgun-style)
  - Aim-down-sights (ADS) with FOV transitions
  - Spread mechanics and recoil patterns
  - Muzzle flashes and shell ejection
  - Damage system with material-based hit decals
  - Visual recoil and camera punching

- **NPC AI System** - Enemy soldier logic with:
  - State-based behavior (idle, enemy detection)
  - Cover awareness and tactical movement
  - Line-of-sight detection
  - Weapon handling and firing patterns
  - Animation integration

- **Animation System** - Advanced animation management:
  - Runtime animation overrides
  - Animation events for sounds and effects
  - Support for complex animation sequences
  - Integration with weapon and movement states

- **HUD System** - Dynamic heads-up display with:
  - Health, armor, and ammo indicators
  - Crosshair with dynamic spread visualization
  - Oxygen meter for underwater sections
  - Target info display with fade effects
  - Timer system
  - Modular element system for runtime UI management

### Technical Features

- **Asset Bundle Support** - All major systems can load assets from bundles
- **Animation Instancing** - GPU instancing support for animated characters
- **Material System** - Material-based decal placement and sound effects
- **Post-Processing** - Camera effects for sights, recoil, and damage
- **Input Manager** - Configurable input system with save/load support
- **Event System** - Local and global event management
- **Sound Manager** - Spatial audio with material-based footsteps

### Included Tools

- Level design tools (ProBuilder, ProGrids, SabreCSG)
- Shader development (ShaderForge)
- JSON serialization (JsonDotNet)
- Decal system for bullet impacts
- NPC and player spawners with gizmo visualization

## Project Structure

```
Purger/
├── Assets/
│   ├── _Scripts/           # Core game scripts
│   │   ├── Animation/      # Animation system
│   │   ├── Character/      # Character and damage system
│   │   ├── HUD/            # Heads-up display components
│   │   ├── NPC/            # AI behavior
│   │   ├── Player/         # Player controllers and systems
│   │   ├── Weapon/         # Weapon implementations
│   │   └── Tools/          # Editor utilities
│   ├── Animators/          # Animator controllers and overrides
│   ├── AssetBundles/       # Pre-built asset bundles
│   ├── HUD/                # HUD prefabs and assets
│   ├── NPC_Generic/        # Base NPC models and materials
│   ├── Player/             # Player prefabs and assets
│   └── Weapons/            # Weapon models and prefabs
├── ProjectSettings/        # Unity project settings
└── Sources/                # Original source files (Blender, etc.)
```

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/Dadaskis/Purger
   ```

2. **Open in Unity** (tested with Unity 5.6)

3. **Build asset bundles**
   - Navigate to the custom menu or use the included build scripts

4. **Start testing**
   - Open one of the test scenes: `_Test/PlayerTest.unity` or `_Test/NPCArena.unity`
   - Press Play to experience the framework

## Usage Examples

### Creating a Custom Weapon

```csharp
public class CustomWeapon : Firearm {
    public override void PrimaryFire() {
        // Custom firing logic
        base.PrimaryFire();
    }
    
    public override void SecondaryFire() {
        // Custom alt-fire
    }
}
```

### Spawning an NPC

```csharp
NPCSpawn spawner = GetComponent<NPCSpawn>();
spawner.assetBundle = "npc_tester";
spawner.weaponAssetBundle = "firearm_tester_pistol";
spawner.Spawn();
```

### Adding HUD Elements

```csharp
HUDElements.AddElement("HealthArmorAmmoData");
HUDElements.AddElement("Crosshair");
HUDTarget.SetTarget("Mission Objective");
```

## Requirements

- Unity 5.6 or higher
- .NET 4.x or equivalent
- Compatible with Windows, macOS, and Linux builds

## License

MIT license. Happiness to everyone! This project is provided as-is for educational and development purposes.

## Acknowledgments

- Animation instancing system based on GPU instancing techniques
- Level design tools from ProCore and SabreCSG
- Shader development tools from ShaderForge
