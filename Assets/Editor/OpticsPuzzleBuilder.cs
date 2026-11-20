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
        private const string LabDir = "Assets/Images/Laboratory/";
        private const string ScifiDir = "Assets/Images/ScifiFacility/";
        private const string NewDataDir = "Assets/Images/NewData/";
        private const string PixelNewDir = "Assets/Images/PixelNew/";
        private const string SfxDir = "Assets/Sounds/SFX/";
        private const string MainMenuPath = "Assets/Scenes/MainMenu.unity";
        private const string TutorialPath = "Assets/Scenes/OpticsPuzzle_Tutorial.unity";
        private const string Level1Path = "Assets/Scenes/OpticsPuzzle.unity";
        private const string Level2Path = "Assets/Scenes/OpticsPuzzle_Level2.unity";
        private const string Level3Path = "Assets/Scenes/OpticsPuzzle_Level3.unity";
        private const string Level4Path = "Assets/Scenes/OpticsPuzzle_Level4.unity";
        private const string Level5Path = "Assets/Scenes/OpticsPuzzle_Level5.unity";
        private const string Level6Path = "Assets/Scenes/OpticsPuzzle_Level6.unity";
        private const string Level7Path = "Assets/Scenes/OpticsPuzzle_Level7.unity";
        private const string Level8Path = "Assets/Scenes/OpticsPuzzle_Level8.unity";
        private const string Level9Path = "Assets/Scenes/OpticsPuzzle_Level9.unity";
        private const string Level10Path = "Assets/Scenes/OpticsPuzzle_Level10.unity";
        private const string Level11Path = "Assets/Scenes/OpticsPuzzle_Level11.unity";
        private const string Level12Path = "Assets/Scenes/OpticsPuzzle_Level12.unity";
        private const string SandboxPath = "Assets/Scenes/OpticsPuzzle_Sandbox.unity";

        private enum WallTileStyle { Long, Medium, Pillar, Ruined }

        private struct CelAssets
        {
            public Sprite EmitterIdle, EmitterActive, EmitterBroken, PushableBlockSprite;
            public Sprite[] MirrorDirections;
            public Sprite ReceiverInactive, ReceiverActiveWhite, ReceiverInactiveDark, ReceiverActiveGlow;
            public Sprite WallLightLong, WallLightMedium, WallLightPillar, WallLightRuined;
            public Sprite WallShadowLong, WallShadowMedium, WallShadowPillar, WallShadowRuined;
            public Sprite DoorClosed, DoorOpen, LeverOff, LeverOn, ButtonUp, ButtonDown;
            public Sprite FloorLight, FloorShadow, PrismCrystal;
            public Sprite UmbraMonster1, UmbraMonster2, MonsterPetrified;
            public Sprite PortalMirrorA, PortalMirrorB, CombinerCrystal, LanternSanctuary, EchoPedestal;
            public Sprite RelayCrystal, LaserBarrier, PhasePad, ColorFilterLens, ExitPortal;

            // LandOfPixels Laboratory Assets
            public Sprite LabFloorLight, LabFloorShadow, LabFloorGrating, LabFloorHazard;
            public Sprite LabWallLightLong, LabWallLightWindow, LabWallLightPillar;
            public Sprite LabWallShadowLong, LabWallShadowWindow, LabWallShadowPillar, LabWallPipes;
            public Sprite LabBlastDoorClosed, LabBlastDoorOpen, LabBlastDoorRedClosed, LabBlastDoorRedOpen;
            public Sprite LabHazardDoorClosed, LabKeypadDoorClosed;
            public Sprite LabButtonUp, LabButtonDown, LabConsoleOff, LabConsoleOn;
            public Sprite LabTeslaTower, LabBioCapsule, LabMonitor, LabServerRack, LabWorkbench, LabShelves;
            public Sprite[] LabBulbs, LabLiquidUmbra, LabLiquidAcid, LabLiquidCryo;
            public Sprite[] LabNpcMale, LabNpcFemale, LabNpcHazmat; // [Down, Up, Left, Right]

            // Sci-Fi Facility Builder Pack 1 Assets
            public Sprite ScifiFloorLight, ScifiFloorShadow, ScifiFloorGrated;
            public Sprite ScifiWallLightLong, ScifiWallLightMedium, ScifiWallLightPillar;
            public Sprite ScifiWallShadowLong, ScifiWallShadowMedium, ScifiWallShadowPillar;
            public Sprite ScifiDoorLabClosed, ScifiDoorLabOpen, ScifiDoorStdClosed, ScifiDoorStdOpen;
            public Sprite ScifiThresholdH, ScifiThresholdV, ScifiDoorMarking;
            public Sprite ScifiElecBoxClosed, ScifiElecBoxOpen, ScifiHatchButtonUp, ScifiHatchButtonDown, ScifiCratePushable;
            public Sprite ScifiBenchFilled, ScifiBenchSide, ScifiShelfFilled1, ScifiShelfFilled2, ScifiCrateGroup1;
            public Sprite ScifiPipesDouble, ScifiPipesTriple;
            public Sprite ScifiAlarmOff, ScifiAlarmOn, ScifiAlarmGlow, ScifiCameraL, ScifiCameraR;
            public Sprite ScifiSignBiohazard, ScifiSignRadiation, ScifiSignCaution, ScifiSignDoNotEnter;
            public Sprite ScifiSignDirExit, ScifiSignDirLab, ScifiSignDirCtrl;
            public Sprite ScifiLabelLab, ScifiLabelCtrl, ScifiLabelMed, ScifiLabelExit;
            public Sprite ScifiArrowR, ScifiArrowL, ScifiArrowUp, ScifiArrowDown;
            public Sprite ScifiBlood1, ScifiBlood2, ScifiHorrorDoorGlow;

            // New data (Animated Flying Obelisk + PixelLabIcons)
            public Sprite[] ObeliskFrames;
            public Sprite IconAtom, IconBattery, IconBulb, IconFlask, IconMagnet, IconTestTube;

            // PixelNew expansion assets
            public Sprite OneWayPolarizer, RealityAnchorOff, RealityAnchorOn, MagneticRailTrack, MagneticRailCart;
            public Sprite SplitTotem, MemoryPrismIdle, MemoryPrismCharged, MimicMirrorIdle, MimicMirrorAwake;
            public Sprite BeamMoth1, BeamMoth2, UmbraMoldTile, UmbraWarden1, UmbraWarden2;
        }

        [MenuItem("Shadow Flip/Create All Levels (Main Menu + Levels 0-12 + Sandbox)")]
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
            CreateOpticsPuzzleLevel7();
            CreateOpticsPuzzleLevel8();
            CreateOpticsPuzzleLevel9();
            CreateOpticsPuzzleLevel10();
            CreateOpticsPuzzleLevel11();
            CreateOpticsPuzzleLevel12();
            CreateSandboxLevel();
            CreateMainMenuScene();
            RegisterScenesInBuildSettings();
            Debug.Log("Created Main Menu, Levels 0-12, and Sandbox Level and registered all 15 in Build Settings.");
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

            // Laboratory showcase props & NPC in perimeter corners
            CreateLabProp("Menu_LabMonitor", new Vector3(-8.2f, 4.4f, 0f), assets.LabMonitor, 1f, 3);
            CreateLabProp("Menu_LabBioCapsule", new Vector3(-8.4f, -4.4f, 0f), assets.LabBioCapsule, 0.9f, 3);
            CreateLabProp("Menu_LabServerRack", new Vector3(-6.4f, 4.4f, 0f), assets.LabServerRack, 0.9f, 3);
            CreateLabNPC("Menu_DrVance", new Vector3(-6.2f, -4.4f, 0f), "Dr. Vance",
                new[] { "Welcome to Shadow Flip's Quantum Optics Lab!" },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

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

        [MenuItem("Shadow Flip/Create Level 0 (Tutorial)")]
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
            hud.lessonTitle = "Level 0 (Tutorial): Basics of Shadow & Light";
            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_Tut1", new Vector3(-8.7f, -4.8f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_Tut2", new Vector3(8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateLabProp("LabMonitor_Tut", new Vector3(-7.0f, -5.0f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("LabBioCapsule_Tut", new Vector3(8.8f, 2.8f, 0f), assets.LabBioCapsule, 0.85f, 3, hasCollider: true);
            CreateLabNPC("Dr_Elena_Tut", new Vector3(-8.4f, -0.3f, 0f), "Dr. Elena",
                new[]
                {
                    "Rotate the mirror [E] at the top to hit the White Crystal, then pull the lever below it!",
                    "Notice the glowing phase wall in the center? Press [F] to flip to the Shadow Realm and pass through!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

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

        [MenuItem("Shadow Flip/Create Level 1 (Chromatic Vault)")]
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

            CreateLabProp("LabMonitor_L1", new Vector3(-7.0f, 4.7f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("LabServerRack_L1", new Vector3(8.7f, -4.4f, 0f), assets.LabServerRack, 0.85f, 3, hasCollider: true);
            CreateLabNPC("Dr_Vance_L1", new Vector3(-7.5f, -0.8f, 0f), "Dr. Vance",
                new[]
                {
                    "Some mirrors are locked to a specific realm! Align the gold Mirror_Red1 while in Light Realm.",
                    "Flip [F] to Shadow Realm after pulling the Green Shutter Lever so the Green beam can hit its Shadow Mirror!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

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

        [MenuItem("Shadow Flip/Create Level 2 (Eclipse Temple)")]
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
            CelAssets assets = WithLaboratoryTheme(LoadCelAssets());
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

            // Laboratory interior props & interactive Dr. Vance in safe West Wing corners
            CreateLabProp("Lab_BioCapsule_L5", new Vector3(-8.8f, 4.4f, 0f), assets.LabBioCapsule, 0.95f, 3, hasCollider: true);
            CreateLabProp("Lab_ServerRack_L5", new Vector3(-7.4f, 4.4f, 0f), assets.LabServerRack, 0.95f, 3, hasCollider: true);
            CreateLabProp("Lab_Monitor_L5", new Vector3(-5.8f, 4.6f, 0f), assets.LabMonitor, 0.9f, 3, hasCollider: true);
            CreateLabProp("Lab_Workbench_L5", new Vector3(-8.2f, -0.5f, 0f), assets.LabWorkbench, 0.95f, 3, hasCollider: true);
            CreateAnimatedLabProp("Lab_CryoVat_L5", new Vector3(-5.8f, -4.9f, 0f), assets.LabLiquidCryo, 0.85f, -18, 4f);
            CreateLabNPC("Dr_Vance_L5", new Vector3(-7.5f, 2.3f, 0f), "Dr. Vance",
                new[]
                {
                    "This wing houses our Flip-Clock Turret! Every time you press [F], the turret steps 90 degrees clockwise.",
                    "Aim it into the Relay Crystal to split power to both sensors and drop the Forcefield!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

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

            PuzzleDoor door = CreateLabBlastDoor("PuzzleDoor_Matrix", new Vector3(6.5f, -2.5f, 0f), new Vector2(1.1f, 2.4f), assets,
                new[] { topSensor, rightSensor }, new[] { vaultButton }, null, requireAll: true, stayOpen: false, vertical: true, redVariant: false);

            CreateExit("LevelExit", new Vector3(8.6f, -2.5f, 0f), assets, new[] { door }, "OpticsPuzzle_Level6");

            EditorSceneManager.SaveScene(scene, Level5Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 5 at " + Level5Path);
        }

        [MenuItem("Shadow Flip/Create Level 6 (Grandmaster Facility)")]
        public static void CreateOpticsPuzzleLevel6()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithScifiFacilityTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.04f, 0.04f, 0.09f, 1f),
                new Color(0.96f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.28f, 0.18f, 0.44f, 1f), 0.46f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 11.0f;
            hud.parFlips = 5;
            hud.lessonTitle = "Level 6: Grandmaster Facility";
            hud.levelHint = "Free the Pushable Mirror Cart via the Forcefield, warp the beam across the wall, charge the 7s Capacitor Crystal in Light Realm, then flip [F] so the Realm Inverter converts the beam to Shadow!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_NorthWest", new Vector3(-7.5f, 3.5f, 0f), 2.2f, assets);

            // Sci-Fi Facility Perimeter Props, Signage, Security & Shadow-Only Horror Decals
            CreateLabProp("Scifi_Label_Ctrl_L6", new Vector3(-6.2f, 5.4f, 0f), assets.ScifiLabelCtrl, 1.0f, 3);
            CreateLabProp("Scifi_Sign_Biohazard_L6", new Vector3(-3.8f, 5.4f, 0f), assets.ScifiSignBiohazard, 0.9f, 3);
            CreateLabProp("Scifi_Sign_DirExit_L6", new Vector3(5.0f, 5.4f, 0f), assets.ScifiSignDirExit, 1.0f, 3);
            CreateLabProp("Scifi_Camera_L_L6", new Vector3(-9.3f, 5.3f, 0f), assets.ScifiCameraL, 2.0f, 4);
            CreateLabProp("Scifi_Camera_R_L6", new Vector3(9.3f, 5.3f, 0f), assets.ScifiCameraR, 2.0f, 4);
            CreateScifiAlarmBeacon("Scifi_Alarm_West_L6", new Vector3(-1.2f, 5.4f, 0f), assets);
            CreateScifiAlarmBeacon("Scifi_Alarm_East_L6", new Vector3(3.2f, 5.4f, 0f), assets);

            CreateLabProp("Scifi_Bench_L6", new Vector3(8.6f, 4.3f, 0f), assets.ScifiBenchFilled, 0.95f, 3, hasCollider: true);
            CreateLabProp("Scifi_Shelf_L6", new Vector3(8.6f, -4.3f, 0f), assets.ScifiShelfFilled1, 1.0f, 3, hasCollider: true);
            CreateShadowHorrorDecal("Horror_Blood_L6_A", new Vector3(-3.8f, -1.8f, 0f), assets.ScifiBlood1, 1.4f, -15);
            CreateShadowHorrorDecal("Horror_Blood_L6_B", new Vector3(5.2f, 0.8f, 0f), assets.ScifiBlood2, 1.3f, -15);
            CreateShadowHorrorDecal("Horror_DoorGlow_L6", new Vector3(6.5f, 0.8f, 0f), assets.ScifiHorrorDoorGlow, 0.52f, -14);

            CreateLabNPC("Dr_Elena_L6", new Vector3(-5.8f, -1.8f, 0f), "Dr. Elena",
                new[]
                {
                    "The Realm Inverter Relay flips any beam into Shadow Energy whenever you shift into the Shadow Realm!",
                    "Charge the 7-second Light Capacitor first, then flip [F] and step beside the Vault Door. Stand near the Tesla Coil to discharge Umbra!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

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

            // Grandmaster Vault at x = 6.5 using Sci-Fi Lab Sliding Door + Threshold
            CreateWall("VaultWall_Upper", new Vector3(6.5f, 3.9f, 0f), new Vector2(0.85f, 4.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -3.2f, 0f), new Vector2(0.85f, 5.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor grandDoor = CreateScifiDoor("PuzzleDoor_Grandmaster", new Vector3(6.5f, 0.8f, 0f), new Vector2(1.1f, 2.4f), assets,
                new[] { lightCapSensor, shadowFinalSensor }, null, null, requireAll: true, stayOpen: false, isLabDoor: true);

            CreateExit("LevelExit", new Vector3(8.6f, 0.8f, 0f), assets, new[] { grandDoor }, "OpticsPuzzle_Level7");

            EditorSceneManager.SaveScene(scene, Level6Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 6 at " + Level6Path);
        }

        [MenuItem("Shadow Flip/Create Level 7 (Quantum Laboratory)")]
        public static void CreateOpticsPuzzleLevel7()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithLaboratoryTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.04f, 0.05f, 0.11f, 1f),
                new Color(0.95f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.26f, 0.22f, 0.56f, 1f), 0.48f,
                new Vector3(-8.2f, -3.5f, 0f));

            stalker.umbraSafeDuration = 11.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 7: Quantum Laboratory";
            hud.levelHint = "Light Realm: talk [E] to Dr. Vance to lower the shutter & rotate Mirror_Red [E] to open the Red Blast Door. Flip [F] to cross the Phase Gate, talk [E] to Unit-7 in Shadow Realm, push the Crate onto the Button & pull the Console!";

            CreateArenaWalls(assets);

            // Floor accents: Grating corridor & Hazard airlock zone (visual layer above main floor, below objects)
            CreateFloorAccent("LabFloor_GratingAisle", new Vector3(1.5f, 0.0f, 0f), new Vector2(3.6f, 9.6f), assets.LabFloorGrating, -18);
            CreateFloorAccent("LabFloor_HazardAirlock", new Vector3(6.75f, 0.0f, 0f), new Vector2(2.6f, 2.8f), assets.LabFloorHazard, -17);

            // Tesla Coil Sanctuary in NW corner
            CreateLabSanctuary("Sanctuary_Tesla", new Vector3(-7.8f, 3.8f, 0f), 2.2f, assets);

            // Laboratory perimeter props, bubbling vats, & Sci-Fi facility wall instrumentation
            CreateLabProp("Lab_Pipes_West", new Vector3(-8.6f, 2.5f, 0f), assets.LabWallPipes, 0.72f, 2);
            CreateLabProp("Lab_BioCapsule_NW", new Vector3(-6.2f, 4.6f, 0f), assets.LabBioCapsule, 0.85f, 3, hasCollider: true);
            CreateLabProp("Lab_Workbench_N", new Vector3(-3.0f, 4.9f, 0f), assets.LabWorkbench, 0.95f, 3, hasCollider: true);
            CreateLabProp("Lab_Monitor_SW", new Vector3(-6.8f, -5.0f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("Lab_ServerRack_NE", new Vector3(8.9f, 4.3f, 0f), assets.LabServerRack, 0.9f, 3, hasCollider: true);
            CreateLabProp("Lab_Shelves_SE", new Vector3(8.8f, -4.4f, 0f), assets.LabShelves, 0.85f, 3, hasCollider: true);
            CreateLabProp("Lab_BioCapsule_Vault", new Vector3(5.0f, 4.6f, 0f), assets.LabBioCapsule, 0.85f, 3, hasCollider: true);
            CreateAnimatedLabProp("Lab_Vat_Umbra", new Vector3(3.8f, 3.8f, 0f), assets.LabLiquidUmbra, 1.35f, -16, 4.5f);
            CreateAnimatedLabProp("Lab_Vat_Acid", new Vector3(-9.1f, -4.9f, 0f), assets.LabLiquidAcid, 0.85f, -16, 4.0f);
            CreateAnimatedLabProp("Lab_Vat_Cryo", new Vector3(3.8f, -5.0f, 0f), assets.LabLiquidCryo, 0.85f, -16, 4.0f);

            CreateScifiAlarmBeacon("Scifi_Alarm_West_L7", new Vector3(-4.0f, 5.5f, 0f), assets);
            CreateScifiAlarmBeacon("Scifi_Alarm_East_L7", new Vector3(2.4f, 5.5f, 0f), assets);
            CreateLabProp("Scifi_Label_Lab_L7", new Vector3(-1.2f, 5.5f, 0f), assets.ScifiLabelLab, 1.0f, 3);
            CreateLabProp("Scifi_Sign_Rad_L7", new Vector3(0.4f, 5.5f, 0f), assets.ScifiSignRadiation, 0.85f, 3);
            CreateLabProp("Scifi_Sign_DirExit_L7", new Vector3(6.75f, 5.5f, 0f), assets.ScifiSignDirExit, 1.0f, 3);
            CreateLabProp("Scifi_Camera_L_L7", new Vector3(-9.3f, 5.3f, 0f), assets.ScifiCameraL, 2.0f, 4);
            CreateLabProp("Scifi_Camera_R_L7", new Vector3(9.3f, 5.3f, 0f), assets.ScifiCameraR, 2.0f, 4);

            // Central Lab Partition Wall (x = -1.2) with a LightOnly Phase Gate in the middle
            CreateWall("LabPartition_Top", new Vector3(-1.2f, 3.8f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("LabPartition_PhaseGate", new Vector3(-1.2f, 0.0f, 0f), new Vector2(0.85f, 3.2f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
            CreateWall("LabPartition_Bot", new Vector3(-1.2f, -3.8f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);

            // West Wing Optics (Light Realm): Emitter -> RGB Prism -> Green (Shutter) + Red (Mirror_Red -> Sensor_Red)
            CreateEmitter("Emitter_Lab_White", new Vector3(-8.8f, 1.5f, 0f), LightColor.White, WorldAffinity.LightOnly, Vector2.right, assets);
            CreatePrism("Prism_Lab_RGB", new Vector3(-4.8f, 1.5f, 0f), 0f, OpticalPrism.PrismMode.DisperseRGB, 90f, false, false, 90f, assets);

            GameObject greenShutter = CreateWall("Lab_Containment_Shutter", new Vector3(-3.3f, 1.5f, 0f), new Vector2(0.75f, 1.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            LightSensor greenSensor = CreateSensor("Sensor_Lab_Green", new Vector3(-2.3f, 1.5f, 0f), LightColor.Green, WorldAffinity.LightOnly, assets);

            CreateIsometricMirror("Mirror_Lab_Red", new Vector3(-4.8f, -3.5f, 0f), 45f, 90f, WorldAffinity.LightOnly, assets);
            LightSensor redSensor = CreateSensor("Sensor_Lab_Red", new Vector3(-2.5f, -3.5f, 0f), LightColor.Red, WorldAffinity.LightOnly, assets);

            // NPC 1: Dr. Vance in West Wing (first talk opens the Green Containment Shutter!)
            LabNPCController drVance = CreateLabNPC("Dr_Vance_L7", new Vector3(-7.8f, -1.2f, 0f), "Dr. Vance",
                new[]
                {
                    "Access granted! I've lowered the optical Containment Shutter so the Green beam can charge its receiver.",
                    "Align Mirror_Red with [E] to unlock the Red Airlock Door, then flip [F] to phase past the center gate!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets, usableWorld: WorldAffinity.Both);
            drVance.toggledLabBarriers = new[] { greenShutter };
            drVance.triggerEquipmentOnce = true;

            // East Wing Shadow Cryo Optics (Shadow Realm): Emitter_Cryo -> Locked Bio-Pod Mirror -> Sensor_Shadow_Vault
            CreateEmitter("Emitter_Cryo_Shadow", new Vector3(0.2f, 3.8f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            GameObject cryoMirrorGo = CreateIsometricMirror("Mirror_Cryo_Locked", new Vector3(3.8f, 3.8f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets, canRotate: false);
            OpticalMirror cryoMirror = cryoMirrorGo.GetComponent<OpticalMirror>();
            LightSensor shadowSensor = CreateSensor("Sensor_Cryo_Shadow", new Vector3(3.8f, 0.2f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            // NPC 2: Hazmat Tech Unit-7 in East Wing (Shadow Realm Only! Rotates Mirror_Cryo_Locked from 45 -> 135)
            LabNPCController unit7 = CreateLabNPC("Hazmat_Unit7_L7", new Vector3(1.5f, 1.8f, 0f), "Hazmat Unit-7",
                new[]
                {
                    "Remote calibration complete! Bio-Pod Cryo Mirror locked onto the Shadow receiver below.",
                    "Push the heavy crate onto the floor button and pull the Security Console to unseal the Blue Blast Door!"
                },
                assets.LabNpcHazmat, assets.LabNpcHazmat, assets, usableWorld: WorldAffinity.ShadowOnly);
            unit7.assistedMirrors = new[] { cryoMirror };
            unit7.triggerEquipmentOnce = true;

            // East Wing Pushable Crate + Lab Button + Security Console
            CreatePushableBlock("Lab_EnergyCrate", new Vector3(1.5f, -2.5f, 0f), assets);
            PressurePlate eastButton = CreatePressurePlate("Lab_Button_East", new Vector3(4.2f, -2.5f, 0f), assets, WorldAffinity.Both, latch: false);
            PuzzleLever eastConsole = CreateLever("Lab_Console_East", new Vector3(2.0f, -4.6f, 0f), assets, WorldAffinity.Both);

            // Double Blast-Door Airlock at x = 6.1 (Red Blast Door, stayOpen) and x = 7.4 (Blue Blast Door)
            CreateWall("Airlock1_Upper", new Vector3(6.1f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Airlock1_Lower", new Vector3(6.1f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor redAirlock = CreateLabBlastDoor("LabBlastDoor_Red", new Vector3(6.1f, 0.0f, 0f), new Vector2(0.95f, 2.8f), assets,
                new[] { greenSensor, redSensor }, null, null, requireAll: true, stayOpen: true, vertical: true, redVariant: true);

            CreateWall("Airlock2_Upper", new Vector3(7.4f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Airlock2_Lower", new Vector3(7.4f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor blueAirlock = CreateLabBlastDoor("LabBlastDoor_Blue", new Vector3(7.4f, 0.0f, 0f), new Vector2(0.95f, 2.8f), assets,
                new[] { shadowSensor }, new[] { eastButton }, new[] { eastConsole }, requireAll: true, stayOpen: true, vertical: true, redVariant: false);

            CreateExit("LevelExit", new Vector3(9.0f, 0.0f, 0f), assets, new[] { redAirlock, blueAirlock }, "OpticsPuzzle_Level8");

            EditorSceneManager.SaveScene(scene, Level7Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 7 (Quantum Laboratory) at " + Level7Path);
        }

        [MenuItem("Shadow Flip/Create Level 8 (Omega Containment Facility)")]
        public static void CreateOpticsPuzzleLevel8()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithScifiFacilityTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.03f, 0.04f, 0.08f, 1f),
                new Color(0.96f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.24f, 0.16f, 0.42f, 1f), 0.44f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 11.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 8: Omega Containment Facility";
            hud.levelHint = "Flip the Electrical Breaker [E] to lower the Containment Shutter, flip [F] to rotate the Turret & phase through the Security Gate, align Mirror_East_Aux [E], and push the Storage Crate onto the Floor Hatch!";

            CreateArenaWalls(assets);

            // Grated Engineering Walkway & Directional Floor Arrows
            CreateFloorAccent("ScifiFloor_Engineering", new Vector3(3.2f, 0.0f, 0f), new Vector2(4.0f, 10.4f), assets.ScifiFloorGrated, -18);
            CreateLabProp("FloorArrow_Gate", new Vector3(-2.6f, -2.5f, 0f), assets.ScifiArrowR, 1.8f, -15);
            CreateLabProp("FloorArrow_Airlock", new Vector3(4.8f, 0.0f, 0f), assets.ScifiArrowR, 1.8f, -15);

            // Tesla Sanctuary in NW corner
            CreateLabSanctuary("Sanctuary_Tesla_L8", new Vector3(-8.0f, 3.8f, 0f), 2.2f, assets);

            // Wall Mounting Props: Pipes, Cameras, Room Labels, Warning Signs, Pulsing Emergency Beacons
            CreateLabProp("Pipes_NorthWest", new Vector3(-3.2f, 4.9f, 0f), assets.ScifiPipesDouble, 0.95f, 2);
            CreateLabProp("Pipes_NorthEast", new Vector3(8.2f, 4.9f, 0f), assets.ScifiPipesTriple, 0.95f, 2);
            CreateLabProp("Camera_NW", new Vector3(-9.3f, 5.3f, 0f), assets.ScifiCameraL, 2.0f, 4);
            CreateLabProp("Camera_NE", new Vector3(9.3f, 5.3f, 0f), assets.ScifiCameraR, 2.0f, 4);

            CreateLabProp("Label_Lab", new Vector3(-6.4f, 5.4f, 0f), assets.ScifiLabelLab, 1.0f, 3);
            CreateLabProp("Label_Ctrl", new Vector3(-1.2f, 5.4f, 0f), assets.ScifiLabelCtrl, 1.0f, 3);
            CreateLabProp("Label_Med", new Vector3(0.8f, 5.4f, 0f), assets.ScifiLabelMed, 1.0f, 3);
            CreateLabProp("Label_Exit", new Vector3(6.85f, 5.4f, 0f), assets.ScifiLabelExit, 1.0f, 3);

            CreateLabProp("Sign_Biohazard", new Vector3(-4.4f, 5.4f, 0f), assets.ScifiSignBiohazard, 0.85f, 3);
            CreateLabProp("Sign_Radiation", new Vector3(3.6f, 5.4f, 0f), assets.ScifiSignRadiation, 0.85f, 3);
            CreateLabProp("Sign_Caution", new Vector3(4.9f, 5.4f, 0f), assets.ScifiSignCaution, 0.85f, 3);
            CreateLabProp("Sign_DirExit", new Vector3(4.9f, 2.4f, 0f), assets.ScifiSignDirExit, 1.0f, 3);

            CreateScifiAlarmBeacon("Scifi_Alarm_West_L8", new Vector3(-6.5f, 5.5f, 0f), assets);
            CreateScifiAlarmBeacon("Scifi_Alarm_Center_L8", new Vector3(2.2f, 5.5f, 0f), assets);
            CreateScifiAlarmBeacon("Scifi_Alarm_Vault_L8", new Vector3(8.6f, 5.5f, 0f), assets);

            // Freestanding Facility Furniture & Storage (clear of all beams and push paths!)
            CreateLabProp("Bench_EastNW", new Vector3(0.5f, 4.6f, 0f), assets.ScifiBenchFilled, 0.9f, 3, hasCollider: true);
            CreateLabProp("Shelf_EastNE", new Vector3(4.6f, 4.6f, 0f), assets.ScifiShelfFilled1, 1.0f, 3, hasCollider: true);
            CreateLabProp("Shelf_VaultNE", new Vector3(8.8f, 4.5f, 0f), assets.ScifiShelfFilled2, 1.0f, 3, hasCollider: true);
            CreateLabProp("Bench_VaultSE", new Vector3(8.9f, -4.2f, 0f), assets.ScifiBenchSide, 0.9f, 3, hasCollider: true);
            CreateLabProp("Crates_SW", new Vector3(-9.0f, -4.8f, 0f), assets.ScifiCrateGroup1, 0.85f, 3, hasCollider: true);

            // Shadow-Only Horror Mood Overlays (appear only when flipped to Shadow Realm!)
            CreateShadowHorrorDecal("Horror_Blood_West", new Vector3(-6.2f, -1.6f, 0f), assets.ScifiBlood1, 1.4f, -15);
            CreateShadowHorrorDecal("Horror_Blood_East", new Vector3(2.8f, -0.5f, 0f), assets.ScifiBlood2, 1.4f, -15);
            CreateShadowHorrorDecal("Horror_Blood_Airlock", new Vector3(6.85f, 0.0f, 0f), assets.ScifiBlood1, 1.2f, -15);
            CreateShadowHorrorDecal("Horror_Glow_Airlock1", new Vector3(6.2f, 0.0f, 0f), assets.ScifiHorrorDoorGlow, 0.52f, -14);
            CreateShadowHorrorDecal("Horror_Glow_Airlock2", new Vector3(7.5f, 0.0f, 0f), assets.ScifiHorrorDoorGlow, 0.52f, -14);

            // West Sector: Turret Emitter + Containment Shutter + Electrical Breaker Box + Engineer Mara
            LightBeamEmitter turret = CreateEmitter("Turret_Emitter_L8", new Vector3(-5.2f, 1.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.up, assets);
            RotatingTurretEmitter turretRot = turret.gameObject.AddComponent<RotatingTurretEmitter>();
            turretRot.rotateOnWorldFlip = true;
            turretRot.stepDegrees = -90f;

            GameObject containmentShutter = CreateWall("Containment_Shutter_L8", new Vector3(-3.0f, 1.5f, 0f), new Vector2(0.75f, 1.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            PuzzleLever breakerLever = CreateLever("ElecBox_Breaker_L8", new Vector3(-7.8f, -0.8f, 0f), assets, WorldAffinity.Both,
                toggledObjects: new[] { containmentShutter });

            CreateLabNPC("Chief_Engineer_Mara", new Vector3(-5.5f, -3.8f, 0f), "Chief Engineer Mara",
                new[]
                {
                    "Omega Containment is in lockdown! Open the Electrical Breaker Box [E] to drop the West optical shutter.",
                    "Flip [F] to rotate the Turret right into Portal Mirror A—you'll also phase through the Security Gate at the same time!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets, usableWorld: WorldAffinity.Both);

            // Center Sterile Partition (x = -1.2): Top wall, LightOnly Phase Gate at y = -2.5, Portal Mirrors A -> B across top wall
            CreateWall("Partition_Top_L8", new Vector3(-1.2f, 2.1f, 0f), new Vector2(0.85f, 7.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Security_PhaseGate_L8", new Vector3(-1.2f, -2.5f, 0f), new Vector2(0.85f, 2.4f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
            CreateWall("Partition_Bot_L8", new Vector3(-1.2f, -4.6f, 0f), new Vector2(0.85f, 2.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);

            PortalMirror portalB = CreatePortalMirror("PortalMirror_B_L8", new Vector3(0.5f, 1.5f, 0f), assets.PortalMirrorB, Vector2.right, null, canRotate: false);
            CreatePortalMirror("PortalMirror_A_L8", new Vector3(-1.8f, 1.5f, 0f), assets.PortalMirrorA, Vector2.right, portalB, canRotate: false);

            // East Sector: 4-Way Relay Crystal -> Top White Sensor + Bottom Mirror_East_Aux -> Shadow Sensor + Pushable Crate -> Hatch Button
            CreateBeamRelay("Relay_4Way_L8", new Vector3(2.5f, 1.5f, 0f), BeamRelayCrystal.RelayMode.CrossRepeater4Way, Vector2.right, false, assets);
            LightSensor topSensor = CreateSensor("Sensor_Facility_Top", new Vector3(2.5f, 4.2f, 0f), LightColor.White, WorldAffinity.Both, assets);

            CreateIsometricMirror("Mirror_East_Aux", new Vector3(2.5f, -2.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor botSensor = CreateSensor("Sensor_Facility_Bot", new Vector3(4.8f, -2.5f, 0f), LightColor.White, WorldAffinity.ShadowOnly, assets);

            CreatePushableBlock("Scifi_StorageCrate", new Vector3(0.6f, -4.2f, 0f), assets);
            PressurePlate hatchButton = CreatePressurePlate("Hatch_Button_L8", new Vector3(3.8f, -4.2f, 0f), assets, WorldAffinity.Both, latch: false);

            // Dual Sliding Facility Door Airlock (x = 6.2 D1 Standard Sliding, x = 7.5 D2 Lab Door)
            CreateWall("Airlock1_Upper_L8", new Vector3(6.2f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Airlock1_Lower_L8", new Vector3(6.2f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor stdAirlock = CreateScifiDoor("ScifiDoor_Std_L8", new Vector3(6.2f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { topSensor, botSensor }, new[] { hatchButton }, new[] { breakerLever }, requireAll: true, stayOpen: true, isLabDoor: false);

            CreateWall("Airlock2_Upper_L8", new Vector3(7.5f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Airlock2_Lower_L8", new Vector3(7.5f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor labAirlock = CreateScifiDoor("ScifiDoor_Lab_L8", new Vector3(7.5f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { topSensor, botSensor }, new[] { hatchButton }, new[] { breakerLever }, requireAll: true, stayOpen: false, isLabDoor: true);

            CreateExit("LevelExit", new Vector3(9.0f, 0.0f, 0f), assets, new[] { stdAirlock, labAirlock }, "OpticsPuzzle_Level9");

            EditorSceneManager.SaveScene(scene, Level8Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 8 (Omega Containment Facility) at " + Level8Path);
        }

        [MenuItem("Shadow Flip/Create Level 9 (Twin-Mirror Observatory)")]
        public static void CreateOpticsPuzzleLevel9()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithLaboratoryTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.05f, 0.06f, 0.12f, 1f),
                new Color(0.97f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.30f, 0.24f, 0.60f, 1f), 0.50f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 12.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 9: Twin-Mirror Observatory";
            hud.levelHint = "Rotate the Linked West Mirror [E] to align BOTH twins, charge the Memory Prism, then flip [F] to pin & aim the Awakened Mimic Mirror!";

            CreateArenaWalls(assets);

            // Center observatory divider (walkable at bottom only in Shadow Realm)
            CreateWall("Observatory_Div_Top", new Vector3(0.0f, 2.8f, 0f), new Vector2(0.85f, 5.4f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Observatory_PhaseGate", new Vector3(0.0f, -2.6f, 0f), new Vector2(0.85f, 5.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Long);

            // West Wing: White Emitter -> Linked Twin Mirror A -> Memory Delay Prism -> West Sensor
            CreateEmitter("Emitter_Obs_Light", new Vector3(-8.5f, 3.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            GameObject twinAGo = CreateIsometricMirror("Mirror_Twin_Light", new Vector3(-4.5f, 3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateMemoryPrism("MemoryPrism_Obs", new Vector3(-4.5f, 0.5f, 0f), 9.0f, Vector2.down, false, assets);
            LightSensor westSensor = CreateSensor("Sensor_Obs_West", new Vector3(-4.5f, -2.6f, 0f), LightColor.White, WorldAffinity.Both, assets);

            // East Wing: Shadow Emitter -> Linked Twin Mirror B -> Mimic Mirror -> East Shadow Sensor
            CreateEmitter("Emitter_Obs_Shadow", new Vector3(1.5f, 3.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            GameObject twinBGo = CreateIsometricMirror("Mirror_Twin_Shadow", new Vector3(4.5f, 3.5f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets, canRotate: false);
            twinAGo.GetComponent<OpticalMirror>().linkedTwinMirror = twinBGo.GetComponent<OpticalMirror>();

            CreateUmbraMimicMirror("MimicMirror_Obs", new Vector3(4.5f, -1.5f, 0f), 45f, new Vector3(0f, -2.5f, 0f), assets);
            LightSensor eastSensor = CreateSensor("Sensor_Obs_East", new Vector3(5.6f, -1.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            CreateLabNPC("Dr_Vance_L9", new Vector3(-7.6f, 1.0f, 0f), "Dr. Vance",
                new[]
                {
                    "Linked Twin Mirrors synchronize rotations across worlds! Rotating Mirror_Twin_Light steers Mirror_Twin_Shadow too.",
                    "That Shadow Beam pins the Umbra Mimic Mirror in place so it can't run away!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

            CreateWall("VaultWall_Top_L9", new Vector3(7.0f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("VaultWall_Bot_L9", new Vector3(7.0f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor obsDoor = CreateLabBlastDoor("Door_Observatory", new Vector3(7.0f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { westSensor, eastSensor }, null, null, requireAll: true, stayOpen: false, vertical: true, redVariant: false);

            CreateExit("LevelExit", new Vector3(8.9f, 0.0f, 0f), assets, new[] { obsDoor }, "OpticsPuzzle_Level10");

            EditorSceneManager.SaveScene(scene, Level9Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 9 at " + Level9Path);
        }

        [MenuItem("Shadow Flip/Create Level 10 (Polarized Rail Foundry)")]
        public static void CreateOpticsPuzzleLevel10()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithScifiFacilityTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.04f, 0.04f, 0.09f, 1f),
                new Color(0.96f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.28f, 0.18f, 0.44f, 1f), 0.46f,
                new Vector3(-8.0f, -4.0f, 0f));

            stalker.umbraSafeDuration = 12.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 10: Polarized Rail Foundry";
            hud.levelHint = "Aim the Red Stun Mirror [E] onto the Beam-Eater Moth, open the Blue One-Way Polarizer [E] to glide the Rail Mirror into place, then flip [F] for the Inverted Shadow Sensor!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_Foundry", new Vector3(-8.2f, 1.2f, 0f), 2.1f, assets);

            // Creeping Umbra Mold patches cleared by lasers
            CreateUmbraMold("UmbraMold_Walkway", new Vector3(-2.5f, 4.0f, 0f), 1.0f, assets);
            CreateUmbraMold("UmbraMold_Center", new Vector3(1.0f, -2.5f, 0f), 1.05f, assets);

            // Upper Rail Power Line: Blue Emitter -> One-Way Polarizer -> Rail Terminal
            CreateEmitter("Emitter_Blue_Rail", new Vector3(-8.5f, 4.0f, 0f), LightColor.Blue, WorldAffinity.Both, Vector2.right, assets);
            CreateOneWayPolarizer("Polarizer_Rail", new Vector3(-5.5f, 4.0f, 0f), Vector2.up, false, true, assets);

            GameObject railCart = CreateMagneticRail("Rail_System", new Vector3(3.5f, 4.0f, 0f),
                new Vector3(2.2f, 1.0f, 0f), new Vector3(4.5f, 1.0f, 0f), LightColor.Blue, assets);
            // Attach 135 deg mirror on the moving cart to bounce vertical bottom emitter LEFT
            BoxCollider2D cartCol = railCart.AddComponent<BoxCollider2D>();
            cartCol.size = new Vector2(1.0f, 1.0f);
            OpticalMirror cartMirror = railCart.AddComponent<OpticalMirror>();
            cartMirror.isTwoSided = true;
            cartMirror.canRotate = false;
            cartMirror.directionalSprites = assets.MirrorDirections;
            cartMirror.logicalAngle = 135f;
            cartMirror.ApplyIsometricVisual();

            // Vertical White Emitter -> Cart Mirror at (4.5, 1.0) -> One-Way Inverter Diode -> Shadow Sensor
            CreateEmitter("Emitter_Vert_Foundry", new Vector3(4.5f, -4.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.up, assets);
            CreateOneWayPolarizer("Diode_Inverter", new Vector3(0.2f, 1.0f, 0f), Vector2.left, true, false, assets);
            LightSensor shadowSensor = CreateSensor("Sensor_Shadow_Foundry", new Vector3(-2.5f, 1.0f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            // Bottom Moth Gauntlet: Red Emitter -> Mirror -> Stun Beam-Eater Moth -> Bottom White Beam -> Light Sensor
            CreateEmitter("Emitter_Red_MothStun", new Vector3(-8.5f, 2.0f, 0f), LightColor.Red, WorldAffinity.Both, Vector2.right, assets);
            CreateIsometricMirror("Mirror_StunAim", new Vector3(-5.0f, 2.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);

            CreateEmitter("Emitter_Bot_White", new Vector3(-8.5f, -2.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreateBeamEaterMoth("BeamMoth_Foundry", new Vector3(-5.0f, -2.5f, 0f), LightColor.Red, assets);
            LightSensor lightSensor = CreateSensor("Sensor_Light_Foundry", new Vector3(-2.5f, -2.5f, 0f), LightColor.White, WorldAffinity.Both, assets);
            lightSensor.holdDuration = 1.0f;

            CreateWall("VaultWall_Upper_L10", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("VaultWall_Lower_L10", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor foundryDoor = CreateScifiDoor("ScifiDoor_Foundry", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { lightSensor, shadowSensor }, null, null, requireAll: true, stayOpen: false, isLabDoor: true);

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { foundryDoor }, "OpticsPuzzle_Level11");

            EditorSceneManager.SaveScene(scene, Level10Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 10 at " + Level10Path);
        }

        [MenuItem("Shadow Flip/Create Level 11 (The Reality Anchor Rift)")]
        public static void CreateOpticsPuzzleLevel11()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithLaboratoryTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.05f, 0.04f, 0.10f, 1f),
                new Color(1f, 0.96f, 0.88f, 1f), 1.05f,
                new Color(0.28f, 0.22f, 0.56f, 1f), 0.48f,
                new Vector3(-8.0f, -3.2f, 0f));

            stalker.umbraSafeDuration = 12.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 11: The Reality Anchor Rift";
            hud.levelHint = "Summon your Mirror Twin [E] to step onto the East Button, feed the Memory Prism, then flip [F] so it powers the Reality Anchor Bubble for Light-Realm devices in Shadow!";

            CreateArenaWalls(assets);

            // Mirror Symmetry Axis at x = 0
            CreateWall("Div_Top_L11", new Vector3(0.0f, 4.75f, 0f), new Vector2(0.9f, 1.3f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Div_Window_LightOnly", new Vector3(0.0f, 3.0f, 0f), new Vector2(0.9f, 2.2f), assets, WorldAffinity.LightOnly, WallTileStyle.Long);
            CreateWall("Div_Mid_L11", new Vector3(0.0f, -0.4f, 0f), new Vector2(0.9f, 4.6f), assets, WorldAffinity.Both, WallTileStyle.Long);

            // West Chamber: Twin Totem + White Emitter -> Memory Prism (8s delay across x=0 when flipped to Shadow!)
            CreateMirrorTwinTotem("Totem_MirrorTwin", new Vector3(-4.5f, 1.5f, 0f), 0f, playerMovement.DarkspriteDown, assets);
            PressurePlate twinEastButton = CreatePressurePlate("Button_Twin_East", new Vector3(4.5f, -2.0f, 0f), assets, WorldAffinity.Both, latch: true);

            CreateEmitter("Emitter_Rift_West", new Vector3(-8.5f, 3.0f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreateMemoryPrism("MemoryPrism_Rift", new Vector3(-2.2f, 3.0f, 0f), 9.0f, Vector2.right, false, assets);

            // East Chamber: Reality Anchor Lantern + LightOnly Sensor + LightOnly Bubble Button
            CreateRealityAnchor("RealityAnchor_East", new Vector3(3.2f, 3.0f, 0f), 3.9f, 7.5f, assets);
            LightSensor riftSensor = CreateSensor("Sensor_Rift_LightOnly", new Vector3(5.2f, 3.0f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);
            PressurePlate bubbleButton = CreatePressurePlate("Button_Bubble_LightOnly", new Vector3(3.2f, 0.8f, 0f), assets, WorldAffinity.LightOnly, latch: true);

            PuzzleDoor midDoor = CreateLabBlastDoor("Door_Rift_Mid", new Vector3(0.0f, -4.0f, 0f), new Vector2(0.9f, 2.6f), assets,
                new[] { riftSensor }, new[] { twinEastButton }, null, requireAll: true, stayOpen: true, vertical: true, redVariant: false);

            // Exit Airlock at x = 6.8
            CreateWall("ExitWall_Top_L11", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("ExitWall_Bot_L11", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor finalDoor = CreateLabBlastDoor("Door_Rift_Exit", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                null, new[] { twinEastButton, bubbleButton }, null, requireAll: true, stayOpen: false, vertical: true, redVariant: true);

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { midDoor, finalDoor }, "OpticsPuzzle_Level12");

            EditorSceneManager.SaveScene(scene, Level11Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 11 at " + Level11Path);
        }

        [MenuItem("Shadow Flip/Create Level 12 (Eclipse Paradox Boss)")]
        public static void CreateOpticsPuzzleLevel12()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = WithScifiFacilityTheme(LoadCelAssets());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController stalker,
                new Color(0.03f, 0.02f, 0.07f, 1f),
                new Color(0.96f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.24f, 0.14f, 0.42f, 1f), 0.44f,
                new Vector3(-8.0f, -3.8f, 0f));

            stalker.umbraSafeDuration = 12.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 12: Eclipse Paradox [BOSS]";
            hud.levelHint = "Shatter the Umbra Warden's 3 Shields in order (RED -> GREEN -> BLUE)! Open Red Polarizer [E], flip [F] for the Shadow Green Mirror, and aim the Blue Mirror [E]!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_Boss", new Vector3(-7.8f, 3.8f, 0f), 2.3f, assets);

            UmbraWardenBoss warden = CreateUmbraWardenBoss("UmbraWarden_Boss", new Vector3(0.0f, 0.5f, 0f), assets);

            // 1. RED Beam (West -> Boss)
            CreateEmitter("Emitter_Boss_Red", new Vector3(-8.5f, 0.5f, 0f), LightColor.Red, WorldAffinity.Both, Vector2.right, assets);
            CreateOneWayPolarizer("Polarizer_Boss_Red", new Vector3(-5.0f, 0.5f, 0f), Vector2.up, false, true, assets);

            // 2. GREEN Beam (Top ShadowMirror -> Green Filter -> Boss)
            CreateEmitter("Emitter_Boss_Green", new Vector3(-3.5f, 4.2f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreateIsometricMirror("Mirror_Boss_Green", new Vector3(0.0f, 4.2f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets);
            CreateColorFilter("Filter_Boss_Green", new Vector3(0.0f, 2.5f, 0f), new Vector2(1.1f, 0.45f), LightColor.Green, assets);

            // 3. BLUE Beam (South -> East Mirror -> Boss)
            CreateEmitter("Emitter_Boss_Blue", new Vector3(4.2f, -4.5f, 0f), LightColor.Blue, WorldAffinity.Both, Vector2.up, assets);
            CreateIsometricMirror("Mirror_Boss_Blue", new Vector3(4.2f, 0.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);

            // Boss Vault Exit
            CreateWall("BossVault_Top", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("BossVault_Bot", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor bossDoor = CreateScifiDoor("Door_BossVault", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                null, null, null, requireAll: true, stayOpen: false, isLabDoor: true);
            bossDoor.requiredBoss = warden;

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { bossDoor }, "MainMenu");

            EditorSceneManager.SaveScene(scene, Level12Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 12 Boss at " + Level12Path);
        }

        [MenuItem("Shadow Flip/Create Sandbox Level Editor Scene")]
        public static void CreateSandboxLevel()
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
                new Color(0.07f, 0.06f, 0.12f, 1f),
                new Color(1f, 0.96f, 0.88f, 1f), 1.05f,
                new Color(0.36f, 0.28f, 0.68f, 1f), 0.52f,
                new Vector3(-8.5f, -4.0f, 0f));

            stalker.enabled = false;
            hud.parFlips = 99;
            hud.lessonTitle = "Sandbox Level Editor";
            hud.levelHint = "Click grid tiles to place/erase props. Press [1-9, 0] to pick tool, [Q]/Right-Click to rotate, [P] to toggle Build vs Play Test!";

            CreateArenaWalls(assets);
            PixelSandboxEditor sandbox = gameLogic.gameObject.AddComponent<PixelSandboxEditor>();
            sandbox.player = playerMovement;

            EditorSceneManager.SaveScene(scene, SandboxPath);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Sandbox Level Editor scene at " + SandboxPath);
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

            RealmCameraController camCtrl = camGo.AddComponent<RealmCameraController>();
            camCtrl.player = playerMovement;
            camCtrl.gameLogic = gameLogic;
            camCtrl.lightOrthoSize = 4.0f;
            camCtrl.umbraOrthoSize = 7.0f;
        }

        private static void CreateArenaWalls(CelAssets assets)
        {
            CreateWall("Wall_Top", new Vector3(0f, 5.95f, 0f), new Vector2(21.8f, 1.1f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Bottom", new Vector3(0f, -5.95f, 0f), new Vector2(21.8f, 1.1f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Left", new Vector3(-10.35f, 0f, 0f), new Vector2(1.1f, 13.0f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Wall_Right", new Vector3(10.35f, 0f, 0f), new Vector2(1.1f, 13.0f), assets, WorldAffinity.Both, WallTileStyle.Long);
        }

        private static GameObject CreatePlayerForm(string name, Transform parent, Vector3 pos, string spritePath, Color fallbackColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            sr.color = fallbackColor;
            sr.sortingOrder = 10;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.66f;

            return go;
        }

        private static GameObject CreateWall(string name, Vector3 pos, Vector2 size, CelAssets assets, WorldAffinity affinity, WallTileStyle style = WallTileStyle.Medium)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            Sprite lightSprite = assets.WallLightLong;
            Sprite shadowSprite = assets.WallShadowLong;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = affinity == WorldAffinity.ShadowOnly ? shadowSprite : lightSprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.sortingOrder = 2;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();

            bool isVertical = size.y > size.x;
            if (isVertical)
            {
                float rotZ = pos.x > 8f ? -90f : 90f;
                go.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
                Vector2 localSize = new Vector2(size.y, size.x);
                sr.size = localSize;
                col.size = localSize;
            }
            else
            {
                sr.size = size;
                col.size = size;
            }

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
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.EmitterActive;
            sr.color = Color.Lerp(Color.white, LightColorUtility.ToUnityColor(color), 0.25f);
            sr.sortingOrder = 4;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

            LightBeamEmitter emitter = go.AddComponent<LightBeamEmitter>();
            emitter.beamColor = color;
            emitter.activeInWorld = world;
            emitter.localEmitDirection = dir;
            emitter.emitterRenderer = sr;
            // Leave activeEmitterSprite null so LabAnimatedProp cycles all 13 Obelisk frames while active,
            // while inactiveEmitterSprite shows the dimmed offline Obelisk in the opposite realm.
            emitter.activeEmitterSprite = (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0) ? null : assets.EmitterActive;
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
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LanternSanctuary;
            sr.color = new Color(1f, 0.94f, 0.72f, 1f);
            sr.sortingOrder = 3;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

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
            go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.RelayCrystal;
            sr.color = new Color(0.88f, 0.76f, 1f, 1f);
            sr.sortingOrder = 4;

            if (assets.ObeliskFrames != null && assets.ObeliskFrames.Length > 0)
            {
                LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
                anim.frames = assets.ObeliskFrames;
                anim.framesPerSecond = 10f;
            }

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
            if (assets.ColorFilterLens != null)
            {
                sr.sprite = assets.ColorFilterLens;
                sr.drawMode = SpriteDrawMode.Simple;
                Vector2 lensBounds = assets.ColorFilterLens.bounds.size;
                bool horizontal = size.x > size.y;
                if (horizontal)
                {
                    filterGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    filterGo.transform.localScale = new Vector3(size.y / lensBounds.x, size.x / lensBounds.y, 1f);
                }
                else
                {
                    filterGo.transform.localScale = new Vector3(size.x / lensBounds.x, size.y / lensBounds.y, 1f);
                }
                sr.sortingOrder = 3;

                BoxCollider2D col = filterGo.AddComponent<BoxCollider2D>();
                col.size = lensBounds;
            }
            else
            {
                sr.sprite = assets.FloorLight;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = size;
                sr.sortingOrder = 3;

                BoxCollider2D col = filterGo.AddComponent<BoxCollider2D>();
                col.size = size;
            }

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

            bool isWideConsole = assets.LeverOff != null && assets.LeverOff.bounds.size.x > 1.8f;
            if (isWideConsole) go.transform.localScale = new Vector3(0.72f, 0.95f, 1f);

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = isWideConsole ? new Vector2(2.4f, 1.1f) : new Vector2(1.1f, 1.0f);
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

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = assets.DoorClosed;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            Vector2 spriteSize = doorSr.sprite != null ? (Vector2)doorSr.sprite.bounds.size : new Vector2(1.28f, 1.60f);
            doorGo.transform.localScale = new Vector3(size.x / spriteSize.x, size.y / spriteSize.y, 1f);

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(spriteSize.x * 0.94f, spriteSize.y * 0.94f);

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
            sr.sprite = assets.PushableBlockSprite != null ? assets.PushableBlockSprite : assets.EmitterBroken;
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
            plateSr.sortingOrder = 1;

            bool isSmallButton = assets.ButtonUp != null && assets.ButtonUp.bounds.size.x < 1.0f;
            if (isSmallButton) plateGo.transform.localScale = new Vector3(1.45f, 1.45f, 1f);

            BoxCollider2D col = plateGo.AddComponent<BoxCollider2D>();
            col.size = isSmallButton ? new Vector2(0.8f, 0.8f) : new Vector2(1.15f, 1.15f);
            col.isTrigger = true;

            PressurePlate plate = plateGo.AddComponent<PressurePlate>();
            plate.acceptedForm = form;
            plate.latchOncePressed = latch;
            plate.plateRenderer = plateSr;
            plate.unpressedSprite = assets.ButtonUp;
            plate.pressedSprite = assets.ButtonDown;
            return plate;
        }

        private static CelAssets WithLaboratoryTheme(CelAssets src)
        {
            CelAssets lab = src;
            lab.FloorLight = src.LabFloorLight != null ? src.LabFloorLight : src.FloorLight;
            lab.FloorShadow = src.LabFloorShadow != null ? src.LabFloorShadow : src.FloorShadow;
            lab.WallLightLong = src.LabWallLightLong != null ? src.LabWallLightLong : src.WallLightLong;
            lab.WallLightMedium = src.LabWallLightWindow != null ? src.LabWallLightWindow : src.WallLightMedium;
            lab.WallLightPillar = src.LabWallLightPillar != null ? src.LabWallLightPillar : src.WallLightPillar;
            lab.WallLightRuined = src.LabWallPipes != null ? src.LabWallPipes : src.WallLightRuined;
            lab.WallShadowLong = src.LabWallShadowLong != null ? src.LabWallShadowLong : src.WallShadowLong;
            lab.WallShadowMedium = src.LabWallShadowWindow != null ? src.LabWallShadowWindow : src.WallShadowMedium;
            lab.WallShadowPillar = src.LabWallShadowPillar != null ? src.LabWallShadowPillar : src.WallShadowPillar;
            lab.WallShadowRuined = src.LabWallPipes != null ? src.LabWallPipes : src.WallShadowRuined;
            lab.ButtonUp = src.LabButtonUp != null ? src.LabButtonUp : src.ButtonUp;
            lab.ButtonDown = src.LabButtonDown != null ? src.LabButtonDown : src.ButtonDown;
            lab.LeverOff = src.LabConsoleOff != null ? src.LabConsoleOff : src.LeverOff;
            lab.LeverOn = src.LabConsoleOn != null ? src.LabConsoleOn : src.LeverOn;
            return lab;
        }

        private static CelAssets WithScifiFacilityTheme(CelAssets src)
        {
            CelAssets fac = src;
            fac.FloorLight = src.ScifiFloorLight != null ? src.ScifiFloorLight : src.FloorLight;
            fac.FloorShadow = src.ScifiFloorShadow != null ? src.ScifiFloorShadow : src.FloorShadow;
            fac.WallLightLong = src.ScifiWallLightLong != null ? src.ScifiWallLightLong : src.WallLightLong;
            fac.WallLightMedium = src.ScifiWallLightMedium != null ? src.ScifiWallLightMedium : src.WallLightMedium;
            fac.WallLightPillar = src.ScifiWallLightPillar != null ? src.ScifiWallLightPillar : src.WallLightPillar;
            fac.WallShadowLong = src.ScifiWallShadowLong != null ? src.ScifiWallShadowLong : src.WallShadowLong;
            fac.WallShadowMedium = src.ScifiWallShadowMedium != null ? src.ScifiWallShadowMedium : src.WallShadowMedium;
            fac.WallShadowPillar = src.ScifiWallShadowPillar != null ? src.ScifiWallShadowPillar : src.WallShadowPillar;
            fac.DoorClosed = src.ScifiDoorLabClosed != null ? src.ScifiDoorLabClosed : src.DoorClosed;
            fac.DoorOpen = src.ScifiDoorLabOpen != null ? src.ScifiDoorLabOpen : src.DoorOpen;
            fac.LeverOff = src.ScifiElecBoxClosed != null ? src.ScifiElecBoxClosed : src.LeverOff;
            fac.LeverOn = src.ScifiElecBoxOpen != null ? src.ScifiElecBoxOpen : src.LeverOn;
            fac.ButtonUp = src.ScifiHatchButtonUp != null ? src.ScifiHatchButtonUp : src.ButtonUp;
            fac.ButtonDown = src.ScifiHatchButtonDown != null ? src.ScifiHatchButtonDown : src.ButtonDown;
            fac.PushableBlockSprite = src.ScifiCratePushable != null ? src.ScifiCratePushable : src.PushableBlockSprite;
            return fac;
        }

        private static PuzzleDoor CreateScifiDoor(
            string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers,
            bool requireAll, bool stayOpen, bool isLabDoor)
        {
            Sprite closedSprite = isLabDoor ? assets.ScifiDoorLabClosed : assets.ScifiDoorStdClosed;
            Sprite openSprite = isLabDoor ? assets.ScifiDoorLabOpen : assets.ScifiDoorStdOpen;
            if (closedSprite == null || openSprite == null)
            {
                return CreateDoor(name, pos, size, assets, sensors, plates, levers, requireAll, stayOpen);
            }

            if (assets.ScifiThresholdV != null)
            {
                CreateLabProp(name + "_Threshold", pos, assets.ScifiThresholdV, 0.9f, -14);
            }
            if (assets.ScifiDoorMarking != null)
            {
                CreateLabProp(name + "_Marking", pos + new Vector3(0f, -size.y * 0.45f, 0f), assets.ScifiDoorMarking, 0.6f, -14);
            }

            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;
            doorGo.transform.localScale = new Vector3(size.x / 1.11f, size.y / 1.39f, 1f);

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = closedSprite;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.32f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = closedSprite;
            door.openSprite = openSprite;
            return door;
        }

        private static void CreateScifiAlarmBeacon(string name, Vector3 pos, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.ScifiAlarmOff;
            sr.sortingOrder = 4;

            SpriteRenderer glowSr = null;
            if (assets.ScifiAlarmGlow != null)
            {
                GameObject glowGo = new GameObject("AlarmGlow");
                glowGo.transform.SetParent(go.transform, false);
                glowGo.transform.localPosition = Vector3.zero;
                glowGo.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                glowSr = glowGo.AddComponent<SpriteRenderer>();
                glowSr.sprite = assets.ScifiAlarmGlow;
                glowSr.sortingOrder = 15;
                glowSr.enabled = false;
            }

            ScifiAlarmBeacon beacon = go.AddComponent<ScifiAlarmBeacon>();
            beacon.beaconRenderer = sr;
            beacon.glowRenderer = glowSr;
            beacon.offSprite = assets.ScifiAlarmOff;
            beacon.onSprite = assets.ScifiAlarmOn;
        }

        private static void CreateShadowHorrorDecal(string name, Vector3 pos, Sprite sprite, float scale, int sortingOrder)
        {
            if (sprite == null) return;
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
            phase.affinity = WorldAffinity.ShadowOnly;
            phase.disableRenderersInsteadOfFade = true;
        }

        private static PuzzleDoor CreateLabBlastDoor(
            string name, Vector3 pos, Vector2 size, CelAssets assets,
            LightSensor[] sensors, PressurePlate[] plates, PuzzleLever[] levers,
            bool requireAll, bool stayOpen, bool vertical, bool redVariant)
        {
            Sprite closedSprite = redVariant ? assets.LabBlastDoorRedClosed : assets.LabBlastDoorClosed;
            Sprite openSprite = redVariant ? assets.LabBlastDoorRedOpen : assets.LabBlastDoorOpen;
            if (closedSprite == null || openSprite == null)
            {
                return CreateDoor(name, pos, size, assets, sensors, plates, levers, requireAll, stayOpen);
            }

            GameObject doorGo = new GameObject(name);
            doorGo.transform.position = pos;
            if (vertical)
            {
                doorGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                doorGo.transform.localScale = new Vector3(size.y / 2.58f, size.x / 1.32f, 1f);
            }
            else
            {
                doorGo.transform.localScale = new Vector3(size.x / 2.58f, size.y / 1.32f, 1f);
            }

            SpriteRenderer doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.sprite = closedSprite;
            doorSr.color = Color.white;
            doorSr.sortingOrder = 3;

            BoxCollider2D col = doorGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.48f, 1.25f);

            PuzzleDoor door = doorGo.AddComponent<PuzzleDoor>();
            door.requiredSensors = sensors;
            door.requiredPlates = plates;
            door.requiredLevers = levers;
            door.requireAll = requireAll;
            door.stayOpenOnceUnlocked = stayOpen;
            door.closedSprite = closedSprite;
            door.openSprite = openSprite;
            return door;
        }

        private static void CreateLabSanctuary(string name, Vector3 pos, float radius, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.5f, 1.15f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.LabTeslaTower != null ? assets.LabTeslaTower : assets.LanternSanctuary;
            sr.sortingOrder = 3;

            if (assets.LabBulbs != null && assets.LabBulbs.Length > 0)
            {
                GameObject bulbGo = new GameObject("TeslaBulb_Glow");
                bulbGo.transform.SetParent(go.transform, false);
                bulbGo.transform.localPosition = new Vector3(0f, 0.72f, 0f);
                bulbGo.transform.localScale = new Vector3(0.55f, 0.62f, 1f);
                SpriteRenderer bulbSr = bulbGo.AddComponent<SpriteRenderer>();
                bulbSr.sprite = assets.LabBulbs[0];
                bulbSr.sortingOrder = 5;
                LabAnimatedProp anim = bulbGo.AddComponent<LabAnimatedProp>();
                anim.frames = assets.LabBulbs;
                anim.framesPerSecond = 6f;
            }

            LightSanctuary sanc = go.AddComponent<LightSanctuary>();
            sanc.sanctuaryRadius = radius;
        }

        private static LabNPCController CreateLabNPC(
            string name, Vector3 pos, string displayName, string[] dialogueLines,
            Sprite[] lightDirs, Sprite[] shadowDirs, CelAssets assets,
            WorldAffinity usableWorld = WorldAffinity.Both)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.92f, 0.92f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = (lightDirs != null && lightDirs.Length > 0) ? lightDirs[0] : null;
            sr.sortingOrder = 8;

            CircleCollider2D triggerCol = go.AddComponent<CircleCollider2D>();
            triggerCol.radius = 0.85f;
            triggerCol.isTrigger = true;

            LabNPCController npc = go.AddComponent<LabNPCController>();
            npc.npcName = displayName;
            npc.dialogueLines = dialogueLines;
            npc.usableInWorld = usableWorld;
            if (lightDirs != null && lightDirs.Length >= 4)
            {
                npc.spriteDown = lightDirs[0];
                npc.spriteUp = lightDirs[1];
                npc.spriteLeft = lightDirs[2];
                npc.spriteRight = lightDirs[3];
            }
            if (shadowDirs != null && shadowDirs.Length >= 4)
            {
                npc.shadowSpriteDown = shadowDirs[0];
                npc.shadowSpriteUp = shadowDirs[1];
                npc.shadowSpriteLeft = shadowDirs[2];
                npc.shadowSpriteRight = shadowDirs[3];
            }
            return npc;
        }

        private static GameObject CreateLabProp(string name, Vector3 pos, Sprite sprite, float scale, int sortingOrder, bool hasCollider = false)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;

            if (hasCollider && sprite != null)
            {
                BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(sprite.bounds.size.x * 0.85f, sprite.bounds.size.y * 0.72f);
            }
            return go;
        }

        private static GameObject CreateAnimatedLabProp(string name, Vector3 pos, Sprite[] frames, float scale, int sortingOrder, float fps)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = (frames != null && frames.Length > 0) ? frames[0] : null;
            sr.sortingOrder = sortingOrder;

            LabAnimatedProp anim = go.AddComponent<LabAnimatedProp>();
            anim.frames = frames;
            anim.framesPerSecond = fps;
            return go;
        }

        private static GameObject CreateFloorAccent(string name, Vector3 pos, Vector2 size, Sprite sprite, int sortingOrder)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private static GameObject CreateOneWayPolarizer(string name, Vector3 pos, Vector2 allowedDir, bool invertColor, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.1f, 0.9f, 1f);
            Vector2 d = allowedDir.sqrMagnitude > 0.001f ? allowedDir.normalized : Vector2.right;
            go.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.OneWayPolarizer != null ? assets.OneWayPolarizer : assets.ColorFilterLens;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.7f, 1.15f);

            OneWayPolarizer p = go.AddComponent<OneWayPolarizer>();
            p.allowedDirection = d;
            p.invertRealmColorOnPass = invertColor;
            p.canRotate = canRotate;
            return go;
        }

        private static GameObject CreateMemoryPrism(string name, Vector3 pos, float holdSeconds, Vector2 outDir, bool canRotate, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MemoryPrismIdle != null ? assets.MemoryPrismIdle : assets.EchoPedestal;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            OpticalMemoryPrism mp = go.AddComponent<OpticalMemoryPrism>();
            mp.holdSeconds = holdSeconds;
            mp.useFixedOutputDir = true;
            mp.localOutputDirection = outDir;
            mp.canRotate = canRotate;
            mp.idleSprite = assets.MemoryPrismIdle;
            mp.chargedSprite = assets.MemoryPrismCharged;
            return go;
        }

        private static GameObject CreateRealityAnchor(string name, Vector3 pos, float radius, float holdDuration, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.RealityAnchorOff != null ? assets.RealityAnchorOff : assets.LanternSanctuary;
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 1.0f);

            RealityAnchorLantern ral = go.AddComponent<RealityAnchorLantern>();
            ral.anchorRadius = radius;
            ral.holdDuration = holdDuration;
            ral.offSprite = assets.RealityAnchorOff;
            ral.onSprite = assets.RealityAnchorOn;
            return go;
        }

        private static GameObject CreateMagneticRail(string name, Vector3 terminalPos, Vector3 pointA, Vector3 pointB, LightColor reqColor, CelAssets assets)
        {
            // Track visual spans pointA -> pointB
            GameObject trackGo = new GameObject(name + "_Track");
            Vector3 mid = (pointA + pointB) * 0.5f;
            Vector3 diff = pointB - pointA;
            float dist = diff.magnitude + 1.0f;
            trackGo.transform.position = mid;
            trackGo.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg);
            SpriteRenderer trackSr = trackGo.AddComponent<SpriteRenderer>();
            trackSr.sprite = assets.MagneticRailTrack != null ? assets.MagneticRailTrack : assets.FloorLight;
            trackSr.drawMode = SpriteDrawMode.Tiled;
            trackSr.size = new Vector2(dist, 0.95f);
            trackSr.sortingOrder = -12;

            // Moving Cart rider
            GameObject cartGo = new GameObject(name + "_Cart");
            cartGo.transform.position = pointA;
            cartGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            SpriteRenderer cartSr = cartGo.AddComponent<SpriteRenderer>();
            cartSr.sprite = assets.MagneticRailCart != null ? assets.MagneticRailCart : assets.MirrorDirections[0];
            cartSr.sortingOrder = 4;

            // Rail Power Terminal
            GameObject termGo = new GameObject(name + "_Terminal");
            termGo.transform.position = terminalPos;
            termGo.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            SpriteRenderer termSr = termGo.AddComponent<SpriteRenderer>();
            termSr.sprite = assets.CombinerCrystal;
            termSr.sortingOrder = 4;
            BoxCollider2D termCol = termGo.AddComponent<BoxCollider2D>();
            termCol.size = new Vector2(1.0f, 1.0f);

            BeamMagneticRail rail = termGo.AddComponent<BeamMagneticRail>();
            rail.pointA = pointA;
            rail.pointB = pointB;
            rail.riderTransform = cartGo.transform;
            rail.requiredColor = reqColor;
            rail.moveSpeed = 2.6f;

            return cartGo;
        }

        private static GameObject CreateMirrorTwinTotem(string name, Vector3 pos, float axisX, Sprite twinSprite, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.SplitTotem != null ? assets.SplitTotem : assets.EchoPedestal;
            sr.sortingOrder = 3;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.1f, 1.0f);
            col.isTrigger = true;

            MirrorTwinTotem totem = go.AddComponent<MirrorTwinTotem>();
            totem.mirrorAxisX = axisX;
            totem.twinSprite = twinSprite;
            return go;
        }

        private static GameObject CreateUmbraMimicMirror(string name, Vector3 pos, float startAngle, Vector3 fleeOffset, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.MimicMirrorIdle != null ? assets.MimicMirrorIdle : assets.MirrorDirections[0];
            sr.sortingOrder = 4;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.05f, 1.05f);

            OpticalMirror mirror = go.AddComponent<OpticalMirror>();
            mirror.isTwoSided = true;
            mirror.canRotate = true;
            mirror.rotationStepDegrees = 90f;
            mirror.directionalSprites = assets.MirrorDirections;
            mirror.logicalAngle = startAngle;
            mirror.ApplyIsometricVisual();

            UmbraMimicMirror mimic = go.AddComponent<UmbraMimicMirror>();
            mimic.awakeSprite = assets.MimicMirrorAwake;
            mimic.fleeTargetOffset = fleeOffset;
            return go;
        }

        private static GameObject CreateBeamEaterMoth(string name, Vector3 pos, LightColor stunColor, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.BeamMoth1 != null ? assets.BeamMoth1 : assets.UmbraMonster1;
            sr.sortingOrder = 11;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            BeamEaterMoth moth = go.AddComponent<BeamEaterMoth>();
            moth.frame1 = assets.BeamMoth1;
            moth.frame2 = assets.BeamMoth2;
            moth.stunColor = stunColor;
            return go;
        }

        private static GameObject CreateUmbraMold(string name, Vector3 pos, float startScale, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(startScale, startScale, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.UmbraMoldTile != null ? assets.UmbraMoldTile : assets.FloorShadow;
            sr.sortingOrder = -5;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.55f;
            col.isTrigger = true;

            go.AddComponent<UmbraMoldPatch>();
            return go;
        }

        private static UmbraWardenBoss CreateUmbraWardenBoss(string name, Vector3 pos, CelAssets assets)
        {
            GameObject go = new GameObject(name);
            go.transform.position = pos;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = assets.UmbraWarden1 != null ? assets.UmbraWarden1 : assets.UmbraMonster1;
            sr.sortingOrder = 12;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.55f, 1.55f);

            UmbraWardenBoss boss = go.AddComponent<UmbraWardenBoss>();
            boss.frame1 = assets.UmbraWarden1;
            boss.frame2 = assets.UmbraWarden2;
            boss.petrifiedSprite = assets.MonsterPetrified;
            return boss;
        }

        private static GameObject CreateExit(string name, Vector3 pos, CelAssets assets, PuzzleDoor[] requiredDoors, string nextScene)
        {
            GameObject exitGo = new GameObject(name);
            exitGo.transform.position = pos;
            exitGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);

            SpriteRenderer exitSr = exitGo.AddComponent<SpriteRenderer>();
            bool hasCustomExit = assets.ExitPortal != null;
            exitSr.sprite = hasCustomExit ? assets.ExitPortal : assets.ReceiverActiveWhite;
            exitSr.color = hasCustomExit ? Color.white : new Color(0.45f, 1f, 0.72f, 1f);
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
            ConfigureSpriteFolder(CelDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(LabDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(ScifiDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(NewDataDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder(PixelNewDir, pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Images/Jimmy/Walk/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/Laboratory/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/ScifiFacility/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/NewData/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/PixelNew/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Resources/Sprites/JimmyWalk/", pointFilter: true, forceSingleMode: true);
            ConfigureSpriteFolder("Assets/Images/Jimmy/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/New data/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/tiles_laboratory_LandOfPixels/", pointFilter: true, recursive: true, forceSingleMode: false);
            ConfigureSpriteFolder("Assets/Scifi_Facility_Buider_Pack_1_ver 2.0/", pointFilter: true, recursive: true, skipPreview: true, forceSingleMode: false);
            ConfigurePixelTexture("Assets/Images/cursor/Cursor.png", TextureImporterType.Cursor, ensureReadable: true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigurePixelTexture(string assetPath, TextureImporterType requiredType, bool ensureReadable)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            bool changed = false;
            if (importer.textureType != requiredType)
            {
                importer.textureType = requiredType;
                changed = true;
            }
            if (ensureReadable && !importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }
            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }
            if (changed) importer.SaveAndReimport();
        }

        private static void ConfigureSpriteFolder(string folderPath, bool pointFilter = true, bool recursive = false, bool skipPreview = false, bool forceSingleMode = true)
        {
            if (!Directory.Exists(folderPath)) return;
            SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] files = Directory.GetFiles(folderPath, "*.png", searchOption);
            foreach (string file in files)
            {
                string assetPath = file.Replace('\\', '/');
                if (assetPath.EndsWith("CelShaded_SpriteSheet.png")) continue;
                if (skipPreview && assetPath.Contains("/Preview/")) continue;
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;
                if (forceSingleMode)
                {
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        changed = true;
                    }
                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
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
                }

                if (Mathf.Abs(importer.spritePixelsPerUnit - 100f) > 0.1f)
                {
                    importer.spritePixelsPerUnit = 100f;
                    changed = true;
                }
                if (pointFilter && importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }
                if (pointFilter && importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (pointFilter && importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
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
                PushableBlockSprite = LoadSprite(CelDir + "Emitter_Broken.png"),
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
                PhasePad = LoadSprite(CelDir + "Phase_Pad.png"),
                ColorFilterLens = LoadSprite(CelDir + "ColorFilter_Lens.png"),
                ExitPortal = LoadSprite(CelDir + "Exit_Portal.png"),

                // LandOfPixels Laboratory Assets
                LabFloorLight = LoadSprite(LabDir + "Lab_Floor_Light.png"),
                LabFloorShadow = LoadSprite(LabDir + "Lab_Floor_Shadow.png"),
                LabFloorGrating = LoadSprite(LabDir + "Lab_Floor_Grating.png"),
                LabFloorHazard = LoadSprite(LabDir + "Lab_Floor_Hazard.png"),
                LabWallLightLong = LoadSprite(LabDir + "Lab_Wall_Light_Long.png"),
                LabWallLightWindow = LoadSprite(LabDir + "Lab_Wall_Light_Window.png"),
                LabWallLightPillar = LoadSprite(LabDir + "Lab_Wall_Light_Pillar.png"),
                LabWallShadowLong = LoadSprite(LabDir + "Lab_Wall_Shadow_Long.png"),
                LabWallShadowWindow = LoadSprite(LabDir + "Lab_Wall_Shadow_Window.png"),
                LabWallShadowPillar = LoadSprite(LabDir + "Lab_Wall_Shadow_Pillar.png"),
                LabWallPipes = LoadSprite(LabDir + "Lab_Wall_Pipes.png"),
                LabBlastDoorClosed = LoadSprite(LabDir + "Lab_BlastDoor_Closed.png"),
                LabBlastDoorOpen = LoadSprite(LabDir + "Lab_BlastDoor_Open.png"),
                LabBlastDoorRedClosed = LoadSprite(LabDir + "Lab_BlastDoorRed_Closed.png"),
                LabBlastDoorRedOpen = LoadSprite(LabDir + "Lab_BlastDoorRed_Open.png"),
                LabHazardDoorClosed = LoadSprite(LabDir + "Lab_HazardDoor_Closed.png"),
                LabKeypadDoorClosed = LoadSprite(LabDir + "Lab_KeypadDoor_Closed.png"),
                LabButtonUp = LoadSprite(LabDir + "Lab_Button_Up.png"),
                LabButtonDown = LoadSprite(LabDir + "Lab_Button_Down.png"),
                LabConsoleOff = LoadSprite(LabDir + "Lab_Console_Off.png"),
                LabConsoleOn = LoadSprite(LabDir + "Lab_Console_On.png"),
                LabTeslaTower = LoadSprite(LabDir + "Lab_TeslaTower.png"),
                LabBioCapsule = LoadSprite(LabDir + "Lab_BioCapsule.png"),
                LabMonitor = LoadSprite(LabDir + "Lab_Monitor.png"),
                LabServerRack = LoadSprite(LabDir + "Lab_ServerRack.png"),
                LabWorkbench = LoadSprite(LabDir + "Lab_Workbench.png"),
                LabShelves = LoadSprite(LabDir + "Lab_Shelves.png"),
                LabBulbs = new[]
                {
                    LoadSprite(LabDir + "Lab_Bulb_1.png"),
                    LoadSprite(LabDir + "Lab_Bulb_2.png"),
                    LoadSprite(LabDir + "Lab_Bulb_3.png"),
                    LoadSprite(LabDir + "Lab_Bulb_4.png")
                },
                LabLiquidUmbra = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Umbra_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Umbra_2.png")
                },
                LabLiquidAcid = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Acid_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Acid_2.png")
                },
                LabLiquidCryo = new[]
                {
                    LoadSprite(LabDir + "Lab_Liquid_Cryo_1.png"),
                    LoadSprite(LabDir + "Lab_Liquid_Cryo_2.png")
                },
                LabNpcMale = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Male_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Male_Right.png")
                },
                LabNpcFemale = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Female_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Female_Right.png")
                },
                LabNpcHazmat = new[]
                {
                    LoadSprite(LabDir + "LabNPC_Hazmat_Down.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Up.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Left.png"),
                    LoadSprite(LabDir + "LabNPC_Hazmat_Right.png")
                },

                // Sci-Fi Facility Builder Pack 1 Assets
                ScifiFloorLight = LoadSprite(ScifiDir + "Scifi_Floor_Light.png"),
                ScifiFloorShadow = LoadSprite(ScifiDir + "Scifi_Floor_Shadow.png"),
                ScifiFloorGrated = LoadSprite(ScifiDir + "Scifi_Floor_Grated.png"),
                ScifiWallLightLong = LoadSprite(ScifiDir + "Scifi_Wall_Light_Long.png"),
                ScifiWallLightMedium = LoadSprite(ScifiDir + "Scifi_Wall_Light_Medium.png"),
                ScifiWallLightPillar = LoadSprite(ScifiDir + "Scifi_Wall_Light_Pillar.png"),
                ScifiWallShadowLong = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Long.png"),
                ScifiWallShadowMedium = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Medium.png"),
                ScifiWallShadowPillar = LoadSprite(ScifiDir + "Scifi_Wall_Shadow_Pillar.png"),
                ScifiDoorLabClosed = LoadSprite(ScifiDir + "Scifi_Door_Lab_Closed.png"),
                ScifiDoorLabOpen = LoadSprite(ScifiDir + "Scifi_Door_Lab_Open.png"),
                ScifiDoorStdClosed = LoadSprite(ScifiDir + "Scifi_Door_Std_Closed.png"),
                ScifiDoorStdOpen = LoadSprite(ScifiDir + "Scifi_Door_Std_Open.png"),
                ScifiThresholdH = LoadSprite(ScifiDir + "Scifi_Threshold_H.png"),
                ScifiThresholdV = LoadSprite(ScifiDir + "Scifi_Threshold_V.png"),
                ScifiDoorMarking = LoadSprite(ScifiDir + "Scifi_Door_Marking.png"),
                ScifiElecBoxClosed = LoadSprite(ScifiDir + "Scifi_ElecBox_Closed.png"),
                ScifiElecBoxOpen = LoadSprite(ScifiDir + "Scifi_ElecBox_Open.png"),
                ScifiHatchButtonUp = LoadSprite(ScifiDir + "Scifi_Hatch_Button_Up.png"),
                ScifiHatchButtonDown = LoadSprite(ScifiDir + "Scifi_Hatch_Button_Down.png"),
                ScifiCratePushable = LoadSprite(ScifiDir + "Scifi_Crate_Pushable.png"),
                ScifiBenchFilled = LoadSprite(ScifiDir + "Scifi_Bench_Filled.png"),
                ScifiBenchSide = LoadSprite(ScifiDir + "Scifi_Bench_Side.png"),
                ScifiShelfFilled1 = LoadSprite(ScifiDir + "Scifi_Shelf_Filled1.png"),
                ScifiShelfFilled2 = LoadSprite(ScifiDir + "Scifi_Shelf_Filled2.png"),
                ScifiCrateGroup1 = LoadSprite(ScifiDir + "Scifi_Crate_Group1.png"),
                ScifiPipesDouble = LoadSprite(ScifiDir + "Scifi_Pipes_Double.png"),
                ScifiPipesTriple = LoadSprite(ScifiDir + "Scifi_Pipes_Triple.png"),
                ScifiAlarmOff = LoadSprite(ScifiDir + "Scifi_Alarm_Off.png"),
                ScifiAlarmOn = LoadSprite(ScifiDir + "Scifi_Alarm_On.png"),
                ScifiAlarmGlow = LoadSprite(ScifiDir + "Scifi_Alarm_Glow.png"),
                ScifiCameraL = LoadSprite(ScifiDir + "Scifi_Camera_L.png"),
                ScifiCameraR = LoadSprite(ScifiDir + "Scifi_Camera_R.png"),
                ScifiSignBiohazard = LoadSprite(ScifiDir + "Scifi_Sign_Biohazard.png"),
                ScifiSignRadiation = LoadSprite(ScifiDir + "Scifi_Sign_Radiation.png"),
                ScifiSignCaution = LoadSprite(ScifiDir + "Scifi_Sign_Caution.png"),
                ScifiSignDoNotEnter = LoadSprite(ScifiDir + "Scifi_Sign_DoNotEnter.png"),
                ScifiSignDirExit = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Exit.png"),
                ScifiSignDirLab = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Lab.png"),
                ScifiSignDirCtrl = LoadSprite(ScifiDir + "Scifi_Sign_Dir_Ctrl.png"),
                ScifiLabelLab = LoadSprite(ScifiDir + "Scifi_Label_Lab.png"),
                ScifiLabelCtrl = LoadSprite(ScifiDir + "Scifi_Label_Ctrl.png"),
                ScifiLabelMed = LoadSprite(ScifiDir + "Scifi_Label_Med.png"),
                ScifiLabelExit = LoadSprite(ScifiDir + "Scifi_Label_Exit.png"),
                ScifiArrowR = LoadSprite(ScifiDir + "Scifi_Arrow_R.png"),
                ScifiArrowL = LoadSprite(ScifiDir + "Scifi_Arrow_L.png"),
                ScifiArrowUp = LoadSprite(ScifiDir + "Scifi_Arrow_Up.png"),
                ScifiArrowDown = LoadSprite(ScifiDir + "Scifi_Arrow_Down.png"),
                ScifiBlood1 = LoadSprite(ScifiDir + "Scifi_Blood_1.png"),
                ScifiBlood2 = LoadSprite(ScifiDir + "Scifi_Blood_2.png"),
                ScifiHorrorDoorGlow = LoadSprite(ScifiDir + "Scifi_Horror_DoorGlow.png"),

                // New data (Animated Flying Obelisk + PixelLabIcons)
                ObeliskFrames = new[]
                {
                    LoadSprite(NewDataDir + "Obelisk_01.png"),
                    LoadSprite(NewDataDir + "Obelisk_02.png"),
                    LoadSprite(NewDataDir + "Obelisk_03.png"),
                    LoadSprite(NewDataDir + "Obelisk_04.png"),
                    LoadSprite(NewDataDir + "Obelisk_05.png"),
                    LoadSprite(NewDataDir + "Obelisk_06.png"),
                    LoadSprite(NewDataDir + "Obelisk_07.png"),
                    LoadSprite(NewDataDir + "Obelisk_08.png"),
                    LoadSprite(NewDataDir + "Obelisk_09.png"),
                    LoadSprite(NewDataDir + "Obelisk_10.png"),
                    LoadSprite(NewDataDir + "Obelisk_11.png"),
                    LoadSprite(NewDataDir + "Obelisk_12.png"),
                    LoadSprite(NewDataDir + "Obelisk_13.png")
                },
                IconAtom = LoadSprite(NewDataDir + "atom.png"),
                IconBattery = LoadSprite(NewDataDir + "battery.png"),
                IconBulb = LoadSprite(NewDataDir + "bulb.png"),
                IconFlask = LoadSprite(NewDataDir + "flask.png"),
                IconMagnet = LoadSprite(NewDataDir + "magnet.png"),
                IconTestTube = LoadSprite(NewDataDir + "test_tube.png"),

                // PixelNew expansion assets
                OneWayPolarizer = LoadSprite(PixelNewDir + "OneWay_Polarizer.png"),
                RealityAnchorOff = LoadSprite(PixelNewDir + "RealityAnchor_Off.png"),
                RealityAnchorOn = LoadSprite(PixelNewDir + "RealityAnchor_On.png"),
                MagneticRailTrack = LoadSprite(PixelNewDir + "MagneticRail_Track.png"),
                MagneticRailCart = LoadSprite(PixelNewDir + "MagneticRail_Cart.png"),
                SplitTotem = LoadSprite(PixelNewDir + "SplitTotem.png"),
                MemoryPrismIdle = LoadSprite(PixelNewDir + "MemoryPrism_Idle.png"),
                MemoryPrismCharged = LoadSprite(PixelNewDir + "MemoryPrism_Charged.png"),
                MimicMirrorIdle = LoadSprite(PixelNewDir + "MimicMirror_Idle.png"),
                MimicMirrorAwake = LoadSprite(PixelNewDir + "MimicMirror_Awake.png"),
                BeamMoth1 = LoadSprite(PixelNewDir + "BeamMoth_1.png"),
                BeamMoth2 = LoadSprite(PixelNewDir + "BeamMoth_2.png"),
                UmbraMoldTile = LoadSprite(PixelNewDir + "UmbraMold_Tile.png"),
                UmbraWarden1 = LoadSprite(PixelNewDir + "UmbraWarden_1.png"),
                UmbraWarden2 = LoadSprite(PixelNewDir + "UmbraWarden_2.png")
            };
        }

        private static void RegisterScenesInBuildSettings()
        {
            string[] managed =
            {
                MainMenuPath, TutorialPath, Level1Path, Level2Path, Level3Path, Level4Path,
                Level5Path, Level6Path, Level7Path, Level8Path, Level9Path, Level10Path,
                Level11Path, Level12Path, SandboxPath
            };
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

        private static Sprite LoadSprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (subAssets != null)
            {
                foreach (Object obj in subAssets)
                {
                    if (obj is Sprite subSprite) return subSprite;
                }
            }

            Debug.LogError($"OpticsPuzzleBuilder: Failed to load Sprite at '{path}'.");
            return null;
        }
        private static AudioClip LoadAudio(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
