using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Optical Memory Prism (Delay Line): captures an incoming beam's color and direction
    /// and continues emitting a stored child beam for holdSeconds after the input stops or realm flips.
    /// </summary>
    public class OpticalMemoryPrism : MonoBehaviour, IOpticalElement, IInteractable
    {
        [Header("Memory Delay")]
        public float holdSeconds = 7.0f;
        public bool useFixedOutputDir = true;
        public Vector2 localOutputDirection = Vector2.right;
        public bool canRotate = false;
        public float rotationStepDegrees = 90f;

        [Header("Visuals")]
        public Sprite idleSprite;
        public Sprite chargedSprite;

        public float RemainingHold => remainingHold;
        public bool IsHoldingBeam => remainingHold > 0f;

        private SpriteRenderer sr;
        private LightBeamEmitter memoryEmitter;
        private LightColor storedColor = LightColor.White;
        private Vector2 storedDir = Vector2.right;
        private float remainingHold = 0f;
        private int lastHitFrame = -100;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null && idleSprite == null) idleSprite = sr.sprite;
            EnsureMemoryEmitter();
        }

        void Update()
        {
            bool receivingDirect = (Time.frameCount - lastHitFrame) <= 1;

            if (!receivingDirect && remainingHold > 0f)
            {
                remainingHold = Mathf.Max(0f, remainingHold - Time.deltaTime);
            }

            bool emitStored = !receivingDirect && remainingHold > 0f;
            if (memoryEmitter != null)
            {
                memoryEmitter.enabled = emitStored;
                if (emitStored)
                {
                    memoryEmitter.beamColor = storedColor;
                    memoryEmitter.localEmitDirection = storedDir;
                }
            }

            if (sr != null)
            {
                bool active = receivingDirect || emitStored;
                if (active && chargedSprite != null) sr.sprite = chargedSprite;
                else if (!active && idleSprite != null) sr.sprite = idleSprite;

                if (active)
                {
                    Color c = LightColorUtility.ToUnityColor(storedColor);
                    sr.color = Color.Lerp(Color.white, c, 0.3f);
                }
                else
                {
                    sr.color = Color.white;
                }
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            Vector2 outDir = useFixedOutputDir
                ? ((Vector2)transform.TransformDirection(localOutputDirection.normalized)).normalized
                : hit.IncomingDirection.normalized;
            if (outDir.sqrMagnitude < 0.001f) outDir = Vector2.right;

            storedColor = hit.Color;
            storedDir = outDir;
            remainingHold = holdSeconds;
            lastHitFrame = Time.frameCount;

            // Forward live beam while directly lit so delay emitter doesn't double-draw.
            emitter.EmitChildBeam(transform.position, storedDir, storedColor, remainingBounces, hit.Collider);
        }

        public void Interact(PlayerMovement player)
        {
            if (!canRotate) return;
            float rad = rotationStepDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            localOutputDirection = new Vector2(
                localOutputDirection.x * cos - localOutputDirection.y * sin,
                localOutputDirection.x * sin + localOutputDirection.y * cos).normalized;
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public string GetInteractPrompt()
        {
            return canRotate ? "[E] Rotate Memory Output" : string.Empty;
        }

        private void EnsureMemoryEmitter()
        {
            if (memoryEmitter != null) return;
            GameObject child = new GameObject("MemoryBeamEmitter");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            memoryEmitter = child.AddComponent<LightBeamEmitter>();
            memoryEmitter.activeInWorld = WorldAffinity.Both;
            memoryEmitter.beamWidth = 0.22f;
            memoryEmitter.maxBounces = 10;
            memoryEmitter.selfIgnoreCollider = GetComponent<Collider2D>();
            memoryEmitter.enabled = false;
        }
    }
}
