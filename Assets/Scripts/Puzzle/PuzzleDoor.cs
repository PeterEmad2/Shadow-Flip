using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Dungeon gate that opens when connected LightSensors, PressurePlates (buttons), and PuzzleLevers are active.
    /// </summary>
    public class PuzzleDoor : MonoBehaviour
    {
        [Header("Inputs")]
        public LightSensor[] requiredSensors;
        public PressurePlate[] requiredPlates;
        public PuzzleLever[] requiredLevers;
        public UmbraWardenBoss requiredBoss;
        public bool requireAll = true;

        [Header("Behavior & Cel-Shaded Visuals")]
        public bool stayOpenOnceUnlocked = false;
        public float fadeSpeed = 8f;
        public Sprite closedSprite;
        public Sprite openSprite;

        private Collider2D doorCollider;
        private SpriteRenderer doorRenderer;
        private Color closedColor;
        private bool isOpen = false;
        private bool hasUnlocked = false;
        private readonly System.Collections.Generic.List<IndicatorVisual> indicators = new System.Collections.Generic.List<IndicatorVisual>();

        private struct IndicatorVisual
        {
            public LightSensor Sensor;
            public SpriteRenderer Gem;
            public SpriteRenderer Core;
            public Color BaseColor;
            public Transform Root;
        }

        private static Sprite sharedPixelSprite;

        public bool IsOpen => isOpen;

        void Awake()
        {
            doorCollider = GetComponent<Collider2D>();
            doorRenderer = GetComponent<SpriteRenderer>();
            if (doorRenderer != null)
            {
                closedColor = doorRenderer.color;
                if (closedSprite != null) doorRenderer.sprite = closedSprite;
            }
        }

        void Start()
        {
            RebuildIndicators();
        }

        void Update()
        {
            bool conditionMet = CheckCondition();
            if (stayOpenOnceUnlocked && conditionMet)
            {
                hasUnlocked = true;
            }

            bool nextOpen = stayOpenOnceUnlocked ? hasUnlocked : conditionMet;
            if (nextOpen != isOpen)
            {
                isOpen = nextOpen;
                if (doorCollider != null) doorCollider.enabled = !isOpen;
                if (isOpen)
                {
                    PuzzleAudioManager.Instance?.PlayDoorOpen();
                    PuzzleVFXManager.SpawnPixelBurst(transform.position, new Color(0.45f, 1f, 0.72f, 1f), 12, 3.5f);
                    RealmCameraController.TriggerShake(0.12f, 0.15f);
                }
            }

            UpdateIndicatorVisuals();

            if (doorRenderer != null)
            {
                if (closedSprite != null && openSprite != null)
                {
                    doorRenderer.sprite = isOpen ? openSprite : closedSprite;
                    doorRenderer.color = closedColor;
                }
                else
                {
                    Color target = closedColor;
                    target.a = isOpen ? 0.15f : closedColor.a;
                    doorRenderer.color = Color.Lerp(doorRenderer.color, target, Time.deltaTime * fadeSpeed);
                }
            }
        }

        public void RebuildIndicators()
        {
            for (int i = 0; i < indicators.Count; i++)
            {
                if (indicators[i].Root != null)
                {
                    Destroy(indicators[i].Root.gameObject);
                }
            }
            indicators.Clear();

            if (requiredSensors == null || requiredSensors.Length == 0) return;

            Sprite sq = GetPixelSprite();
            int validCount = 0;
            for (int i = 0; i < requiredSensors.Length; i++)
            {
                if (requiredSensors[i] != null) validCount++;
            }
            if (validCount == 0) return;

            bool rotatedAlongX = Mathf.Abs(transform.right.y) > Mathf.Abs(transform.up.y);
            Vector3 lossy = transform.lossyScale;
            Vector3 bulbScale = new Vector3(
                0.24f / Mathf.Max(0.2f, Mathf.Abs(lossy.x)),
                0.24f / Mathf.Max(0.2f, Mathf.Abs(lossy.y)),
                1f);

            int idx = 0;
            float spacing = 0.42f;
            for (int i = 0; i < requiredSensors.Length; i++)
            {
                LightSensor sensor = requiredSensors[i];
                if (sensor == null) continue;

                float worldOffset = (idx - (validCount - 1) * 0.5f) * spacing;
                Vector3 localPos = rotatedAlongX
                    ? new Vector3(-worldOffset / Mathf.Max(0.2f, Mathf.Abs(lossy.x)), 0f, 0f)
                    : new Vector3(0f, -worldOffset / Mathf.Max(0.2f, Mathf.Abs(lossy.y)), 0f);

                GameObject root = new GameObject($"DoorIndicator_{idx}");
                root.transform.SetParent(transform, false);
                root.transform.localPosition = localPos;
                root.transform.localScale = bulbScale;

                SpriteRenderer bezel = root.AddComponent<SpriteRenderer>();
                bezel.sprite = sq;
                bezel.color = new Color(0.07f, 0.05f, 0.11f, 0.96f);
                bezel.sortingOrder = 14;

                GameObject gemGo = new GameObject("Gem");
                gemGo.transform.SetParent(root.transform, false);
                gemGo.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
                SpriteRenderer gemSr = gemGo.AddComponent<SpriteRenderer>();
                gemSr.sprite = sq;
                gemSr.sortingOrder = 15;

                GameObject coreGo = new GameObject("Core");
                coreGo.transform.SetParent(root.transform, false);
                coreGo.transform.localScale = new Vector3(0.32f, 0.32f, 1f);
                SpriteRenderer coreSr = coreGo.AddComponent<SpriteRenderer>();
                coreSr.sprite = sq;
                coreSr.color = Color.white;
                coreSr.sortingOrder = 16;

                Color baseCol = sensor.acceptAnyColor
                    ? new Color(1f, 0.95f, 0.72f, 1f)
                    : LightColorUtility.ToUnityColor(sensor.requiredColor);

                indicators.Add(new IndicatorVisual
                {
                    Sensor = sensor,
                    Gem = gemSr,
                    Core = coreSr,
                    BaseColor = baseCol,
                    Root = root.transform
                });
                idx++;
            }

            UpdateIndicatorVisuals();
        }

        private void UpdateIndicatorVisuals()
        {
            for (int i = 0; i < indicators.Count; i++)
            {
                IndicatorVisual ind = indicators[i];
                if (ind.Sensor == null || ind.Gem == null) continue;

                bool lit = ind.Sensor.IsActivated;
                Color dimCol = Color.Lerp(ind.BaseColor, new Color(0.08f, 0.08f, 0.12f, 1f), 0.70f);
                ind.Gem.color = lit ? ind.BaseColor : dimCol;
                if (ind.Core != null) ind.Core.enabled = lit;
            }
        }

        private static Sprite GetPixelSprite()
        {
            if (sharedPixelSprite == null)
            {
                Texture2D tex = new Texture2D(4, 4);
                tex.filterMode = FilterMode.Point;
                Color[] px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.Apply();
                sharedPixelSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return sharedPixelSprite;
        }

        private bool CheckCondition()
        {
            int totalInputs = (requiredSensors != null ? requiredSensors.Length : 0)
                            + (requiredPlates != null ? requiredPlates.Length : 0)
                            + (requiredLevers != null ? requiredLevers.Length : 0)
                            + (requiredBoss != null ? 1 : 0);
            if (totalInputs == 0) return false;

            int activeCount = 0;

            if (requiredSensors != null)
            {
                foreach (LightSensor sensor in requiredSensors)
                {
                    if (sensor != null && sensor.IsActivated) activeCount++;
                }
            }

            if (requiredPlates != null)
            {
                foreach (PressurePlate plate in requiredPlates)
                {
                    if (plate != null && plate.IsPressed) activeCount++;
                }
            }

            if (requiredLevers != null)
            {
                foreach (PuzzleLever lever in requiredLevers)
                {
                    if (lever != null && lever.isActivated) activeCount++;
                }
            }

            if (requiredBoss != null && requiredBoss.IsDefeated)
            {
                activeCount++;
            }

            return requireAll ? activeCount == totalInputs : activeCount > 0;
        }
    }
}
