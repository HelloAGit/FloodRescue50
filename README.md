# Flood Rescue 50

A family-friendly, single-player urban flood rescue prototype set in the
UK-inspired Riverside district. The vertical slice targets five rescue locations,
a third-person rescuer, time-sensitive scoring, mission HUD, results and retry.

## Milestone Status

- Milestone 1: Core gameplay architecture implemented; Unity verification pending.
- Milestone 2: Unity playable Riverside integration in progress.
- Rodin import, prefab approval, Unity tests and an actual playtest are not complete.

## Prerequisites

Unity Hub and a supported Unity Editor are required. No Editor version is pinned:
this repository did not yet contain Packages or ProjectSettings.
Install compatible Input System, TextMeshPro/Unity UI and Unity Test Framework
packages through the chosen Editor's Package Manager. Do not guess versions.

Follow [the Unity setup guide](docs/MILESTONE_2_UNITY_SETUP.md) to create real
project metadata, import packages, and let Unity generate .meta GUIDs.

## Run Riverside

After package setup and successful script compilation, import TMP Essential
Resources and select **Flood Rescue 50 > Create Prototype Scenes** in the Editor.
The builder prepares FloodTown, MainMenu, Boot, AssetReview and five POI data assets,
using Unity's asset APIs. It refuses to overwrite existing scenes.

Open Assets/Game/Scenes/FloodTown.unity and enter Play Mode.
Register Boot, MainMenu and FloodTown in the build/profile scene list to use
Start Rescue, Retry and Main Menu. Place Boot first for a full startup flow.

If approved Rodin prefabs are absent, labelled primitive blockouts are used.
These are provisional visuals, not imported or approved Rodin assets.
The supplied AssetHyper JPGs are reference renders; no 3D exports were found.

## Controls

| Input | Action |
| --- | --- |
| WASD | Move |
| Mouse | Third-person camera |
| Left Shift | Sprint |
| Space | Jump |
| E | Start rescue |

Stay within the configured interaction range until rescue completes.
Leaving range cancels the operation. Completed points score only once.
The mission ends when all five points are complete or 15 minutes expire.

## Gameplay

RescuePointData defines identity, victim counts, scoring and interaction values.
RescuePointController emits lifecycle events; it does not control UI or storage.
MissionManager validates configuration, awards score through ScoringService and
publishes progress/results. MissionTimer supports explicit stepping for tests.
World markers and HUD subscribe to gameplay events.

The score remains baseScore + maxTimeBonus * (1 - clamped elapsed/duration):
with defaults, completion at 0 / 450 / 900 seconds scores 400 / 250 / 100.
ID-based awards prevent duplicate scoring; the original anonymous award method
is retained for existing callers.

## Tests

Use Unity Test Runner to run both Edit Mode and Play Mode suites.
Original scoring tests are retained. Added coverage includes score boundaries,
deduplication, timer events/clamping, rescue transitions/cancellation,
configuration validation and mission success/expiry.
No Unity Test Runner pass is claimed until an Editor run is recorded.

## Source Layout

```text
Assets/Game/
  Art/Generated/Rodin/     Original models/textures; immutable source
  Data/RescuePoints/       Unity-created ScriptableObject assets
  Materials/Prototype/    Builder-created blockout materials
  Prefabs/                Reviewed Environment, Props and Vehicles
  Scenes/                 Unity-created scenes
  Scripts/
    Core/
    Player/
    Rescue/
    Scoring/
    UI/
    Editor/               Scene setup command, excluded from player builds
    Tests/                Edit Mode tests
      PlayMode/           Coroutine and mission tests
docs/
  MILESTONE_2_UNITY_SETUP.md
```

Only scripts and documentation exist before the Editor setup command is run.
Do not recreate ssets/. Keep game source under Assets/Game.
Commit Unity-generated metadata with its assets, but never Library, Temp,
Logs, obj, Build, Builds, UserSettings or secrets.

## Scope

No multiplayer, Tencent Cloud, runtime OpenAI APIs, Rodin MCP calls, asset
regeneration, vehicle controls or dynamic flood simulation are included.
The boat is visual-only. Keep gameplay roots separate from imported visual meshes.
Future milestones are not implemented in this pass.
