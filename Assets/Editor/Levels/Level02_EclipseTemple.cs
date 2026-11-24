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

    }
}
