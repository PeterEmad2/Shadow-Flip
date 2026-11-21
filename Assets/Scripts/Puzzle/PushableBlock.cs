using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Physics-based pushable block that can only be pushed in Light Mode.
    /// When flipped into Umbra (Dark) Mode, its Rigidbody2D freezes as Static so it cannot move.
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
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = linearDamping;
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            SyncBodyType();
        }

        void FixedUpdate()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = SyncBodyType();

            bool moving = !isDark && Time.timeScale > 0f && rb != null && rb.linearVelocity.sqrMagnitude > 0.08f;
            if (moving != wasMoving)
            {
                wasMoving = moving;
                PuzzleAudioManager.Instance?.SetBlockPushing(moving);
            }
        }

        private bool SyncBodyType()
        {
            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;
            if (rb != null)
            {
                RigidbodyType2D target = isDark ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
                if (rb.bodyType != target)
                {
                    if (isDark) rb.linearVelocity = Vector2.zero;
                    rb.bodyType = target;
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
