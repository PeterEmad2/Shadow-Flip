using System.Collections.Generic;
using System.IO;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShadowFlip.EditorTools
{
    public static class OpticsPuzzleBuilder
    {
        private const string CelDir = "Assets/Images/CelShaded/";
        private const string SfxDir = "Assets/Sounds/SFX/";
        private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string TutorialPath = "Assets/Scenes/OpticsPuzzle_Tutorial.unity";
        private const string Level1Path = "Assets/Scenes/OpticsPuzzle.unity";
        private const string Level2Path = "Assets/Scenes/OpticsPuzzle_Level2.unity";
        private const string Level3Path = "Assets/Scenes/OpticsPuzzle_Level3.unity";
        private const string Level4Path = "Assets/Scenes/OpticsPuzzle_Level4.unity";
        private const string Level5Path = "Assets/Scenes/OpticsPuzzle_Level5.unity";
        private const string Level6Path = "Assets/Scenes/OpticsPuzzle_Level6.unity";

        private enum WallTileStyle { Long, Medium, Pillar, Ruined }

        private struct CelAssets
        {
            public Sprite EmitterIdle, EmitterActive, EmitterBroken;
            public Sprite[] MirrorDirections;
            public Sprite ReceiverInactive, ReceiverActiveWhite, ReceiverInactiveDark, ReceiverActiveGlow;
            public Sprite WallLightLong, WallLightMedium, WallLightPillar, WallLightRuined;
            public Sprite WallShadowLong, WallShadowMedium, WallShadowPillar, WallShadowRuined;
            public Sprite DoorClosed, DoorOpen, LeverOff, LeverOn, ButtonUp, ButtonDown;
            public Sprite FloorLight, FloorShadow, PrismCrystal;
            public Sprite UmbraMonster1, UmbraMonster2, MonsterPetrified;
            public Sprite PortalMirrorA, PortalMirrorB, CombinerCrystal, LanternSanctuary, EchoPedestal;
            public Sprite RelayCrystal, LaserBarrier, PhasePad;
        }

        [MenuItem("Shadow Flip/Create All Levels (Main Menu + Tutorial + Levels 1-6)")]
        public static void CreateAllLevels()
        {
            if (IsInPlayModeGuard()) return;
            ConfigureCelShadedSprites();
            CreateTutorialLevel();
            CreateOpticsPuzzleLevel();
            CreateOpticsPuzzleLevel2();
            CreateOpticsPuzzleLevel3();
            CreateOpticsPuzzleLevel4();
            CreateOpticsPuzzleLevel5();
            CreateOpticsPuzzleLevel6();
            CreateMainMenuScene();
            RegisterScenesInBuildSettings();
            Debug.Log("Created Main Menu, Tutorial, and Levels 1-6 and registered all 8 in Build Settings.");
        }

        [MenuItem("Shadow Flip/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = new Color(0.06f, 0.05f, 0.11f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            GameObject globalLightGo = new GameObject("Global Light 2D");
            Light2D globalLight = globalLightGo.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.intensity = 1.0f;

            GameObject floorGo = new GameObject("DungeonFloor");
            floorGo.transform.position = Vector3.zero;
            SpriteRenderer floorSr = floorGo.AddComponent<SpriteRenderer>();
            floorSr.sprite = assets.FloorLight;
            floorSr.drawMode = SpriteDrawMode.Tiled;
            floorSr.size = new Vector2(22f, 13f);
            floorSr.sortingOrder = -20;

            CreateArenaWalls(assets);

            // Decorative cel-shaded optical diorama behind the Main Menu UI
            CreateEmitter("Menu_Emitter_White", new Vector3(-8.2f, 0f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreatePrism("Menu_Prism", new Vector3(0f, 0f, 0f), 0f, OpticalPrism.PrismMode.DisperseRGB, 90f, true, false, 90f, assets);
            CreateIsometricMirror("Menu_Mirror_Blue", new Vector3(0f, 4.2f, 0f), 225f, 90f, WorldAffinity.Both, assets, canRotate: false);
            CreateIsometricMirror("Menu_Mirror_Red", new Vector3(0f, -4.2f, 0f), 135f, 90f, WorldAffinity.Both, assets, canRotate: false);
            CreateSensor("Menu_Sensor_Blue", new Vector3(7.5f, 4.2f, 0f), LightColor.Blue, WorldAffinity.Both, assets);
            CreateSensor("Menu_Sensor_Green", new Vector3(7.5f, 0f, 0f), LightColor.Green, WorldAffinity.Both, assets);
            CreateSensor("Menu_Sensor_Red", new Vector3(7.5f, -4.2f, 0f), LightColor.Red, WorldAffinity.Both, assets);

            GameObject logicGo = new GameObject("Logic");
            logic gameLogic = logicGo.AddComponent<logic>();
            gameLogic.globalLight = globalLight;
            gameLogic.isDarkActive = false;

            PuzzleAudioManager audioMgr = logicGo.AddComponent<PuzzleAudioManager>();
            audioMgr.buttonClickClip = LoadAudio(SfxDir + "button_click.wav");
            audioMgr.bgmLightClip = LoadAudio(SfxDir + "bgm_light_realm.wav");
            audioMgr.bgmShadowClip = LoadAudio(SfxDir + "bgm_shadow_realm.wav");

            logicGo.AddComponent<PuzzleVFXManager>();
            PuzzleHUD hud = logicGo.AddComponent<PuzzleHUD>();
            hud.gameLogic = gameLogic;

            EditorSceneManager.SaveScene(scene, MainMenuPath);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Main Menu scene at " + MainMenuPath);
        }

        [MenuItem("Shadow Flip/Create Tutorial Level")]
        public static void CreateTutorialLevel()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out _,
                new Color(0.11f, 0.09f, 0.16f, 1f),
                new Color(1f, 0.96f, 0.88f, 1f), 1.05f,
                new Color(0.42f, 0.36f, 0.78f, 1f), 0.58f,
                new Vector3(-8.0f, -2.5f, 0f));

            hud.parFlips = 3;
            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_Tut1", new Vector3(-8.7f, -4.8f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_Tut2", new Vector3(8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateEmitter("Emitter_Tut1", new Vector3(-8.5f, 3.0f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            GameObject mirror1 = CreateIsometricMirror("Mirror_Tut1", new Vector3(-6.0f, 3.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor sensor1 = CreateSensor("Sensor_Tut1", new Vector3(-6.0f, -0.5f, 0f), LightColor.White, WorldAffinity.Both, assets, acceptAny: true);
            PuzzleLever lever1 = CreateLever("Lever_Tut", new Vector3(-6.0f, -3.5f, 0f), assets, WorldAffinity.Both);

            CreateWall("TutWall1_Top", new Vector3(-4.0f, 3.2f, 0f), new Vector2(0.85f, 5.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("TutWall1_Bottom", new Vector3(-4.0f, -4.0f, 0f), new Vector2(0.85f, 3.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door1 = CreateDoor("TutDoor_1", new Vector3(-4.0f, -0.8f, 0f), new Vector2(1.1f, 2.8f), assets,
                new[] { sensor1 }, null, new[] { lever1 }, requireAll: true, stayOpen: true);

            GameObject phaseWall = CreateWall("TutPhaseWall_LightOnly", new Vector3(-1.5f, 0.0f, 0f), new Vector2(0.85f, 11.4f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);

            CreateEmitter("Emitter_Tut2_Shadow", new Vector3(0.5f, 3.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            GameObject mirror2 = CreateIsometricMirror("Mirror_Tut2_Shadow", new Vector3(4.0f, 3.5f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets);
            LightSensor sensor2 = CreateSensor("Sensor_Tut2_Shadow", new Vector3(4.0f, 0.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            GameObject blockTut = CreatePushableBlock("RuinBlock_Tut", new Vector3(0.5f, -3.0f, 0f), assets);
            PressurePlate plateTut = CreatePressurePlate("Button_Tut", new Vector3(4.0f, -3.0f, 0f), assets, WorldAffinity.Both, latch: false);

            CreateWall("TutVault_Top", new Vector3(6.5f, 3.0f, 0f), new Vector2(0.85f, 5.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("TutVault_Bottom", new Vector3(6.5f, -4.4f, 0f), new Vector2(0.85f, 2.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door2 = CreateDoor("TutDoor_Final", new Vector3(6.5f, -1.5f, 0f), new Vector2(1.1f, 3.0f), assets,
                new[] { sensor2 }, new[] { plateTut }, null, requireAll: true, stayOpen: false);

            GameObject exitGo = CreateExit("LevelExit", new Vector3(8.5f, -1.5f, 0f), assets, new[] { door1, door2 }, "OpticsPuzzle");

            GameObject tutGo = new GameObject("TutorialManager");
            TutorialManager tut = tutGo.AddComponent<TutorialManager>();
            tut.player = playerMovement;
            tut.gameLogic = gameLogic;
            tut.hud = hud;
            tut.tutorialSensor1 = sensor1;
            tut.tutorialLever1 = lever1;
            tut.tutorialDoor1 = door1;
            tut.crossXThreshold = -0.6f;
            tut.tutorialSensor2 = sensor2;
            tut.tutorialDoor2 = door2;
            tut.stageTargets = new[]
            {
                mirror1.transform,
                phaseWall.transform,
                mirror2.transform,
                blockTut.transform,
                exitGo.transform,
                lever1.transform
            };

            EditorSceneManager.SaveScene(scene, TutorialPath);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Tutorial Level at " + TutorialPath);
        }

        [MenuItem("Shadow Flip/Create Optics Puzzle Level")]
        public static void CreateOpticsPuzzleLevel()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out _,
                new Color(0.08f, 0.09f, 0.13f, 1f),
                new Color(1f, 0.98f, 0.92f, 1f), 1.0f,
                new Color(0.45f, 0.52f, 0.85f, 1f), 0.55f,
                new Vector3(-7.8f, -3.5f, 0f));

            hud.parFlips = 3;
            hud.lessonTitle = "Level 1: Chromatic Vault";
            hud.levelHint = "Align Mirror_Red1 in Light Realm, pull the Green Shutter Lever, step on the Vault Button, then flip [F] to Shadow Realm!";

            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_L1_Top", new Vector3(-8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_L1_Vault", new Vector3(8.7f, 4.6f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateWall("LightOnly_Shutter", new Vector3(-6.0f, 1.5f, 0f), new Vector2(0.8f, 2.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
            CreateEmitter("WhiteBeamEmitter", new Vector3(-8.5f, 1.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);

            CreatePrism("RGB_Prism", new Vector3(-3.5f, 1.5f, 0f), 90f, OpticalPrism.PrismMode.DisperseRGB, 90f, true, true, 90f, assets);

            CreateIsometricMirror("Mirror_Blue1", new Vector3(-3.5f, 4.2f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            CreateColorFilter("BlueFilter", new Vector3(0.5f, 4.2f, 0f), new Vector2(0.45f, 1.1f), LightColor.Blue, assets);
            LightSensor blueSensor = CreateSensor("Sensor_Blue", new Vector3(4.2f, 4.2f, 0f), LightColor.Blue, WorldAffinity.ShadowOnly, assets);

            CreateColorFilter("GreenFilter", new Vector3(-0.5f, 1.5f, 0f), new Vector2(0.45f, 1.1f), LightColor.Green, assets);
            GameObject greenBarrier = CreateWall("GreenPath_Barrier", new Vector3(0.8f, 1.5f, 0f), new Vector2(0.65f, 1.4f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateIsometricMirror("Mirror_Green", new Vector3(2.2f, 1.5f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets);
            LightSensor greenSensor = CreateSensor("Sensor_Green", new Vector3(2.2f, -1.2f, 0f), LightColor.Green, WorldAffinity.Both, assets);

            PuzzleLever greenLever = CreateLever("Lever_GreenShutter", new Vector3(-1.0f, -2.2f, 0f), assets, WorldAffinity.Both,
                toggledObjects: new[] { greenBarrier });

            GameObject mirrorRed1 = CreateIsometricMirror("Mirror_Red1", new Vector3(-3.5f, -3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            mirrorRed1.GetComponent<OpticalMirror>().requireLightWorldToRotate = true;
            mirrorRed1.GetComponent<SpriteRenderer>().color = new Color(1f, 0.88f, 0.55f, 1f);

            CreateIsometricMirror("Mirror_Red2", new Vector3(4.5f, -3.5f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            LightSensor redSensor = CreateSensor("Sensor_Red", new Vector3(4.5f, -0.8f, 0f), LightColor.Red, WorldAffinity.Both, assets);

            PressurePlate vaultButton = CreatePressurePlate("Button_VaultSeal", new Vector3(0.5f, 2.8f, 0f), assets, WorldAffinity.Both, latch: true);

            CreateWall("VaultWall_Upper", new Vector3(6.5f, 3.8f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -3.6f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door = CreateDoor("PuzzleDoor", new Vector3(6.5f, 0.2f, 0f), new Vector2(1.1f, 2.8f), assets,
                new[] { redSensor, greenSensor, blueSensor }, new[] { vaultButton }, new[] { greenLever }, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, 0.2f, 0f), assets, new[] { door }, "OpticsPuzzle_Level2");

            EditorSceneManager.SaveScene(scene, Level1Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 1 at " + Level1Path);
        }

        [MenuItem("Shadow Flip/Create Optics Puzzle Level 2 (Eclipse Temple)")]
        public static void CreateOpticsPuzzleLevel2()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out _,
                new Color(0.05f, 0.04f, 0.09f, 1f),
                new Color(1f, 0.91f, 0.70f, 1f), 1.15f,
                new Color(0.32f, 0.22f, 0.58f, 1f), 0.48f,
                new Vector3(-7.5f, 0f, 0f));

            hud.parFlips = 3;
            hud.lessonTitle = "Level 2: Eclipse Temple";
            hud.levelHint = "Light Realm: route Red & press the Light Button. Pull the Lever to rotate the enclosed Shadow Mirror, then flip [F] to push the Ruin Block!";

            CreateArenaWalls(assets);

            CreateEmitter("LightOnly_Emitter", new Vector3(-8.5f, 3.8f, 0f), LightColor.White, WorldAffinity.LightOnly, Vector2.right, assets);
            CreateIsometricMirror("Mirror_L1", new Vector3(-5.2f, 3.8f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreatePrism("Splitter_Prism", new Vector3(-5.2f, 0.5f, 0f), -90f, OpticalPrism.PrismMode.Splitter, 90f, true, false, 90f, assets);
            CreateIsometricMirror("Mirror_L2", new Vector3(-2.8f, 0.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateColorFilter("RedFilter", new Vector3(-2.8f, -1.2f, 0f), new Vector2(1.1f, 0.45f), LightColor.Red, assets);
            CreateIsometricMirror("Mirror_L3", new Vector3(-2.8f, -2.8f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor stage1Sensor = CreateSensor("Sensor_Stage1_Red", new Vector3(4.2f, -2.8f, 0f), LightColor.Red, WorldAffinity.LightOnly, assets);
            PressurePlate lightButton = CreatePressurePlate("Light_Button", new Vector3(1.5f, -1.6f, 0f), assets, WorldAffinity.LightOnly, latch: true);

            CreateEmitter("ShadowOnly_Emitter", new Vector3(-8.5f, -4.3f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            CreateIsometricMirror("Mirror_S1", new Vector3(-1.2f, -4.3f, 0f), 135f, 90f, WorldAffinity.ShadowOnly, assets);

            GameObject mirrorS2Go = CreateIsometricMirror("Mirror_S2", new Vector3(-1.2f, 4.3f, 0f), 135f, 90f, WorldAffinity.ShadowOnly, assets, canRotate: false);
            OpticalMirror mirrorS2 = mirrorS2Go.GetComponent<OpticalMirror>();
            CreateWall("AlcoveWall_Left", new Vector3(-2.4f, 4.5f, 0f), new Vector2(0.65f, 2.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("AlcoveWall_Ruined", new Vector3(0.2f, 5.1f, 0f), new Vector2(1.1f, 0.9f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            PuzzleLever remoteLever = CreateLever("Lever_RemoteMirror", new Vector3(-5.2f, -2.8f, 0f), assets, WorldAffinity.Both,
                controlledMirrors: new[] { mirrorS2 });

            LightSensor shadowSensor = CreateSensor("Sensor_Stage2_Shadow", new Vector3(4.2f, 4.3f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            CreateWall("ChannelWall_Top", new Vector3(3.0f, 3.3f, 0f), new Vector2(6.4f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("ChannelWall_Bottom", new Vector3(3.0f, 0.4f, 0f), new Vector2(6.4f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("LightOnly_Gate", new Vector3(0.4f, 1.85f, 0f), new Vector2(0.75f, 2.5f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);

            CreatePushableBlock("PushableBlock", new Vector3(-2.5f, 1.85f, 0f), assets);
            PressurePlate shadowPlate = CreatePressurePlate("Shadow_PressurePlate", new Vector3(3.6f, 1.85f, 0f), assets, WorldAffinity.ShadowOnly, latch: false);

            CreateWall("Vault1_Upper", new Vector3(6.2f, 4.6f, 0f), new Vector2(0.75f, 3.0f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Vault1_Lower", new Vector3(6.2f, -2.7f, 0f), new Vector2(0.75f, 6.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor lightDoor = CreateDoor("PuzzleDoor_Light", new Vector3(6.2f, 1.85f, 0f), new Vector2(1.0f, 2.5f), assets,
                new[] { stage1Sensor }, new[] { lightButton }, null, requireAll: true, stayOpen: true);

            CreateWall("Vault2_Upper", new Vector3(7.3f, 4.6f, 0f), new Vector2(0.75f, 3.0f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Vault2_Lower", new Vector3(7.3f, -2.7f, 0f), new Vector2(0.75f, 6.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor shadowDoor = CreateDoor("PuzzleDoor_Shadow", new Vector3(7.3f, 1.85f, 0f), new Vector2(1.0f, 2.5f), assets,
                new[] { shadowSensor }, new[] { shadowPlate }, new[] { remoteLever }, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.9f, 1.85f, 0f), assets, new[] { lightDoor, shadowDoor }, "OpticsPuzzle_Level3");

            EditorSceneManager.SaveScene(scene, Level2Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 2 at " + Level2Path);
        }

        [MenuItem("Shadow Flip/Create Level 3 (The Prism Sanctum)")]
        public static void CreateOpticsPuzzleLevel3()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out _,
                new Color(0.06f, 0.08f, 0.14f, 1f),
                new Color(1f, 0.96f, 0.86f, 1f), 1.05f,
                new Color(0.35f, 0.30f, 0.68f, 1f), 0.52f,
                new Vector3(-8.0f, -3.5f, 0f));

            hud.parFlips = 4;
            hud.lessonTitle = "Level 3: The Prism Sanctum";
            hud.levelHint = "Combine Red+Blue into White, warp across the wall via Portal Mirrors, push & rotate the Mirror Cart to charge the 7s Capacitor Crystal, then flip [F] and dash through!";

            CreateArenaWalls(assets);

            // Center Divider Wall at x = -0.5 with a LightOnly gate at (-0.5, -0.5)
            CreateWall("Divider_Top", new Vector3(-0.5f, 3.3f, 0f), new Vector2(0.8f, 5.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Divider_Gate_LightOnly", new Vector3(-0.5f, -0.5f, 0f), new Vector2(0.8f, 2.4f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
            CreateWall("Divider_Bottom", new Vector3(-0.5f, -3.9f, 0f), new Vector2(0.8f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);

            // West Chamber: Red + Blue -> ColorCombiner -> PortalMirror_A -> PortalMirror_B
            CreateEmitter("Emitter_Red", new Vector3(-8.5f, 4.0f, 0f), LightColor.Red, WorldAffinity.Both, Vector2.right, assets);
            CreateIsometricMirror("Mirror_W1", new Vector3(-4.5f, 4.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateEmitter("Emitter_Blue", new Vector3(-8.5f, 1.5f, 0f), LightColor.Blue, WorldAffinity.Both, Vector2.right, assets);
            CreateColorCombiner("Combiner_White", new Vector3(-4.5f, 1.5f, 0f), LightColor.Red, LightColor.Blue, LightColor.White, assets);

            PortalMirror portalB = CreatePortalMirror("PortalMirror_B", new Vector3(1.2f, 4.0f, 0f), assets.PortalMirrorB, Vector2.down, null);
            CreatePortalMirror("PortalMirror_A", new Vector3(-2.0f, 1.5f, 0f), assets.PortalMirrorA, Vector2.right, portalB, canRotate: false);

            // East Chamber: Backstop wall + Pushable Mirror Cart + Timed Capacitor Crystal
            CreateWall("Cart_Backstop", new Vector3(4.2f, 5.05f, 0f), new Vector2(1.4f, 0.7f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreatePushableMirrorCart("PushableMirrorCart", new Vector3(4.2f, 1.8f, 0f), 45f, 90f, assets);

            LightSensor capSensor = CreateSensor("Sensor_Capacitor_7s", new Vector3(4.2f, -0.2f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);
            capSensor.holdDuration = 7.0f;

            // Inner Shutter corridor before PuzzleDoor
            CreateWall("Shutter_Corridor_Top", new Vector3(5.95f, -0.4f, 0f), new Vector2(1.7f, 0.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("Shutter_Corridor_Bot", new Vector3(5.95f, -3.6f, 0f), new Vector2(1.7f, 0.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("InnerShutter_LightOnly", new Vector3(5.4f, -2.0f, 0f), new Vector2(0.65f, 2.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);

            CreateWall("VaultWall_Upper", new Vector3(6.5f, 2.8f, 0f), new Vector2(0.8f, 6.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -4.7f, 0f), new Vector2(0.8f, 2.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door = CreateDoor("PuzzleDoor_Capacitor", new Vector3(6.5f, -2.0f, 0f), new Vector2(1.05f, 2.6f), assets,
                new[] { capSensor }, null, null, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, -2.0f, 0f), assets, new[] { door }, "OpticsPuzzle_Level4");

            EditorSceneManager.SaveScene(scene, Level3Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 3 at " + Level3Path);
        }

        [MenuItem("Shadow Flip/Create Level 4 (Umbra Labyrinth)")]
        public static void CreateOpticsPuzzleLevel4()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.04f, 0.03f, 0.08f, 1f),
                new Color(1f, 0.94f, 0.80f, 1f), 1.05f,
                new Color(0.28f, 0.18f, 0.52f, 1f), 0.45f,
                new Vector3(-8.0f, 3.5f, 0f));

            stalker.umbraSafeDuration = 8.0f;
            stalker.canBePetrifiedByLaser = true;
            stalker.petrifyExposureTime = 1.1f;

            hud.parFlips = 4;
            hud.lessonTitle = "Level 4: Umbra Labyrinth";
            hud.levelHint = "Record a Light Echo [E] holding the Light Button, step out of the Sanctuary in Shadow Realm to lure the Umbra Stalker into the Laser to petrify it, then push its Statue onto the final Button!";

            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_L4", new Vector3(4.5f, 4.5f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            // Light Sanctuary in bottom-left
            CreateSanctuary("Lantern_Sanctuary", new Vector3(-7.5f, -3.0f, 0f), 2.2f, assets);

            // Echo Pedestal + LightOnly Button
            CreateEchoPedestal("Echo_Pedestal", new Vector3(-7.5f, 0.5f, 0f), playerMovement.LightspriteDown, assets);
            PressurePlate echoButton = CreatePressurePlate("Button_Echo_LightOnly", new Vector3(-4.5f, 0.5f, 0f), assets, WorldAffinity.LightOnly, latch: false);

            // Central Petrification Laser Beam
            CreateEmitter("Emitter_Petrify", new Vector3(-1.0f, 4.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.down, assets);
            LightSensor arenaSensor = CreateSensor("Sensor_Arena", new Vector3(-1.0f, -4.5f, 0f), LightColor.White, WorldAffinity.Both, assets);

            // Heavy Statue Button (no PushableBlock exists—must petrify the Umbra Stalker!)
            PressurePlate statueButton = CreatePressurePlate("Button_Statue", new Vector3(3.5f, -1.5f, 0f), assets, WorldAffinity.Both, latch: false);

            // Sealed Vault at x = 6.5
            CreateWall("VaultWall_Top", new Vector3(6.5f, 3.6f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Bot", new Vector3(6.5f, -3.6f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor finalDoor = CreateDoor("PuzzleDoor_Final", new Vector3(6.5f, 0.0f, 0f), new Vector2(1.1f, 2.8f), assets,
                new[] { arenaSensor }, new[] { echoButton, statueButton }, null, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, 0.0f, 0f), assets, new[] { finalDoor }, "OpticsPuzzle_Level5");

            EditorSceneManager.SaveScene(scene, Level4Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 4 at " + Level4Path);
        }

        [MenuItem("Shadow Flip/Create Level 5 (The Flip-Clock Matrix)")]
        public static void CreateOpticsPuzzleLevel5()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out _,
                new Color(0.06f, 0.07f, 0.13f, 1f),
                new Color(1f, 0.96f, 0.86f, 1f), 1.05f,
                new Color(0.32f, 0.25f, 0.64f, 1f), 0.50f,
                new Vector3(-8.0f, -3.5f, 0f));

            hud.parFlips = 3;
            hud.lessonTitle = "Level 5: The Flip-Clock Matrix";
            hud.levelHint = "Flip [F] to aim the Turret into the 4-Way Relay Crystal, align Mirror_E1 [E], then cross the dissolved Forcefield onto the Phase Teleport Pad!";

            CreateArenaWalls(assets);

            // Rotating Turret Emitter (starts UP; flipping [F] rotates -90 deg -> RIGHT into Relay_4Way)
            LightBeamEmitter turret = CreateEmitter("Turret_Emitter", new Vector3(-3.5f, 0.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.up, assets);
            RotatingTurretEmitter turretRot = turret.gameObject.AddComponent<RotatingTurretEmitter>();
            turretRot.rotateOnWorldFlip = true;
            turretRot.stepDegrees = -90f;

            // 4-Way Relay Crystal at (0.5, 0.5)
            CreateBeamRelay("Relay_4Way", new Vector3(0.5f, 0.5f, 0f), BeamRelayCrystal.RelayMode.CrossRepeater4Way, Vector2.right, false, assets);

            // Up -> Sensor_Top, Right -> Mirror_E1 -> Sensor_Right
            LightSensor topSensor = CreateSensor("Sensor_Top", new Vector3(0.5f, 4.2f, 0f), LightColor.White, WorldAffinity.Both, assets);
            CreateIsometricMirror("Mirror_E1", new Vector3(3.8f, 0.5f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            LightSensor rightSensor = CreateSensor("Sensor_Right", new Vector3(3.8f, 4.2f, 0f), LightColor.White, WorldAffinity.ShadowOnly, assets);

            // Down -> Sealed Alcove guarded by ShadowBarrierGate at (0.5, -1.8)
            CreateWall("Alcove_Left", new Vector3(-1.0f, -3.8f, 0f), new Vector2(0.9f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Alcove_Right", new Vector3(2.0f, -3.8f, 0f), new Vector2(0.9f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateShadowBarrierGate("Forcefield_Gate", new Vector3(0.5f, -1.8f, 0f), new Vector2(2.1f, 0.9f), assets);

            // Teleport Pads: Alcove -> Upper-Right Vault Button -> Exit Hallway
            PhaseShiftTile padOut = CreatePhaseShiftTile("PhasePad_Vault", new Vector3(8.5f, 4.0f, 0f), null, false, assets);
            CreatePhaseShiftTile("PhasePad_In", new Vector3(0.5f, -4.3f, 0f), padOut, false, assets);

            PhaseShiftTile padReturnTarget = CreatePhaseShiftTile("PhasePad_ReturnTarget", new Vector3(4.5f, -3.8f, 0f), null, false, assets);
            CreatePhaseShiftTile("PhasePad_Return", new Vector3(7.5f, 4.0f, 0f), padReturnTarget, false, assets);

            PressurePlate vaultButton = CreatePressurePlate("Button_Vault", new Vector3(8.5f, 2.5f, 0f), assets, WorldAffinity.Both, latch: true);

            // Sealed Vault at x = 6.5 & Horizontal Vault Shelf at y = 1.4
            CreateWall("VaultWall_Upper", new Vector3(6.5f, 2.6f, 0f), new Vector2(0.85f, 6.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultShelf_H", new Vector3(8.4f, 1.4f, 0f), new Vector2(3.8f, 0.7f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -4.7f, 0f), new Vector2(0.85f, 2.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);

            PuzzleDoor door = CreateDoor("PuzzleDoor_Matrix", new Vector3(6.5f, -2.5f, 0f), new Vector2(1.1f, 2.4f), assets,
                new[] { topSensor, rightSensor }, new[] { vaultButton }, null, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, -2.5f, 0f), assets, new[] { door }, "OpticsPuzzle_Level6");

            EditorSceneManager.SaveScene(scene, Level5Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 5 at " + Level5Path);
        }

        [MenuItem("Shadow Flip/Create Level 6 (Grandmaster Sanctum)")]
        public static void CreateOpticsPuzzleLevel6()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.04f, 0.03f, 0.09f, 1f),
                new Color(1f, 0.94f, 0.80f, 1f), 1.05f,
                new Color(0.26f, 0.18f, 0.50f, 1f), 0.45f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 11.0f;
            hud.parFlips = 5;
            hud.lessonTitle = "Level 6: Grandmaster Sanctum";
            hud.levelHint = "Free the Pushable Mirror Cart via the Forcefield, warp the beam across the wall, charge the 7s Capacitor Crystal in Light Realm, then flip [F] so the Realm Inverter converts the beam to Shadow!";

            CreateArenaWalls(assets);
            CreateSanctuary("Sanctuary_NorthWest", new Vector3(-7.5f, 3.5f, 0f), 2.2f, assets);

            // Main Emitter -> RealmInverter Relay -> Splitter_Master
            CreateEmitter("Emitter_Grand", new Vector3(-8.5f, 0.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreateBeamRelay("Relay_RealmInverter", new Vector3(-5.0f, 0.5f, 0f), BeamRelayCrystal.RelayMode.RealmInverter, Vector2.right, false, assets);
            CreatePrism("Splitter_Grand", new Vector3(-2.0f, 0.5f, 0f), 0f, OpticalPrism.PrismMode.Splitter, 90f, false, false, 90f, assets);

            // Top Branch: PortalMirror_A -> PortalMirror_B across Divider_Top (x = 0)
            CreateWall("Divider_Top", new Vector3(0.0f, 3.3f, 0f), new Vector2(0.8f, 5.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PortalMirror portalB = CreatePortalMirror("PortalMirror_B", new Vector3(1.8f, 4.2f, 0f), assets.PortalMirrorB, Vector2.down, null, canRotate: false);
            CreatePortalMirror("PortalMirror_A", new Vector3(-2.0f, 4.2f, 0f), assets.PortalMirrorA, Vector2.right, portalB, canRotate: false);

            // Bottom Branch: Mirror_Bot -> Forcefield Cage guarding PushableMirrorCart
            CreateIsometricMirror("Mirror_Bot", new Vector3(-2.0f, -3.8f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateShadowBarrierGate("Cage_Forcefield", new Vector3(2.3f, -3.8f, 0f), new Vector2(0.8f, 2.2f), assets);
            CreateWall("Cage_Top", new Vector3(4.0f, -2.3f, 0f), new Vector2(3.8f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("Cage_Right", new Vector3(5.7f, -3.8f, 0f), new Vector2(0.65f, 3.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Cage_Bot", new Vector3(4.0f, -5.2f, 0f), new Vector2(3.8f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);

            CreatePushableMirrorCart("PushableMirrorCart", new Vector3(3.6f, -3.8f, 0f), 45f, 90f, assets);

            // Dual Target Sensors on x = 4.8 (Light 7s Capacitor at y = 2.0, Shadow at y = -0.2)
            LightSensor lightCapSensor = CreateSensor("Sensor_Capacitor_7s", new Vector3(4.8f, 2.0f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);
            lightCapSensor.holdDuration = 7.0f;
            LightSensor shadowFinalSensor = CreateSensor("Sensor_Shadow_Grand", new Vector3(4.8f, -0.2f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            // Grandmaster Vault at x = 6.5
            CreateWall("VaultWall_Upper", new Vector3(6.5f, 3.9f, 0f), new Vector2(0.85f, 4.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -3.2f, 0f), new Vector2(0.85f, 5.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor grandDoor = CreateDoor("PuzzleDoor_Grandmaster", new Vector3(6.5f, 0.8f, 0f), new Vector2(1.1f, 2.4f), assets,
                new[] { lightCapSensor, shadowFinalSensor }, null, null, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, 0.8f, 0f), assets, new[] { grandDoor }, "");

            EditorSceneManager.SaveScene(scene, Level6Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 6 at " + Level6Path);
        }

        private static void SetupCoreScene(
            CelAssets assets,
            out logic gameLogic,
            out PlayerMovement playerMovement,
            out PuzzleHUD hud,
            out ShadowStalkerController stalker,
            Color camBg,
            Color lightColor,
            float lightIntensity,
            Color shadowColor,
            float shadowIntensity,
            Vector3 playerStart)
        {
            GameObject camGo = new GameObject("Main Camera");
            Camera cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 7f;
            cam.backgroundColor = camBg;
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);

            GameObject globalLightGo = new GameObject("Global Light 2D");
            Light2D globalLight = globalLightGo.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.intensity = lightIntensity;

            GameObject floorGo = new GameObject("DungeonFloor");
            floorGo.transform.position = Vector3.zero;
            SpriteRenderer floorSr = floorGo.AddComponent<SpriteRenderer>();
            floorSr.sprite = assets.FloorLight;
            floorSr.drawMode = SpriteDrawMode.Tiled;
            floorSr.size = new Vector2(22f, 13f);
            floorSr.sortingOrder = -20;
            WorldPhaseObject floorPhase = floorGo.AddComponent<WorldPhaseObject>();
            floorPhase.affinity = WorldAffinity.Both;
            floorPhase.lightWorldSprite = assets.FloorLight;
            floorPhase.shadowWorldSprite = assets.FloorShadow;

            GameObject logicGo = new GameObject("Logic");
            gameLogic = logicGo.AddComponent<logic>();
            gameLogic.globalLight = globalLight;
            gameLogic.lightWorldColor = lightColor;
            gameLogic.lightWorldIntensity = lightIntensity;
            gameLogic.shadowWorldColor = shadowColor;
            gameLogic.shadowWorldIntensity = shadowIntensity;

            PuzzleAudioManager audioMgr = logicGo.AddComponent<PuzzleAudioManager>();
            audioMgr.flipLightClip = LoadAudio(SfxDir + "flip_light.wav");
            audioMgr.flipShadowClip = LoadAudio(SfxDir + "flip_shadow.wav");
            audioMgr.flipBlockedClip = LoadAudio(SfxDir + "flip_blocked.wav");
            audioMgr.mirrorRotateClip = LoadAudio(SfxDir + "mirror_rotate.wav");
            audioMgr.leverPullClip = LoadAudio(SfxDir + "lever_pull.wav");
            audioMgr.sensorOnClip = LoadAudio(SfxDir + "sensor_on.wav");
            audioMgr.sensorOffClip = LoadAudio(SfxDir + "sensor_off.wav");
            audioMgr.platePressClip = LoadAudio(SfxDir + "plate_press.wav");
            audioMgr.buttonClickClip = LoadAudio(SfxDir + "button_click.wav");
            audioMgr.doorOpenClip = LoadAudio(SfxDir + "door_open.wav");
            audioMgr.levelWinClip = LoadAudio(SfxDir + "level_win.wav");
            audioMgr.blockPushClip = LoadAudio(SfxDir + "block_push.wav");
            audioMgr.monsterWarnClip = LoadAudio(SfxDir + "monster_warn.wav");
            audioMgr.monsterSpawnClip = LoadAudio(SfxDir + "monster_spawn.wav");
            audioMgr.monsterCatchClip = LoadAudio(SfxDir + "monster_catch.wav");
            audioMgr.portalWarpClip = LoadAudio(SfxDir + "portal_warp.wav");
            audioMgr.echoRecordClip = LoadAudio(SfxDir + "echo_record.wav");
            audioMgr.monsterPetrifyClip = LoadAudio(SfxDir + "monster_petrify.wav");
            audioMgr.teleportPadClip = LoadAudio(SfxDir + "teleport_pad.wav");
            audioMgr.barrierHumClip = LoadAudio(SfxDir + "barrier_hum.wav");
            audioMgr.bgmLightClip = LoadAudio(SfxDir + "bgm_light_realm.wav");
            audioMgr.bgmShadowClip = LoadAudio(SfxDir + "bgm_shadow_realm.wav");

            stalker = logicGo.AddComponent<ShadowStalkerController>();
            stalker.monsterFrame1 = assets.UmbraMonster1;
            stalker.monsterFrame2 = assets.UmbraMonster2;
            stalker.petrifiedSprite = assets.MonsterPetrified;

            logicGo.AddComponent<PuzzleVFXManager>();

            hud = logicGo.AddComponent<PuzzleHUD>();
            hud.gameLogic = gameLogic;

            GameObject jimmyGo = new GameObject("Jimmy");
            playerMovement = jimmyGo.AddComponent<PlayerMovement>();
            playerMovement.logic = gameLogic;
            hud.player = playerMovement;

            AudioSource walkSource = jimmyGo.AddComponent<AudioSource>();
            walkSource.playOnAwake = false;
            walkSource.volume = 0.55f;
            MusicShuffler shuffler = jimmyGo.AddComponent<MusicShuffler>();
            shuffler.Jimmy = playerMovement;
            shuffler.walking = walkSource;
            shuffler.songs = new[]
            {
                LoadAudio("Assets/Sounds/Walking/walking_1.mp3"),
                LoadAudio("Assets/Sounds/Walking/walking_2.mp3")
            };

            GameObjectToggle switcher = jimmyGo.AddComponent<GameObjectToggle>();
            switcher.logic = gameLogic;

            GameObject lightForm = CreatePlayerForm("LightJimmy", jimmyGo.transform, playerStart,
                "Assets/Images/Jimmy/Light/light_down.PNG", Color.white);
            Light lightComp = lightForm.AddComponent<Light>();
            lightComp.Jimmy = playerMovement;
            lightComp.moveSpeed = 8f;

            GameObject darkForm = CreatePlayerForm("DarkJimmy", jimmyGo.transform, playerStart,
                "Assets/Images/Jimmy/Dark/dark_down.PNG", new Color(0.85f, 0.85f, 1f, 1f));
            Dark darkComp = darkForm.AddComponent<Dark>();
            darkComp.Jimmy = playerMovement;
            darkComp.moveSpeed = 6.5f;
            darkForm.SetActive(false);

            switcher.lightObject = lightForm;
            switcher.darkObject = darkForm;

            playerMovement.LightspriteRenderer = lightForm.GetComponent<SpriteRenderer>();
            playerMovement.DarkspriteRenderer = darkForm.GetComponent<SpriteRenderer>();
            playerMovement.LightspriteUp = LoadSprite("Assets/Images/Jimmy/Light/light_up.PNG");
            playerMovement.LightspriteDown = LoadSprite("Assets/Images/Jimmy/Light/light_down.PNG");
            playerMovement.LightspriteLeft = LoadSprite("Assets/Images/Jimmy/Light/light_left.PNG");
            playerMovement.LightspriteRight = LoadSprite("Assets/Images/Jimmy/Light/light_right.PNG");
            playerMovement.DarkspriteUp = LoadSprite("Assets/Images/Jimmy/Dark/dark_up.PNG");
            playerMovement.DarkspriteDown = LoadSprite("Assets/Images/Jimmy/Dark/dark_down.PNG");
            playerMovement.DarkspriteLeft = LoadSprite("Assets/Images/Jimmy/Dark/dark_left.PNG");
            playerMovement.DarkspriteRight = LoadSprite("Assets/Images/Jimmy/Dark/dark_right.PNG");
        }

        private static void CreateArenaWalls(CelAssets assets)
        {
            CreateWall("Wall_Top", new Vector3(0f, 6.1f, 0f), new Vector2(22f, 1.05f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Bottom", new Vector3(0f, -6.1f, 0f), new Vector2(22f, 1.05f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Left", new Vector3(-10.5f, 0f, 0f), new Vector2(1.0f, 13.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Wall_Right", new Vector3(10.5f, 0f, 0f), new Vector2(1.0f, 13.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
        }

        private static GameObject CreatePlayerForm(string name, Transform parent, Vector3 pos, string spritePath, Color fallbackColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.color = fallbackColor;
            sr.sortingOrder = 10;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.38f;

            return go;
        }

        private static GameObject CreateWall(string name, Vector3 pos, Vector2 size, CelAssets assets, WorldAffinity affinity, WallTileStyle style = WallTileStyle.Medium)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            Sprite lightSprite = assets.WallLightMedium;
            Sprite shadowSprite = assets.WallShadowMedium;
            switch (style)
            {
                case WallTileStyle.Long:
                    lightSprite = assets.WallLightLong;
                    shadowSprite = assets.WallShadowLong;
                    break;
                case WallTileStyle.Pillar:
                    lightSprite = assets.WallLightPillar;
                    shadowSprite = assets.WallShadowPillar;
                    break;
                case WallTileStyle.Ruined:
                    lightSprite = assets.WallLightRuined;
                    shadowSprite = assets.WallShadowRuined;
                    break;
            }

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = affinity == WorldAffinity.ShadowOnly ? shadowSprite : lightSprite;
            sr.drawMode = style == WallTileStyle.Ruined ? SpriteDrawMode.Simple : SpriteDrawMode.Tiled;
            if (sr.drawMode == SpriteDrawMode.Tiled)
            {
                sr.size = size;
            }
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = affinity;
            if (affinity == WorldAffinity.Both)
            {
                phase.lightWorldSprite = lightSprite;
                phase.shadowWorldSprite = shadowSprite;
            }
            return go;
        }

        private static LightBeamEmitter CreateEmitter(string name, Vector3 pos, LightColor color, WorldAffinity world, Vector2 dir, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EmitterActive;
            sr.color = Color.Lerp(Color.white, LightColorUtility.ToUnityColor(color), 0.25f);
            sr.sortingOrder = 4;

            LightBeamEmitter emitter = go.AddComponent<LightBeamEmitter>();
            emitter.beamColor = color;
            emitter.activeInWorld = world;
            emitter.localEmitDirection = dir;
            emitter.emitterRenderer = sr;
            emitter.activeEmitterSprite = assets.EmitterActive;
            emitter.inactiveEmitterSprite = assets.EmitterIdle;
            return emitter;
        }

        private static GameObject CreateIsometricMirror(string name, Vector3 pos, float startAngle, float stepDegrees, WorldAffinity affinity, CelAssets assets, bool canRotate = true)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MirrorDirections[0];
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = canRotate;
            mirror.rotationStepDegrees = stepDegrees;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = affinity;
            return go;
        }

        private static GameObject CreatePushableMirrorCart(string name, Vector3 pos, float startAngle, float stepDegrees, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MirrorDirections[0];
            sr.color = new Color(0.9f, 0.95f, 1f, 1f);
            sr.sortingOrder = 5;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = true;
            mirror.rotationStepDegrees = stepDegrees;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            go.AddComponent<PushableBlock>();
            return go;
        }

        private static PortalMirror CreatePortalMirror(string name, Vector3 pos, Sprite sprite, Vector2 exitDir, PortalMirror linked, bool canRotate = true)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            PortalMirror pm = go.AddComponent<PortalMirror>();
            pm.localExitDirection = exitDir;
            pm.linkedPortal = linked;
            pm.canRotateExit = canRotate;
            return pm;
        }

        private static GameObject CreateColorCombiner(string name, Vector3 pos, LightColor a, LightColor b, LightColor output, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.CombinerCrystal;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            ColorCombiner cc = go.AddComponent<ColorCombiner>();
            cc.requiredColorA = a;
            cc.requiredColorB = b;
            cc.outputColor = output;
            cc.localOutputDirection = Vector2.right;
            cc.canRotate = false;
            return go;
        }

        private static void CreateSanctuary(string name, Vector3 pos, float radius, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LanternSanctuary;
            sr.sortingOrder = 3;

            LightSanctuary sanc = go.AddComponent<LightSanctuary>();
            sanc.sanctuaryRadius = radius;
        }

        private static void CreateEchoPedestal(string name, Vector3 pos, Sprite echoSprite, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EchoPedestal;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            ShadowEchoPedestal echo = go.AddComponent<ShadowEchoPedestal>();
            echo.echoSprite = echoSprite;
        }

        private static void CreateBeamRelay(string name, Vector3 pos, BeamRelayCrystal.RelayMode mode, Vector2 outDir, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.RelayCrystal;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            BeamRelayCrystal relay = go.AddComponent<BeamRelayCrystal>();
            relay.mode = mode;
            relay.localOutputDirection = outDir;
            relay.canRotate = canRotate;
        }

        private static void CreateShadowBarrierGate(string name, Vector3 pos, Vector2 size, CelAssets assets, bool openWhenLit = true)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x / 1.28f, size.y / 1.28f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LaserBarrier;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.1f);
            col.isTrigger = true;

            ShadowBarrierGate gate = go.AddComponent<ShadowBarrierGate>();
            gate.openWhenLit = openWhenLit;
            gate.acceptAnyColor = true;
        }

        private static PhaseShiftTile CreatePhaseShiftTile(string name, Vector3 pos, PhaseShiftTile linkedPad, bool flipOnStep, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.PhasePad;
            sr.sortingOrder = 0;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.1f);
            col.isTrigger = true;

            PhaseShiftTile pad = go.AddComponent<PhaseShiftTile>();
            pad.linkedTeleportPad = linkedPad;
            pad.flipRealmOnStep = flipOnStep;
            return pad;
        }

        private static GameObject CreatePrism(string name, Vector3 pos, float zAngle, OpticalPrism.PrismMode mode, float splitAngle, bool orientAlongTransform, bool canRotate, float stepDegrees, CelAssets assets)
        {
            GameObject prismGo = new GameObject(name);
            prismGo.transform.position = pos;
            prismGo.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);
            prismGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer prismSr = prismGo.AddComponent<SpriteRenderer>();
            prismSr.sprite = assets.PrismCrystal;
            prismSr.sortingOrder = 4;

            BoxCollider2D col = prismGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            OpticalPrism prism = prismGo.AddComponent<OpticalPrism>();
            prism.mode = mode;
            prism.splitAngle = splitAngle;
            prism.orientAlongTransform = orientAlongTransform;
            prism.canRotate = canRotate;
            prism.rotationStepDegrees = stepDegrees;
            return prismGo;
        }

        private static GameObject CreateColorFilter(string name, Vector3 pos, Vector2 size, LightColor color, CelAssets assets)
        {
            GameObject filterGo = new GameObject(name);
            filterGo.transform.position = pos;

            SpriteRenderer sr = filterGo.AddComponent<SpriteRenderer>();
            sr.sprite = assets.FloorLight;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = 3;

            BoxCollider2D col = filterGo.AddComponent<BoxCollider2D>();
            col.size = size;

            ColorFilter filter = filterGo.AddComponent<ColorFilter>();
            filter.filterColor = color;
            return filterGo;
        }

        private static LightSensor CreateSensor(string name, Vector3 pos, LightColor color, WorldAffinity requiredWorld, CelAssets assets, bool acceptAny = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = requiredWorld == WorldAffinity.ShadowOnly ? assets.ReceiverInactiveDark : assets.ReceiverInactive;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            LightSensor sensor = go.AddComponent<LightSensor>();
            sensor.requiredColor = color;
            sensor.acceptAnyColor = acceptAny;
            sensor.requiredWorld = requiredWorld;
            sensor.indicatorRenderer = sr;
            sensor.inactiveSprite = requiredWorld == WorldAffinity.ShadowOnly ? assets.ReceiverInactiveDark : assets.ReceiverInactive;
            sensor.activeSprite = assets.ReceiverActiveGlow;
            return sensor;
        }

        private static PuzzleLever CreateLever(string name, Vector3 pos, CelAssets assets, WorldAffinity usableWorld,
            OpticalMirror[] controlledMirrors = null, GameObject[] toggledObjects = null)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LeverOff;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            PuzzleLever lever = go.AddComponent<PuzzleLever>();
            lever.usableInWorld = usableWorld;
            lever.leverRenderer = sr;
            lever.leverOffSprite = assets.LeverOff;
            lever.leverOnSprite = assets.LeverOn;
            lever.controlledMirrors = controlledMirrors;
            lever.toggledObjects = toggledObjects;
            return lever;
        }

        private static PuzzleDoor CreateDoor(string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers, bool requireAll, bool stayOpen)
        {
            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;
            doorGo.transform.localScale = new Vector3(size.x / 1.28f, size.y / 1.60f, 1f);

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = assets.DoorClosed;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.15f, 1.5f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = assets.DoorClosed;
            door.openSprite = assets.DoorOpen;
            return door;
        }

        private static GameObject CreatePushableBlock(string name, Vector3 pos, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EmitterBroken;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.85f);

            go.AddComponent<PushableBlock>();
            return go;
        }

        private static PressurePlate CreatePressurePlate(string name, Vector3 pos, CelAssets assets, WorldAffinity form, bool latch = false)
        {
            GameObject plateGo = new GameObject(name);
            plateGo.transform.position = pos;
            plateGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            SpriteRenderer plateSr = plateGo.AddComponent<SpriteRenderer>();
            plateSr.sprite = assets.ButtonUp;
            plateSr.sortingOrder = -1;

            BoxCollider2D col = plateGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.15f, 1.15f);
            col.isTrigger = true;

            PressurePlate plate = plateGo.AddComponent<PressurePlate>();
            plate.acceptedForm = form;
            plate.latchOncePressed = latch;
            plate.plateRenderer = plateSr;
            plate.unpressedSprite = assets.ButtonUp;
            plate.pressedSprite = assets.ButtonDown;
            return plate;
        }

        private static GameObject CreateExit(string name, Vector3 pos, CelAssets assets, PuzzleDoor[] requiredDoors, string nextScene)
        {
            GameObject exitGo = new GameObject(name);
            exitGo.transform.position = pos;
            exitGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

            SpriteRenderer exitSr = exitGo.AddComponent<SpriteRenderer>();
            exitSr.sprite = assets.ReceiverActiveWhite;
            exitSr.color = new Color(0.45f, 1f, 0.72f, 1f);
            exitSr.sortingOrder = 1;

            BoxCollider2D col = exitGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.0f);
            col.isTrigger = true;

            LevelExit exit = exitGo.AddComponent<LevelExit>();
            exit.requiredDoors = requiredDoors;
            exit.nextSceneName = nextScene;
            return exitGo;
        }

        private static bool IsInPlayModeGuard()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return false;
            EditorUtility.DisplayDialog("Stop Play Mode", "Please exit Play Mode before generating puzzle scenes.", "OK");
            return true;
        }

        private static void ConfigureCelShadedSprites()
        {
            ConfigureSpriteFolder(CelDir);
            ConfigureSpriteFolder("Assets/Resources/Sprites/");
            AssetDatabase.Refresh();
        }

        private static void ConfigureSpriteFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return;
            string[] files = Directory.GetFiles(folderPath, "*.png");
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                if (assetPath.EndsWith("CelShaded_SpriteSheet.png")) continue;
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (Mathf.Abs(importer.spritePixelsPerUnit - 100f) > 0.1f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    changed = true;
                }
                if (importer.wrapMode != TextureWrapMode.Repeat)
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    changed = true;
                }

                TextureImporterSettings settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                if (settings.spriteMeshType != SpriteMeshType.FullRect)
                {
                    settings.spriteMeshType = SpriteMeshType.FullRect;
                    importer.SetTextureSettings(settings);
                    changed = true;
                }

                if (changed) importer.SaveAndReimport();
            }
        }

        private static CelAssets LoadCelAssets()
        {
            ConfigureCelShadedSprites();
            return new CelAssets
            {
                EmitterIdle = LoadSprite(CelDir + "Emitter_Idle.png"),
                EmitterActive = LoadSprite(CelDir + "Emitter_Active.png"),
                EmitterBroken = LoadSprite(CelDir + "Emitter_Broken.png"),
                MirrorDirections = new[]
                {
                    LoadSprite(CelDir + "Mirror_45.png"),
                    LoadSprite(CelDir + "Mirror_135.png"),
                    LoadSprite(CelDir + "Mirror_225.png"),
                    LoadSprite(CelDir + "Mirror_315.png")
                },
                ReceiverInactive = LoadSprite(CelDir + "Receiver_Inactive.png"),
                ReceiverActiveWhite = LoadSprite(CelDir + "Receiver_Active_White.png"),
                ReceiverInactiveDark = LoadSprite(CelDir + "Receiver_Inactive_Dark.png"),
                ReceiverActiveGlow = LoadSprite(CelDir + "Receiver_Active_Glow.png"),
                WallLightLong = LoadSprite(CelDir + "Wall_Light_Long.png"),
                WallLightMedium = LoadSprite(CelDir + "Wall_Light_Medium.png"),
                WallLightPillar = LoadSprite(CelDir + "Wall_Light_Pillar.png"),
                WallLightRuined = LoadSprite(CelDir + "Wall_Light_Ruined.png"),
                WallShadowLong = LoadSprite(CelDir + "Wall_Shadow_Long.png"),
                WallShadowMedium = LoadSprite(CelDir + "Wall_Shadow_Medium.png"),
                WallShadowPillar = LoadSprite(CelDir + "Wall_Shadow_Pillar.png"),
                WallShadowRuined = LoadSprite(CelDir + "Wall_Shadow_Ruined.png"),
                DoorClosed = LoadSprite(CelDir + "Door_Closed.png"),
                DoorOpen = LoadSprite(CelDir + "Door_Open.png"),
                LeverOff = LoadSprite(CelDir + "Lever_Off.png"),
                LeverOn = LoadSprite(CelDir + "Lever_On.png"),
                ButtonUp = LoadSprite(CelDir + "Button_Up.png"),
                ButtonDown = LoadSprite(CelDir + "Button_Down.png"),
                FloorLight = LoadSprite(CelDir + "Floor_Light.png"),
                FloorShadow = LoadSprite(CelDir + "Floor_Shadow.png"),
                PrismCrystal = LoadSprite(CelDir + "Prism_Crystal.png"),
                UmbraMonster1 = LoadSprite(CelDir + "UmbraMonster_1.png"),
                UmbraMonster2 = LoadSprite(CelDir + "UmbraMonster_2.png"),
                MonsterPetrified = LoadSprite(CelDir + "Monster_Petrified.png"),
                PortalMirrorA = LoadSprite(CelDir + "PortalMirror_A.png"),
                PortalMirrorB = LoadSprite(CelDir + "PortalMirror_B.png"),
                CombinerCrystal = LoadSprite(CelDir + "Combiner_Crystal.png"),
                LanternSanctuary = LoadSprite(CelDir + "Lantern_Sanctuary.png"),
                EchoPedestal = LoadSprite(CelDir + "Echo_Pedestal.png"),
                RelayCrystal = LoadSprite(CelDir + "Relay_Crystal.png"),
                LaserBarrier = LoadSprite(CelDir + "Laser_Barrier.png"),
                PhasePad = LoadSprite(CelDir + "Phase_Pad.png")
            };
        }

        private static void RegisterScenesInBuildSettings()
        {
            string[] managed = { MainMenuPath, TutorialPath, Level1Path, Level2Path, Level3Path, Level4Path, Level5Path, Level6Path };
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in managed) AddSceneIfMissing(scenes, path);
            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (!File.Exists(existing.path)) continue;
                bool isManaged = false;
                foreach (string m in managed) if (existing.path == m) { isManaged = true; break; }
                if (!isManaged) scenes.Add(existing);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
        {
            if (!File.Exists(path)) return;
            foreach (var s in scenes)
            {
                if (s.path == path) return;
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

        private static Sprite LoadSprite(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
        private static AudioClip LoadAudio(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
