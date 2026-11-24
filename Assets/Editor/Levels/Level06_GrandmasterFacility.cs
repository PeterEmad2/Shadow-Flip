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
