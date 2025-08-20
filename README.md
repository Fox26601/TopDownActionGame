# IsometricActionGame

An isometric action game built on MonoGame with a robust resolution management architecture and comprehensive quest system. Features modular design, clean code organization, and optimized performance.

## Project Architecture

### Core Systems

#### Core Architecture
- **Core/Data/GameConstants** - Centralized game constants and configuration
- **Core/Entities** - Player, enemies, and game entities
- **Core/Graphics** - Animation system and rendering components
- **Core/Systems** - Game systems (combat, health, projectiles)
- **Core/Managers** - Game managers (spawning, levels, restarts)
- **Core/Helpers** - Utility classes for calculations and debugging

#### Resolution Management System
- **IResolutionManager** - Interface for resolution management
- **ResolutionManager** - Singleton implementation with error handling and rollback
- **ICameraManager** - Interface for camera management
- **CameraManager** - Viewport-aware camera with smooth following

#### Quest System
- **Quest** - Abstract base class for all quests
- **ExtendedKillBatsQuest** - Flexible quest with multiple objectives
- **QuestManager** - Centralized quest management with save/load support
- **QuestObjective** - Individual quest objectives with optional requirements

#### Combat System
- **Attack** - Data class for attack information
- **AttackSystem** - Centralized attack processing with cone-based targeting
- **IAttackable** - Interface for entities that can attack and be attacked
- **HealthSystem** - Modular health management

#### Save System
- **ISaveable** - Interface for saveable objects
- **SaveManager** - Centralized save/load operations
- **GameSaveLoadManager** - Game-specific save coordination
- **SaveData** - Flexible data storage system

#### Settings System
- **Settings/GameSettings** - Game configuration management
- **Settings/InputSettings** - Input configuration management
- **JSON Configuration** - Persistent settings storage

#### UI System
- **UIManager** - Centralized UI management
- **UIScalingManager** - Automatic UI scaling across resolutions
- **IUIElement** - Interface for UI elements
- **DialogueManager** - Advanced dialogue system with positioning strategies

### Design Patterns

#### Strategy Pattern
- **IDialoguePositioningStrategy** - Different positioning algorithms
- **IDialogueInputHandler** - Different input handling methods

#### State Pattern
- **EnemyState** - Enemy behavior states
- **InventoryDragState** - Drag and drop state management

#### Observer Pattern
- **GameEventSystem** - Event-driven architecture
- **Quest events** - Quest state change notifications

#### Factory Pattern
- **ItemFactory** - Item creation
- **Quest creation** - Dynamic quest instantiation

#### Singleton Pattern
- **ResolutionManager** - Centralized graphics management
- **GameEventSystem** - Global event coordination

## Key Features

### Code Organization & Quality
- **Modular Architecture**: Clean separation of concerns with dedicated folders
- **Centralized Constants**: All game values in `GameConstants.cs` for easy maintenance
- **Clean Code**: No hardcoded values, proper naming conventions
- **Zero Warnings**: Clean compilation without warnings or unused variables
- **Event-Driven Design**: Loose coupling through event system

### Quest System
- **Dynamic Objectives**: Quests can have multiple objectives with optional requirements
- **Progressive Difficulty**: Quest complexity increases based on player level
- **Save/Load Support**: Complete quest state persistence
- **Event-Driven**: Automatic quest updates based on game events

### Combat System
- **Cone-Based Attacks**: Realistic attack targeting with configurable angles
- **Isometric Distance**: Accurate distance calculations for isometric projection
- **Hitbox System**: Precise collision detection with entity hitboxes
- **Animation Integration**: Seamless combat animations

### Resolution Management
- **Dynamic Scaling**: All UI elements scale proportionally
- **Error Recovery**: Automatic rollback on resolution change failures
- **Event Propagation**: All components update automatically
- **Safe Fallbacks**: Graceful degradation on errors

### Save System
- **Modular Design**: Each system implements ISaveable independently
- **Flexible Data**: Generic save data with type safety
- **Coordinated Saving**: Centralized save coordination
- **Incremental Updates**: Efficient save data updates

## Performance Optimizations

### Memory Management
- **Object Pooling**: Reusable object pools for frequently created objects
- **Texture Caching**: Shared textures to reduce memory usage
- **Event-Driven Updates**: Only process when necessary

### Rendering
- **Layer Depth Sorting**: Proper isometric depth rendering
- **Viewport Culling**: Only render visible entities
- **Batch Rendering**: Efficient sprite batch operations

### Data Structures
- **List<T>** for ordered data (quests, entities)
- **Dictionary<K,V>** for lookups (save data, settings)
- **HashSet<T>** for uniqueness (active quests)

## Code Quality

### SOLID Principles
- **Single Responsibility**: Each class has one clear purpose
- **Open/Closed**: Easy to extend with new quests, items, etc.
- **Liskov Substitution**: Interface-based design
- **Interface Segregation**: Focused, specific interfaces
- **Dependency Inversion**: Depends on abstractions

### Clean Code
- **Descriptive Names**: Clear method and variable names
- **Small Methods**: Each method does one thing well
- **Consistent Formatting**: Follows C# conventions
- **Comprehensive Comments**: Only for complex logic

### Error Handling
- **Validation**: Null checks and state validation
- **Graceful Degradation**: System continues working with errors
- **Debug Information**: Comprehensive logging
- **Safe Fallbacks**: Default behaviors on failures

## Project Structure

```
IsometricActionGame/
├── Core/                         # Core game systems
│   ├── Data/
│   │   └── GameConstants.cs      # Centralized game constants
│   ├── Entities/                 # Game entities
│   │   ├── Player.cs
│   │   ├── Bat.cs
│   │   └── Pebble.cs
│   ├── Graphics/                 # Animation and rendering
│   │   └── AnimatedSprite.cs
│   ├── Systems/                  # Game systems
│   │   ├── AttackSystem.cs
│   │   ├── HealthSystem.cs
│   │   └── ProjectileManager.cs
│   ├── Managers/                 # Game managers
│   │   ├── BatSpawnManager.cs
│   │   ├── GameRestartManager.cs
│   │   └── LevelSystem.cs
│   ├── Helpers/                  # Utility classes
│   │   ├── GridHelper.cs
│   │   └── IsometricHelper.cs
│   ├── Interfaces/               # Core interfaces
│   │   ├── IEntity.cs
│   │   ├── IAttackable.cs
│   │   └── IDrawable.cs
│   └── Factories/                # Object factories
│       └── EntityFactory.cs
├── Settings/                     # Configuration management
│   ├── GameSettings.cs
│   ├── InputSettings.cs
│   ├── gamesettings.json
│   └── input_settings.json
├── Graphics/                     # Resolution and camera
│   ├── IResolutionManager.cs
│   ├── ResolutionManager.cs
│   ├── ICameraManager.cs
│   └── CameraManager.cs
├── UI/                          # User interface
│   ├── UIManager.cs
│   ├── UIScalingManager.cs
│   ├── DialogueManager.cs
│   └── IUIElement.cs
├── Combat/                      # Combat system
│   ├── Attack.cs
│   ├── EnemyState.cs
│   └── Fireball.cs
├── World/                       # Game world
│   ├── Map/
│   │   └── GameMap.cs
│   ├── Tiles/
│   │   ├── Tile.cs
│   │   └── TileType.cs
│   └── Environment/
│       ├── Chest.cs
│       └── Door.cs
├── NPCs/                        # Non-player characters
│   ├── NPC.cs
│   ├── Mayor.cs
│   └── Vendor.cs
├── Items/                       # Game items
│   ├── Item.cs
│   ├── ItemFactory.cs
│   ├── HealthPotion.cs
│   ├── Gold.cs
│   └── KeyItem.cs
├── Events/                      # Event system
│   ├── GameEventSystem.cs
│   ├── UnifiedEventSystem.cs
│   └── EventHandlers/
├── SaveSystem/                  # Save/load system
│   ├── ISaveable.cs
│   ├── SaveManager.cs
│   ├── GameSaveLoadManager.cs
│   └── SaveData.cs
├── Quests/                      # Quest system
│   ├── Quest.cs
│   ├── ExtendedKillBatsQuest.cs
│   └── QuestManager.cs
├── Inventory/                   # Inventory system
│   ├── Inventory.cs
│   ├── InventoryRenderer.cs
│   └── InventoryDragManager.cs
├── Dialogue/                    # Dialogue system
│   ├── DialogueManager.cs
│   ├── IDialogueInputHandler.cs
│   └── IDialoguePositioningStrategy.cs
├── Input/                       # Input handling
│   ├── InputHandler.cs
│   └── Handlers/
└── Game1.cs                     # Main game class
```

## Build and Run

```bash
dotnet build
dotnet run
```

Project builds on .NET 8.0 with MonoGame Framework.
