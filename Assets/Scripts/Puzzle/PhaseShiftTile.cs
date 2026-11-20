using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Floor pad that teleports the player to a linked PhaseShiftTile and/or flips realms on step.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PhaseShiftTile : MonoBehaviour
    {
        public PhaseShiftTile linkedTeleportPad;
        public bool flipRealmOnStep = false;
        public float cooldown = 0.6f;

        private float lastTriggerTime = -10f;

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Time.timeScale == 0f) return;
            if (Time.unscaledTime - lastTriggerTime < cooldown) return;
            Light lightComp = other.GetComponent<Light>();
            bool isRealPlayer = (lightComp != null && lightComp.Jimmy != null) || other.GetComponent<Dark>() != null;
            if (!isRealPlayer) return;

            lastTriggerTime = Time.unscaledTime;

            if (linkedTeleportPad != null)
            {
                linkedTeleportPad.lastTriggerTime = Time.unscaledTime;
                other.transform.position = linkedTeleportPad.transform.position;
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                PuzzleAudioManager.Instance?.PlayTeleportPad();
            }

            if (flipRealmOnStep)
            {
                FindFirstObjectByType<GameObjectToggle>()?.TryFlip();
            }
        }
    }
}
