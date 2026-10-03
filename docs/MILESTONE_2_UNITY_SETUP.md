# Milestone 2: Unity Riverside Setup

## Current State

The repository started as 12 C# scripts without `Packages`, `ProjectSettings`, scenes,
prefabs, model exports, or `.meta` files. Unity Hub is installed on the inspected
machine, but no Unity Editor installation was found. No Unity version is pinned.
The runtime, tests and Editor builder are prepared; compilation against Unity,
asset import, scene creation and actual playtesting remain unverified.

`C:/Users/kumar/OneDrive/Pictures/AssetHyper` contains eight JPEG reference renders,
not FBX/OBJ/GLB/GLTF models. The reference set includes houses, a corner shop,
barrier, boat, sandbags and a town overview. It does not supply the nine expected
3D models, textures, materials or mesh topology. Never regenerate assets as part
of this milestone and never connect to Rodin MCP.

## Create Unity Project Metadata

1. In Unity Hub, install a supported Unity Editor with your intended desktop
   build support. Record the actual version once chosen; no version was guessed
   by this pass.
2. Create an empty **3D Built-in** project in a temporary sibling directory.
   URP is also supported by the builder, but choose one pipeline consistently.
3. Close the Editor. Copy the newly generated `Packages/` and `ProjectSettings/`
   directories into the FloodRescue50 repository. Do not replace `Assets/`,
   `.git`, README, or the existing game scripts. Do not copy `Library/` or caches.
4. Before importing game scripts, open the temporary project and use Package
   Manager to install the **Input System**, **TextMeshPro** and **Test Framework**
   versions compatible with that Editor. In Editors where TMP is supplied by
   Unity UI, enable that package instead of installing conflicting TMP versions.
   Unity UI is also required for the builder's Canvas and Button components.
   Copy the resulting real manifest and lockfile into the repository after
   closing the Editor. Do not write speculative package versions by hand.
5. In Hub, add the FloodRescue50 folder as a project and open it. Let Unity
   generate all `.meta` files and real GUID references. Commit generated metas
   alongside the corresponding source assets once the Editor import succeeds.
6. Project Settings > Player > Active Input Handling: **Input System Package
   (New)**, or **Both** if another legitimate project dependency needs legacy
   input. Accept the requested Editor restart.
7. Window > TextMeshPro > Import TMP Essential Resources. Confirm a default TMP
   font exists. For markers, add a font/fallback with the U+2713 checkmark glyph.
8. Set Asset Serialization to **Force Text** and Version Control to **Visible
   Meta Files** where those settings are exposed. Keep caches ignored.
9. Wait for compilation. The runtime assembly references `Unity.InputSystem`
   and `Unity.TextMeshPro`, plus `Unity.ugui` for TMP's UI base classes. The
   Editor assembly references the same packages. Resolve these through Package
   Manager, not fabricated GUIDs.

## Generate Prototype Scenes

1. Save any current work and exit Play Mode.
2. Select **Flood Rescue 50 > Create Prototype Scenes**. This uses Unity's
   `AssetDatabase`, serialization and scene APIs, so Unity owns all GUIDs.
3. The command creates Boot, MainMenu, FloodTown and AssetReview, five data
   assets, prototype materials and the intended source/prefab folders. It aborts
   if any of those four scenes already exists and preserves existing data and
   materials. It never generates or edits a Rodin model.
4. Approved prefabs at the exact paths below are instantiated if available.
   Otherwise labelled primitive blockouts are placed and warnings are logged.
   These are prototype scene visuals, **not approved Rodin prefabs**.
5. Boot, MainMenu and FloodTown are appended to existing build scenes. In
   File > Build Settings, or Build Profiles > Scene List, enable these scenes
   and place **Boot first**, followed by MainMenu and FloodTown. AssetReview
   need not be in the playable build. An active profile's custom scene list may
   need the same entries even after the builder updates EditorBuildSettings.
6. Open `Assets/Game/Scenes/FloodTown.unity` and enter Play Mode. For subsequent
   edits, use the instructions below; do not delete working scenes to rerun the
   builder. Replace labelled blockouts with reviewed prefabs manually after
   importing the models.

## Raw Model Import and Approval

Keep original model files and companion textures/materials unchanged under:

```text
Assets/Game/Art/Generated/Rodin/Buildings/
Assets/Game/Art/Generated/Rodin/Infrastructure/
Assets/Game/Art/Generated/Rodin/Props/
Assets/Game/Art/Generated/Rodin/Vehicles/
```

Locate the actual downloaded model exports first. FBX or OBJ may be imported by
Unity directly; GLB/GLTF requires a compatible importer or an offline conversion
to a supported format. Do not add an unused importer or treat JPGs as models.
Keep material/texture directory relationships intact. Do not rename existing
Unity assets outside the Editor once `.meta` files exist.

For each model:

1. Inspect importer warnings, mesh normals, face orientation and materials.
   Record vertices/triangles from the mesh Inspector; polycount is unknown until
   an actual model is available.
2. Confirm albedo, normal, metallic and roughness/smoothness texture assignments
   in the chosen render pipeline. Work on separate gameplay material copies if
   conversion is needed. Never overwrite the raw Rodin originals.
3. Create an empty prefab root at ground level, a `Visual` child, and place the
   imported Rodin model below it. Gameplay scripts stay off the raw mesh.
4. Correct scale, rotation and offsets on `Visual` or its model instance. Use
   one Unity unit per metre, Y up, and a consistent +Z forward. Compare a door
   with the 1.8 m rescuer; use roughly 2 m door height as a scale check. Put the
   root at a practical ground-centred placement pivot. Do not scale the trigger
   or gameplay object to compensate for a model's units.
5. Add simple colliders to the prefab root or dedicated collider children.
   Buildings use fitted BoxCollider combinations. Small props use boxes,
   capsules or spheres; the buoy may use simple spheres/boxes without requiring
   an exact ring collision shape. Avoid automatic complex MeshColliders.
6. Remove embedded readable brand names/logos from approved material copies or
   keep the model unapproved. Boat.jpg contains lettering and symbols, and
   town1.jpg contains shop lettering. Those renders are reference-only.
7. Save reviewed prefabs at these paths (append `.prefab`):

| Model | Raw Category | Prefab Directory | Prefab Name | Collider |
| --- | --- | --- | --- | --- |
| FR50_Riverside_TerraceHouse_A | Buildings | Environment | FR50_PF_TerraceHouse_A | Box combinations |
| FR50_Riverside_Clinic_A | Buildings | Environment | FR50_PF_Clinic_A | Box combinations |
| FR50_Riverside_CornerShop_A | Buildings | Environment | FR50_PF_CornerShop_A | Box combinations |
| FR50_Riverside_BusShelter_A | Infrastructure | Environment | FR50_PF_BusShelter_A | Boxes for posts/roof |
| FR50_Riverside_Substation_A | Infrastructure | Environment | FR50_PF_Substation_A | Box combinations |
| FR50_Prop_RescueBuoy_A | Props | Props | FR50_PF_RescueBuoy_A | Simple spheres/boxes |
| FR50_Prop_SandbagStack_A | Props | Props | FR50_PF_SandbagStack_A | Box |
| FR50_Prop_RoadBarrier_A | Props | Props | FR50_PF_RoadBarrier_A | Box |
| FR50_Vehicle_RescueBoat_A | Vehicles | Vehicles | FR50_PF_RescueBoat_A | Visual only; no physics |

Prefab directories are beneath `Assets/Game/Prefabs/`. Do not mark any prefab
approved just because its filename matches. Approval requires the visual/import
checks above. The boat has no controller, Rigidbody, buoyancy or scoring logic.

## AssetReview

The builder places the nine prefab slots on a 3 x 3 grid. If models arrive later,
replace each blockout with its corresponding approved prefab at the same root
position. Inspect the nine together in Scene and Game views for scale, pivot,
orientation, normals, materials, textures, moderate detail and visual consistency.
Reference JPGs are not placed into this scene as 3D objects. The scene has no
mission or scoring components. Record asset approval findings before using
the prefabs in FloodTown.

## Player and Camera Wiring

- `Player` tag: Player. Transform scale `(1,1,1)`; spawn `(-12,0.2,-34)`.
- CharacterController: height 1.8, radius 0.35, centre `(0,0.9,0)`.
- RescuerMovementController: walk 4, sprint 7, jump 1.2; Camera Transform is
  MainCamera's transform. Gravity remains -20.
- PlayerRescueInteractor: search radius 4, rescue-point layer mask includes
  the POI trigger layer. It does not award score.
- Visual capsule is a placeholder, with its duplicate collider removed. No
  character animations or victim models are generated.
- MainCamera: tag MainCamera, Camera, one AudioListener, and
  ThirdPersonCameraController. Set Target to Player using its Inspector target
  field. Distance 5, offset `(0,1.6,0)`, sensitivity 0.15, pitch -25 to 65.
- Test following and clipping at building corners. The existing camera has
  no collision avoidance; adjust placement/distance if necessary before approval.

## Five POI Definitions and Placement

Create assets with Create > Flood Rescue 50 > Rescue Point Data if not using the
builder. Store them under `Assets/Game/Data/RescuePoints/`.
All five: district Riverside, base score 100, discovery radius 12 m,
interaction range 3 m. Add a short description for the rescue task.

| ID | Name | Type | Victims | Critical | Duration | Position (X,Y,Z) |
| --- | --- | --- | --- | --- | --- | --- |
| POI_001 | Flooded House | Residential | 3 | 0 | 4 s | (-24,0,34) |
| POI_002 | Riverside Medical Clinic | Medical | 5 | 1 | 5 s | (-8,0,15) |
| POI_003 | Corner Shop | Commercial | 2 | 0 | 3 s | (20,0,1) |
| POI_004 | Bus Stop | Transport | 4 | 0 | 4 s | (23,0,-20) |
| POI_005 | Electrical Substation | Infrastructure | 0 | 0 | 5 s | (27,0,35) |

Each POI is a separate root under RescuePoints, scale `(1,1,1)`, with
RescuePointController, the corresponding data reference, SphereCollider radius
12 with Is Trigger enabled, and a kinematic Rigidbody with Use Gravity disabled.
Place the matching visual independently under Riverside at a 6 m +Z offset,
or under a `Visual` child of the gameplay root if more convenient. Keep the
interaction origin outside walls and reachable within 3 m. Do not put the
RescuePointController on the imported mesh. Keep solid colliders out of the
interaction access area.

WorldMarker: child world-space Canvas at `(0,3,0)` with scale 0.01, TMP text and
RescuePointMarker referencing the controller and text. Keep the marker component
enabled so it can receive state events; Unknown hides only the text. States are
hidden, `?`, `!`, and U+2713. Confirm the checkmark font fallback works.

## Mission, HUD and Results References

Under Gameplay, create objects with MissionTimer, ScoringService and
MissionManager. MissionTimer duration is 900. MissionManager references that
timer, the scoring component and the five distinct RescuePointControllers.
MissionManager starts the timer after resetting score. Do not add a second
auto-start timer. Invalid duplicate IDs are warned, excluded from the mission
total and disabled. Empty IDs use runtime controller identity as a fallback;
replace them with the intended stable IDs before approval.

Use a screen-space overlay Canvas, CanvasScaler reference resolution 1440x900,
Scale With Screen Size and GraphicRaycaster. EventSystem must use
InputSystemUIInputModule, with its default actions assigned. Remove any legacy
StandaloneInputModule if Active Input Handling is New only.

MissionHudController references:

| Field | Reference |
| --- | --- |
| Mission Timer | Gameplay/MissionTimer |
| Scoring Service | Gameplay/ScoringService |
| Mission Manager | Gameplay/MissionManager |
| Player Interactor | Player/PlayerRescueInteractor |
| Timer Text | TMP label for TIME 15:00 |
| Score Text | TMP label for SCORE 0 |
| Progress Text | TMP label for RESCUE POINTS 0/5 |
| Objective Text | TMP objective label |
| Interaction Text | TMP prompt label |

MissionResultsController must remain on the always-active UI root, not inside
ResultsPanel (which starts hidden). References: MissionManager, ResultsPanel,
title/score/progress/time TMP labels, Player's movement and interactor, and
MainCamera's ThirdPersonCameraController. Assign persistent Button OnClick
handlers to `RetryMission()` and `LoadMainMenu()`. Finishing disables those
player components and unlocks the cursor. Retry reloads the registered scene;
Main Menu loads the registered MainMenu. Missing scene registration warns
instead of attempting an invalid scene load.

MainMenu: active Canvas with MainMenuController; Start Rescue button invokes
StartRescue and Quit invokes Quit. Boot: one BootController. Quit only exits a
built player; stopping Editor Play Mode remains manual.

## Tests and Acceptance

1. Window > General > Test Runner (or the Editor's Test Runner entry).
2. Run Edit Mode tests: the five existing score tests, score boundaries/events/
   reset/deduplication, timer stepping/events/clamps, and data validation.
3. Run Play Mode tests: rescue lifecycle, cancellation, missing data, mission
   success, timer expiry, zero-point mission and duplicate configuration.
4. Record results and verify no unexpected Console errors. Test assemblies are
   marked TestAssemblies and are separate from the runtime assembly. The nested
   PlayMode assembly overrides the Editor-only test assembly for coroutine tests.
5. Open FloodTown and verify WASD, mouse, Shift, Space and E. Confirm points
   cannot be interacted with from spawn and that different routes are possible.
6. Verify entering discovery range reveals the marker; E shows Active; leaving
   3 m range cancels; staying for the configured duration completes exactly once.
   A completed point must not display an interaction prompt or score again.
7. Verify score reference calculations 400 / 250 / 100 at 0 / 450 / 900 seconds.
   Complete several points and verify event-driven score/progress HUD updates.
8. Complete all five. Verify results, final score, 5/5, stopped timer, unlocked
   cursor, clickable Retry, fresh 0/5 and SCORE 0 after retry.
9. In a separate playtest, set Mission Duration temporarily to 10 seconds and
   wait. Verify MISSION ENDED, cancelled active rescue, no late scoring and a
   working Retry. Restore 900 seconds before saving.
10. Test MainMenu -> FloodTown -> Results -> MainMenu and Boot -> MainMenu in
    a desktop build. Check HUD readability at 1280x720 and 1920x1080.
11. Review every real Rodin asset and prefab in AssetReview, then confirm the
    same models/materials render correctly in FloodTown. Keep the river visual
    only and the boat non-drivable.
12. Commit real Unity-generated metadata, scenes, data and approved prefabs only
    after import and testing. Do not commit caches or secrets. Only then mark
    Milestone 2 complete.

## Known Limits

Camera collision avoidance, animations, victim actors, dynamic flood mechanics
and vehicle physics are absent. There is no networking, cloud integration,
runtime OpenAI API or MCP connection. Raw model availability and actual Unity
compilation/playtesting remain required gates. The legacy anonymous scoring
method remains for compatibility; mission gameplay uses the ID-guarded method.

## Unity API References

- [Test assembly setup](https://docs.unity.com/en-us/engine/6000.0/manual/packages-list/cus-pkg-development/cus-tests)
- [Assembly definition format](https://docs.unity.com/en-us/engine/6000.0/manual/scripting/compilation-and-code-reload/script-compilation/assembly-definition-files/file-format)
- [InputSystemUIInputModule and default actions](https://docs.unity.cn/Packages/com.unity.inputsystem%401.13/api/UnityEngine.InputSystem.UI.InputSystemUIInputModule.html)

These API references are not a project version or package-version pin.
