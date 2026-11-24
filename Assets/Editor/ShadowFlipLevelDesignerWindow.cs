using System.Collections.Generic;
using ShadowFlip.Optics;
using ShadowFlip.Puzzle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ShadowFlip.EditorTools
{
    /// <summary>
    /// Visual Level Designer Window for creating new playable levels, spawning any prop from Levels 0-13
    /// into the active scene with grid snap, generating reusable prefabs, and wiring doors/portals/exits.
    /// </summary>
    public class ShadowFlipLevelDesignerWindow : EditorWindow
    {
        private static readonly string[] Categories =
        {
            "All", "Walls", "Optics", "Receivers & Doors", "Puzzle Props", "Enemies", "Decor"
        };

        private static readonly string[] TabNames = { "New Level", "Prop Palette", "Quick Wire" };

        private int activeTab = 1;

        // New Level fields
        private string newSceneName = "OpticsPuzzle_Level14";
        private LevelTheme newTheme = LevelTheme.Laboratory;
        private string newTitle = "Level 14: Custom Chamber";
        private string newHint = "Flip [F] to solve the optics puzzle and open the exit!";
        private int newParFlips = 4;
        private Vector3 newPlayerSpawn = new Vector3(-7.8f, 0f, 0f);
        private string newNextScene = "MainMenu";
        private bool newIncludeStarterSetup = true;

        // Prop Palette fields
        private LevelTheme paletteTheme = LevelTheme.Dungeon;
        private int selectedCategory = 0;
        private float snapStep = 0.5f;
        private bool snapToGrid = true;
        private Vector2 paletteScroll;
        private string searchFilter = "";

        [MenuItem("Shadow Flip/Level Designer Window")]
        public static void OpenWindow()
        {
            var win = GetWindow<ShadowFlipLevelDesignerWindow>("Shadow Flip Designer");
            win.minSize = new Vector2(360f, 440f);
            win.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Shadow Flip Level Designer", EditorStyles.boldLabel);
            activeTab = GUILayout.Toolbar(activeTab, TabNames);
            EditorGUILayout.Space(6);

            switch (activeTab)
            {
                case 0: DrawNewLevelTab(); break;
                case 1: DrawPropPaletteTab(); break;
                case 2: DrawQuickWireTab(); break;
            }
        }

        private void DrawNewLevelTab()
        {
            EditorGUILayout.HelpBox(
                "Creates a new playable scene pre-wired with Jimmy, Lighting, Camera, NPC, HUD, Tesla zone, outer walls, and automatically adds it to Build Settings and the Main Menu.",
                MessageType.Info);

            newSceneName = EditorGUILayout.TextField("Scene Name", newSceneName);
            newTheme = (LevelTheme)EditorGUILayout.EnumPopup("Visual Theme", newTheme);
            newTitle = EditorGUILayout.TextField("Level Title", newTitle);
            newHint = EditorGUILayout.TextField("HUD Hint", newHint);
            newParFlips = EditorGUILayout.IntSlider("Par Flips (3-Star)", newParFlips, 1, 20);
            newPlayerSpawn = EditorGUILayout.Vector3Field("Player Spawn", newPlayerSpawn);
            newNextScene = EditorGUILayout.TextField("Next Scene", newNextScene);
            newIncludeStarterSetup = EditorGUILayout.Toggle("Add Starter Emitter/Sensor/Exit", newIncludeStarterSetup);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Create New Playable Level", GUILayout.Height(34)))
            {
                CreateNewPlayableLevel();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Library & Build Maintenance", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate All Prop Prefabs (Assets/Prefabs/ShadowFlip)", GUILayout.Height(28)))
            {
                OpticsPuzzleBuilder.GenerateAllPropPrefabs();
            }

            if (GUILayout.Button("Sync All Scenes to Main Menu & Build Settings", GUILayout.Height(26)))
            {
                OpticsPuzzleBuilder.RegisterScenesInBuildSettings();
                Debug.Log("Synced all scenes in Assets/Scenes/ into EditorBuildSettings.");
            }
        }

        private void CreateNewPlayableLevel()
        {
            if (OpticsPuzzleBuilder.IsInPlayModeGuard()) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string cleanName = string.IsNullOrWhiteSpace(newSceneName) ? "CustomLevel" : newSceneName.Trim();
            if (!cleanName.EndsWith(".unity")) cleanName += ".unity";
            string scenePath = "Assets/Scenes/" + cleanName;

            using (var lvl = LevelBuilderContext.Begin(scenePath, newTheme, newTitle, newHint, newParFlips, newPlayerSpawn))
            {
                lvl.AddNPC("NPC_Guide", newPlayerSpawn + new Vector3(1.2f, -1.2f, 0f), "Dr. Vance",
                    new[] { "Welcome to your custom Shadow Flip chamber! Use [F] to flip realms and [E] to interact." });

                if (newIncludeStarterSetup)
                {
                    if (newTheme != LevelTheme.Dungeon)
                    {
                        lvl.AddTeslaZone("Tesla_Start", newPlayerSpawn + new Vector3(0f, 2.2f, 0f), 1.4f);
                    }
                    lvl.AddEmitter("Emitter_Start", new Vector3(-8.5f, 2.8f, 0f), Vector2.right, LightColor.White);
                    var m = lvl.AddMirror("Mirror_Start", new Vector3(-3.0f, 2.8f, 0f), 45f);
                    LightSensor s = lvl.AddSensor("Sensor_Start", new Vector3(-3.0f, -1.0f, 0f), LightColor.White, WorldAffinity.Both);
                    lvl.AddWall("VaultWall_Top", new Vector3(6.5f, 3.8f, 0f), new Vector2(0.85f, 4.8f));
                    lvl.AddWall("VaultWall_Bot", new Vector3(6.5f, -3.8f, 0f), new Vector2(0.85f, 4.8f));
                    PuzzleDoor exitDoor = lvl.AddDoor("Door_Exit", new Vector3(6.5f, 0f, 0f), new Vector2(1.05f, 2.6f), sensors: new[] { s });
                    lvl.AddExit(new Vector3(8.5f, 0f, 0f), newNextScene, exitDoor);
                    Selection.activeGameObject = m.gameObject;
                }

                lvl.Save();
            }

            Debug.Log($"Created custom playable level at '{scenePath}' and registered it in Main Menu & Build Settings.");
        }

        private void DrawPropPaletteTab()
        {
            paletteTheme = (LevelTheme)EditorGUILayout.EnumPopup("Prop Visual Theme", paletteTheme);
            selectedCategory = GUILayout.SelectionGrid(selectedCategory, Categories, 3);

            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("Search", searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(52))) searchFilter = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            snapToGrid = EditorGUILayout.Toggle("Grid Snap", snapToGrid);
            snapStep = EditorGUILayout.FloatField("Snap Step", snapStep);
            if (snapStep < 0.1f) snapStep = 0.1f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            string filterCat = Categories[selectedCategory];
            string query = string.IsNullOrEmpty(searchFilter) ? "" : searchFilter.Trim().ToLowerInvariant();

            paletteScroll = EditorGUILayout.BeginScrollView(paletteScroll);
            var catalog = OpticsPuzzleBuilder.PropCatalog;
            for (int i = 0; i < catalog.Length; i++)
            {
                var entry = catalog[i];
                if (filterCat != "All" && entry.Category != filterCat) continue;
                if (query.Length > 0 && !entry.Label.ToLowerInvariant().Contains(query) && !entry.Key.ToLowerInvariant().Contains(query)) continue;

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(entry.Label, EditorStyles.boldLabel);
                if (GUILayout.Button("Spawn", GUILayout.Width(72), GUILayout.Height(22)))
                {
                    SpawnIntoActiveScene(entry);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField(entry.Description, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndScrollView();
        }

        private void SpawnIntoActiveScene(OpticsPuzzleBuilder.PropEntry entry)
        {
            if (OpticsPuzzleBuilder.IsInPlayModeGuard()) return;

            Vector3 spawnPos = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.pivot
                : Vector3.zero;
            spawnPos.z = 0f;

            if (snapToGrid && snapStep > 0.05f)
            {
                spawnPos.x = Mathf.Round(spawnPos.x / snapStep) * snapStep;
                spawnPos.y = Mathf.Round(spawnPos.y / snapStep) * snapStep;
            }

            GameObject go = OpticsPuzzleBuilder.SpawnProp(entry.Key, spawnPos, paletteTheme);
            if (go != null)
            {
                Undo.RegisterCreatedObjectUndo(go, "Spawn " + entry.Label);
                Selection.activeGameObject = go;
            }
        }

        private void DrawQuickWireTab()
        {
            EditorGUILayout.HelpBox(
                "Select objects in the Hierarchy / Scene View and click a wiring action below to hook up doors, portals, teleporters, levers, and exits automatically.",
                MessageType.Info);

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Link Selected Sensors / Plates / Levers / Boss -> Door", GUILayout.Height(30)))
            {
                LinkSelectedToDoor();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Pair 2 Selected Portals or 2 Teleporters", GUILayout.Height(30)))
            {
                PairSelectedLinks();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Link Selected Doors / Sensors -> LevelExit", GUILayout.Height(30)))
            {
                LinkSelectedToExit();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Link Selected Mirrors / Objects -> Lever", GUILayout.Height(30)))
            {
                LinkSelectedToLever();
            }
        }

        private static void LinkSelectedToDoor()
        {
            GameObject[] sel = Selection.gameObjects;
            PuzzleDoor door = FindFirstInSelection<PuzzleDoor>(sel);
            if (door == null)
            {
                Debug.LogWarning("Select a PuzzleDoor along with Sensors, PressurePlates, Levers, or Boss.");
                return;
            }

            List<LightSensor> sensors = CollectInSelection<LightSensor>(sel);
            List<PressurePlate> plates = CollectInSelection<PressurePlate>(sel);
            List<PuzzleLever> levers = CollectInSelection<PuzzleLever>(sel);
            UmbraWardenBoss boss = FindFirstInSelection<UmbraWardenBoss>(sel);

            Undo.RecordObject(door, "Wire PuzzleDoor Inputs");
            if (sensors.Count > 0) door.requiredSensors = MergeDistinct(door.requiredSensors, sensors);
            if (plates.Count > 0) door.requiredPlates = MergeDistinct(door.requiredPlates, plates);
            if (levers.Count > 0) door.requiredLevers = MergeDistinct(door.requiredLevers, levers);
            if (boss != null) door.requiredBoss = boss;
            EditorUtility.SetDirty(door);
            Debug.Log($"Wired inputs to Door '{door.name}'.");
        }

        private static void PairSelectedLinks()
        {
            GameObject[] sel = Selection.gameObjects;
            List<PortalMirror> portals = CollectInSelection<PortalMirror>(sel);
            if (portals.Count == 2)
            {
                Undo.RecordObjects(new Object[] { portals[0], portals[1] }, "Pair PortalMirrors");
                portals[0].linkedPortal = portals[1];
                portals[1].linkedPortal = portals[0];
                EditorUtility.SetDirty(portals[0]);
                EditorUtility.SetDirty(portals[1]);
                Debug.Log($"Paired PortalMirrors '{portals[0].name}' <-> '{portals[1].name}'.");
                return;
            }

            List<PhaseShiftTile> pads = CollectInSelection<PhaseShiftTile>(sel);
            if (pads.Count == 2)
            {
                Undo.RecordObjects(new Object[] { pads[0], pads[1] }, "Pair Teleporter Pads");
                pads[0].linkedTeleportPad = pads[1];
                pads[1].linkedTeleportPad = pads[0];
                EditorUtility.SetDirty(pads[0]);
                EditorUtility.SetDirty(pads[1]);
                Debug.Log($"Paired Teleporters '{pads[0].name}' <-> '{pads[1].name}'.");
                return;
            }

            Debug.LogWarning("Select exactly 2 PortalMirrors or 2 PhaseShiftTile teleporter pads.");
        }

        private static void LinkSelectedToExit()
        {
            GameObject[] sel = Selection.gameObjects;
            LevelExit exit = FindFirstInSelection<LevelExit>(sel);
            if (exit == null)
            {
                Debug.LogWarning("Select a LevelExit along with PuzzleDoors and/or LightSensors.");
                return;
            }

            List<PuzzleDoor> doors = CollectInSelection<PuzzleDoor>(sel);
            List<LightSensor> sensors = CollectInSelection<LightSensor>(sel);
            Undo.RecordObject(exit, "Wire LevelExit Requirements");
            if (doors.Count > 0) exit.requiredDoors = MergeDistinct(exit.requiredDoors, doors);
            if (sensors.Count > 0) exit.requiredSensors = MergeDistinct(exit.requiredSensors, sensors);
            EditorUtility.SetDirty(exit);
            Debug.Log($"Wired requirements to LevelExit '{exit.name}'.");
        }

        private static void LinkSelectedToLever()
        {
            GameObject[] sel = Selection.gameObjects;
            PuzzleLever lever = FindFirstInSelection<PuzzleLever>(sel);
            if (lever == null)
            {
                Debug.LogWarning("Select a PuzzleLever along with OpticalMirrors or other GameObjects to toggle.");
                return;
            }

            List<OpticalMirror> mirrors = new List<OpticalMirror>();
            List<GameObject> toggled = new List<GameObject>();
            for (int i = 0; i < sel.Length; i++)
            {
                if (sel[i] == null || sel[i] == lever.gameObject) continue;
                OpticalMirror m = sel[i].GetComponent<OpticalMirror>();
                if (m != null) mirrors.Add(m);
                else toggled.Add(sel[i]);
            }

            Undo.RecordObject(lever, "Wire PuzzleLever Targets");
            if (mirrors.Count > 0) lever.controlledMirrors = MergeDistinct(lever.controlledMirrors, mirrors);
            if (toggled.Count > 0) lever.toggledObjects = MergeDistinct(lever.toggledObjects, toggled);
            EditorUtility.SetDirty(lever);
            Debug.Log($"Wired targets to Lever '{lever.name}'.");
        }

        private static T FindFirstInSelection<T>(GameObject[] selection) where T : Component
        {
            for (int i = 0; i < selection.Length; i++)
            {
                if (selection[i] == null) continue;
                T comp = selection[i].GetComponentInChildren<T>();
                if (comp != null) return comp;
            }
            return null;
        }

        private static List<T> CollectInSelection<T>(GameObject[] selection) where T : Component
        {
            List<T> list = new List<T>();
            for (int i = 0; i < selection.Length; i++)
            {
                if (selection[i] == null) continue;
                T comp = selection[i].GetComponentInChildren<T>();
                if (comp != null && !list.Contains(comp)) list.Add(comp);
            }
            return list;
        }

        private static T[] MergeDistinct<T>(T[] existing, List<T> additions) where T : class
        {
            List<T> merged = new List<T>();
            if (existing != null)
            {
                for (int i = 0; i < existing.Length; i++)
                {
                    if (existing[i] != null && !merged.Contains(existing[i])) merged.Add(existing[i]);
                }
            }
            for (int i = 0; i < additions.Count; i++)
            {
                if (additions[i] != null && !merged.Contains(additions[i])) merged.Add(additions[i]);
            }
            return merged.ToArray();
        }
    }
}
