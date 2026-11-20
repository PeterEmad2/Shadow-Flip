using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Beam-driven magnetic rail terminal: glides riderTransform along [pointA -> pointB]
    /// while powered by a matching optical beam (e.g., Blue crystal beam).
    /// </summary>
    public class BeamMagneticRail : MonoBehaviour, IOpticalElement
    {
        [Header("Rail Path")]
        public Vector3 pointA;
        public Vector3 pointB;
        public Transform riderTransform;
        public float moveSpeed = 2.6f;
        public bool pingPong = false;

        [Header("Power Requirement")]
        public LightColor requiredColor = LightColor.Blue;
        public bool acceptAnyColor = false;

        public bool IsPowered { get; private set; }

        private int lastPoweredFrame = -100;
        private bool headingToB = true;
        private SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (riderTransform != null)
            {
                riderTransform.position = pointA;
            }
        }

        void Update()
        {
            IsPowered = (Time.frameCount - lastPoweredFrame) <= 1;

            if (sr != null)
            {
                sr.color = IsPowered
                    ? Color.Lerp(Color.white, LightColorUtility.ToUnityColor(requiredColor), 0.35f)
                    : new Color(0.75f, 0.78f, 0.86f, 1f);
            }

            if (!IsPowered || riderTransform == null) return;

            Vector3 target = headingToB ? pointB : pointA;
            riderTransform.position = Vector3.MoveTowards(riderTransform.position, target, moveSpeed * Time.deltaTime);

            if ((riderTransform.position - target).sqrMagnitude <= 0.0004f)
            {
                riderTransform.position = target;
                if (pingPong)
                {
                    headingToB = !headingToB;
                }
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (LightColorUtility.Matches(hit.Color, requiredColor, acceptAnyColor))
            {
                if (!IsPowered)
                {
                    PuzzleVFXManager.SpawnPixelBurst(transform.position, LightColorUtility.ToUnityColor(requiredColor), 6, 2.5f);
                }
                lastPoweredFrame = Time.frameCount;
            }
        }
    }
}
