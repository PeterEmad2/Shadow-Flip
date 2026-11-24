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

    }
}
