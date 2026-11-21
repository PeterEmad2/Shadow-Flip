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
    public static partial class OpticsPuzzleBuilder
    {
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
                out ShadowStalkerController tutStalker,
                new Color(0.11f, 0.09f, 0.16f, 1f),
                new Color(1f, 0.96f, 0.88f, 1f), 1.05f,
                new Color(0.42f, 0.36f, 0.78f, 1f), 0.58f,
                new Vector3(-8.0f, -2.5f, 0f));

            tutStalker.enabled = false;
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
            GameObject mirror2 = CreateIsometricMirror("Mirror_Tut2_Shadow", new Vector3(4.0f, 3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);
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
                out ShadowStalkerController stalker,
                new Color(0.08f, 0.09f, 0.13f, 1f),
                new Color(1f, 0.98f, 0.92f, 1f), 1.0f,
                new Color(0.45f, 0.52f, 0.85f, 1f), 0.55f,
                new Vector3(-7.8f, -3.5f, 0f));

            stalker.umbraSafeDuration = 18.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 1: Chromatic Vault";
            hud.levelHint = "Align mirrors & RGB_Prism [E] in Light Realm, push the Crate onto the Vault Button, pull the Green Shutter Lever, then flip [F] to Shadow Realm!";

            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_L1_Top", new Vector3(-8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_L1_Vault", new Vector3(8.7f, 4.6f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateLabProp("LabMonitor_L1", new Vector3(-7.0f, 4.7f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("LabServerRack_L1", new Vector3(8.7f, -4.4f, 0f), assets.LabServerRack, 0.85f, 3, hasCollider: true);
            CreateLabNPC("Dr_Vance_L1", new Vector3(-7.5f, -0.8f, 0f), "Dr. Vance",
                new[]
                {
                    "Interactions and pushing crates only work in the Light Realm! Align all mirrors & RGB_Prism and weigh down the Vault Button first.",
                    "Then flip [F] to the Shadow Realm so the beams pass the LightOnly Shutter and strike Sensor_Blue!"
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
            CreateIsometricMirror("Mirror_Green", new Vector3(2.2f, 1.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor greenSensor = CreateSensor("Sensor_Green", new Vector3(2.2f, -1.2f, 0f), LightColor.Green, WorldAffinity.Both, assets);

            PuzzleLever greenLever = CreateLever("Lever_GreenShutter", new Vector3(-1.0f, -2.2f, 0f), assets, WorldAffinity.Both,
                toggledObjects: new[] { greenBarrier });

            GameObject mirrorRed1 = CreateIsometricMirror("Mirror_Red1", new Vector3(-3.5f, -3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            mirrorRed1.GetComponent<OpticalMirror>().requireLightWorldToRotate = true;
            mirrorRed1.GetComponent<SpriteRenderer>().color = new Color(1f, 0.88f, 0.55f, 1f);

            CreateIsometricMirror("Mirror_Red2", new Vector3(4.5f, -3.5f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            LightSensor redSensor = CreateSensor("Sensor_Red", new Vector3(4.5f, -0.8f, 0f), LightColor.Red, WorldAffinity.Both, assets);

            CreatePushableBlock("PushableBlock_L1", new Vector3(-1.6f, 2.8f, 0f), assets);
            PressurePlate vaultButton = CreatePressurePlate("Button_VaultSeal", new Vector3(0.5f, 2.8f, 0f), assets, WorldAffinity.Both, latch: false);

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
                out ShadowStalkerController stalker,
                new Color(0.05f, 0.04f, 0.09f, 1f),
                new Color(1f, 0.91f, 0.70f, 1f), 1.15f,
                new Color(0.32f, 0.22f, 0.58f, 1f), 0.48f,
                new Vector3(-7.5f, 0f, 0f));

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 2: Eclipse Temple";
            hud.levelHint = "Light Realm: route Red, push both crates onto the Floor Plates & pull the Lever; flip [F] to Umbra to rotate the violet Shadow Mirrors [E]!";

            CreateArenaWalls(assets);
            CreateLabNPC("Dr_Elena_L2", new Vector3(-7.8f, -2.0f, 0f), "Dr. Elena",
                new[]
                {
                    "Pressure plates only stay active while weighed down! Push crates onto the plates and pull the Lever in Light Mode first.",
                    "Then flip [F] into Umbra Mode to rotate the violet Shadow Mirrors (Mirror_S1 & Mirror_S2) with [E]!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

            CreateEmitter("LightOnly_Emitter", new Vector3(-8.5f, 3.8f, 0f), LightColor.White, WorldAffinity.LightOnly, Vector2.right, assets);
            CreateIsometricMirror("Mirror_L1", new Vector3(-5.2f, 3.8f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreatePrism("Splitter_Prism", new Vector3(-5.2f, 0.5f, 0f), -90f, OpticalPrism.PrismMode.Splitter, 90f, true, false, 90f, assets);
            CreateIsometricMirror("Mirror_L2", new Vector3(-2.8f, 0.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateColorFilter("RedFilter", new Vector3(-2.8f, -1.2f, 0f), new Vector2(1.1f, 0.45f), LightColor.Red, assets);
            CreateIsometricMirror("Mirror_L3", new Vector3(-2.8f, -2.8f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor stage1Sensor = CreateSensor("Sensor_Stage1_Red", new Vector3(4.2f, -2.8f, 0f), LightColor.Red, WorldAffinity.LightOnly, assets);
            CreatePushableBlock("PushableBlock_L2_Light", new Vector3(-0.4f, -1.6f, 0f), assets);
            PressurePlate lightButton = CreatePressurePlate("Light_Button", new Vector3(1.5f, -1.6f, 0f), assets, WorldAffinity.Both, latch: false);

            CreateEmitter("ShadowOnly_Emitter", new Vector3(-8.5f, -4.3f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            CreateIsometricMirror("Mirror_S1", new Vector3(-1.2f, -4.3f, 0f), 135f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);

            GameObject mirrorS2Go = CreateIsometricMirror("Mirror_S2", new Vector3(-1.2f, 4.3f, 0f), 135f, 90f, WorldAffinity.ShadowOnly, assets, canRotate: true, requireShadowToRotate: true);
            OpticalMirror mirrorS2 = mirrorS2Go.GetComponent<OpticalMirror>();
            CreateWall("AlcoveWall_Left", new Vector3(-2.4f, 4.5f, 0f), new Vector2(0.65f, 2.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("AlcoveWall_Ruined", new Vector3(0.2f, 5.1f, 0f), new Vector2(1.1f, 0.9f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            PuzzleLever remoteLever = CreateLever("Lever_RemoteMirror", new Vector3(-5.2f, -2.8f, 0f), assets, WorldAffinity.Both,
                controlledMirrors: new[] { mirrorS2 });

            LightSensor shadowSensor = CreateSensor("Sensor_Stage2_Shadow", new Vector3(4.2f, 4.3f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            CreateWall("ChannelWall_Top", new Vector3(3.0f, 3.55f, 0f), new Vector2(6.4f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("ChannelWall_Bottom", new Vector3(3.0f, 0.15f, 0f), new Vector2(6.4f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("ShadowOnly_ChannelGate", new Vector3(0.4f, 1.85f, 0f), new Vector2(0.75f, 2.5f), assets, WorldAffinity.ShadowOnly, WallTileStyle.Pillar);

            CreatePushableBlock("PushableBlock", new Vector3(-2.5f, 1.85f, 0f), assets);
            PressurePlate shadowPlate = CreatePressurePlate("Shadow_PressurePlate", new Vector3(3.6f, 1.85f, 0f), assets, WorldAffinity.Both, latch: false);

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
                out ShadowStalkerController stalker,
                new Color(0.06f, 0.08f, 0.14f, 1f),
                new Color(1f, 0.96f, 0.86f, 1f), 1.05f,
                new Color(0.35f, 0.30f, 0.68f, 1f), 0.52f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 3: The Prism Sanctum";
            hud.levelHint = "Light Mode: combine Red+Blue & rotate PortalMirror_B [E]; in Umbra [F], cross Divider Gate, push & aim the violet Umbra Cart to charge the Capacitor, then flip to Light to exit!";

            CreateArenaWalls(assets);
            CreateLabNPC("Dr_Vance_L3", new Vector3(-7.8f, -1.2f, 0f), "Dr. Vance",
                new[]
                {
                    "Twin-coil Capacitor Receivers ONLY charge in the Umbra (Dark) Side, then slowly discharge!",
                    "Charge it in Umbra [F], then flip back to Light Mode so the Shadow Shutter vanishes before the capacitor drains!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

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

            // East Chamber: Backstop wall + Umbra Pushable Mirror Cart + Umbra Capacitor Crystal
            CreateWall("Cart_Backstop", new Vector3(4.2f, 4.88f, 0f), new Vector2(1.4f, 0.7f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreatePushableMirrorCart("PushableMirrorCart", new Vector3(4.2f, 1.8f, 0f), 45f, 90f, assets, umbraOnly: true);

            LightSensor capSensor = CreateSensor("Sensor_Capacitor_7s", new Vector3(4.2f, -0.2f, 0f), LightColor.White, WorldAffinity.ShadowOnly, assets, isCapacitor: true, holdDuration: 11.0f);

            // Inner Shutter corridor before PuzzleDoor
            CreateWall("Shutter_Corridor_Top", new Vector3(5.95f, -0.4f, 0f), new Vector2(1.7f, 0.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("Shutter_Corridor_Bot", new Vector3(5.95f, -3.6f, 0f), new Vector2(1.7f, 0.6f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("InnerShutter_ShadowOnly", new Vector3(5.4f, -2.0f, 0f), new Vector2(0.65f, 2.6f), assets, WorldAffinity.ShadowOnly, WallTileStyle.Pillar);

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

            stalker.umbraSafeDuration = 12.0f;
            stalker.canBePetrifiedByLaser = true;
            stalker.petrifyExposureTime = 1.1f;

            hud.parFlips = 4;
            hud.lessonTitle = "Level 4: Umbra Labyrinth";
            hud.levelHint = "Record a Light Echo [E] holding the Light Button, step outside the Tesla Sanctuary in Shadow Realm to lure & petrify the Stalker, then push its Statue in Light Mode onto the Final Button!";

            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_L4", new Vector3(4.5f, 4.5f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            // Tesla Coil Sanctuary in bottom-left
            CreateLabSanctuary("Lantern_Sanctuary", new Vector3(-7.5f, -3.0f, 0f), 1.4f, assets);
            CreateLabNPC("Dr_Elena_L4", new Vector3(-4.8f, -3.6f, 0f), "Dr. Elena",
                new[]
                {
                    "Only the Tesla Coil's glowing perimeter keeps you safe from the Umbra Stalker!",
                    "Petrify the Stalker in the central laser, then flip to Light Mode [F] to push its stone Statue onto Button_Statue."
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

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
                out ShadowStalkerController stalker,
                new Color(0.06f, 0.07f, 0.13f, 1f),
                new Color(1f, 0.96f, 0.86f, 1f), 1.05f,
                new Color(0.32f, 0.25f, 0.64f, 1f), 0.50f,
                new Vector3(-8.0f, -3.5f, 0f));

            stalker.umbraSafeDuration = 18.0f;
            hud.parFlips = 5;
            hud.lessonTitle = "Level 5: The Flip-Clock Matrix";
            hud.levelHint = "Power the Forcefield with the Turret, push the Crate onto PhasePad_In in Light Mode to teleport it onto the Vault Button, then sync the Turret in Umbra Mode to open the Exit!";

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
                    "Phase Teleport Pads can now warp Pushable Crates too! Lower the Forcefield, then push the crate into PhasePad_In in Light Mode.",
                    "Follow the crate through the teleporter, push it onto the Vault Button, return via the second pad, and align the Flip-Clock Turret in Umbra!"
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
            CreateIsometricMirror("Mirror_E1", new Vector3(3.8f, 0.5f, 0f), 135f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);
            LightSensor rightSensor = CreateSensor("Sensor_Right", new Vector3(3.8f, 4.2f, 0f), LightColor.White, WorldAffinity.ShadowOnly, assets);

            // Down -> Sealed Alcove guarded by ShadowBarrierGate at (0.5, -1.8)
            CreateWall("Alcove_Left", new Vector3(-1.0f, -3.8f, 0f), new Vector2(0.9f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Alcove_Right", new Vector3(2.0f, -3.8f, 0f), new Vector2(0.9f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateShadowBarrierGate("Forcefield_Gate", new Vector3(0.5f, -1.8f, 0f), new Vector2(2.1f, 0.9f), assets, openWhenLit: true, stayOpenOnceLit: true);

            // Pushable Block inside Alcove + Teleport Pads (pushable blocks enabled!) -> Upper-Right Vault Button
            CreatePushableBlock("PushableBlock_Vault", new Vector3(0.5f, -3.0f, 0f), assets);
            PhaseShiftTile padOut = CreatePhaseShiftTile("PhasePad_Vault", new Vector3(8.9f, 4.3f, 0f), null, false, assets);
            CreatePhaseShiftTile("PhasePad_In", new Vector3(0.5f, -4.3f, 0f), padOut, false, assets, allowPushableBlocks: true);

            PhaseShiftTile padReturnTarget = CreatePhaseShiftTile("PhasePad_ReturnTarget", new Vector3(4.5f, -3.8f, 0f), null, false, assets);
            CreatePhaseShiftTile("PhasePad_Return", new Vector3(7.4f, 2.5f, 0f), padReturnTarget, false, assets, allowPushableBlocks: true);

            PressurePlate vaultButton = CreatePressurePlate("Button_Vault", new Vector3(8.9f, 2.5f, 0f), assets, WorldAffinity.Both, latch: false);

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

            stalker.umbraSafeDuration = 18.0f;
            hud.parFlips = 5;
            hud.lessonTitle = "Level 6: Grandmaster Facility";
            hud.levelHint = "Free & push the Mirror Cart in Light Mode, flip [F] so the Realm Inverter converts the beam to Shadow to charge the Upper Umbra Capacitor, then flip back to Light to light Sensor_Light_Grand!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_NorthWest", new Vector3(-7.5f, 3.5f, 0f), 1.4f, assets);

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
                    "Push the Mirror Cart in Light Mode to aim at the Upper Umbra Capacitor first, then flip [F] to charge it via the Realm Inverter!",
                    "While the Capacitor slowly discharges, flip back to Light Mode to push the cart down onto Sensor_Light_Grand!"
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
            CreateShadowBarrierGate("Cage_Forcefield", new Vector3(2.3f, -3.8f, 0f), new Vector2(0.8f, 2.5f), assets, openWhenLit: true, stayOpenOnceLit: true);
            CreateWall("Cage_Top", new Vector3(4.0f, -2.1f, 0f), new Vector2(3.8f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateWall("Cage_Right", new Vector3(5.7f, -3.7f, 0f), new Vector2(0.65f, 3.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("Cage_Bot", new Vector3(4.0f, -5.3f, 0f), new Vector2(3.8f, 0.65f), assets, WorldAffinity.Both, WallTileStyle.Medium);

            CreatePushableMirrorCart("PushableMirrorCart", new Vector3(3.6f, -3.8f, 0f), 45f, 90f, assets);

            // Dual Target Sensors on x = 4.8 (Umbra 11s Capacitor at y = 2.0, Light Receiver at y = -0.2)
            LightSensor umbraCapSensor = CreateSensor("Sensor_Capacitor_7s", new Vector3(4.8f, 2.0f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets, isCapacitor: true, holdDuration: 11.0f);
            LightSensor lightFinalSensor = CreateSensor("Sensor_Light_Grand", new Vector3(4.8f, -0.2f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);

            // Grandmaster Vault at x = 6.5 using Sci-Fi Lab Sliding Door + Threshold
            CreateWall("VaultWall_Upper", new Vector3(6.5f, 3.9f, 0f), new Vector2(0.85f, 4.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -3.2f, 0f), new Vector2(0.85f, 5.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor grandDoor = CreateScifiDoor("PuzzleDoor_Grandmaster", new Vector3(6.5f, 0.8f, 0f), new Vector2(1.1f, 2.4f), assets,
                new[] { umbraCapSensor, lightFinalSensor }, null, null, requireAll: true, stayOpen: false, isLabDoor: true);

            CreateExit("LevelExit", new Vector3(8.6f, 0.8f, 0f), assets, new[] { grandDoor }, "OpticsPuzzle_Level7");

            EditorSceneManager.SaveScene(scene, Level6Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 6 at " + Level6Path);
        }

    }
}
