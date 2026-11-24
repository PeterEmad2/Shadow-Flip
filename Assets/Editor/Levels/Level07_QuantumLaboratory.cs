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

    }
}
