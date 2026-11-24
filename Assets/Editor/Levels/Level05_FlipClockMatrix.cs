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

    }
}
