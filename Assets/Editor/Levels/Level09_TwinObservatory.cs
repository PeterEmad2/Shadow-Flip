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

    }
}
