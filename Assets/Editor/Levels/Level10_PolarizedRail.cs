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
        [MenuItem("Shadow Flip/Create Level 10 (Polarized Rail Foundry)")]
        public static void CreateOpticsPuzzleLevel10()
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
                new Color(0.04f, 0.04f, 0.09f, 1f),
                new Color(0.96f, 0.98f, 1.0f, 1f), 1.05f,
                new Color(0.28f, 0.18f, 0.44f, 1f), 0.46f,
                new Vector3(-8.0f, -4.0f, 0f));

            stalker.umbraSafeDuration = 16.0f;
            hud.parFlips = 4;
            hud.lessonTitle = "Level 10: Polarized Rail Foundry";
            hud.levelHint = "Aim the Red Stun Mirror [E] onto the Beam-Eater Moth, open the Blue One-Way Polarizer [E] to glide the Rail Mirror into place, then flip [F] for the Inverted Shadow Sensor!";

            CreateArenaWalls(assets);
            CreateLabSanctuary("Sanctuary_Foundry", new Vector3(-8.2f, 1.2f, 0f), 1.4f, assets);
            CreateLabNPC("Dr_Elena_L10", new Vector3(-8.2f, -1.0f, 0f), "Dr. Elena",
                new[]
                {
                    "That Beam-Eater Moth absorbs the White beam below! Aim Red light at it via Mirror_StunAim [E] to stun it.",
                    "Open the Blue Polarizer [E] so the Rail Mirror glides to (4.5, 1.0), then flip [F] for the Shadow Diode Sensor!"
                },
                assets.LabNpcFemale, assets.LabNpcHazmat, assets);

            // Creeping Umbra Mold patches cleared by lasers
            CreateUmbraMold("UmbraMold_Walkway", new Vector3(-2.5f, 4.0f, 0f), 1.0f, assets);
            CreateUmbraMold("UmbraMold_Center", new Vector3(1.0f, -2.5f, 0f), 1.05f, assets);

            // Upper Rail Power Line: Blue Emitter -> One-Way Polarizer -> Rail Terminal
            CreateEmitter("Emitter_Blue_Rail", new Vector3(-8.5f, 4.0f, 0f), LightColor.Blue, WorldAffinity.Both, Vector2.right, assets);
            CreateOneWayPolarizer("Polarizer_Rail", new Vector3(-5.5f, 4.0f, 0f), Vector2.up, false, true, assets);

            GameObject railCart = CreateMagneticRail("Rail_System", new Vector3(3.5f, 4.0f, 0f),
                new Vector3(2.2f, 1.0f, 0f), new Vector3(4.5f, 1.0f, 0f), LightColor.Blue, assets);
            // Attach 135 deg mirror on the moving cart to bounce vertical bottom emitter LEFT
            BoxCollider2D cartCol = railCart.AddComponent<BoxCollider2D>();
            cartCol.size = new Vector2(1.0f, 1.0f);
            OpticalMirror cartMirror = railCart.AddComponent<OpticalMirror>();
            cartMirror.isTwoSided = true;
            cartMirror.canRotate = false;
            cartMirror.directionalSprites = assets.MirrorDirections;
            cartMirror.logicalAngle = 135f;
            cartMirror.ApplyIsometricVisual();

            // Vertical White Emitter -> Cart Mirror at (4.5, 1.0) -> One-Way Inverter Diode -> Shadow Sensor
            CreateEmitter("Emitter_Vert_Foundry", new Vector3(4.5f, -4.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.up, assets);
            CreateOneWayPolarizer("Diode_Inverter", new Vector3(0.2f, 1.0f, 0f), Vector2.left, true, false, assets);
            LightSensor shadowSensor = CreateSensor("Sensor_Shadow_Foundry", new Vector3(-2.5f, 1.0f, 0f), LightColor.Shadow, WorldAffinity.ShadowOnly, assets);

            // Bottom Moth Gauntlet: Red Emitter -> Mirror -> Stun Beam-Eater Moth -> Bottom White Beam -> Light Sensor
            CreateEmitter("Emitter_Red_MothStun", new Vector3(-8.5f, 2.0f, 0f), LightColor.Red, WorldAffinity.Both, Vector2.right, assets);
            CreateIsometricMirror("Mirror_StunAim", new Vector3(-5.0f, 2.0f, 0f), 45f, 90f, WorldAffinity.Both, assets);

            CreateEmitter("Emitter_Bot_White", new Vector3(-8.5f, -2.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);
            CreateBeamEaterMoth("BeamMoth_Foundry", new Vector3(-5.0f, -2.5f, 0f), LightColor.Red, assets);
            LightSensor lightSensor = CreateSensor("Sensor_Light_Foundry", new Vector3(-2.5f, -2.5f, 0f), LightColor.White, WorldAffinity.Both, assets);
            lightSensor.holdDuration = 1.0f;

            CreateWall("VaultWall_Upper_L10", new Vector3(6.8f, 3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            CreateWall("VaultWall_Lower_L10", new Vector3(6.8f, -3.8f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Long);
            PuzzleDoor foundryDoor = CreateScifiDoor("ScifiDoor_Foundry", new Vector3(6.8f, 0.0f, 0f), new Vector2(1.05f, 2.8f), assets,
                new[] { lightSensor, shadowSensor }, null, null, requireAll: true, stayOpen: false, isLabDoor: true);

            CreateExit("LevelExit", new Vector3(8.8f, 0.0f, 0f), assets, new[] { foundryDoor }, "OpticsPuzzle_Level11");

            EditorSceneManager.SaveScene(scene, Level10Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 10 at " + Level10Path);
        }

    }
}
