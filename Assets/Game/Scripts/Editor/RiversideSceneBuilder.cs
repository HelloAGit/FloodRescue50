using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;
using FloodRescue50.Core;
using FloodRescue50.Player;
using FloodRescue50.Rescue;
using FloodRescue50.Scoring;
using FloodRescue50.UI;

namespace FloodRescue50.Editor
{
    public static class RiversideSceneBuilder
    {
        private const string SceneRoot = "Assets/Game/Scenes/";
        private static readonly string[] PrefabNames = {
            "TerraceHouse", "Clinic", "CornerShop", "BusShelter", "Substation",
            "RescueBuoy", "SandbagStack", "RoadBarrier", "RescueBoat"
        };
        private static readonly string[] PoiNames = {
            "Flooded House", "Riverside Medical Clinic", "Corner Shop", "Bus Stop", "Electrical Substation"
        };
        private static readonly RescuePointType[] PoiTypes = {
            RescuePointType.Residential, RescuePointType.Medical, RescuePointType.Commercial,
            RescuePointType.Transport, RescuePointType.Infrastructure
        };
        private static readonly Vector3[] PoiPositions = {
            new Vector3(-24f, 0f, 34f), new Vector3(-8f, 0f, 15f), new Vector3(20f, 0f, 1f),
            new Vector3(23f, 0f, -20f), new Vector3(27f, 0f, 35f)
        };

        [MenuItem("Flood Rescue 50/Create Prototype Scenes")]
        public static void CreatePrototypeScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before creating scenes.");
            if (TMP_Settings.defaultFontAsset == null)
                throw new InvalidOperationException("Import TMP Essential Resources before creating scenes.");
            foreach (string name in new[] { "Boot", "MainMenu", "FloodTown", "AssetReview" })
                if (File.Exists(SceneRoot + name + ".unity"))
                    throw new InvalidOperationException("Existing scene preserved: " + name + ". Use the manual setup guide.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Data/RescuePoints");
            EnsureFolder("Assets/Game/Materials/Prototype");
            foreach (string category in new[] { "Buildings", "Infrastructure", "Props", "Vehicles" })
                EnsureFolder("Assets/Game/Art/Generated/Rodin/" + category);
            foreach (string category in new[] { "Environment", "Props", "Vehicles" })
                EnsureFolder("Assets/Game/Prefabs/" + category);

            CreateFloodTown();
            CreateMainMenu();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Boot").AddComponent<BootController>();
            SaveScene("Boot");
            CreateAssetReview();
            RegisterScenes();
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(SceneRoot + "FloodTown.unity");
            Debug.Log("Prototype scenes created. Missing approved prefabs use labelled blockouts. Complete the asset review and playtest checklist before marking Milestone 2 complete.");
        }

        private static void CreateFloodTown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var environment = new GameObject("Environment");
            Block("Ground", environment.transform, new Vector3(0f, -0.5f, 5f), new Vector3(100f, 1f, 110f), new Color(0.26f, 0.39f, 0.3f));
            Block("Road", environment.transform, new Vector3(0f, 0.015f, 5f), new Vector3(10f, 0.03f, 95f), new Color(0.3f, 0.32f, 0.34f));
            Block("CrossRoad", environment.transform, new Vector3(0f, 0.02f, -20f), new Vector3(90f, 0.04f, 9f), new Color(0.3f, 0.32f, 0.34f));
            Block("Pavement", environment.transform, new Vector3(7f, 0.08f, 5f), new Vector3(3f, 0.16f, 95f), new Color(0.55f, 0.56f, 0.57f));
            Block("River", environment.transform, new Vector3(0f, 0.04f, 48f), new Vector3(95f, 0.08f, 10f), new Color(0.15f, 0.48f, 0.58f), false);
            Block("RiverBank", environment.transform, new Vector3(0f, 0.6f, 42f), new Vector3(100f, 1.2f, 0.6f), Color.gray);
            Block("WestBoundary", environment.transform, new Vector3(-49f, 1f, 5f), new Vector3(1f, 2f, 110f), Color.gray);
            Block("EastBoundary", environment.transform, new Vector3(49f, 1f, 5f), new Vector3(1f, 2f, 110f), Color.gray);
            Block("SouthBoundary", environment.transform, new Vector3(0f, 1f, -49f), new Vector3(100f, 2f, 1f), Color.gray);
            Lighting();

            var gameplay = new GameObject("Gameplay");
            var timer = Child("MissionTimer", gameplay.transform).AddComponent<MissionTimer>();
            var scoring = Child("ScoringService", gameplay.transform).AddComponent<ScoringService>();
            var mission = Child("MissionManager", gameplay.transform).AddComponent<MissionManager>();

            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-12f, 0.2f, -34f);
            var character = player.AddComponent<CharacterController>();
            character.height = 1.8f;
            character.radius = 0.35f;
            character.center = new Vector3(0f, 0.9f, 0f);
            var movement = player.AddComponent<RescuerMovementController>();
            var interactor = player.AddComponent<PlayerRescueInteractor>();
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "RescuerVisual";
            visual.transform.SetParent(player.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.transform.localScale = new Vector3(0.6f, 0.9f, 0.6f);
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = MaterialFor("Rescuer", new Color(0.95f, 0.45f, 0.1f));

            var cameraObject = new GameObject("MainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            var cameraController = cameraObject.AddComponent<ThirdPersonCameraController>();
            cameraController.SetTarget(player.transform);
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 3f, -5f);
            SetReference(movement, "cameraTransform", cameraObject.transform);

            var rescueRoot = new GameObject("RescuePoints");
            var riverside = new GameObject("Riverside");
            var points = new List<RescuePointController>();
            for (int i = 0; i < 5; i++)
            {
                var pointObject = Child("POI_00" + (i + 1) + "_" + PrefabNames[i], rescueRoot.transform);
                pointObject.transform.position = PoiPositions[i];
                var point = pointObject.AddComponent<RescuePointController>();
                SetReference(point, "data", PoiData(i));
                var sphere = pointObject.GetComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 12f;
                var body = pointObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
                points.Add(point);
                AddMarker(point);
                PlaceVisual(i, riverside.transform, PoiPositions[i] + new Vector3(0f, 0f, 6f));
            }
            SetReference(mission, "missionTimer", timer);
            SetReference(mission, "scoringService", scoring);
            var missionSerialized = new SerializedObject(mission);
            var configured = missionSerialized.FindProperty("rescuePoints");
            configured.arraySize = points.Count;
            for (int i = 0; i < points.Count; i++) configured.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
            missionSerialized.ApplyModifiedPropertiesWithoutUndo();

            PlaceVisual(5, riverside.transform, PoiPositions[0] + Vector3.left * 2f);
            PlaceVisual(6, riverside.transform, PoiPositions[0] + Vector3.right * 2f);
            PlaceVisual(6, riverside.transform, PoiPositions[1] + Vector3.left * 2f);
            PlaceVisual(7, riverside.transform, PoiPositions[1] + Vector3.right * 2f);
            PlaceVisual(7, riverside.transform, PoiPositions[3] + Vector3.right * 2f);
            PlaceVisual(7, riverside.transform, PoiPositions[4] + Vector3.right * 2f);
            PlaceVisual(8, riverside.transform, new Vector3(15f, 0.1f, 48f));
            CreateMissionUI(timer, scoring, mission, interactor, movement, cameraController);
            SaveScene("FloodTown");
        }

        private static void CreateMissionUI(MissionTimer timer, ScoringService scoring, MissionManager mission,
            PlayerRescueInteractor interactor, RescuerMovementController movement, ThirdPersonCameraController camera)
        {
            var canvas = ScreenCanvas();
            var hudObject = Child("MissionHUD", canvas.transform);
            var hud = hudObject.AddComponent<MissionHudController>();
            SetReference(hud, "missionTimer", timer);
            SetReference(hud, "scoringService", scoring);
            SetReference(hud, "missionManager", mission);
            SetReference(hud, "playerInteractor", interactor);
            SetReference(hud, "timerText", Text(canvas.transform, "Timer", "TIME 15:00", new Vector2(24f, -24f), new Vector2(460f, 44f), 28));
            SetReference(hud, "scoreText", Text(canvas.transform, "Score", "SCORE 0", new Vector2(24f, -72f), new Vector2(460f, 44f), 28));
            SetReference(hud, "progressText", Text(canvas.transform, "Progress", "RESCUE POINTS 0/5", new Vector2(24f, -120f), new Vector2(460f, 44f), 26));
            SetReference(hud, "objectiveText", Text(canvas.transform, "Objective", "Explore Riverside and complete rescue operations.", new Vector2(24f, -175f), new Vector2(500f, 100f), 24));
            SetReference(hud, "interactionText", Text(canvas.transform, "Interaction", "", new Vector2(24f, -300f), new Vector2(550f, 70f), 26));

            var panel = new GameObject("ResultsPanel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.06f, 0.09f, 0.08f, 0.96f);
            var results = canvas.gameObject.AddComponent<MissionResultsController>();
            SetReference(results, "missionManager", mission);
            SetReference(results, "resultsPanel", panel);
            SetReference(results, "titleText", Text(panel.transform, "Title", "MISSION COMPLETE", new Vector2(80f, -100f), new Vector2(900f, 70f), 42));
            SetReference(results, "scoreText", Text(panel.transform, "Score", "", new Vector2(80f, -205f), new Vector2(800f, 60f), 30));
            SetReference(results, "progressText", Text(panel.transform, "Progress", "", new Vector2(80f, -280f), new Vector2(800f, 60f), 30));
            SetReference(results, "timeText", Text(panel.transform, "Time", "", new Vector2(80f, -355f), new Vector2(800f, 60f), 30));
            SetReference(results, "playerMovement", movement);
            SetReference(results, "playerInteractor", interactor);
            SetReference(results, "playerCamera", camera);
            ButtonAt(panel.transform, "Retry", new Vector2(80f, -470f), results.RetryMission);
            ButtonAt(panel.transform, "Main Menu", new Vector2(400f, -470f), results.LoadMainMenu);
            panel.SetActive(false);
        }

        private static void CreateMainMenu()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var canvas = ScreenCanvas();
            var menu = canvas.gameObject.AddComponent<MainMenuController>();
            Text(canvas.transform, "Title", "FLOOD RESCUE 50", new Vector2(100f, -130f), new Vector2(1000f, 90f), 48);
            ButtonAt(canvas.transform, "START RESCUE", new Vector2(100f, -300f), menu.StartRescue);
            ButtonAt(canvas.transform, "QUIT", new Vector2(100f, -405f), menu.Quit);
            var camera = new GameObject("MainCamera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.16f, 0.13f);
            SaveScene("MainMenu");
        }

        private static void CreateAssetReview()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("AssetReview");
            Block("Ground", root.transform, new Vector3(0f, -0.5f, 10f), new Vector3(70f, 1f, 70f), Color.gray);
            for (int i = 0; i < PrefabNames.Length; i++)
                PlaceVisual(i, root.transform, new Vector3((i % 3 - 1) * 18f, 0f, (i / 3) * 18f));
            Lighting();
            var camera = new GameObject("MainCamera").AddComponent<Camera>();
            camera.transform.position = new Vector3(35f, 40f, -40f);
            camera.transform.LookAt(new Vector3(0f, 0f, 18f));
            SaveScene("AssetReview");
        }

        private static void AddMarker(RescuePointController point)
        {
            var obj = new GameObject("WorldMarker", typeof(RectTransform), typeof(Canvas));
            obj.transform.SetParent(point.transform, false);
            obj.transform.localPosition = Vector3.up * 3f;
            obj.transform.localScale = Vector3.one * 0.01f;
            obj.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(200f, 100f);
            var label = Text(obj.transform, "State", "?", Vector2.zero, new Vector2(200f, 100f), 80);
            label.alignment = TextAlignmentOptions.Center;
            var marker = obj.AddComponent<RescuePointMarker>();
            SetReference(marker, "rescuePoint", point);
            SetReference(marker, "markerText", label);
        }

        private static RescuePointData PoiData(int index)
        {
            string id = "POI_00" + (index + 1);
            string path = "Assets/Game/Data/RescuePoints/" + id + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<RescuePointData>(path);
            if (existing != null) return existing;
            var data = ScriptableObject.CreateInstance<RescuePointData>();
            data.poiId = id;
            data.displayName = PoiNames[index];
            data.description = "Complete the rescue operation at " + PoiNames[index] + ".";
            data.rescuePointType = PoiTypes[index];
            data.numberOfVictims = new[] { 3, 5, 2, 4, 0 }[index];
            data.criticalVictims = index == 1 ? 1 : 0;
            data.interactionDuration = new[] { 4f, 5f, 3f, 4f, 5f }[index];
            AssetDatabase.CreateAsset(data, path);
            return data;
        }

        private static void PlaceVisual(int index, Transform parent, Vector3 position)
        {
            string category = index < 5 ? "Environment" : index == 8 ? "Vehicles" : "Props";
            string name = "FR50_PF_" + PrefabNames[index] + "_A";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/" + category + "/" + name + ".prefab");
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.transform.SetParent(parent, false);
                instance.transform.position = position;
                return;
            }
            Debug.LogWarning("Approved prefab unavailable; using blockout: " + name);
            var blockout = Child("Blockout_" + PrefabNames[index], parent);
            blockout.transform.position = position;
            Vector3 size = index < 3 ? new Vector3(6f, 7f, 5f) : index < 5 ? new Vector3(4f, 3f, 2f) :
                index == 8 ? new Vector3(2f, 0.8f, 5f) : new Vector3(1.8f, 0.8f, 0.5f);
            Block(PrefabNames[index] + "Visual", blockout.transform, position + Vector3.up * size.y * 0.5f, size,
                index < 5 ? new Color(0.58f, 0.3f, 0.25f) : new Color(0.95f, 0.5f, 0.12f), index != 8);
            var text = new GameObject("Label").AddComponent<TextMeshPro>();
            text.transform.SetParent(blockout.transform, false);
            text.transform.localPosition = new Vector3(0f, size.y + 0.5f, 0f);
            text.text = PrefabNames[index];
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 4f;
            text.alignment = TextAlignmentOptions.Center;
        }

        private static Canvas ScreenCanvas()
        {
            var obj = new GameObject("UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1440f, 900f);
            scaler.matchWidthOrHeight = 0.5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule))
                .GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            return canvas;
        }

        private static TMP_Text Text(Transform parent, string name, string content, Vector2 position, Vector2 size, int fontSize)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            Rect(obj.GetComponent<RectTransform>(), position, size);
            var label = obj.GetComponent<TextMeshProUGUI>();
            label.font = TMP_Settings.defaultFontAsset;
            label.text = content;
            label.fontSize = fontSize;
            label.color = Color.white;
            label.raycastTarget = false;
            return label;
        }

        private static void ButtonAt(Transform parent, string caption, Vector2 position, UnityAction action)
        {
            var obj = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            Rect(obj.GetComponent<RectTransform>(), position, new Vector2(280f, 75f));
            obj.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.32f);
            var button = obj.GetComponent<Button>();
            button.targetGraphic = obj.GetComponent<Image>();
            UnityEventTools.AddPersistentListener(button.onClick, action);
            var label = Text(obj.transform, "Label", caption, Vector2.zero, new Vector2(280f, 75f), 26);
            label.alignment = TextAlignmentOptions.Center;
        }

        private static void Rect(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static GameObject Child(string name, Transform parent)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void Block(string name, Transform parent, Vector3 position, Vector3 size, Color color, bool collider = true)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.position = position;
            obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = MaterialFor(name, color);
            if (!collider) UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        }

        private static Material MaterialFor(string name, Color color)
        {
            string path = "Assets/Game/Materials/Prototype/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var shader = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null
                ? Shader.Find("Standard") : Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("Choose a Built-in or URP project for the prototype builder.");
            var material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void Lighting()
        {
            var light = new GameObject("DirectionalLight").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.5f, 0.5f, 0.5f);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }

        private static void SetReference(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null) throw new InvalidOperationException("Unknown serialized field: " + field);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SaveScene(string name)
        {
            if (!EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), SceneRoot + name + ".unity"))
                throw new IOException("Unable to save scene: " + name);
        }

        private static void RegisterScenes()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string name in new[] { "Boot", "MainMenu", "FloodTown" })
            {
                string path = SceneRoot + name + ".unity";
                if (!scenes.Exists(scene => scene.path == path)) scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
