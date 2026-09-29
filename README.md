# Sokoban

A modern implementation of the classic Sokoban puzzle game built with **Unity** and **C#**. This project demonstrates solid game development practices including architecture patterns, UI management, and gameplay mechanics.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Game Mechanics](#game-mechanics)
- [Architecture](#architecture)
- [Key Systems](#key-systems)
- [Contributing](#contributing)

## 🎮 Overview

Sokoban is a classic puzzle game where the player must push boxes onto designated storage locations. This implementation provides a polished gaming experience with modern UI interactions, smooth animations, and proper level management.

The project showcases professional game development patterns including singleton management, event-driven architecture, and modular component design.

## ✨ Features

- **Multiple Levels**: Structured level management system with proper progression
- **Smooth Animations**: Polished transitions and visual feedback using DOTween
- **Responsive UI**: Clean, modern interface with fail state handling
- **Grid-Based Level Editor**: Visual gizmo system for level design
- **Mobile-Ready**: Support for mobile ads integration and responsive layouts
- **Debug Utilities**: In-game debug console for development and testing
- **Professional Polish**: Animation tweens, UI state management, and visual feedback

## 🛠️ Tech Stack

| Technology | Purpose |
|-----------|---------|
| **Unity** | Game engine |
| **C#** | Primary scripting language |
| **DOTween** | Smooth animations and transitions |
| **TextMesh Pro** | Advanced UI text rendering |
| **Google Mobile Ads** | Mobile advertisement integration |
| **ShaderLab / HLSL** | Custom graphics and shader effects |

**Language Composition:**
- C# (94%)
- ShaderLab (4.4%)
- Python (0.5%)
- HLSL (0.4%)
- Swift & Objective-C (platform-specific integrations)

## 📁 Project Structure

```
Sokoban/
├── Assets/
│   ├── Scripts/
│   │   ├── Level/
│   │   │   └── LevelRoot.cs           # Level management and grid visualization
│   │   ├── UI/
│   │   │   └── FailPopupController.cs # Fail state and retry UI
│   │   └── [Game Logic Scripts]
│   ├── Resources/                     # Game assets and materials
│   ├── Feel/                          # Third-party VFX and tools
│   ├── GoogleMobileAds/               # Mobile ads integration
│   ├── Plugins/                       # Third-party plugins
│   └── TextMesh Pro/                  # UI text assets
├── Packages/                          # Unity package dependencies
├── ProjectSettings/                   # Unity project configuration
└── README.md                          # This file
```

## 🚀 Getting Started

### Prerequisites

- **Unity 2022.1+** (or compatible version)
- **C# 9.0+**
- Git

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/DioBey7/Sokoban.git
   cd Sokoban
   ```

2. Open the project in Unity:
   - Launch Unity Hub
   - Click "Open Project"
   - Select the Sokoban folder
   - Let Unity import all assets and dependencies

3. Open the main scene:
   - Navigate to `Assets/Scenes/` (if available)
   - Open the main gameplay scene

4. Press **Play** to test the game

## 🎯 Game Mechanics

### Core Gameplay

- **Player Movement**: Navigate the grid using directional input
- **Box Pushing**: Push boxes toward designated storage locations
- **Goal**: Place all boxes on their storage targets to complete the level
- **Level Progression**: Complete levels to progress through increasingly difficult puzzles

### Controls

- **Arrow Keys / WASD**: Move player
- **Mouse Click**: Alternative input (if configured)
- **Retry Button**: Restart current level on failure

## 🏗️ Architecture

### Key Patterns

#### Singleton Pattern
```csharp
public class FailPopupController : MonoBehaviour
{
    public static FailPopupController Instance { get; private set; }
    // Ensures single instance for UI management
}
```

#### Component-Based Design
- `LevelRoot`: Manages level structure and grid visualization
- `FailPopupController`: Handles failure UI state
- Grid-based game objects for modular gameplay elements

#### UI State Management
- Canvas-based UI with proper layer management
- DOTween animations for smooth transitions
- Event-driven retry and restart mechanisms

### Level System

The `LevelRoot` component provides:
- Grid-based level layout validation
- Visual gizmo debugging for level bounds
- Dynamic grid rendering in the editor

```csharp
private void OnDrawGizmos()
{
    // Visualizes game grid for level design
}
```

## 🔧 Key Systems

### UI Management
- **FailPopupController**: Singleton managing failure states and retry functionality
- **Canvas Group Animations**: DOTween integration for smooth fade effects
- **Button Events**: Proper event subscription and cleanup

### Level Management
- **LevelRoot**: Hierarchical level structure with grid visualization
- **Dynamic Grid Calculation**: Automatically computes level bounds
- **Editor Gizmos**: Real-time visual feedback during level design

### Animation System
- **DOTween Integration**: Smooth transitions and tweens
- **Fade Effects**: Canvas group alpha animations
- **Professional Polish**: Frame-rate independent animations

## 📚 Learning Outcomes

This project demonstrates:

- ✅ **OOP Principles**: Proper encapsulation, inheritance, and composition
- ✅ **Design Patterns**: Singleton, Component patterns, and MVC-inspired architecture
- ✅ **Game Development**: Proper scene management, UI handling, and game state
- ✅ **C# Best Practices**: Proper null checking, event management, and lifecycle methods
- ✅ **Editor Tools**: Custom gizmos and inspector integration
- ✅ **Performance**: Efficient grid-based systems and component caching
- ✅ **Mobile Integration**: Cross-platform compatibility considerations

## 🎓 Professional Development Notes

This codebase is structured for:
- **Code Reusability**: Modular components that can be extended
- **Maintainability**: Clear separation of concerns and proper naming conventions
- **Scalability**: Easy to add new levels, UI states, and gameplay features
- **Testing**: Component isolation enables unit testing and debugging
- **Documentation**: Code comments and structured organization

## 🚦 Development Status

- ✅ Core gameplay implemented
- ✅ UI and failure state management
- ✅ Level system and grid visualization
- ✅ Mobile integration ready
- 🔄 Ongoing level design and polish

## 📝 Contributing

This is a personal project for portfolio demonstration. However, suggestions and feedback are welcome!

For questions or suggestions:
1. Open an issue on GitHub
2. Describe the feature or improvement
3. Include relevant context or examples

## 📄 License

This project is open source. See LICENSE file for details (if applicable).

## 🙏 Acknowledgments

- Unity Engine and community
- DOTween for animation framework
- GoogleMobileAds SDK
- More Mountains Tools library
- TextMesh Pro for UI text rendering

---

**Portfolio Note**: This project showcases my ability to develop complete game systems with proper architecture, clean code practices, and professional polish. Feel free to explore the codebase and reach out with any questions!

**Contact**: [Your Contact Info]  
**Portfolio**: [Your Portfolio Link]  
**LinkedIn**: [Your LinkedIn]
