using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Spreading Umbra Mold patch: grows during the Shadow Realm and slows Jimmy when overgrown,
    /// while shrinking rapidly when swept or crossed by any optical beam (without blocking the beam).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class UmbraMoldPatch : MonoBehaviour, IOpticalElement
    {
        public float minScale = 0.35f;
        public float maxScale = 1.65f;
        public float growRate = 0.22f;
        public float burnRate = 1.4f;

        private float currentScale = 0.9f;
        private int lastBurnFrame = -100;
        private float sparkCooldown = 0f;
        private logic cachedLogic;

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            currentScale = Mathf.Clamp(transform.localScale.x, minScale, maxScale);
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            bool beingBurned = (Time.frameCount - lastBurnFrame) <= 1;
            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;

            if (beingBurned)
            {
                currentScale = Mathf.Max(minScale, currentScale - burnRate * Time.deltaTime);
                sparkCooldown -= Time.deltaTime;
                if (sparkCooldown <= 0f)
                {
                    sparkCooldown = 0.22f;
                    PuzzleVFXManager.SpawnPixelBurst(transform.position, new Color(0.85f, 0.38f, 1f, 1f), 4, 2.0f);
                }
            }
            else if (isDark)
            {
                currentScale = Mathf.Min(maxScale, currentScale + growRate * Time.deltaTime);
            }

            transform.localScale = new Vector3(currentScale, currentScale, 1f);
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            lastBurnFrame = Time.frameCount;
            // Forward the beam cleanly across the floor patch without wasting a bounce.
            Vector2 dir = hit.IncomingDirection.normalized;
            int keepBounces = Mathf.Min(emitter.maxBounces, remainingBounces + 1);
            emitter.EmitChildBeam(hit.Point + dir * 0.02f, dir, hit.Color, keepBounces, hit.Collider);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (currentScale <= 0.6f) return;
            PlayerMovement pm = other.GetComponentInParent<PlayerMovement>();
            if (pm == null && (other.GetComponent<Light>() != null || other.GetComponent<Dark>() != null))
            {
                pm = FindFirstObjectByType<PlayerMovement>();
            }
            if (pm != null)
            {
                pm.ApplyMoldSlow(0.15f);
            }
        }
    }
}
