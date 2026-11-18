using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Physics-based pushable block that can block light beams, hold down floor plates, and play sliding audio.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PushableBlock : MonoBehaviour
    {
        public float linearDamping = 12f;

        private Rigidbody2D rb;
        private bool wasMoving = false;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearDamping = linearDamping;
        }

        void FixedUpdate()
        {
            bool moving = Time.timeScale > 0f && rb != null && rb.linearVelocity.sqrMagnitude > 0.08f;
            if (moving != wasMoving)
            {
                wasMoving = moving;
                PuzzleAudioManager.Instance?.SetBlockPushing(moving);
            }
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
