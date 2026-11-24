using System;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowFlip.EditorTools
{
    /// <summary>
    /// Zero-boilerplate fluent builder for authoring complete Shadow Flip levels in code
    /// by reusing all factory props, themes, lighting, NPCs, and Build Settings sync.
    /// </summary>
    public sealed class LevelBuilderContext : IDisposable
    {
        public readonly string ScenePath;
        public readonly Scene Scene;
        public readonly logic GameLogic;
        public readonly PlayerMovement Player;
        public readonly PuzzleHUD Hud;
        public readonly ShadowStalkerController Stalker;

        internal readonly OpticsPuzzleBuilder.CelAssets Assets;
        private bool saved;

        private LevelBuilderContext(
            string scenePath, Scene scene, OpticsPuzzleBuilder.CelAssets assets,
            logic gameLogic, PlayerMovement player, PuzzleHUD hud, ShadowStalkerController stalker)
        {
            ScenePath = scenePath;
            Scene = scene;
            Assets = assets;
            GameLogic = gameLogic;
            Player = player;
            Hud = hud;
            Stalker = stalker;
        }

        public static LevelBuilderContext Begin(
            string scenePath,
            LevelTheme theme,
            string title,
            string hint,
            int parFlips = 4,
            Vector3? playerSpawn = null,
            float umbraSafeDuration = 16.0f,
            bool buildOuterArena = true)
        {
            OpticsPuzzleBuilder.CelAssets assets = OpticsPuzzleBuilder.LoadThemedAssets(theme);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color camBg = theme == LevelTheme.Dungeon
                ? new Color(0.05f, 0.04f, 0.09f, 1f)
                : new Color(0.04f, 0.05f, 0.09f, 1f);
            Color lightTint = theme == LevelTheme.ScifiFacility
                ? new Color(0.95f, 0.97f, 1.0f, 1f)
                : new Color(1f, 0.94f, 0.80f, 1f);
            Color shadowTint = new Color(0.28f, 0.22f, 0.56f, 1f);

            OpticsPuzzleBuilder.SetupCoreScene(
                assets, out logic l, out PlayerMovement pm, out PuzzleHUD hud, out ShadowStalkerController stalker,
                camBg, lightTint, 1.1f, shadowTint, 0.48f,
                playerSpawn ?? new Vector3(-7.8f, 0f, 0f));

            hud.lessonTitle = title;
            hud.levelHint = hint;
            hud.parFlips = parFlips;
            stalker.umbraSafeDuration = umbraSafeDuration;

            if (buildOuterArena)
            {
                OpticsPuzzleBuilder.CreateArenaWalls(assets);
            }

            return new LevelBuilderContext(scenePath, scene, assets, l, pm, hud, stalker);
        }

        public GameObject AddWall(string name, Vector3 pos, Vector2 size, WorldAffinity affinity = WorldAffinity.Both)
            => OpticsPuzzleBuilder.CreateWall(name, pos, size, Assets, affinity, OpticsPuzzleBuilder.WallTileStyle.Medium);

        public GameObject AddPhaseWall(string name, Vector3 pos, Vector2 size, bool umbraPassThrough = true)
            => OpticsPuzzleBuilder.CreateWall(name, pos, size, Assets, umbraPassThrough ? WorldAffinity.LightOnly : WorldAffinity.ShadowOnly, OpticsPuzzleBuilder.WallTileStyle.Pillar);

        public LightBeamEmitter AddEmitter(string name, Vector3 pos, Vector2 dir, LightColor color = LightColor.White, WorldAffinity world = WorldAffinity.Both)
            => OpticsPuzzleBuilder.CreateEmitter(name, pos, color, world, dir, Assets);

        public OpticalMirror AddMirror(string name, Vector3 pos, float startAngle = 45f, WorldAffinity world = WorldAffinity.Both, bool umbraOnlyRotate = false, bool canRotate = true)
            => OpticsPuzzleBuilder.CreateIsometricMirror(name, pos, startAngle, 90f, world, Assets, canRotate, umbraOnlyRotate).GetComponent<OpticalMirror>();

        public GameObject AddMirrorCart(string name, Vector3 pos, float startAngle = 45f, bool umbraOnly = false)
            => OpticsPuzzleBuilder.CreatePushableMirrorCart(name, pos, startAngle, 90f, Assets, umbraOnly);

        public GameObject AddPrism(string name, Vector3 pos, OpticalPrism.PrismMode mode = OpticalPrism.PrismMode.Splitter, bool canRotate = true)
            => OpticsPuzzleBuilder.CreatePrism(name, pos, 0f, mode, 90f, true, canRotate, 90f, Assets);

        public GameObject AddColorFilter(string name, Vector3 pos, LightColor color, bool vertical = false)
            => OpticsPuzzleBuilder.CreateColorFilter(name, pos, vertical ? new Vector2(0.45f, 1.1f) : new Vector2(1.1f, 0.45f), color, Assets);

        public GameObject AddCombiner(string name, Vector3 pos, LightColor a = LightColor.Red, LightColor b = LightColor.Blue, LightColor output = LightColor.White)
            => OpticsPuzzleBuilder.CreateColorCombiner(name, pos, a, b, output, Assets);

        public (PortalMirror a, PortalMirror b) AddPortalPair(string baseName, Vector3 posA, Vector2 exitA, Vector3 posB, Vector2 exitB, bool canRotate = true)
        {
            PortalMirror b = OpticsPuzzleBuilder.CreatePortalMirror(baseName + "_B", posB, Assets.PortalMirrorB, exitB, null, canRotate);
            PortalMirror a = OpticsPuzzleBuilder.CreatePortalMirror(baseName + "_A", posA, Assets.PortalMirrorA, exitA, b, canRotate);
            b.linkedPortal = a;
            return (a, b);
        }

        public GameObject AddRelay(string name, Vector3 pos, BeamRelayCrystal.RelayMode mode, Vector2 outDir, bool canRotate = false)
            => OpticsPuzzleBuilder.CreateBeamRelay(name, pos, mode, outDir, canRotate, Assets);

        public GameObject AddPolarizer(string name, Vector3 pos, Vector2 allowedDir, bool canRotate = true, bool invertColor = false)
            => OpticsPuzzleBuilder.CreateOneWayPolarizer(name, pos, allowedDir, invertColor, canRotate, Assets);

        public GameObject AddMemoryPrism(string name, Vector3 pos, float holdSeconds, Vector2 outDir, bool canRotate = true)
            => OpticsPuzzleBuilder.CreateMemoryPrism(name, pos, holdSeconds, outDir, canRotate, Assets);

        public GameObject AddRealityAnchor(string name, Vector3 pos, float radius = 3.8f, float holdDuration = 8.0f)
            => OpticsPuzzleBuilder.CreateRealityAnchor(name, pos, radius, holdDuration, Assets);

        public BlackHoleLens AddBlackHole(string name, Vector3 pos, WorldAffinity world = WorldAffinity.Both, bool canAdjust = true, int startPreset = 1, float radius = 3.3f, float strength = 2.8f)
            => OpticsPuzzleBuilder.CreateBlackHole(name, pos, radius, 0.48f, strength, world, canAdjust, startPreset, Assets);

        public LightSensor AddSensor(string name, Vector3 pos, LightColor color = LightColor.White, WorldAffinity world = WorldAffinity.Both, bool acceptAny = false)
            => OpticsPuzzleBuilder.CreateSensor(name, pos, color, world, Assets, acceptAny: acceptAny);

        public LightSensor AddCapacitor(string name, Vector3 pos, LightColor color = LightColor.White, float holdDuration = 11.0f)
            => OpticsPuzzleBuilder.CreateSensor(name, pos, color, WorldAffinity.ShadowOnly, Assets, isCapacitor: true, holdDuration: holdDuration);

        public PressurePlate AddPlate(string name, Vector3 pos, WorldAffinity form = WorldAffinity.Both)
            => OpticsPuzzleBuilder.CreatePressurePlate(name, pos, Assets, form, latch: false);

        public GameObject AddBlock(string name, Vector3 pos, WorldAffinity pushableInWorld = WorldAffinity.LightOnly)
            => OpticsPuzzleBuilder.CreatePushableBlock(name, pos, Assets, pushableInWorld);

        public PuzzleLever AddLever(string name, Vector3 pos, WorldAffinity world = WorldAffinity.Both, OpticalMirror[] mirrors = null, GameObject[] toggled = null)
            => OpticsPuzzleBuilder.CreateLever(name, pos, Assets, world, mirrors, toggled);

        public GameObject AddTeslaZone(string name, Vector3 pos, float radius = 1.4f)
            => OpticsPuzzleBuilder.CreateLabSanctuary(name, pos, radius, Assets);

        public (PhaseShiftTile enter, PhaseShiftTile exit) AddTeleporterPair(string baseName, Vector3 posA, Vector3 posB, bool allowBlocks = true, bool flipOnStep = false)
        {
            PhaseShiftTile exit = OpticsPuzzleBuilder.CreatePhaseShiftTile(baseName + "_Out", posB, null, flipOnStep, Assets, allowBlocks);
            PhaseShiftTile enter = OpticsPuzzleBuilder.CreatePhaseShiftTile(baseName + "_In", posA, exit, flipOnStep, Assets, allowBlocks);
            exit.linkedTeleportPad = enter;
            return (enter, exit);
        }

        public GameObject AddMirrorTwinTotem(string name, Vector3 pos, float mirrorAxisX = 0f)
            => OpticsPuzzleBuilder.CreateMirrorTwinTotem(name, pos, mirrorAxisX, Player != null ? Player.DarkspriteDown : null, Assets);

        public GameObject AddEchoPedestal(string name, Vector3 pos)
            => OpticsPuzzleBuilder.CreateEchoPedestal(name, pos, Player != null ? Player.LightspriteDown : null, Assets);

        public GameObject AddMagneticRail(string name, Vector3 terminalPos, Vector3 pointA, Vector3 pointB, LightColor reqColor = LightColor.White)
            => OpticsPuzzleBuilder.CreateMagneticRail(name, terminalPos, pointA, pointB, reqColor, Assets);

        public LabNPCController AddNPC(string name, Vector3 pos, string displayName, string[] lines, bool female = false)
            => OpticsPuzzleBuilder.CreateLabNPC(name, pos, displayName, lines, female ? Assets.LabNpcFemale : Assets.LabNpcMale, Assets.LabNpcHazmat, Assets);

        public GameObject AddBeamMoth(string name, Vector3 pos, LightColor stunColor = LightColor.White)
            => OpticsPuzzleBuilder.CreateBeamEaterMoth(name, pos, stunColor, Assets);

        public GameObject AddMimicMirror(string name, Vector3 pos, Vector3 fleeOffset, float startAngle = 45f)
            => OpticsPuzzleBuilder.CreateUmbraMimicMirror(name, pos, startAngle, fleeOffset, Assets);

        public GameObject AddMold(string name, Vector3 pos, float scale = 0.9f)
            => OpticsPuzzleBuilder.CreateUmbraMold(name, pos, scale, Assets);

        public UmbraWardenBoss AddWardenBoss(string name, Vector3 pos)
            => OpticsPuzzleBuilder.CreateUmbraWardenBoss(name, pos, Assets);

        public PuzzleDoor AddDoor(string name, Vector3 pos, Vector2 size, LightSensor[] sensors = null, PressurePlate[] plates = null, PuzzleLever[] levers = null, bool stayOpen = false)
            => OpticsPuzzleBuilder.CreateDoor(name, pos, size, Assets, sensors, plates, levers, requireAll: true, stayOpen: stayOpen);

        public PuzzleDoor AddScifiDoor(string name, Vector3 pos, Vector2 size, LightSensor[] sensors = null, PressurePlate[] plates = null, PuzzleLever[] levers = null, bool stayOpen = false, bool isLabDoor = true)
            => OpticsPuzzleBuilder.CreateScifiDoor(name, pos, size, Assets, sensors, plates, levers, requireAll: true, stayOpen: stayOpen, isLabDoor: isLabDoor);

        public PuzzleDoor AddLabBlastDoor(string name, Vector3 pos, Vector2 size, LightSensor[] sensors = null, PressurePlate[] plates = null, PuzzleLever[] levers = null, bool stayOpen = false, bool vertical = true, bool red = false)
            => OpticsPuzzleBuilder.CreateLabBlastDoor(name, pos, size, Assets, sensors, plates, levers, requireAll: true, stayOpen: stayOpen, vertical: vertical, redVariant: red);

        public GameObject AddExit(Vector3 pos, string nextScene, params PuzzleDoor[] requiredDoors)
            => OpticsPuzzleBuilder.CreateExit("LevelExit", pos, Assets, requiredDoors, nextScene);

        public GameObject AddPropByKey(string key, Vector3 pos, LevelTheme theme = LevelTheme.Dungeon)
            => OpticsPuzzleBuilder.SpawnProp(key, pos, theme);

        public void Save()
        {
            saved = true;
            EditorSceneManager.SaveScene(Scene, ScenePath);
            OpticsPuzzleBuilder.RegisterScenesInBuildSettings();
            AssetDatabase.Refresh();
        }

        public void Dispose()
        {
            if (!saved) Save();
        }
    }
}
