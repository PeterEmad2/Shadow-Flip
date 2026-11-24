using System;
using System.Collections.Generic;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowFlip.EditorTools
{
    public static partial class OpticsPuzzleBuilder
    {
        public readonly struct PropEntry
        {
            public readonly string Key;
            public readonly string Label;
            public readonly string Category;
            public readonly string Subfolder;
            public readonly LevelTheme DefaultTheme;
            public readonly string Description;

            public PropEntry(string key, string label, string category, string subfolder, LevelTheme defaultTheme, string description)
            {
                Key = key;
                Label = label;
                Category = category;
                Subfolder = subfolder;
                DefaultTheme = defaultTheme;
                Description = description;
            }
        }

        public static readonly PropEntry[] PropCatalog =
        {
            // Walls & Barriers
            new PropEntry("wall_solid", "Solid Wall", "Walls", "Walls", LevelTheme.Dungeon, "Impassable wall active in both realms"),
            new PropEntry("wall_light_phase", "Light Phase Wall", "Walls", "Walls", LevelTheme.Dungeon, "Solid in Light Mode; Jimmy can phase through in Umbra Mode"),
            new PropEntry("wall_shadow_phase", "Umbra Phase Wall", "Walls", "Walls", LevelTheme.Dungeon, "Solid in Umbra Mode; Jimmy can phase through in Light Mode"),
            new PropEntry("wall_ruined", "Ruined Wall Block", "Walls", "Walls", LevelTheme.Dungeon, "Small square solid stone pillar"),
            new PropEntry("beam_window", "Beam Glass Window", "Walls", "Walls", LevelTheme.Laboratory, "Solid wall window that transmits optical beams rightward"),

            // Optics
            new PropEntry("emitter_white", "White Laser Emitter", "Optics", "Optics", LevelTheme.Dungeon, "Continuous White optical laser emitter"),
            new PropEntry("emitter_red", "Red Laser Emitter", "Optics", "Optics", LevelTheme.Dungeon, "Red optical laser emitter"),
            new PropEntry("emitter_blue", "Blue Laser Emitter", "Optics", "Optics", LevelTheme.Dungeon, "Blue optical laser emitter"),
            new PropEntry("emitter_shadow", "Shadow Laser Emitter", "Optics", "Optics", LevelTheme.Dungeon, "Umbra-only Shadow beam emitter"),
            new PropEntry("turret_emitter", "Rotating Flip Turret", "Optics", "Optics", LevelTheme.Dungeon, "Emitter that rotates 90 degrees on every [F] realm flip"),
            new PropEntry("mirror_light", "Light Mirror [E]", "Optics", "Optics", LevelTheme.Dungeon, "Rotatable 4-way isometric mirror (Light Mode)"),
            new PropEntry("mirror_umbra", "Umbra Mirror [E]", "Optics", "Optics", LevelTheme.Dungeon, "Violet rotatable mirror (Umbra Mode only)"),
            new PropEntry("mirror_cart_light", "Pushable Mirror Cart (Light)", "Optics", "Optics", LevelTheme.Dungeon, "Pushable and rotatable mirror cart (Light Mode)"),
            new PropEntry("mirror_cart_umbra", "Pushable Mirror Cart (Umbra)", "Optics", "Optics", LevelTheme.Dungeon, "Violet pushable and rotatable mirror cart (Umbra Mode)"),
            new PropEntry("prism_splitter", "Splitter Prism", "Optics", "Optics", LevelTheme.Dungeon, "Splits incoming beam perpendicular to incidence"),
            new PropEntry("prism_rgb", "RGB Dispersion Prism", "Optics", "Optics", LevelTheme.Dungeon, "Splits White beam into Red, Green, and Blue rays"),
            new PropEntry("color_filter_red", "Red Color Filter", "Optics", "Optics", LevelTheme.Dungeon, "Filters passing beam to Red"),
            new PropEntry("color_combiner", "Red+Blue Combiner", "Optics", "Optics", LevelTheme.Dungeon, "Combines Red + Blue inputs into a White beam"),
            new PropEntry("portal_mirror_a", "Portal Mirror A (Cyan)", "Optics", "Optics", LevelTheme.Dungeon, "Teleports optical beams to a linked Portal Mirror B"),
            new PropEntry("portal_mirror_b", "Portal Mirror B (Magenta)", "Optics", "Optics", LevelTheme.Dungeon, "Destination mirror for Portal Mirror A"),
            new PropEntry("relay_4way", "4-Way Beam Relay", "Optics", "Optics", LevelTheme.Dungeon, "Splits/repeats beam in all 4 cardinal directions"),
            new PropEntry("relay_inverter", "Realm Inverter Relay", "Optics", "Optics", LevelTheme.Laboratory, "Passes beam while inverting realm semantics"),
            new PropEntry("polarizer", "One-Way Polarizer [E]", "Optics", "Optics", LevelTheme.Laboratory, "Allows beam only along its arrow direction; rotatable"),
            new PropEntry("memory_prism", "Memory Prism (8s)", "Optics", "Optics", LevelTheme.Laboratory, "Stores incoming beam and continues emitting after flip/interruption"),
            new PropEntry("reality_anchor", "Reality Anchor Lantern", "Optics", "Optics", LevelTheme.Laboratory, "When lit, projects a Light-Realm bubble inside Umbra"),
            new PropEntry("laser_barrier_gate", "Forcefield Laser Barrier", "Optics", "Optics", LevelTheme.ScifiFacility, "Walkable barrier/cage opened by a beam hit"),
            new PropEntry("blackhole_tunable", "Black Hole Lens (Tunable [E])", "Optics", "Optics", LevelTheme.Laboratory, "Gravitational singularity bending/swallowing beams"),
            new PropEntry("blackhole_shadow", "Black Hole Lens (Umbra)", "Optics", "Optics", LevelTheme.Laboratory, "Umbra-only gravitational lensing singularity"),

            // Receivers & Doors
            new PropEntry("sensor_light", "Light Crystal Receiver", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.Dungeon, "Standard optical receiver active in Light Mode"),
            new PropEntry("sensor_shadow", "Shadow Crystal Receiver", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.Dungeon, "Optical receiver active in Umbra Mode"),
            new PropEntry("capacitor_umbra", "Umbra Capacitor Receiver", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.Dungeon, "Twin-coil receiver that charges in Umbra and slowly discharges"),
            new PropEntry("door_dungeon", "Dungeon Gate Door", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.Dungeon, "Cel-shaded dungeon gate with linked crystal indicator lights"),
            new PropEntry("door_scifi", "Sci-Fi Sliding Door", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.ScifiFacility, "Sci-Fi automated sliding door with floor threshold"),
            new PropEntry("door_lab_blast", "Lab Blast Door", "Receivers & Doors", "ReceiversAndDoors", LevelTheme.Laboratory, "Heavy laboratory blast door"),
            new PropEntry("level_exit", "Level Exit Portal", "Receivers & Doors", "Core", LevelTheme.Dungeon, "Victory exit portal triggered when Jimmy enters"),

            // Puzzle Props
            new PropEntry("pressure_plate", "Hold Pressure Plate", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Hold-only floor button pressed by crates, twin, or player"),
            new PropEntry("block_light", "Pushable Crate (Light)", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Movable only in Light Mode; freezes in Umbra"),
            new PropEntry("block_umbra", "Pushable Crate (Umbra)", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Violet crate movable only in Umbra Mode"),
            new PropEntry("lever", "Dungeon / Console Lever [E]", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Interactive [E] switch linked to doors, mirrors, or objects"),
            new PropEntry("tesla_zone", "Tesla Safe Zone (1.4m)", "Puzzle Props", "PuzzleProps", LevelTheme.Laboratory, "Protected safe-zone coil that drains the Umbra Stalker timer"),
            new PropEntry("lantern_sanctuary", "Non-Safe Lantern Pillar", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Decorative pillar (does not grant Umbra monster safety)"),
            new PropEntry("teleporter_pad", "Phase Shift Teleporter", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Teleports Jimmy (and optionally crates) to a linked pad"),
            new PropEntry("mirror_twin_totem", "Mirror Twin Totem [E]", "Puzzle Props", "PuzzleProps", LevelTheme.Laboratory, "Summons a mirrored clone across x=0 to hold plates"),
            new PropEntry("echo_pedestal", "Light Echo Pedestal [E]", "Puzzle Props", "PuzzleProps", LevelTheme.Dungeon, "Records movement and replays a Light Echo clone"),
            new PropEntry("magnetic_rail", "Beam Magnetic Rail", "Puzzle Props", "PuzzleProps", LevelTheme.Laboratory, "Motorized track + mirror cart driven when terminal receives beam"),
            new PropEntry("npc_vance", "NPC: Dr. Vance", "Puzzle Props", "PuzzleProps", LevelTheme.Laboratory, "Interactive scientist switching to Hazmat Lab Uniform in Umbra"),
            new PropEntry("npc_elena", "NPC: Dr. Elena", "Puzzle Props", "PuzzleProps", LevelTheme.Laboratory, "Interactive scientist switching to Hazmat Lab Uniform in Umbra"),

            // Enemies
            new PropEntry("enemy_beam_moth", "Beam Eater Moth", "Enemies", "Enemies", LevelTheme.ScifiFacility, "Orbits receivers and interrupts beams until stunned"),
            new PropEntry("enemy_mimic_mirror", "Umbra Mimic Mirror", "Enemies", "Enemies", LevelTheme.Dungeon, "Mirror that flees in Umbra when approached"),
            new PropEntry("enemy_umbra_mold", "Umbra Mold Patch", "Enemies", "Enemies", LevelTheme.ScifiFacility, "Spreading dark hazard slowed/shrunk by laser beams"),
            new PropEntry("enemy_warden_boss", "Umbra Warden Boss", "Enemies", "Enemies", LevelTheme.ScifiFacility, "3-shield boss requiring Red -> Green -> Blue beams"),

            // Decor
            new PropEntry("decor_lab_monitor", "Lab Computer Monitor", "Decor", "Decor", LevelTheme.Laboratory, "Laboratory computer terminal prop with collider"),
            new PropEntry("decor_lab_capsule", "Lab Bio Capsule", "Decor", "Decor", LevelTheme.Laboratory, "Bio containment cylinder prop with collider"),
            new PropEntry("decor_lab_rack", "Lab Server Rack", "Decor", "Decor", LevelTheme.Laboratory, "Server cabinet prop with collider"),
            new PropEntry("decor_lab_bench", "Lab Workbench", "Decor", "Decor", LevelTheme.Laboratory, "Laboratory workbench prop with collider"),
            new PropEntry("decor_scifi_bench", "Sci-Fi Console Bench", "Decor", "Decor", LevelTheme.ScifiFacility, "Sci-Fi facility bench prop with collider"),
            new PropEntry("decor_scifi_shelf", "Sci-Fi Supply Shelf", "Decor", "Decor", LevelTheme.ScifiFacility, "Sci-Fi supply rack prop with collider"),
            new PropEntry("decor_scifi_alarm", "Sci-Fi Alarm Beacon", "Decor", "Decor", LevelTheme.ScifiFacility, "Animated facility alarm beacon")
        };

        internal static GameObject SpawnProp(string key, Vector3 pos, LevelTheme theme)
        {
            CelAssets assets = LoadThemedAssets(theme);
            Scene activeScene = SceneManager.GetActiveScene();
            HashSet<GameObject> existingRoots = activeScene.IsValid() && activeScene.isLoaded
                ? new HashSet<GameObject>(activeScene.GetRootGameObjects())
                : new HashSet<GameObject>();

            GameObject primary = SpawnPropDirect(key, pos, assets);

            if (!activeScene.IsValid() || !activeScene.isLoaded) return primary;

            GameObject[] currentRoots = activeScene.GetRootGameObjects();
            List<GameObject> spawnedRoots = new List<GameObject>();
            for (int i = 0; i < currentRoots.Length; i++)
            {
                if (!existingRoots.Contains(currentRoots[i]))
                {
                    spawnedRoots.Add(currentRoots[i]);
                }
            }

            if (spawnedRoots.Count <= 1) return primary != null ? primary : (spawnedRoots.Count == 1 ? spawnedRoots[0] : null);

            GameObject wrapper = new GameObject(key);
            wrapper.transform.position = pos;
            for (int i = 0; i < spawnedRoots.Count; i++)
            {
                spawnedRoots[i].transform.SetParent(wrapper.transform, true);
            }
            return wrapper;
        }

        private static GameObject SpawnPropDirect(string key, Vector3 pos, CelAssets assets)
        {
            switch (key)
            {
                case "wall_solid":
                    return CreateWall("Wall_Solid", pos, new Vector2(1.2f, 2.4f), assets, WorldAffinity.Both, WallTileStyle.Medium);
                case "wall_light_phase":
                    return CreateWall("Wall_LightOnly", pos, new Vector2(0.85f, 2.6f), assets, WorldAffinity.LightOnly, WallTileStyle.Pillar);
                case "wall_shadow_phase":
                    return CreateWall("Wall_ShadowOnly", pos, new Vector2(0.85f, 2.6f), assets, WorldAffinity.ShadowOnly, WallTileStyle.Pillar);
                case "wall_ruined":
                    return CreateWall("Wall_Ruined", pos, new Vector2(1.1f, 1.0f), assets, WorldAffinity.Both, WallTileStyle.Ruined);
                case "beam_window":
                {
                    GameObject win = CreateOneWayPolarizer("Window_BeamGlass", pos, Vector2.right, invertColor: false, canRotate: false, assets);
                    BoxCollider2D col = win.GetComponent<BoxCollider2D>();
                    if (col != null) col.size = new Vector2(0.95f, 1.25f);
                    return win;
                }

                case "emitter_white":
                    return CreateEmitter("Emitter_White", pos, LightColor.White, WorldAffinity.Both, Vector2.right, assets).gameObject;
                case "emitter_red":
                    return CreateEmitter("Emitter_Red", pos, LightColor.Red, WorldAffinity.Both, Vector2.right, assets).gameObject;
                case "emitter_blue":
                    return CreateEmitter("Emitter_Blue", pos, LightColor.Blue, WorldAffinity.Both, Vector2.right, assets).gameObject;
                case "emitter_shadow":
                    return CreateEmitter("Emitter_Shadow", pos, LightColor.Shadow, WorldAffinity.ShadowOnly, Vector2.right, assets).gameObject;
                case "turret_emitter":
                {
                    LightBeamEmitter em = CreateEmitter("Turret_Flip", pos, LightColor.White, WorldAffinity.Both, Vector2.up, assets);
                    RotatingTurretEmitter rot = em.gameObject.AddComponent<RotatingTurretEmitter>();
                    rot.rotateOnWorldFlip = true;
                    rot.stepDegrees = -90f;
                    return em.gameObject;
                }
                case "mirror_light":
                    return CreateIsometricMirror("Mirror_Light", pos, 45f, 90f, WorldAffinity.Both, assets);
                case "mirror_umbra":
                    return CreateIsometricMirror("Mirror_Umbra", pos, 45f, 90f, WorldAffinity.Both, assets, canRotate: true, requireShadowToRotate: true);
                case "mirror_cart_light":
                    return CreatePushableMirrorCart("MirrorCart_Light", pos, 45f, 90f, assets, umbraOnly: false);
                case "mirror_cart_umbra":
                    return CreatePushableMirrorCart("MirrorCart_Umbra", pos, 45f, 90f, assets, umbraOnly: true);
                case "prism_splitter":
                    return CreatePrism("Prism_Splitter", pos, 0f, OpticalPrism.PrismMode.Splitter, 90f, true, true, 90f, assets);
                case "prism_rgb":
                    return CreatePrism("Prism_RGB", pos, 0f, OpticalPrism.PrismMode.DisperseRGB, 90f, true, false, 90f, assets);
                case "color_filter_red":
                    return CreateColorFilter("Filter_Red", pos, new Vector2(1.1f, 0.45f), LightColor.Red, assets);
                case "color_combiner":
                    return CreateColorCombiner("Combiner_White", pos, LightColor.Red, LightColor.Blue, LightColor.White, assets);
                case "portal_mirror_a":
                    return CreatePortalMirror("PortalMirror_A", pos, assets.PortalMirrorA, Vector2.right, null, canRotate: true).gameObject;
                case "portal_mirror_b":
                    return CreatePortalMirror("PortalMirror_B", pos, assets.PortalMirrorB, Vector2.down, null, canRotate: true).gameObject;
                case "relay_4way":
                    return CreateBeamRelay("Relay_4Way", pos, BeamRelayCrystal.RelayMode.CrossRepeater4Way, Vector2.right, false, assets);
                case "relay_inverter":
                    return CreateBeamRelay("Relay_RealmInverter", pos, BeamRelayCrystal.RelayMode.RealmInverter, Vector2.right, false, assets);
                case "polarizer":
                    return CreateOneWayPolarizer("Polarizer", pos, Vector2.right, invertColor: false, canRotate: true, assets);
                case "memory_prism":
                    return CreateMemoryPrism("MemoryPrism", pos, 8.0f, Vector2.right, true, assets);
                case "reality_anchor":
                    return CreateRealityAnchor("RealityAnchor", pos, 3.8f, 8.0f, assets);
                case "laser_barrier_gate":
                    return CreateShadowBarrierGate("LaserBarrierGate", pos, new Vector2(1.1f, 2.4f), assets, openWhenLit: true, stayOpenOnceLit: true);
                case "blackhole_tunable":
                    return CreateBlackHole("BlackHole_Tunable", pos, 3.3f, 0.48f, 2.8f, WorldAffinity.Both, canAdjust: true, startPreset: 1, assets: assets).gameObject;
                case "blackhole_shadow":
                    return CreateBlackHole("BlackHole_Shadow", pos, 3.3f, 0.48f, 2.8f, WorldAffinity.ShadowOnly, canAdjust: false, startPreset: 1, assets: assets).gameObject;

                case "sensor_light":
                    return CreateSensor("Sensor_Light", pos, LightColor.White, WorldAffinity.LightOnly, assets).gameObject;
                case "sensor_shadow":
                    return CreateSensor("Sensor_Shadow", pos, LightColor.Shadow, WorldAffinity.ShadowOnly, assets).gameObject;
                case "capacitor_umbra":
                    return CreateSensor("Capacitor_Umbra", pos, LightColor.White, WorldAffinity.ShadowOnly, assets, isCapacitor: true, holdDuration: 11.0f).gameObject;
                case "door_dungeon":
                    return CreateDoor("PuzzleDoor", pos, new Vector2(1.1f, 2.6f), assets, null, null, null, requireAll: true, stayOpen: false).gameObject;
                case "door_scifi":
                    return CreateScifiDoor("ScifiDoor", pos, new Vector2(1.05f, 2.6f), assets, null, null, null, requireAll: true, stayOpen: false, isLabDoor: true).gameObject;
                case "door_lab_blast":
                    return CreateLabBlastDoor("LabBlastDoor", pos, new Vector2(1.05f, 2.6f), assets, null, null, null, requireAll: true, stayOpen: false, vertical: true, redVariant: false).gameObject;
                case "level_exit":
                    return CreateExit("LevelExit", pos, assets, null, "OpticsPuzzle");

                case "pressure_plate":
                    return CreatePressurePlate("PressurePlate", pos, assets, WorldAffinity.Both, latch: false).gameObject;
                case "block_light":
                    return CreatePushableBlock("PushableBlock", pos, assets, WorldAffinity.LightOnly);
                case "block_umbra":
                    return CreatePushableBlock("PushableBlock_Umbra", pos, assets, WorldAffinity.ShadowOnly);
                case "lever":
                    return CreateLever("Lever", pos, assets, WorldAffinity.Both).gameObject;
                case "tesla_zone":
                    return CreateLabSanctuary("TeslaSafeZone", pos, 1.4f, assets);
                case "lantern_sanctuary":
                    return CreateSanctuary("LanternPillar", pos, 1.4f, assets);
                case "teleporter_pad":
                    return CreatePhaseShiftTile("TeleporterPad", pos, null, false, assets, allowPushableBlocks: true).gameObject;
                case "mirror_twin_totem":
                    return CreateMirrorTwinTotem("Totem_MirrorTwin", pos, 0f, LoadSprite("Assets/Images/Jimmy/Dark/dark_down.PNG"), assets);
                case "echo_pedestal":
                    return CreateEchoPedestal("EchoPedestal", pos, LoadSprite("Assets/Images/Jimmy/Light/light_down.PNG"), assets);
                case "magnetic_rail":
                    return CreateMagneticRail("MagneticRail", pos + new Vector3(-2f, 0f, 0f), pos, pos + new Vector3(3f, 0f, 0f), LightColor.White, assets);
                case "npc_vance":
                    return CreateLabNPC("Dr_Vance", pos, "Dr. Vance", new[] { "Press [F] to flip between the Light and Umbra Realms!" }, assets.LabNpcMale, assets.LabNpcHazmat, assets).gameObject;
                case "npc_elena":
                    return CreateLabNPC("Dr_Elena", pos, "Dr. Elena", new[] { "Use Light Mode for crystals & crates, and Umbra Mode to phase past Light Walls!" }, assets.LabNpcFemale, assets.LabNpcHazmat, assets).gameObject;

                case "enemy_beam_moth":
                    return CreateBeamEaterMoth("BeamEaterMoth", pos, LightColor.White, assets);
                case "enemy_mimic_mirror":
                    return CreateUmbraMimicMirror("UmbraMimicMirror", pos, 45f, new Vector3(2f, 0f, 0f), assets);
                case "enemy_umbra_mold":
                    return CreateUmbraMold("UmbraMold", pos, 0.9f, assets);
                case "enemy_warden_boss":
                    return CreateUmbraWardenBoss("UmbraWarden_Boss", pos, assets).gameObject;

                case "decor_lab_monitor":
                    return CreateLabProp("Lab_Monitor", pos, assets.LabMonitor, 1.0f, 3, hasCollider: true);
                case "decor_lab_capsule":
                    return CreateLabProp("Lab_BioCapsule", pos, assets.LabBioCapsule, 0.9f, 3, hasCollider: true);
                case "decor_lab_rack":
                    return CreateLabProp("Lab_ServerRack", pos, assets.LabServerRack, 0.9f, 3, hasCollider: true);
                case "decor_lab_bench":
                    return CreateLabProp("Lab_Workbench", pos, assets.LabWorkbench, 0.9f, 3, hasCollider: true);
                case "decor_scifi_bench":
                    return CreateLabProp("Scifi_Bench", pos, assets.ScifiBenchFilled, 0.95f, 3, hasCollider: true);
                case "decor_scifi_shelf":
                    return CreateLabProp("Scifi_Shelf", pos, assets.ScifiShelfFilled1, 1.0f, 3, hasCollider: true);
                case "decor_scifi_alarm":
                    return CreateScifiAlarmBeacon("Scifi_AlarmBeacon", pos, assets);

                default:
                    Debug.LogWarning($"OpticsPuzzleBuilder.SpawnProp: Unknown prop key '{key}'.");
                    return null;
            }
        }

        [MenuItem("Shadow Flip/Level Designer/Generate All Prop Prefabs")]
        public static void GenerateAllPropPrefabs()
        {
            if (IsInPlayModeGuard()) return;
            ConfigureCelShadedSprites();

            int count = 0;
            for (int i = 0; i < PropCatalog.Length; i++)
            {
                PropEntry entry = PropCatalog[i];
                string folder = $"Assets/Prefabs/ShadowFlip/{entry.Subfolder}";
                EnsureAssetFolder(folder);
                string prefabPath = $"{folder}/{entry.Key}.prefab";

                GameObject go = SpawnProp(entry.Key, Vector3.zero, entry.DefaultTheme);
                if (go == null) continue;
                try
                {
                    PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                    count++;
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated {count} reusable Shadow Flip prefabs under Assets/Prefabs/ShadowFlip/.");
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
