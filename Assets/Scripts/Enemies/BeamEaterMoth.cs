using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Beam-Eater Moth: flutters toward nearby active laser beams and absorbs them.
    /// Striking it with stunColor (e.g., Red) stuns it for stunDuration so other beams pass safely.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BeamEaterMoth : MonoBehaviour, IOpticalElement
    {
        [Header("Sprites")]
        public Sprite frame1;
        public Sprite frame2;

        [Header("Behavior")]
        public LightColor stunColor = LightColor.Red;
        public float flySpeed = 2.2f;
        public float detectRadius = 3.2f;
        public float stunExposureThreshold = 0.35f;
        public float stunDuration = 8.0f;

        public bool IsStunned => stunRemaining > 0f;

        private SpriteRenderer sr;
        private Collider2D col;
        private LightBeamEmitter[] cachedEmitters;
        private Vector3 homePosition;
        private float stunRemaining = 0f;
        private float stunExposure = 0f;
        private int lastStunHitFrame = -100;

        void Awake()
        {
            homePosition = transform.position;
            sr = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();
            if (sr != null && frame1 == null) frame1 = sr.sprite;
        }

        void Start()
        {
            cachedEmitters = FindObjectsByType<LightBeamEmitter>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            if (sr != null && frame1 != null && frame2 != null)
            {
                int idx = Mathf.FloorToInt(Time.time * 8f) % 2;
                sr.sprite = idx == 0 ? frame1 : frame2;
            }

            if (stunRemaining > 0f)
            {
                stunRemaining = Mathf.Max(0f, stunRemaining - Time.deltaTime);
                if (col != null) col.enabled = false;
                if (sr != null) sr.color = new Color(1f, 0.88f, 0.45f, 0.45f);
                return;
            }

            if (col != null) col.enabled = true;
            if (sr != null) sr.color = Color.white;

            if ((Time.frameCount - lastStunHitFrame) > 2)
            {
                stunExposure = Mathf.Max(0f, stunExposure - Time.deltaTime);
            }

            Vector3 target = FindNearestBeamTarget(out bool hasBeam);
            if (!hasBeam)
            {
                target = homePosition + new Vector3(Mathf.Sin(Time.time * 1.8f) * 0.4f, Mathf.Cos(Time.time * 2.2f) * 0.25f, 0f);
            }
            transform.position = Vector3.MoveTowards(transform.position, target, flySpeed * Time.deltaTime);
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (stunRemaining > 0f) return;

            if (hit.Color == stunColor)
            {
                lastStunHitFrame = Time.frameCount;
                stunExposure += Time.deltaTime;
                if (stunExposure >= stunExposureThreshold)
                {
                    stunExposure = 0f;
                    stunRemaining = stunDuration;
                    PuzzleAudioManager.Instance?.PlayMonsterPetrify();
                    PuzzleVFXManager.SpawnPixelBurst(transform.position, new Color(1f, 0.4f, 0.35f, 1f), 12, 3.2f);
                }
            }
            // Non-stun beams are absorbed cleanly (do not emit child beam).
        }

        private Vector3 FindNearestBeamTarget(out bool found)
        {
            found = false;
            Vector3 bestPos = homePosition;
            float bestDistSq = detectRadius * detectRadius;
            if (cachedEmitters == null) return bestPos;

            Vector2 pos = transform.position;
            for (int i = 0; i < cachedEmitters.Length; i++)
            {
                LightBeamEmitter emitter = cachedEmitters[i];
                if (emitter == null || !emitter.gameObject.activeInHierarchy) continue;

                LineRenderer[] lines = emitter.GetComponentsInChildren<LineRenderer>(false);
                for (int j = 0; j < lines.Length; j++)
                {
                    LineRenderer lr = lines[j];
                    if (lr == null || !lr.enabled || lr.positionCount < 2) continue;

                    int segCount = lr.positionCount - 1;
                    for (int k = 0; k < segCount; k++)
                    {
                        Vector2 a = lr.GetPosition(k);
                        Vector2 b = lr.GetPosition(k + 1);
                        Vector2 ab = b - a;
                        float lenSq = ab.sqrMagnitude;
                        if (lenSq < 0.0025f) continue;

                        float t = Mathf.Clamp01(Vector2.Dot(pos - a, ab) / lenSq);
                        Vector2 closest = a + ab * t;
                        float dSq = (pos - closest).sqrMagnitude;
                        if (dSq < bestDistSq)
                        {
                            bestDistSq = dSq;
                            bestPos = new Vector3(closest.x, closest.y, 0f);
                            found = true;
                        }
                    }
                }
            }

            return bestPos;
        }
    }
}
