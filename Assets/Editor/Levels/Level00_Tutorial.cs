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
        [MenuItem("Shadow Flip/Create Level 0 (Tutorial)")]
        public static void CreateTutorialLevel()
        {
            if (IsInPlayModeGuard()) return;
            CelAssets assets = LoadCelAssets();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetupCoreScene(
                assets,
                out logic gameLogic,
                out PlayerMovement playerMovement,
                out PuzzleHUD hud,
                out ShadowStalkerController tutStalker,
                new Color(0.11f, 0.09f, 0.16f, 1f),
                new Color(1f, 0.96f, 0.88f, 1f), 1.05f,
                new Color(0.42f, 0.36f, 0.78f, 1f), 0.58f,
                new Vector3(-8.0f, -2.5f, 0f));

            tutStalker.enabled = false;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 0 (Tutorial): Basics of Shadow & Light";
            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_Tut1", new Vector3(-8.7f, -4.8f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_Tut2", new Vector3(8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateLabProp("LabMonitor_Tut", new Vector3(-7.0f, -5.0f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("LabBioCapsule_Tut", new Vector3(8.8f, 2.8f, 0f), assets.LabBioCapsule, 0.85f, 3, hasCollider: true);
            CreateLabNPC("Dr_Elena_Tut", new Vector3(-8.4f, -0.3f, 0f), "Dr. Elena",
                new[]
                {
                    "Rotate the mirror [E] at the top to hit the White Crystal, then pull the lever below it!",
                    "Notice the glowing phase wall in the center? Press [F] to flip to the Shadow Realm and pass through!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

            CreateEmitter("Emitter_Tut1", new Vector3(-8.5f, 3.0f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            GameObject mirror1 = CreateIsometricMirror("Mirror_Tut1", new Vector3(-6.0f, 3.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor sensor1 = CreateSensor("Sensor_Tut1", new Vector3(-6.0f, -0.5f, 0f), LightColor.White, WorldAffinity.Both, assets, acceptAny: true);
            PuzzleLever lever1 = CreateLever("Lever_Tut", new Vector3(-6.0f, -3.5f, 0f), assets, WorldAffinity.Both);

            CreateWall("TutWall1_Top", new Vector3(-4.0f, 3.2f, 0f), new Vector2(0.85f, 5.2f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("TutWall1_Bottom", new Vector3(-4.0f, -4.0f, 0f), new Vector2(0.85f, 3.6f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door1 = CreateDoor("TutDoor_1", new Vector3(-4.0f, -0.8f, 0f), new Vector2(1.1f, 2.8f), assets,
                new[] { sensor1 }, null, new[] { lever1 }, requireAll: true, stayOpen: true);

            GameObject phaseWall = CreateWall("TutPhaseWall_LightOnly", new Vector3(-1.5f, 0.0f, 0f), new Vector2(0.85f, 11.4f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);

            CreateEmitter("Emitter_Tut2_Shadow", new Vector3(0.5f, 3.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets);
            GameObject mirror2 = CreateIsometricMirror("Mirror_Tut2_Shadow", new Vector3(4.0f, 3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets, requireShadowToRotate: true);
            LightSensor sensor2 = CreateSensor("Sensor_Tut2_Shadow", new Vector3(4.0f, 0.5f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            GameObject blockTut = CreatePushableBlock("RuinBlock_Tut", new Vector3(0.5f, -3.0f, 0f), assets);
            PressurePlate plateTut = CreatePressurePlate("Button_Tut", new Vector3(4.0f, -3.0f, 0f), assets, WorldAffinity.Both, latch: false);

            CreateWall("TutVault_Top", new Vector3(6.5f, 3.0f, 0f), new Vector2(0.85f, 5.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("TutVault_Bottom", new Vector3(6.5f, -4.4f, 0f), new Vector2(0.85f, 2.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door2 = CreateDoor("TutDoor_Final", new Vector3(6.5f, -1.5f, 0f), new Vector2(1.1f, 3.0f), assets,
                new[] { sensor2 }, new[] { plateTut }, null, requireAll: true, stayOpen: false);

            GameObject exitGo = CreateExit("LevelExit", new Vector3(8.5f, -1.5f, 0f), assets, new[] { door1, door2 }, "OpticsPuzzle");

            GameObject tutGo = new GameObject("TutorialManager");
            TutorialManager tut = tutGo.AddComponent<TutorialManager>();
            tut.player = playerMovement;
            tut.gameLogic = gameLogic;
            tut.hud = hud;
            tut.tutorialSensor1 = sensor1;
            tut.tutorialLever1 = lever1;
            tut.tutorialDoor1 = door1;
            tut.crossXThreshold = -0.6f;
            tut.tutorialSensor2 = sensor2;
            tut.tutorialDoor2 = door2;
            tut.stageTargets = new[]
            {
                mirror1.transform,
                phaseWall.transform,
                mirror2.transform,
                blockTut.transform,
                exitGo.transform,
                lever1.transform
            };

            EditorSceneManager.SaveScene(scene, TutorialPath);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Tutorial Level at " + TutorialPath);
        }

    }
}
