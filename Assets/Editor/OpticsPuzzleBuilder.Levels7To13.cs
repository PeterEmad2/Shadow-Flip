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

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 7: Quantum Laboratory";
            hud.levelHint = "Light Mode: talk [E] to Dr. Vance & align Mirror_Red. Flip [F] past the Phase Gate & back to Light to talk to Unit-7, push the Crate & pull the Console, then flip to Shadow for the Cryo Beam!";

            CreateArenaWalls(assets);

            // Floor accents: Grating corridor & Hazard airlock zone (visual layer above main floor, below objects)
            CreateFloorAccent("LabFloor_GratingAisle", new Vector3(1.5f, 0.0f, 0f), new Vector2(3.6f, 9.6f), assets.LabFloorGrating, -18);
            CreateFloorAccent("LabFloor_HazardAirlock", new Vector3(6.75f, 0.0f, 0f), new Vector2(2.6f, 2.8f), assets.LabFloorHazard, -17);

            // Tesla Coil Sanctuary in NW corner (reduced radius)
            CreateLabSanctuary("Sanctuary_Tesla", new Vector3(-7.8f, 3.8f, 0f), 1.4f, assets);

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

            // NPC 2: Unit-7 in East Wing (talk [E] in Light Mode -> rotates Cryo Mirror; wears Chemistry Lab Uniform in Umbra)
            LabNPCController unit7 = CreateLabNPC("Hazmat_Unit7_L7", new Vector3(1.5f, 1.8f, 0f), "Tech Unit-7",
                new[]
                {
                    "Remote calibration complete! Bio-Pod Cryo Mirror locked onto the Shadow receiver below.",
                    "Push the crate onto the button and pull the Security Console in Light Mode, then flip [F] to power the Shadow receiver!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets, usableWorld: WorldAffinity.Both);
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

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 9: Twin-Mirror Observatory";
            hud.levelHint = "Rotate the Linked West Mirror [E] in Light Mode, flip [F] across the Phase Gate, flip back to Light Mode to rotate [E] MimicMirror_Obs, then flip [F] to pin & power the East Sensor!";

            CreateArenaWalls(assets);

            // Center observatory divider (walkable at bottom only in Shadow Realm)
            CreateWall("Observatory_Div_Top", new Vector3(0.0f, 2.8f, 0f), new Vector2(0.85f, 5.4f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("Observatory_PhaseGate", new Vector3(0.0f, -2.6f, 0f), new Vector2(0.85f, 5.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Long);

            // West Wing: White Emitter -> Linked Twin Mirror A -> Memory Delay Prism -> West Sensor
            CreateEmitter("Emitter_Obs_Light", new Vector3(-8.5f, 3.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            GameObject twinAGo = CreateIsometricMirror("Mirror_Twin_Light", new Vector3(-4.5f, 3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateMemoryPrism("MemoryPrism_Obs", new Vector3(-4.5f, 0.5f, 0f), 14.0f, Vector2.down, false, assets);
            LightSensor westSensor = CreateSensor("Sensor_Obs_West", new Vector3(-4.5f, -2.6f, 0f), LightColor.White, WorldAffinity.Both, assets);

            // East Wing: Shadow Emitter -> Linked Twin Mirror B -> Mimic Mirror -> East Shadow Sensor
            CreateEmitter("Emitter_Obs_Shadow", new Vector3(1.5f, 3.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            GameObject twinBGo = CreateIsometricMirror("Mirror_Twin_Shadow", new Vector3(4.0f, 3.5f, 0f), 45f, 90f, WorldAffinity.ShadowOnly, assets, canRotate: false);
            twinAGo.GetComponent<OpticalMirror>().linkedTwinMirror = twinBGo.GetComponent<OpticalMirror>();

            CreateUmbraMimicMirror("MimicMirror_Obs", new Vector3(4.0f, -1.5f, 0f), 45f, new Vector3(0f, -2.5f, 0f), assets);
            LightSensor eastSensor = CreateSensor("Sensor_Obs_East", new Vector3(5.8f, -1.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            CreateLabNPC("Dr_Vance_L9", new Vector3(-7.6f, 1.0f, 0f), "Dr. Vance",
                new[]
                {
                    "Linked Twin Mirrors synchronize rotations across worlds! Rotating Mirror_Twin_Light steers Mirror_Twin_Shadow too.",
                    "In the East Wing, return to Light Mode to rotate [E] MimicMirror_Obs before flipping [F] to pin it with the Shadow Beam!"
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

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 10: Polarized Rail Foundry";
            hud.levelHint = "Aim the Red Stun Mirror [E] onto the Beam-Eater Moth, open the Blue One-Way Polarizer [E] to glide the Rail Mirror into place, then flip [F] for the Inverted Shadow Sensor!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_Foundry", new Vector3(-8.2f, 1.2f, 0f), 1.4f, assets);
            CreateLabNPC("Dr_Elena_L10", new Vector3(-8.2f, -1.0f, 0f), "Dr. Elena",
                new[]
                {
                    "That Beam-Eater Moth absorbs the White beam below! Aim Red light at it via Mirror_StunAim [E] to stun it.",
                    "Open the Blue Polarizer [E] so the Rail Mirror glides to (4.5, 1.0), then flip [F] for the Shadow Diode Sensor!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

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

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 11: The Reality Anchor Rift";
            hud.levelHint = "Summon your Mirror Twin [E] onto Button_Twin_East while the beam powers Sensor_Rift to open Door_Rift_Mid! Cross through the Door and push the crate onto Button_Bubble to exit.";

            CreateArenaWalls(assets);

            CreateLabNPC("Dr_Vance_L11", new Vector3(-7.8f, -1.0f, 0f), "Dr. Vance",
                new[]
                {
                    "Only Door_Rift_Mid lets you cross the sealed dividing wall to the East chamber!",
                    "Summon your Mirror Twin [E] onto Button_Twin_East while MemoryPrism_Rift feeds the Reality Anchor and Sensor_Rift, then cross the Door and weigh down Button_Bubble with the crate!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

            // Sealed Dividing Wall along x = 0: ONLY Door_Rift_Mid (y = -4.0) lets Jimmy cross to x > 0!
            CreateWall("Div_Top_L11", new Vector3(0.0f, 4.45f, 0f), new Vector2(0.9f, 1.9f), assets, WorldAffinity.Both, WallTileStyle.Long);
            GameObject beamWindow = CreateOneWayPolarizer("Window_BeamGlass_L11", new Vector3(0.0f, 3.0f, 0f), Vector2.right, invertColor: false, canRotate: false, assets);
            BoxCollider2D bwCol = beamWindow.GetComponent<BoxCollider2D>();
            if (bwCol != null) bwCol.size = new Vector2(0.95f, 1.25f);
            CreateWall("Div_Mid_L11", new Vector3(0.0f, -0.05f, 0f), new Vector2(0.9f, 5.1f), assets, WorldAffinity.Both, WallTileStyle.Long);

            // West Chamber: Twin Totem + White Emitter -> Memory Prism (10s hold across beam window into Reality Anchor!)
            CreateMirrorTwinTotem("Totem_MirrorTwin", new Vector3(-4.5f, 1.5f, 0f), 0f, playerMovement.DarkspriteDown, assets);
            PressurePlate twinEastButton = CreatePressurePlate("Button_Twin_East", new Vector3(4.5f, -2.0f, 0f), assets, WorldAffinity.Both, latch: false);

            CreateEmitter("Emitter_Rift_West", new Vector3(-8.5f, 3.0f, 0f), LightColor.White, WorldAffinity.LightOnly, Vector2.right, assets);
            CreateMemoryPrism("MemoryPrism_Rift", new Vector3(-2.2f, 3.0f, 0f), 10.0f, Vector2.right, false, assets);

            // East Chamber: Reality Anchor Lantern + LightOnly Sensor + Crate -> LightOnly Bubble Button
            CreateRealityAnchor("RealityAnchor_East", new Vector3(3.2f, 3.0f, 0f), 3.9f, 8.0f, assets);
            LightSensor riftSensor = CreateSensor("Sensor_Rift_LightOnly", new Vector3(5.2f, 3.0f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);
            CreatePushableBlock("Block_East_L11", new Vector3(1.8f, 0.8f, 0f), assets);
            PressurePlate bubbleButton = CreatePressurePlate("Button_Bubble_LightOnly", new Vector3(3.2f, 0.8f, 0f), assets, WorldAffinity.LightOnly, latch: false);

            PuzzleDoor midDoor = CreateLabBlastDoor("Door_Rift_Mid", new Vector3(0.0f, -4.0f, 0f), new Vector2(0.9f, 2.8f), assets,
                new[] { riftSensor }, new[] { twinEastButton }, null, requireAll: true, stayOpen: true, vertical: true, redVariant: false);

            // Exit Airlock at x = 6.8
            CreateWall("ExitWall_Top_L11", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("ExitWall_Bot_L11", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor finalDoor = CreateLabBlastDoor("Door_Rift_Exit", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { riftSensor }, new[] { bubbleButton }, null, requireAll: true, stayOpen: false, vertical: true, redVariant: true);

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

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 2;
            hud.lessonTitle = "Level 12: Eclipse Paradox [BOSS]";
            hud.levelHint = "Light Mode: open Red Polarizer [E] to shatter Shield 1, and pre-align both Top Green & East Blue Mirrors [E]! Then flip [F] to Umbra to shatter Shields 2 & 3!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_Boss", new Vector3(-7.8f, 3.8f, 0f), 1.4f, assets);
            CreateLabNPC("Chief_Engineer_Mara_L12", new Vector3(-7.6f, 0.8f, 0f), "Chief Engineer Mara",
                new[]
                {
                    "Wait! The Warden's shields drop in order: RED -> GREEN -> BLUE.",
                    "Open Polarizer_Boss_Red [E] & align Mirror_Boss_Green and Mirror_Boss_Blue in Light Mode first, then flip [F] to Umbra for the Green Emitter!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

            UmbraWardenBoss warden = CreateUmbraWardenBoss("UmbraWarden_Boss", new Vector3(0.0f, 0.5f, 0f), assets);

            // 1. RED Beam (West -> Boss)
            CreateEmitter("Emitter_Boss_Red", new Vector3(-8.5f, 0.5f, 0f), LightColor.Red, WorldAffinity.Both, Vector2.right, assets);
            CreateOneWayPolarizer("Polarizer_Boss_Red", new Vector3(-5.0f, 0.5f, 0f), Vector2.up, false, true, assets);

            // 2. GREEN Beam (Top ShadowOnly Emitter -> Umbra Mirror -> Green Filter -> Boss)
            CreateEmitter("Emitter_Boss_Green", new Vector3(-3.5f, 4.2f, 0f), LightColor.White, WorldAffinity.ShadowOnly, Vector2.right, assets);
            CreateIsometricMirror("Mirror_Boss_Green", new Vector3(0.0f, 4.2f, 0f), 45f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);
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

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { bossDoor }, "OpticsPuzzle_Level13");

            EditorSceneManager.SaveScene(scene, Level12Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 12 Boss at " + Level12Path);
        }

        [MenuItem("Shadow Flip/Create Level 13 (Event Horizon Singularity)")]
        public static void CreateOpticsPuzzleLevel13()
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
                new Color(0.03f, 0.02f, 0.07f, 1f),
                new Color(0.95f, 0.96f, 1.0f, 1f), 1.05f,
                new Color(0.24f, 0.16f, 0.50f, 1f), 0.46f,
                new Vector3(-8.0f, -1.5f, 0f));

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 2;
            hud.lessonTitle = "Level 13: Event Horizon Singularity";
            hud.levelHint = "Light Mode: aim Mirror_BH_Top [E] & tune Bottom Black Hole [E] to LENS. Flip [F] to Umbra so BlackHole_Shadow charges the Capacitor, then flip back to Light while it discharges!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_EventHorizon", new Vector3(-8.0f, 1.0f, 0f), 1.4f, assets);

            CreateLabNPC("Dr_Elena_L13", new Vector3(-6.8f, -1.2f, 0f), "Dr. Elena",
                new[]
                {
                    "Tune [E] the bottom Singularity to LENS in Light Mode, aim Mirror_BH_Top [E], then flip [F] to Umbra so BlackHole_Shadow bends the beam into the Capacitor!",
                    "While the Capacitor slowly discharges, flip back to Light Mode so Sensor_Straight_Light activates too!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

            // Top Lane: Straight Light Receiver in Light -> Lensed DOWN into Umbra Capacitor in Shadow
            CreateEmitter("Emitter_BH_Top", new Vector3(-6.5f, 5.05f, 0f), LightColor.White, WorldAffinity.Both, Vector2.down, assets);
            CreateIsometricMirror("Mirror_BH_Top", new Vector3(-6.5f, 4.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            CreateBlackHole("BlackHole_Shadow", new Vector3(-3.0f, 2.0f, 0f), 3.3f, 0.48f, 2.8f, WorldAffinity.ShadowOnly, canAdjust: false, startPreset: 1, assets: assets);
            CreateWall("Wall_ShadowPillar", new Vector3(1.5f, 4.0f, 0f), new Vector2(0.85f, 1.2f), assets, WorldAffinity.ShadowOnly, WallTileStyle.Pillar);

            LightSensor straightLight = CreateSensor("Sensor_Straight_Light", new Vector3(4.5f, 4.0f, 0f), LightColor.White, WorldAffinity.LightOnly, assets);
            LightSensor lensedCapShadow = CreateSensor("Sensor_Lensed_Capacitor", new Vector3(4.5f, 1.64f, 0f), LightColor.White, WorldAffinity.ShadowOnly, assets, isCapacitor: true, holdDuration: 11.0f);

            // Bottom Lane: Starts Overdrive (swallows Blue); [E] tunes to LENS (2.8f), arcing Blue beam over blocker wall
            CreateEmitter("Emitter_BH_Bot", new Vector3(-8.5f, -4.0f, 0f), LightColor.Blue, WorldAffinity.Both, Vector2.right, assets);
            CreateBlackHole("BlackHole_Tunable", new Vector3(-3.0f, -2.0f, 0f), 3.3f, 0.48f, 6.2f, WorldAffinity.Both, canAdjust: true, startPreset: 0, assets: assets);
            CreateWall("Wall_GravityBlocker", new Vector3(1.5f, -4.0f, 0f), new Vector2(0.85f, 1.2f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            LightSensor blueSensor = CreateSensor("Sensor_BH_Blue", new Vector3(4.5f, -1.64f, 0f), LightColor.Blue, WorldAffinity.Both, assets);

            // Singularity Exit Vault at x = 6.8
            CreateWall("VaultWall_Top_L13", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("VaultWall_Bot_L13", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor ehDoor = CreateLabBlastDoor("Door_EventHorizon", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { straightLight, lensedCapShadow, blueSensor }, null, null, requireAll: true, stayOpen: false, vertical: true, redVariant: false);

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { ehDoor }, "MainMenu");

            EditorSceneManager.SaveScene(scene, Level13Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 13 (Event Horizon Singularity) at " + Level13Path);
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

    }
}
