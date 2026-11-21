using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Physics-based pushable block that can only be pushed in Light Mode.
    /// When flipped into Umbra (Dark) Mode, its constraints freeze so it cannot move while keeping trigger contacts active.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PushableBlock : MonoBehaviour
    {
        public float linearDamping = 12f;

        private Rigidbody2D rb;
        private bool wasMoving = false;
        private logic cachedLogic;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = linearDamping;
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            SyncPhysicsLock();
        }

        void FixedUpdate()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = SyncPhysicsLock();

            bool moving = !isDark && Time.timeScale > 0f && rb != null && rb.linearVelocity.sqrMagnitude > 0.08f;
            if (moving != wasMoving)
            {
                wasMoving = moving;
                PuzzleAudioManager.Instance?.SetBlockPushing(moving);
            }
        }

        private bool SyncPhysicsLock()
        {
            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;
            if (rb != null)
            {
                // Always stay Dynamic so Box2D never tears down active PressurePlate trigger overlaps on flip.
                if (rb.bodyType != RigidbodyType2D.Dynamic)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }

                RigidbodyConstraints2D targetConstraints = isDark
                    ? RigidbodyConstraints2D.FreezeAll
                    : RigidbodyConstraints2D.FreezeRotation;

                if (isDark)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                if (rb.constraints != targetConstraints)
                {
                    rb.constraints = targetConstraints;
                }
            }
            return isDark;
        }

        void OnDisable()
        {
            if (wasMoving)
            {
                wasMoving = false;
                PuzzleAudioManager.Instance?.SetBlockPushing(false);
            }
        }
    }
}
