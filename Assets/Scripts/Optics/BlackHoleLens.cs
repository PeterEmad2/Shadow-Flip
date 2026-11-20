using System.Collections.Generic;
using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Gravitational-lensing singularity that bends incoming optical beams along curved spacetime
    /// paths inside gravityRadius and swallows photons crossing eventHorizonRadius.
    /// Optionally tunable with [E] between Overdrive (Swallow), Lens (Bend), and White Hole (Repel).
    /// </summary>
    public class BlackHoleLens : MonoBehaviour, IInteractable
    {
        public static readonly List<BlackHoleLens> ActiveBlackHoles = new List<BlackHoleLens>();

        [Header("Realm & Lensing")]
        public WorldAffinity activeInWorld = WorldAffinity.Both;
        public float gravityRadius = 3.3f;
        public float eventHorizonRadius = 0.48f;
        public float gravityStrength = 2.8f;

        [Header("Interactive Tuning (Optional)")]
        public bool canAdjustStrength = false;
        public float[] strengthPresets = { 6.2f, 2.8f, -2.6f };
        public string[] presetLabels = { "OVERDRIVE (Swallow)", "LENS (Bend)", "WHITE HOLE (Repel)" };
        public int currentPresetIndex = 1;

        [Header("Visuals")]
        public Sprite frame1;
        public Sprite frame2;
        public float spinDegreesPerSecond = 28f;

        public bool IsFieldActive { get; private set; }

        public float EffectiveGravityStrength
        {
            get
            {
                if (canAdjustStrength && strengthPresets != null && strengthPresets.Length > 0)
                {
                    int idx = ((currentPresetIndex % strengthPresets.Length) + strengthPresets.Length) % strengthPresets.Length;
                    return strengthPresets[idx];
                }
                return gravityStrength;
            }
        }

        private SpriteRenderer sr;
        private CircleCollider2D coreCollider;
        private LineRenderer gravityRing;
        private LineRenderer horizonRing;
        private float lastAbsorbVfxTime = -10f;
        private logic cachedLogic;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ActiveBlackHoles.Clear();
        }

        public static bool HasAnyActiveBlackHoles()
        {
            for (int i = 0; i < ActiveBlackHoles.Count; i++)
            {
                BlackHoleLens bh = ActiveBlackHoles[i];
                if (bh != null && bh.gameObject.activeInHierarchy && bh.IsFieldActive)
                {
                    return true;
                }
            }
            return false;
        }

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            coreCollider = GetComponent<CircleCollider2D>();

            if (frame1 == null) frame1 = sr != null ? sr.sprite : null;
            if (frame1 == null) frame1 = Resources.Load<Sprite>("Sprites/PixelNew/BlackHole_1");
            if (frame2 == null) frame2 = Resources.Load<Sprite>("Sprites/PixelNew/BlackHole_2");
            if (frame2 == null) frame2 = frame1;

            if (sr != null && frame1 != null && sr.sprite == null)
            {
                sr.sprite = frame1;
            }

            if (canAdjustStrength && strengthPresets != null && strengthPresets.Length > 0)
            {
                currentPresetIndex = ((currentPresetIndex % strengthPresets.Length) + strengthPresets.Length) % strengthPresets.Length;
                gravityStrength = strengthPresets[currentPresetIndex];
            }

            gravityRing = CreateRingRenderer("GravityFieldRing", 0.06f, 1);
            horizonRing = CreateRingRenderer("EventHorizonRing", 0.08f, 6);
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            SyncColliderAndRings();
            RefreshFieldState();
        }

        void OnEnable()
        {
            if (!ActiveBlackHoles.Contains(this))
            {
                ActiveBlackHoles.Add(this);
            }
        }

        void OnDisable()
        {
            ActiveBlackHoles.Remove(this);
        }

        void Update()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            RefreshFieldState();

            if (Time.timeScale == 0f) return;

            if (IsFieldActive)
            {
                if (sr != null && frame1 != null && frame2 != null)
                {
                    int idx = Mathf.FloorToInt(Time.time * 6f) % 2;
                    sr.sprite = idx == 0 ? frame1 : frame2;
                }

                float spinDir = EffectiveGravityStrength < 0f ? -1f : 1f;
                transform.Rotate(0f, 0f, spinDegreesPerSecond * spinDir * Time.deltaTime);
            }
        }

        public void SyncColliderAndRings()
        {
            float invScale = 1f / Mathf.Max(0.1f, Mathf.Abs(transform.lossyScale.x));

            if (coreCollider != null)
            {
                coreCollider.radius = eventHorizonRadius * invScale * 0.9f;
            }

            PopulatePixelRing(gravityRing, gravityRadius * invScale, 32);
            PopulatePixelRing(horizonRing, eventHorizonRadius * invScale, 16);
        }

        public void NotifyBeamAbsorbed(LightColor color, Vector2 hitPos)
        {
            if (Time.unscaledTime - lastAbsorbVfxTime < 0.25f) return;
            lastAbsorbVfxTime = Time.unscaledTime;
            Color col = Color.Lerp(LightColorUtility.ToUnityColor(color), new Color(1f, 0.72f, 0.25f, 1f), 0.45f);
            PuzzleVFXManager.SpawnPixelBurst(hitPos, col, 5, 2.4f);
        }

        public void Interact(PlayerMovement player)
        {
            if (!canAdjustStrength || !IsFieldActive) return;
            if (strengthPresets == null || strengthPresets.Length == 0) return;

            currentPresetIndex = (currentPresetIndex + 1) % strengthPresets.Length;
            gravityStrength = strengthPresets[currentPresetIndex];

            PuzzleAudioManager.Instance?.PlayMirrorRotate();
            PuzzleVFXManager.SpawnPixelBurst(transform.position, GetFieldAccentColor(), 12, 3.4f);
            RealmCameraController.TriggerShake(0.08f, 0.10f);
        }

        public string GetInteractPrompt()
        {
            if (!canAdjustStrength || !IsFieldActive)
            {
                if (canAdjustStrength && !IsFieldActive)
                {
                    return activeInWorld == WorldAffinity.ShadowOnly
                        ? "Tune Singularity in Shadow Realm"
                        : "Tune Singularity in Light Realm";
                }
                return string.Empty;
            }

            string label = (presetLabels != null && currentPresetIndex < presetLabels.Length)
                ? presetLabels[currentPresetIndex]
                : $"{EffectiveGravityStrength:0.0}G";
            return $"[E] Tune Singularity: {label}";
        }

        private void RefreshFieldState()
        {
            IsFieldActive = EvaluateRealmActive();

            if (coreCollider != null)
            {
                coreCollider.enabled = IsFieldActive;
            }

            Color accent = GetFieldAccentColor();
            if (sr != null)
            {
                if (IsFieldActive)
                {
                    sr.color = EffectiveGravityStrength < 0f
                        ? new Color(0.65f, 0.96f, 1f, 1f)
                        : Color.white;
                }
                else
                {
                    sr.color = new Color(0.65f, 0.65f, 0.85f, 0.22f);
                }
            }

            float pulse = 0.82f + 0.18f * Mathf.Sin(Time.time * 4.5f);
            if (gravityRing != null)
            {
                gravityRing.enabled = IsFieldActive;
                Color outer = accent;
                outer.a = 0.34f * pulse;
                gravityRing.startColor = outer;
                gravityRing.endColor = outer;
            }

            if (horizonRing != null)
            {
                horizonRing.enabled = IsFieldActive;
                Color inner = Color.Lerp(accent, Color.white, 0.25f);
                inner.a = 0.9f;
                horizonRing.startColor = inner;
                horizonRing.endColor = inner;
            }
        }

        private bool EvaluateRealmActive()
        {
            if (!gameObject.activeInHierarchy) return false;
            if (activeInWorld == WorldAffinity.Both) return true;

            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;
            if (activeInWorld == WorldAffinity.LightOnly && isDark && RealityAnchorLantern.IsPositionAnchoredInLight(transform.position))
            {
                return true;
            }

            return (activeInWorld == WorldAffinity.ShadowOnly && isDark)
                || (activeInWorld == WorldAffinity.LightOnly && !isDark);
        }

        private Color GetFieldAccentColor()
        {
            float s = EffectiveGravityStrength;
            if (s < 0f) return new Color(0.35f, 0.95f, 1.0f, 1f);     // White Hole Repel (Cyan)
            if (s >= 4.5f) return new Color(1.0f, 0.56f, 0.22f, 1f);  // Overdrive Swallow (Amber-Orange)
            return new Color(0.68f, 0.42f, 1.0f, 1f);                 // Gravitational Lens (Violet)
        }

        private LineRenderer CreateRingRenderer(string childName, float width, int sortingOrder)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.numCornerVertices = 0;
            lr.numCapVertices = 0;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.sortingOrder = sortingOrder;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) lr.material = new Material(shader);
            return lr;
        }

        private static void PopulatePixelRing(LineRenderer lr, float radius, int segments)
        {
            if (lr == null) return;
            lr.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float px = Mathf.Round(Mathf.Cos(angle) * radius * 16f) / 16f;
                float py = Mathf.Round(Mathf.Sin(angle) * radius * 16f) / 16f;
                lr.SetPosition(i, new Vector3(px, py, 0f));
            }
        }
    }
}
