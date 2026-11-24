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
        [MenuItem("Shadow Flip/Create Level 1 (Chromatic Vault)")]
        public static void CreateOpticsPuzzleLevel()
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
                new Color(0.08f, 0.09f, 0.13f, 1f),
                new Color(1f, 0.98f, 0.92f, 1f), 1.0f,
                new Color(0.45f, 0.52f, 0.85f, 1f), 0.55f,
                new Vector3(-7.8f, -3.5f, 0f));

            stalker.umbraSafeDuration = 18.0f;
            hud.parFlips = 3;
            hud.lessonTitle = "Level 1: Chromatic Vault";
            hud.levelHint = "Align mirrors & RGB_Prism [E] in Light Realm, push the Crate onto the Vault Button, pull the Green Shutter Lever, then flip [F] to Shadow Realm!";

            CreateArenaWalls(assets);
            CreateWall("RuinedDecor_L1_Top", new Vector3(-8.7f, 4.7f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
            CreateWall("RuinedDecor_L1_Vault", new Vector3(8.7f, 4.6f, 0f), new Vector2(1.24f, 1.14f), assets, WorldAffinity.Both, WallTileStyle.Ruined);

            CreateLabProp("LabMonitor_L1", new Vector3(-7.0f, 4.7f, 0f), assets.LabMonitor, 0.85f, 3, hasCollider: true);
            CreateLabProp("LabServerRack_L1", new Vector3(8.7f, -4.4f, 0f), assets.LabServerRack, 0.85f, 3, hasCollider: true);
            CreateLabNPC("Dr_Vance_L1", new Vector3(-7.5f, -0.8f, 0f), "Dr. Vance",
                new[]
                {
                    "Interactions and pushing crates only work in the Light Realm! Align all mirrors & RGB_Prism and weigh down the Vault Button first.",
                    "Then flip [F] to the Shadow Realm so the beams pass the LightOnly Shutter and strike Sensor_Blue!"
                },
                assets.LabNpcMale, assets.LabNpcHazmat, assets);

            CreateWall("LightOnly_Shutter", new Vector3(-6.0f, 1.5f, 0f), new Vector2(0.8f, 2.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
            CreateEmitter("WhiteBeamEmitter", new Vector3(-8.5f, 1.5f, 0f), LightColor.White, WorldAffinity.Both, Vector2.right, assets);

            CreatePrism("RGB_Prism", new Vector3(-3.5f, 1.5f, 0f), 90f, OpticalPrism.PrismMode.DisperseRGB, 90f, true, true, 90f, assets);

            CreateIsometricMirror("Mirror_Blue1", new Vector3(-3.5f, 4.2f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            CreateColorFilter("BlueFilter", new Vector3(0.5f, 4.2f, 0f), new Vector2(0.45f, 1.1f), LightColor.Blue, assets);
            LightSensor blueSensor = CreateSensor("Sensor_Blue", new Vector3(4.2f, 4.2f, 0f), LightColor.Blue, WorldAffinity.ShadowOnly, assets);

            CreateColorFilter("GreenFilter", new Vector3(-0.5f, 1.5f, 0f), new Vector2(0.45f, 1.1f), LightColor.Green, assets);
            GameObject greenBarrier = CreateWall("GreenPath_Barrier", new Vector3(0.8f, 1.5f, 0f), new Vector2(0.65f, 1.4f), assets, WorldAffinity.Both, WallTileStyle.Medium);
            CreateIsometricMirror("Mirror_Green", new Vector3(2.2f, 1.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            LightSensor greenSensor = CreateSensor("Sensor_Green", new Vector3(2.2f, -1.2f, 0f), LightColor.Green, WorldAffinity.Both, assets);

            PuzzleLever greenLever = CreateLever("Lever_GreenShutter", new Vector3(-1.0f, -2.2f, 0f), assets, WorldAffinity.Both,
                toggledObjects: new[] { greenBarrier });

            GameObject mirrorRed1 = CreateIsometricMirror("Mirror_Red1", new Vector3(-3.5f, -3.5f, 0f), 45f, 90f, WorldAffinity.Both, assets);
            mirrorRed1.GetComponent<OpticalMirror>().requireLightWorldToRotate = true;
            mirrorRed1.GetComponent<SpriteRenderer>().color = new Color(1f, 0.88f, 0.55f, 1f);

            CreateIsometricMirror("Mirror_Red2", new Vector3(4.5f, -3.5f, 0f), 135f, 90f, WorldAffinity.Both, assets);
            LightSensor redSensor = CreateSensor("Sensor_Red", new Vector3(4.5f, -0.8f, 0f), LightColor.Red, WorldAffinity.Both, assets);

            CreatePushableBlock("PushableBlock_L1", new Vector3(-1.6f, 2.8f, 0f), assets);
            PressurePlate vaultButton = CreatePressurePlate("Button_VaultSeal", new Vector3(0.5f, 2.8f, 0f), assets, WorldAffinity.Both, latch: false);

            CreateWall("VaultWall_Upper", new Vector3(6.5f, 3.8f, 0f), new Vector2(0.85f, 4.4f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            CreateWall("VaultWall_Lower", new Vector3(6.5f, -3.6f, 0f), new Vector2(0.85f, 4.8f), assets, WorldAffinity.Both, WallTileStyle.Pillar);
            PuzzleDoor door = CreateDoor("PuzzleDoor", new Vector3(6.5f, 0.2f, 0f), new Vector2(1.1f, 2.8f), assets,
                new[] { redSensor, greenSensor, blueSensor }, new[] { vaultButton }, new[] { greenLever }, requireAll: true, stayOpen: false);

            CreateExit("LevelExit", new Vector3(8.6f, 0.2f, 0f), assets, new[] { door }, "OpticsPuzzle_Level2");

            EditorSceneManager.SaveScene(scene, Level1Path);
            RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
            Debug.Log("Created Optics Puzzle Level 1 at " + Level1Path);
        }

    }
}
