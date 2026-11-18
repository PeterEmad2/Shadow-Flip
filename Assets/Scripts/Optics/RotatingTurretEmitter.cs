using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Automatically rotates its attached LightBeamEmitter whenever the realm flips
    /// or at fixed intervals.
    /// </summary>
    [RequireComponent(typeof(LightBeamEmitter))]
    public class RotatingTurretEmitter : MonoBehaviour
    {
        public bool rotateOnWorldFlip = true;
        public float autoRotateInterval = 0f;
        public float stepDegrees = 90f;

        private LightBeamEmitter emitter;
        private logic cachedLogic;
        private bool lastKnownDark;
        private float intervalTimer = 0f;

        void Awake()
        {
            emitter = GetComponent<LightBeamEmitter>();
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedLogic != null)
            {
                lastKnownDark = cachedLogic.isDarkActive;
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
            }
        }

        void OnDestroy()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
        }

        void Update()
        {
            if (Time.timeScale == 0f || autoRotateInterval <= 0f) return;

            intervalTimer += Time.deltaTime;
            if (intervalTimer >= autoRotateInterval)
            {
                intervalTimer -= autoRotateInterval;
                RotateEmitter();
            }
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (isDarkActive == lastKnownDark) return;
            lastKnownDark = isDarkActive;
            if (rotateOnWorldFlip)
            {
                RotateEmitter();
            }
        }

        private void RotateEmitter()
        {
            if (emitter == null) return;
            float rad = stepDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            Vector2 v = emitter.localEmitDirection.normalized;
            emitter.localEmitDirection = new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
        }
    }
}
