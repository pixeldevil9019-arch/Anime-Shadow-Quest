# Gameplay scripts

`GameBootstrap` creates the 3D valley, stylized placeholder hero, lighting, HUD, and enemy waves. `AnimePlayerController` implements third-person movement, sprint, jump, dodge, and directional melee. `TrainingEnemy` implements pursuit, attacks, health, hit feedback, and defeat rewards. `HeroVitals` manages player health. `ThirdPersonCamera` follows and supports editor mouse orbit. `Assets/Editor/ProjectSetup.cs` creates/registers a build scene automatically for CI.
