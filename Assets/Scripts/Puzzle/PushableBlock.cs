using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Physics-based pushable block that is pushable only in its designated realm (Light Mode by default,
    /// or Umbra Mode when pushableInWorld == ShadowOnly), freezing constraints in the opposite realm.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PushableBlock : MonoBehaviour
    {
        public WorldAffinity pushableInWorld = WorldAffinity.LightOnly;
        public float linearDamping = 12f;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private bool wasMoving = false;
        private logic cachedLogic;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = linearDamping;
            ApplyRealmVisualTint();
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            ApplyRealmVisualTint();
            SyncPhysicsLock();
        }

        void FixedUpdate()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool canPush = SyncPhysicsLock();

            bool moving = canPush && Time.timeScale > 0f && rb != null && rb.linearVelocity.sqrMagnitude > 0.08f;
            if (moving != wasMoving)
            {
                wasMoving = moving;
                PuzzleAudioManager.Instance?.SetBlockPushing(moving);
            }
        }

        private void ApplyRealmVisualTint()
        {
            if (sr != null && pushableInWorld == WorldAffinity.ShadowOnly)
            {
                sr.color = new Color(0.76f, 0.56f, 1f, 1f);
            }
        }

        private bool SyncPhysicsLock()
        {
            bool isDark = cachedLogic != null && cachedLogic.isDarkActive;
            bool canPush = pushableInWorld == WorldAffinity.Both
                || (pushableInWorld == WorldAffinity.LightOnly && !isDark)
                || (pushableInWorld == WorldAffinity.ShadowOnly && isDark);

            if (rb != null)
            {
                // Always stay Dynamic so Box2D never tears down active PressurePlate trigger overlaps on flip.
                if (rb.bodyType != RigidbodyType2D.Dynamic)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                }

                RigidbodyConstraints2D targetConstraints = canPush
                    ? RigidbodyConstraints2D.FreezeRotation
                    : RigidbodyConstraints2D.FreezeAll;

                if (!canPush)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                if (rb.constraints != targetConstraints)
                {
                    rb.constraints = targetConstraints;
                }
            }
            return canPush;
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
