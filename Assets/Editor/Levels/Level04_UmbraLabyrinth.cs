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

    }
}
