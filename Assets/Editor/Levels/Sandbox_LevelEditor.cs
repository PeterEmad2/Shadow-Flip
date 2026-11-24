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
