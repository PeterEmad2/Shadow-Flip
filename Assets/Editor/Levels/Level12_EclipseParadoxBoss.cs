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

    }
}
