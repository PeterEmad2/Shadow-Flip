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

    }
}
