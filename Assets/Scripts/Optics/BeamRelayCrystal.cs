using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Optical relay crystal: either repeats an incoming beam in the 3 remaining cardinal
    /// directions (CrossRepeater4Way), or converts White <-> Shadow based on the active realm (RealmInverter).
    /// </summary>
    public class BeamRelayCrystal : MonoBehaviour, IOpticalElement, IInteractable
    {
        public enum RelayMode
        {
            CrossRepeater4Way,
            RealmInverter
        }

        public RelayMode mode = RelayMode.CrossRepeater4Way;
        public Vector2 localOutputDirection = Vector2.right;
        public bool canRotate = true;
        public bool allowModeToggle = false;
        public float rotationStepDegrees = 90f;

        private static readonly Vector2[] CardinalDirs =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        public Vector2 GetOutputDirection()
        {
            Vector2 dir = localOutputDirection.sqrMagnitude > 0.001f ? localOutputDirection.normalized : Vector2.right;
            return ((Vector2)transform.TransformDirection(dir)).normalized;
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (mode == RelayMode.CrossRepeater4Way)
            {
                Vector2 backDir = -SnapCardinal(hit.IncomingDirection);
                for (int i = 0; i < CardinalDirs.Length; i++)
                {
                    if (Vector2.Dot(CardinalDirs[i], backDir) > 0.9f) continue;
                    emitter.EmitChildBeam(transform.position, CardinalDirs[i], hit.Color, remainingBounces, hit.Collider);
                }
                return;
            }

            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            LightColor outColor = hit.Color;
            if (isDark && hit.Color == LightColor.White) outColor = LightColor.Shadow;
            else if (!isDark && hit.Color == LightColor.Shadow) outColor = LightColor.White;

            emitter.EmitChildBeam(transform.position, GetOutputDirection(), outColor, remainingBounces, hit.Collider);
        }

        public void Interact(PlayerMovement player)
        {
            if (allowModeToggle)
            {
                mode = mode == RelayMode.CrossRepeater4Way ? RelayMode.RealmInverter : RelayMode.CrossRepeater4Way;
                PuzzleAudioManager.Instance?.PlayMirrorRotate();
                return;
            }

            if (!canRotate) return;
            localOutputDirection = Rotate(localOutputDirection, rotationStepDegrees);
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public string GetInteractPrompt()
        {
            if (allowModeToggle) return "[E] Switch Relay Mode";
            if (!canRotate || mode == RelayMode.CrossRepeater4Way) return string.Empty;

            Vector2 d = GetOutputDirection();
            string arrow = Mathf.Abs(d.x) >= Mathf.Abs(d.y) ? (d.x >= 0 ? "→" : "←") : (d.y >= 0 ? "↑" : "↓");
            return $"[E] Rotate Inverter ({arrow})";
        }

        private static Vector2 SnapCardinal(Vector2 v)
        {
            if (Mathf.Abs(v.x) >= Mathf.Abs(v.y)) return v.x >= 0f ? Vector2.right : Vector2.left;
            return v.y >= 0f ? Vector2.up : Vector2.down;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
        }
    }
}
