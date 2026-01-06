# Step Pet

A virtual pet mobile app where walking earns currency to feed and collect cute baby animals.

## About

Step Pet is a pastel-colored virtual pet game that connects to your phone's health data. Walk in real life to earn food for your pets, unlock new animals through step challenges, and customize your pet sanctuary.

**Core Loop:**
- Walk to earn food (1 step = 1 food)
- Meet your pets' daily food quota
- Overflow food goes to your wallet for spending
- Unlock new pets by completing step challenges
- Customize with hats, decorations, and backdrops

## Tech Stack

- **Engine:** Unity (LTS)
- **Language:** C#
- **Step Tracking:**
  - Android: Health Connect API
  - iOS: Apple HealthKit
- **Backend:** Firebase (Auth, Firestore, Cloud Functions)
- **Orientation:** Portrait only

## Getting Started

### Prerequisites
- Unity 2022.3 LTS or later
- Android SDK (for Android builds)
- Xcode (for iOS builds)
- Firebase project configured

### Setup
1. Clone this repository
2. Open the project in Unity
3. Configure Firebase:
   - Add your `google-services.json` (Android) to `Assets/`
   - Add your `GoogleService-Info.plist` (iOS) to `Assets/`
4. Build and run on your target platform

## Documentation

See [DESIGN.md](DESIGN.md) for the complete design specification including:
- Account system
- Economy and daily quota mechanics
- Pet unlock challenge system
- Screen layouts and navigation
- Firebase data model
- MVP feature phases

## Project Structure

```
Assets/
├── Scripts/          # C# game logic
├── Prefabs/          # Reusable game objects
├── Scenes/           # Unity scenes
├── UI/               # UI assets and prefabs
└── Art/              # Sprites and animations (placeholders)
```

## License

All rights reserved.
