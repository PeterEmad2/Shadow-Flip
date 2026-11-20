using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Umbra Mimic Mirror: behaves as an ordinary rotatable mirror in the Light Realm,
    /// but awakens in the Shadow Realm and creeps away toward fleeTargetOffset
    /// unless pinned in place by an active laser beam.
    /// </summary>
    public class UmbraMimicMirror : MonoBehaviour
    {
        public Sprite awakeSprite;
        public float creepSpeed = 1.6f;
        public Vector3 fleeTargetOffset = new Vector3(1.8f, 0f, 0f);
        public float beamPinRadius = 0.65f;

        private OpticalMirror mirror;
        private SpriteRenderer sr;
        private logic cachedLogic;
        private LightBeamEmitter[] cachedEmitters;
        private Vector3 homePos;

        void Awake()
        {
            homePos = transform.position;
            mirror = GetComponent<OpticalMirror>();
            sr = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            cachedEmitters = FindObjectsByType<LightBeamEmitter>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;
            if (!isDark)
            {
                if (mirror != null) mirror.ApplyIsometricVisual();
                if (sr != null) sr.color = Color.white;
                return;
            }

            if (sr != null && awakeSprite != null)
            {
                sr.sprite = awakeSprite;
            }

            bool pinned = IsPinnedByBeam(transform.position);
            if (pinned)
            {
                if (sr != null) sr.color = new Color(0.75f, 0.95f, 1f, 1f);
                return;
            }

            if (sr != null) sr.color = Color.white;
            Vector3 fleeTarget = homePos + fleeTargetOffset;
            transform.position = Vector3.MoveTowards(transform.position, fleeTarget, creepSpeed * Time.deltaTime);
        }

        private bool IsPinnedByBeam(Vector2 point)
        {
            if (cachedEmitters == null) return false;
            float rSq = beamPinRadius * beamPinRadius;
            for (int i = 0; i < cachedEmitters.Length; i++)
            {
                LightBeamEmitter emitter = cachedEmitters[i];
                if (emitter == null || !emitter.gameObject.activeInHierarchy) continue;

                LineRenderer[] lines = emitter.GetComponentsInChildren<LineRenderer>(false);
                for (int j = 0; j < lines.Length; j++)
                {
                    LineRenderer lr = lines[j];
                    if (lr == null || !lr.enabled || lr.positionCount < 2) continue;

                    Vector2 a = lr.GetPosition(0);
                    Vector2 b = lr.GetPosition(1);
                    if (DistSq(point, a, b) <= rSq) return true;
                }
            }
            return false;
        }

        private static float DistSq(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return (p - a).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            Vector2 proj = a + ab * t;
            return (p - proj).sqrMagnitude;
        }
    }
}
