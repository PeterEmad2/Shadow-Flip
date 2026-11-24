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
    public enum LevelTheme
    {
        Dungeon,
        Laboratory,
        ScifiFacility
    }

    public static partial class OpticsPuzzleBuilder
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
        private const string Level13Path = "Assets/Scenes/OpticsPuzzle_Level13.unity";
        private const string SandboxPath = "Assets/Scenes/OpticsPuzzle_Sandbox.unity";
        private const string Peter = "Assets/Scenes/Peter.unity";

        internal enum WallTileStyle { Long, Medium, Pillar, Ruined }

        internal struct CelAssets
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
            public Sprite BlackHole1, BlackHole2, CapacitorIdle, CapacitorActive;
        }

        [MenuItem("Shadow Flip/Create All Levels (Main Menu + Levels 0-13 + Sandbox)")]
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
            CreateOpticsPuzzleLevel13();
            CreateSandboxLevel();
            CreateMainMenuScene();
            RegisterScenesInBuildSettings();
            Debug.Log("Created Main Menu, Levels 0-13, and Sandbox Level and registered all 16 in Build Settings.");
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

        internal static void SetupCoreScene(
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

        internal static bool IsInPlayModeGuard()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode) return false;
            EditorUtility.DisplayDialog("Stop Play Mode", "Please exit Play Mode before generating puzzle scenes.", "OK");
            return true;
        }

        [MenuItem("Shadow Flip/Level Designer/Sync All Scenes to Main Menu & Build Settings")]
        public static void RegisterScenesInBuildSettings()
        {
            string[] managed =
            {
                MainMenuPath, TutorialPath, Level1Path, Level2Path, Level3Path, Level4Path,
                Level5Path, Level6Path, Level7Path, Level8Path, Level9Path, Level10Path,
                Level11Path, Level12Path, Level13Path, SandboxPath
            };
            var scenes = new List<EditorBuildSettingsScene>();
            foreach (string path in managed) AddSceneIfMissing(scenes, path);

            // Auto-discover any custom scenes under Assets/Scenes/ (such as Peter.unity or newly created levels)
            // so they appear in Build Settings and PuzzleHUD's Main Menu automatically.
            if (Directory.Exists("Assets/Scenes"))
            {
                string[] allScenes = Directory.GetFiles("Assets/Scenes", "*.unity", SearchOption.TopDirectoryOnly);
                System.Array.Sort(allScenes);
                for (int i = 0; i < allScenes.Length; i++)
                {
                    AddSceneIfMissing(scenes, allScenes[i].Replace('\\', '/'));
                }
            }

            foreach (var existing in EditorBuildSettings.scenes)
            {
                if (!File.Exists(existing.path)) continue;
                AddSceneIfMissing(scenes, existing.path.Replace('\\', '/'));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        internal static void AddSceneIfMissing(List<EditorBuildSettingsScene> scenes, string path)
        {
            if (!File.Exists(path)) return;
            foreach (var s in scenes)
            {
                if (s.path == path) return;
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }

    }
}
