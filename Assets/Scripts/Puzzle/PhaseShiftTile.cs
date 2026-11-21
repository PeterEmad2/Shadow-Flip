using System.Collections.Generic;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Floor pad that teleports the player (and optionally PushableBlocks) to a linked
    /// PhaseShiftTile with per-object loop cooldown, and/or flips realms on step.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PhaseShiftTile : MonoBehaviour
    {
        public PhaseShiftTile linkedTeleportPad;
        public bool flipRealmOnStep = false;
        public bool allowPushableBlocks = false;
        public float cooldown = 0.7f;

        private readonly Dictionary<int, float> objectCooldowns = new Dictionary<int, float>();

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        public void RegisterArrival(int instanceId)
        {
            objectCooldowns[instanceId] = Time.unscaledTime + cooldown;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Time.timeScale == 0f || other == null) return;

            Light lightComp = other.GetComponent<Light>();
            bool isRealPlayer = (lightComp != null && lightComp.Jimmy != null) || other.GetComponent<Dark>() != null;
            bool isPushableBlock = allowPushableBlocks && other.GetComponent<PushableBlock>() != null;

            if (!isRealPlayer && !isPushableBlock) return;

            int id = other.gameObject.GetInstanceID();
            if (objectCooldowns.TryGetValue(id, out float nextAllowed) && Time.unscaledTime < nextAllowed)
            {
                return;
            }

            objectCooldowns[id] = Time.unscaledTime + cooldown;

            if (linkedTeleportPad != null)
            {
                linkedTeleportPad.RegisterArrival(id);
                Vector3 dest = linkedTeleportPad.transform.position;
                if (isRealPlayer)
                {
                    Collider2D overlap = Physics2D.OverlapCircle(dest, 0.45f);
                    if (overlap != null && overlap.GetComponent<PushableBlock>() != null)
                    {
                        dest += new Vector3(0f, 0.85f, 0f);
                    }
                }
                PuzzleVFXManager.SpawnPixelBurst(other.transform.position, new Color(0.45f, 0.92f, 1f, 1f), 8, 2.8f);
                other.transform.position = dest;
                Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
                Physics2D.SyncTransforms();
                PuzzleVFXManager.SpawnPixelBurst(dest, new Color(0.72f, 0.48f, 1f, 1f), 10, 3.2f);
                PuzzleAudioManager.Instance?.PlayTeleportPad();
            }

            if (isRealPlayer && flipRealmOnStep)
            {
                FindFirstObjectByType<GameObjectToggle>()?.TryFlip();
            }
        }
    }
}
