using ShadowFlip.Puzzle;
using UnityEngine;
using UnityEngine.Events;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Optical receptor that activates when hit by a matching light beam.
    /// Supports timed capacitor hold duration after losing its beam.
    /// </summary>
    public class LightSensor : MonoBehaviour, IOpticalElement
    {
        [Header("Requirements")]
        public LightColor requiredColor = LightColor.White;
        public bool acceptAnyColor = false;
        public WorldAffinity requiredWorld = WorldAffinity.Both;
        public float chargeTime = 0f;

        [Header("Capacitor Mode (Charges in Umbra, Discharges Slowly)")]
        public bool isCapacitor = false;
        public float holdDuration = 0f;

        [Header("Cel-Shaded Visuals")]
        public SpriteRenderer indicatorRenderer;
        public Sprite inactiveSprite;
        public Sprite activeSprite;
        public Color inactiveColor = new Color(0.25f, 0.25f, 0.3f, 1f);
        public Color activeColor = new Color(1f, 0.9f, 0.3f, 1f);

        [Header("Events")]
        public UnityEvent onActivated;
        public UnityEvent onDeactivated;

        public bool IsCapacitor => isCapacitor;
        public bool IsActivated { get; private set; }
        public bool IsReceivingValidBeam { get; private set; }
        public float HoldRemaining { get; private set; }

        private float chargeProgress = 0f;
        private int lastValidHitFrame = -100;
        private Transform capGaugeRoot;
        private SpriteRenderer capGaugeFill;
        private float capGaugeWidth = 0.96f;
        private static Sprite sharedGaugeSprite;

        void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponent<SpriteRenderer>();

            if (isCapacitor)
            {
                if (holdDuration <= 0.1f) holdDuration = 9.0f;
                requiredWorld = WorldAffinity.ShadowOnly;
                Sprite capIdle = Resources.Load<Sprite>("Sprites/PixelNew/Capacitor_Idle");
                Sprite capAct = Resources.Load<Sprite>("Sprites/PixelNew/Capacitor_Active");
                if (capIdle != null && (inactiveSprite == null || inactiveSprite.name.StartsWith("Receiver_")))
                {
                    inactiveSprite = capIdle;
                }
                if (capAct != null && (activeSprite == null || activeSprite.name.StartsWith("Receiver_")))
                {
                    activeSprite = capAct;
                }
                BuildCapacitorGauge();
            }

            UpdateVisuals();
            UpdateCapacitorGaugeVisual();
        }

        void Update()
        {
            IsReceivingValidBeam = (Time.frameCount - lastValidHitFrame) <= 1;

            if (IsReceivingValidBeam)
            {
                if (chargeTime <= 0f)
                {
                    HoldRemaining = holdDuration;
                    SetActivated(true);
                }
                else
                {
                    chargeProgress = Mathf.Min(chargeTime, chargeProgress + Time.deltaTime);
                    if (chargeProgress >= chargeTime)
                    {
                        HoldRemaining = holdDuration;
                        SetActivated(true);
                    }
                }
            }
            else
            {
                chargeProgress = 0f;
                if (HoldRemaining > 0f)
                {
                    HoldRemaining = Mathf.Max(0f, HoldRemaining - Time.deltaTime);
                    SetActivated(true);
                }
                else
                {
                    SetActivated(false);
                }
            }

            UpdateCapacitorGaugeVisual();
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            emitter.RegisterSensorHit(this, hit.Color);
        }

        internal void NotifyBeamHit(LightColor incomingColor)
        {
            if (!IsWorldValid()) return;
            if (!LightColorUtility.Matches(incomingColor, requiredColor, acceptAnyColor)) return;

            lastValidHitFrame = Time.frameCount;
            if (chargeTime <= 0f)
            {
                IsReceivingValidBeam = true;
                HoldRemaining = holdDuration;
                SetActivated(true);
            }
        }

        private bool IsWorldValid()
        {
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool globalDark = l != null && l.isDarkActive;
            bool isDark = SplitRealmZone.GetEffectiveDarkState(transform.position, globalDark);

            // Capacitor Receivers charge strictly in the Umbra (Dark) Side.
            if (isCapacitor)
            {
                return isDark;
            }

            if (requiredWorld == WorldAffinity.Both) return true;
            if (requiredWorld == WorldAffinity.LightOnly && isDark && RealityAnchorLantern.IsPositionAnchoredInLight(transform.position))
            {
                return true;
            }
            return (requiredWorld == WorldAffinity.ShadowOnly && isDark)
                || (requiredWorld == WorldAffinity.LightOnly && !isDark);
        }

        private void BuildCapacitorGauge()
        {
            if (capGaugeRoot != null) return;
            Sprite sq = GetGaugeSprite();

            GameObject root = new GameObject("CapacitorChargeBar");
            root.transform.SetParent(transform, false);
            float invScale = 1f / Mathf.Max(0.2f, Mathf.Abs(transform.lossyScale.x));
            root.transform.localPosition = new Vector3(0f, 0.72f * invScale, 0f);
            root.transform.localScale = new Vector3(invScale, invScale, 1f);
            capGaugeRoot = root.transform;

            GameObject bgGo = new GameObject("Bg");
            bgGo.transform.SetParent(capGaugeRoot, false);
            bgGo.transform.localScale = new Vector3(capGaugeWidth + 0.10f, 0.22f, 1f);
            SpriteRenderer bgSr = bgGo.AddComponent<SpriteRenderer>();
            bgSr.sprite = sq;
            bgSr.color = new Color(0.07f, 0.05f, 0.12f, 0.96f);
            bgSr.sortingOrder = 14;

            GameObject fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(capGaugeRoot, false);
            fillGo.transform.localScale = new Vector3(capGaugeWidth, 0.12f, 1f);
            capGaugeFill = fillGo.AddComponent<SpriteRenderer>();
            capGaugeFill.sprite = sq;
            capGaugeFill.color = new Color(0.32f, 0.94f, 1f, 1f);
            capGaugeFill.sortingOrder = 15;
        }

        private void UpdateCapacitorGaugeVisual()
        {
            if (!isCapacitor || capGaugeFill == null) return;
            float ratio = holdDuration > 0.01f ? Mathf.Clamp01(HoldRemaining / holdDuration) : (IsActivated ? 1f : 0f);
            float w = Mathf.Max(0.001f, capGaugeWidth * ratio);
            capGaugeFill.enabled = ratio > 0.01f;
            capGaugeFill.transform.localScale = new Vector3(w, 0.12f, 1f);
            capGaugeFill.transform.localPosition = new Vector3(-(capGaugeWidth - w) * 0.5f, 0f, 0f);
            capGaugeFill.color = Color.Lerp(new Color(0.68f, 0.34f, 1f, 1f), new Color(0.32f, 0.96f, 1f, 1f), ratio);
        }

        private static Sprite GetGaugeSprite()
        {
            if (sharedGaugeSprite == null)
            {
                Texture2D tex = new Texture2D(4, 4);
                tex.filterMode = FilterMode.Point;
                Color[] px = new Color[16];
                for (int i = 0; i < 16; i++) px[i] = Color.white;
                tex.SetPixels(px);
                tex.Apply();
                sharedGaugeSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }
            return sharedGaugeSprite;
        }

        private void SetActivated(bool active)
        {
            if (IsActivated == active) return;
            IsActivated = active;
            UpdateVisuals();
            PuzzleAudioManager.Instance?.PlaySensorState(active);

            if (IsActivated)
            {
                Color c = acceptAnyColor ? activeColor : LightColorUtility.ToUnityColor(requiredColor);
                PuzzleVFXManager.SpawnPixelBurst(transform.position, c, 8, 2.8f);
                onActivated?.Invoke();
            }
            else
            {
                onDeactivated?.Invoke();
            }
        }

        private void UpdateVisuals()
        {
            if (indicatorRenderer == null) return;

            bool hasSprites = inactiveSprite != null && activeSprite != null;
            if (hasSprites)
            {
                indicatorRenderer.sprite = IsActivated ? activeSprite : inactiveSprite;
                if (IsActivated)
                {
                    Color targetTint = acceptAnyColor ? activeColor : LightColorUtility.ToUnityColor(requiredColor);
                    indicatorRenderer.color = Color.Lerp(Color.white, targetTint, 0.4f);
                }
                else
                {
                    indicatorRenderer.color = Color.white;
                }
                return;
            }

            indicatorRenderer.color = IsActivated
                ? (acceptAnyColor ? activeColor : LightColorUtility.ToUnityColor(requiredColor))
                : inactiveColor;
        }
    }
}
