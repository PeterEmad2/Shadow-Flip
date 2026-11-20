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

        [Header("Capacitor Hold (Seconds active after beam stops)")]
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

        public bool IsActivated { get; private set; }
        public bool IsReceivingValidBeam { get; private set; }
        public float HoldRemaining { get; private set; }

        private float chargeProgress = 0f;
        private int lastValidHitFrame = -100;

        void Awake()
        {
            if (indicatorRenderer == null) indicatorRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
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
            if (requiredWorld == WorldAffinity.Both) return true;
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            if (requiredWorld == WorldAffinity.LightOnly && isDark && RealityAnchorLantern.IsPositionAnchoredInLight(transform.position))
            {
                return true;
            }
            return (requiredWorld == WorldAffinity.ShadowOnly && isDark)
                || (requiredWorld == WorldAffinity.LightOnly && !isDark);
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
