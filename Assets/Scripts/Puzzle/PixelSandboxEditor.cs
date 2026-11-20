using System;
using System.Collections.Generic;
using System.IO;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Interactive Play-Mode Pixel-Art Sandbox Level Editor (`OpticsPuzzle_Sandbox`).
    /// Snap items to 1x1 world grid tiles, toggle Build/Play Test ([P]), and Save/Load JSON.
    /// </summary>
    public class PixelSandboxEditor : MonoBehaviour
    {
        public enum ToolType
        {
            Wall = 0,
            LightOnlyWall = 1,
            Emitter = 2,
            Mirror = 3,
            Prism = 4,
            Receiver = 5,
            Button = 6,
            Crate = 7,
            Door = 8,
            Eraser = 9
        }

        [Serializable]
        private class SavedItem
        {
            public int tool;
            public int gx;
            public int gy;
            public int rotStep;
        }

        [Serializable]
        private class SandboxSaveData
        {
            public List<SavedItem> items = new List<SavedItem>();
        }

        public PlayerMovement player;

        private static readonly string[] ToolLabels =
        {
            "[1] Wall",
            "[2] Phase Wall",
            "[3] Emitter",
            "[4] Mirror",
            "[5] Prism",
            "[6] Receiver",
            "[7] Button",
            "[8] Crate",
            "[9] Door",
            "[0] Erase"
        };

        private static readonly Vector2[] DirVectors =
        {
            Vector2.right,
            Vector2.up,
            Vector2.left,
            Vector2.down
        };

        private readonly Dictionary<Vector2Int, GameObject> placedObjects = new Dictionary<Vector2Int, GameObject>();
        private readonly Dictionary<Vector2Int, SavedItem> placedRecords = new Dictionary<Vector2Int, SavedItem>();

        private ToolType selectedTool = ToolType.Wall;
        private int rotationStep = 0; // 0..3 -> 0/90/180/270 deg
        private bool buildMode = true;
        private string statusNotice = "Click map to place pixel props! Press [Q] to rotate, [P] to Play Test.";

        private Sprite wallSprite, wallShadowSprite, emitterSprite, prismSprite, receiverSprite, btnUp, btnDown, crateSprite, doorClosed, doorOpen;
        private Sprite[] mirrorDirs;
        private Texture2D whiteTex;
        private GUIStyle labelStyle;
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, "shadowflip_sandbox.json");

        void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            LoadSpritesFromResources();
            ApplyModeState();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            HandleHotkeys();

            if (!buildMode) return;

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Q))
            {
                rotationStep = (rotationStep + 1) % 4;
                PuzzleAudioManager.Instance?.PlayButtonClick();
            }

            if (Input.GetMouseButtonDown(0))
            {
                float guiY = Screen.height - Input.mousePosition.y;
                if (guiY < 118f || guiY > Screen.height - 96f) return;

                Camera cam = Camera.main;
                if (cam == null) return;
                Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
                int gx = Mathf.Clamp(Mathf.RoundToInt(wp.x), -9, 9);
                int gy = Mathf.Clamp(Mathf.RoundToInt(wp.y), -5, 5);
                Vector2Int cell = new Vector2Int(gx, gy);

                if (selectedTool == ToolType.Eraser)
                {
                    RemoveAt(cell);
                }
                else
                {
                    PlaceItem(cell, selectedTool, rotationStep);
                }
            }
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                buildMode = !buildMode;
                ApplyModeState();
                PuzzleAudioManager.Instance?.PlayButtonClick();
            }

            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i)) selectedTool = (ToolType)i;
            }
            if (Input.GetKeyDown(KeyCode.Alpha0)) selectedTool = ToolType.Eraser;
        }

        private void ApplyModeState()
        {
            if (player != null)
            {
                player.enabled = !buildMode;
                if (buildMode)
                {
                    Transform form = player.GetActiveFormTransform();
                    Rigidbody2D rb = form != null ? form.GetComponent<Rigidbody2D>() : null;
                    if (rb != null) rb.linearVelocity = Vector2.zero;
                }
            }

            if (RealmCameraController.Instance != null)
            {
                RealmCameraController.Instance.lightOrthoSize = buildMode ? 7.0f : 4.2f;
            }
        }

        private void RemoveAt(Vector2Int cell)
        {
            if (placedObjects.TryGetValue(cell, out GameObject go) && go != null)
            {
                Destroy(go);
            }
            placedObjects.Remove(cell);
            placedRecords.Remove(cell);
            RelinkSandboxDoors();
        }

        private void PlaceItem(Vector2Int cell, ToolType tool, int rot)
        {
            RemoveAt(cell);

            Vector3 pos = new Vector3(cell.x, cell.y, 0f);
            GameObject go = new GameObject($"Sandbox_{tool}_{cell.x}_{cell.y}");
            go.transform.position = pos;

            switch (tool)
            {
                case ToolType.Wall:
                case ToolType.LightOnlyWall:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = wallSprite;
                        sr.drawMode = SpriteDrawMode.Tiled;
                        sr.size = Vector2.one;
                        sr.sortingOrder = 2;
                        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                        col.size = Vector2.one;
                        WorldPhaseObject phase = go.AddComponent<WorldPhaseObject>();
                        phase.affinity = tool == ToolType.LightOnlyWall ? WorldAffinity.LightOnly : WorldAffinity.Both;
                        if (tool == ToolType.Wall)
                        {
                            phase.lightWorldSprite = wallSprite;
                            phase.shadowWorldSprite = wallShadowSprite;
                        }
                        break;
                    }
                case ToolType.Emitter:
                    {
                        go.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = emitterSprite;
                        sr.sortingOrder = 4;
                        LightBeamEmitter em = go.AddComponent<LightBeamEmitter>();
                        em.emitterRenderer = sr;
                        em.beamColor = LightColor.White;
                        em.activeInWorld = WorldAffinity.Both;
                        em.localEmitDirection = DirVectors[rot % 4];
                        break;
                    }
                case ToolType.Mirror:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = (mirrorDirs != null && mirrorDirs.Length == 4) ? mirrorDirs[rot % 4] : crateSprite;
                        sr.sortingOrder = 4;
                        go.AddComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
                        OpticalMirror m = go.AddComponent<OpticalMirror>();
                        m.isTwoSided = true;
                        m.directionalSprites = mirrorDirs;
                        m.logicalAngle = 45f + (rot % 4) * 90f;
                        m.ApplyIsometricVisual();
                        break;
                    }
                case ToolType.Prism:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = prismSprite;
                        sr.sortingOrder = 4;
                        go.AddComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
                        OpticalPrism pr = go.AddComponent<OpticalPrism>();
                        pr.mode = OpticalPrism.PrismMode.Splitter;
                        pr.splitAngle = 90f;
                        break;
                    }
                case ToolType.Receiver:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = receiverSprite;
                        sr.sortingOrder = 3;
                        go.AddComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
                        LightSensor s = go.AddComponent<LightSensor>();
                        s.acceptAnyColor = true;
                        s.indicatorRenderer = sr;
                        s.inactiveSprite = receiverSprite;
                        s.activeSprite = Resources.Load<Sprite>("Sprites/Receiver_Active_Glow");
                        break;
                    }
                case ToolType.Button:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = btnUp;
                        sr.sortingOrder = 1;
                        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
                        col.size = new Vector2(0.95f, 0.95f);
                        col.isTrigger = true;
                        PressurePlate pp = go.AddComponent<PressurePlate>();
                        pp.plateRenderer = sr;
                        pp.unpressedSprite = btnUp;
                        pp.pressedSprite = btnDown;
                        break;
                    }
                case ToolType.Crate:
                    {
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = crateSprite;
                        sr.sortingOrder = 4;
                        go.AddComponent<BoxCollider2D>().size = new Vector2(0.9f, 0.85f);
                        go.AddComponent<PushableBlock>();
                        break;
                    }
                case ToolType.Door:
                    {
                        go.transform.localScale = new Vector3(0.9f, 0.85f, 1f);
                        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                        sr.sprite = doorClosed;
                        sr.sortingOrder = 3;
                        go.AddComponent<BoxCollider2D>().size = new Vector2(1.05f, 1.3f);
                        PuzzleDoor door = go.AddComponent<PuzzleDoor>();
                        door.closedSprite = doorClosed;
                        door.openSprite = doorOpen;
                        door.requireAll = true;
                        break;
                    }
            }

            placedObjects[cell] = go;
            placedRecords[cell] = new SavedItem { tool = (int)tool, gx = cell.x, gy = cell.y, rotStep = rot };
            RelinkSandboxDoors();
            PuzzleAudioManager.Instance?.PlayButtonClick();
            PuzzleVFXManager.SpawnPixelBurst(pos, Color.white, 5, 2.0f);
        }

        private void RelinkSandboxDoors()
        {
            List<LightSensor> sensors = new List<LightSensor>();
            List<PressurePlate> plates = new List<PressurePlate>();
            List<PuzzleDoor> doors = new List<PuzzleDoor>();

            foreach (GameObject go in placedObjects.Values)
            {
                if (go == null) continue;
                LightSensor s = go.GetComponent<LightSensor>();
                if (s != null) sensors.Add(s);
                PressurePlate p = go.GetComponent<PressurePlate>();
                if (p != null) plates.Add(p);
                PuzzleDoor d = go.GetComponent<PuzzleDoor>();
                if (d != null) doors.Add(d);
            }

            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].requiredSensors = sensors.ToArray();
                doors[i].requiredPlates = plates.ToArray();
            }
        }

        private void ClearAllPlaced()
        {
            foreach (GameObject go in placedObjects.Values)
            {
                if (go != null) Destroy(go);
            }
            placedObjects.Clear();
            placedRecords.Clear();
            statusNotice = "Cleared sandbox grid.";
        }

        private void SaveJson()
        {
            SandboxSaveData data = new SandboxSaveData();
            foreach (SavedItem item in placedRecords.Values)
            {
                data.items.Add(item);
            }
            File.WriteAllText(SaveFilePath, JsonUtility.ToJson(data, true));
            statusNotice = $"Saved {data.items.Count} props to shadowflip_sandbox.json";
        }

        private void LoadJson()
        {
            if (!File.Exists(SaveFilePath))
            {
                statusNotice = "No saved sandbox layout found yet.";
                return;
            }
            ClearAllPlaced();
            SandboxSaveData data = JsonUtility.FromJson<SandboxSaveData>(File.ReadAllText(SaveFilePath));
            if (data != null && data.items != null)
            {
                for (int i = 0; i < data.items.Count; i++)
                {
                    SavedItem item = data.items[i];
                    PlaceItem(new Vector2Int(item.gx, item.gy), (ToolType)item.tool, item.rotStep);
                }
                statusNotice = $"Loaded {data.items.Count} props!";
            }
        }

        void OnGUI()
        {
            if (Time.timeScale == 0f) return;
            EnsureStyles();

            // Palette bar across top-right (leaves top-left HUD clear)
            float xStart = 650f;
            float panelW = Mathf.Max(460f, Screen.width - xStart - 16f);
            Rect bar = new Rect(xStart, 14f, panelW, 94f);

            GUI.color = new Color(0.06f, 0.05f, 0.10f, 0.94f);
            GUI.DrawTexture(bar, whiteTex);
            GUI.color = new Color(0.16f, 0.14f, 0.24f, 0.94f);
            GUI.DrawTexture(new Rect(bar.x + 3f, bar.y + 3f, bar.width - 6f, bar.height - 6f), whiteTex);
            GUI.color = Color.white;

            string dirStr = new[] { "→", "↑", "←", "↓" }[rotationStep % 4];
            GUI.Label(new Rect(bar.x + 10f, bar.y + 6f, 300f, 20f),
                $"SANDBOX EDITOR | Dir:{dirStr} | {statusNotice}", labelStyle);

            if (GUI.Button(new Rect(bar.x + bar.width - 285f, bar.y + 5f, 92f, 22f), buildMode ? "▶ TEST [P]" : "✎ EDIT [P]"))
            {
                buildMode = !buildMode;
                ApplyModeState();
            }
            if (GUI.Button(new Rect(bar.x + bar.width - 189f, bar.y + 5f, 60f, 22f), "SAVE")) SaveJson();
            if (GUI.Button(new Rect(bar.x + bar.width - 125f, bar.y + 5f, 60f, 22f), "LOAD")) LoadJson();
            if (GUI.Button(new Rect(bar.x + bar.width - 61f, bar.y + 5f, 52f, 22f), "CLR")) ClearAllPlaced();

            float btnW = Mathf.Floor((bar.width - 24f) / 5f);
            for (int i = 0; i < ToolLabels.Length; i++)
            {
                int row = i / 5;
                int col = i % 5;
                Rect tb = new Rect(bar.x + 10f + col * (btnW + 2f), bar.y + 32f + row * 28f, btnW - 2f, 24f);
                GUI.backgroundColor = selectedTool == (ToolType)i ? new Color(0.45f, 0.95f, 1f, 1f) : Color.white;
                if (GUI.Button(tb, ToolLabels[i]))
                {
                    selectedTool = (ToolType)i;
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void LoadSpritesFromResources()
        {
            wallSprite = Resources.Load<Sprite>("Sprites/Wall_Light_Long");
            wallShadowSprite = Resources.Load<Sprite>("Sprites/Wall_Shadow_Long");
            emitterSprite = Resources.Load<Sprite>("Sprites/Emitter_Active");
            prismSprite = Resources.Load<Sprite>("Sprites/Prism_Crystal");
            receiverSprite = Resources.Load<Sprite>("Sprites/Receiver_Inactive");
            btnUp = Resources.Load<Sprite>("Sprites/Button_Up");
            btnDown = Resources.Load<Sprite>("Sprites/Button_Down");
            crateSprite = Resources.Load<Sprite>("Sprites/Emitter_Broken");
            doorClosed = Resources.Load<Sprite>("Sprites/Door_Closed");
            doorOpen = Resources.Load<Sprite>("Sprites/Door_Open");
            mirrorDirs = new[]
            {
                Resources.Load<Sprite>("Sprites/Mirror_45"),
                Resources.Load<Sprite>("Sprites/Mirror_135"),
                Resources.Load<Sprite>("Sprites/Mirror_225"),
                Resources.Load<Sprite>("Sprites/Mirror_315")
            };
        }

        private void EnsureStyles()
        {
            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.filterMode = FilterMode.Point;
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }
    }
}
