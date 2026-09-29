# Sokoban Mobile

A mobile-first puzzle game inspired by the classic Sokoban concept, developed in Unity using C#. The project focuses on smooth touch controls, polished UI, responsive gameplay, and a clean architecture suitable for mobile deployment.

This repository demonstrates a complete gameplay loop, level progression, failure/retry flow, and mobile game implementation practices that are relevant for game development portfolios and internship/job applications.

## Overview

Sokoban is a logic-based puzzle game where the player pushes crates onto target locations. The goal is to solve each level efficiently while managing movement, obstacles, and retries.

This version is designed with mobile gameplay in mind, including simplified touch-friendly controls, responsive interfaces, and a streamlined game flow suitable for Android and other mobile platforms.

## Why This Project Matters

This project showcases:

- Unity game development with C#
- Game systems design for mobile environments
- UI/UX implementation for gameplay and failure states
- Level-based mechanics and progression logic
- Responsive project structure and code organization
- Integration of ads, tools, and Unity packages for production-ready mobile projects

## Features

- Classic Sokoban gameplay with push-and-solve mechanics
- Mobile-oriented controls and user interface
- Level-based progression and retry flow
- Failure popup system with smooth animations
- Responsive UI states and transition effects
- Grid-based level design and visualization
- Unity project structure prepared for platform extension
- Mobile advertising integration support
- Debugging tools and developer utilities

## Tech Stack

- Unity Engine
- C#
- DOTween for animation and UI transitions
- TextMesh Pro for UI text rendering
- Google Mobile Ads for monetization support
- ShaderLab / HLSL for visual polish

### Language Composition

- C#: 94%
- ShaderLab: 4.4%
- Python: 0.5%
- HLSL: 0.4%
- Swift / Objective-C / Other: minimal platform-specific support

## Mobile Game Focus

This project is built as a mobile game, which means the design priorities include:

- Touch-friendly input patterns
- Lightweight gameplay loops
- Clean, readable interfaces on smaller screens
- Reduced visual clutter and clear UI feedback
- Control responsiveness and smooth transitions
- Optimized structure for future mobile builds and deployment

## Project Structure

```text
Sokoban/
├── Assets/
│   ├── Scripts/
│   │   ├── Level/
│   │   │   └── LevelRoot.cs
│   │   ├── UI/
│   │   │   └── FailPopupController.cs
│   │   └── Game logic scripts
│   ├── Plugins/
│   ├── Feel/
│   ├── GoogleMobileAds/
│   ├── TextMesh Pro/
│   └── Resources/
├── Packages/
├── ProjectSettings/
├── README.md
├── .gitignore
├── .vsconfig
└── LICENSE (if added later)
```

## Gameplay Mechanics

The core loop is based on the classic Sokoban formula:

- Move the player across the grid
- Push boxes into open spaces
- Place each box on a destination tile
- Complete the level when all objective tiles are filled
- Retry after mistakes with instant feedback

## Key Systems

### Level System
The level structure is built around a grid-based layout, with logic for boundaries and visual debugging. The code supports object organization and layout refinement during development.

### UI System
A dedicated fail popup system provides a polished retry flow. It handles visibility, animation, and restart logic for a better player experience.

### Mobile Integration
The project includes mobile-oriented tooling and integrations such as advertising support and Unity packages that fit a real-world mobile game workflow.

## Getting Started

### Prerequisites

- Unity 2022.1 or newer
- C# development knowledge
- Git

### Installation

1. Clone the repository

```bash
git clone https://github.com/DioBey7/Sokoban.git
cd Sokoban
```

2. Open the project in Unity Hub

3. Open the project folder from the repository root

4. Let Unity import dependencies and packages

5. Press Play to run the game in the editor

## Controls

- Movement: swiping (Swipe motion with mouse if it is tested in PC)
- Retry: fail popup button / restart flow
- Mobile-ready build: designed to be adapted to touch input and touch-friendly UI

## Architecture Highlights

This project shows practical game design principles, including:

- Component-based object structure
- Object lifecycle handling in Unity
- Separation between gameplay logic and UI logic
- Simple event-driven retry flow
- Reusable systems for future feature expansion

## Skills Demonstrated

This project reflects skills in:

- Unity and C# game development
- Mobile game design and user flow
- Game UI implementation
- Problem-solving for logic puzzle gameplay
- Project organization and code structure
- Integration of game tools and platform services

## Current Status

- Core Sokoban mechanics implemented
- UI and gameplay flow in place
- Mobile-friendly structure established
- Project is suitable for further expansion with more levels, polish, and monetization features

## Future Improvements

- Add more levels and difficulty progression
- Improve touch controls for mobile gameplay
- Add sound effects and music
- Add level selection and save system
- Add win screen and level completion flow
- Optimize for Android packaging and deployment

## License

This project is currently shared as an open portfolio project. If a license is added later, it will be documented here.

## Acknowledgments

- Unity Technologies
- DOTween
- TextMesh Pro
- Google Mobile Ads
- Community Unity tools and packages used in the project

## Portfolio Note

This project is a practical example of mobile game development in Unity, combining gameplay systems, polished UI, and production-oriented project structure. It reflects interest in game programming, logic design, and creating playable experiences for mobile platforms.
