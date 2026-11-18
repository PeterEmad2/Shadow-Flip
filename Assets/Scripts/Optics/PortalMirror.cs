using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Teleports an incoming light beam to a linked PortalMirror and emits it along localExitDirection.
    /// </summary>
    public class PortalMirror : MonoBehaviour, IOpticalElement, IInteractable
    {
        public PortalMirror linkedPortal;
        public Vector2 localExitDirection = Vector2.right;
        public bool canRotateExit = true;
        public float rotationStepDegrees = 90f;

        private int lastHitFrame = -100;

        public Vector2 GetExitDirection()
        {
            Vector2 dir = localExitDirection.sqrMagnitude > 0.001f ? localExitDirection.normalized : Vector2.right;
            return ((Vector2)transform.TransformDirection(dir)).normalized;
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (linkedPortal == null || !linkedPortal.gameObject.activeInHierarchy) return;

            if (Time.frameCount - lastHitFrame > 2)
            {
                PuzzleAudioManager.Instance?.PlayPortalWarp();
            }
            lastHitFrame = Time.frameCount;

            Collider2D linkedCol = linkedPortal.GetComponent<Collider2D>();
            emitter.EmitChildBeam(linkedPortal.transform.position, linkedPortal.GetExitDirection(), hit.Color, remainingBounces, linkedCol);
        }

        public void Interact(PlayerMovement player)
        {
            if (!canRotateExit) return;
            localExitDirection = Rotate(localExitDirection, rotationStepDegrees);
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public string GetInteractPrompt()
        {
            if (!canRotateExit) return string.Empty;
            Vector2 d = GetExitDirection();
            string arrow = Mathf.Abs(d.x) >= Mathf.Abs(d.y) ? (d.x >= 0 ? "→" : "←") : (d.y >= 0 ? "↑" : "↓");
            return $"[E] Rotate Portal Exit ({arrow})";
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Vector3 dir = GetExitDirection();
            Gizmos.DrawLine(transform.position, transform.position + dir * 1.4f);
            if (linkedPortal != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, linkedPortal.transform.position);
            }
        }
    }
}
