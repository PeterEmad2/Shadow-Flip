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

            stalker.umbraSafeDuration = 18.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 8: Omega Containment Facility";
            hud.levelHint = "Flip the Electrical Breaker [E] to lower the Containment Shutter, flip [F] to rotate the Turret & phase through the Security Gate, align Mirror_East_Aux [E], and push the Storage Crate onto the Floor Hatch!";

            CreateArenaWalls(assets);

            // Grated Engineering Walkway & Directional Floor Arrows
            CreateFloorAccent("ScifiFloor_Engineering", new Vector3(3.2f, 0.0f, 0f), new Vector2(4.0f, 10.4f), assets.ScifiFloorGrated, -18);
            CreateLabProp("FloorArrow_Gate", new Vector3(-2.6f, -2.5f, 0f), assets.ScifiArrowR, 1.8f, -15);
            CreateLabProp("FloorArrow_Airlock", new Vector3(4.8f, 0.0f, 0f), assets.ScifiArrowR, 1.8f, -15);

            // Tesla Sanctuary in NW corner (reduced radius)
            CreateLabSanctuary("Sanctuary_Tesla_L8", new Vector3(-8.0f, 3.8f, 0f), 1.4f, assets);

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
            CreatePortalMirror("PortalMirror_A_L8", new Vector3(-2.15f, 1.5f, 0f), assets.PortalMirrorA, Vector2.right, portalB, canRotate: false);

            // East Sector: 4-Way Relay Crystal -> Top White Sensor + Bottom Mirror_East_Aux -> Shadow Sensor + Pushable Crate -> Hatch Button
            CreateBeamRelay("Relay_4Way_L8", new Vector3(2.5f, 1.5f, 0f), BeamRelayCrystal.RelayMode.CrossRepeater4Way, Vector2.right, false, assets);
            LightSensor topSensor = CreateSensor("Sensor_Facility_Top", new Vector3(2.5f, 4.2f, 0f), LightColor.White, WorldAffinity.Both, assets);

            CreateIsometricMirror("Mirror_East_Aux", new Vector3(2.5f, -2.5f, 0f), 45f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);
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

    }
}
