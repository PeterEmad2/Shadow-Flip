using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// One-way optical polarizer / diode glass: lets incoming beams aligned with
    /// allowedDirection pass through (optionally inverting White <-> Shadow)
    /// while blocking beams arriving from any other direction.
    /// </summary>
    public class OneWayPolarizer : MonoBehaviour, IOpticalElement, IInteractable
    {
        [Header("Polarization")]
        public Vector2 allowedDirection = Vector2.right;
        public bool invertRealmColorOnPass = false;
        public bool canRotate = true;
        public float rotationStepDegrees = 90f;

        void Awake()
        {
            ApplyRotationVisual();
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            Vector2 inDir = hit.IncomingDirection.normalized;
            Vector2 passDir = allowedDirection.sqrMagnitude > 0.001f ? allowedDirection.normalized : Vector2.right;

            if (Vector2.Dot(inDir, passDir) > 0.7f)
            {
                LightColor outColor = hit.Color;
                if (invertRealmColorOnPass)
                {
                    if (outColor == LightColor.White) outColor = LightColor.Shadow;
                    else if (outColor == LightColor.Shadow) outColor = LightColor.White;
                }
                emitter.EmitChildBeam(transform.position, inDir, outColor, remainingBounces, hit.Collider);
            }
        }

        public void Interact(PlayerMovement player)
        {
            if (!canRotate) return;
            allowedDirection = RotateVector(allowedDirection, rotationStepDegrees);
            ApplyRotationVisual();
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
            PuzzleVFXManager.SpawnPixelBurst(transform.position, Color.cyan, 6, 2.2f);
        }

        public string GetInteractPrompt()
        {
            if (!canRotate) return string.Empty;
            Vector2 d = allowedDirection.normalized;
            string arrow = Mathf.Abs(d.x) >= Mathf.Abs(d.y) ? (d.x >= 0 ? "→" : "←") : (d.y >= 0 ? "↑" : "↓");
            return invertRealmColorOnPass
                ? $"[E] Rotate Diode ({arrow}) [Invert]"
                : $"[E] Rotate Polarizer ({arrow})";
        }

        private void ApplyRotationVisual()
        {
            Vector2 d = allowedDirection.sqrMagnitude > 0.001f ? allowedDirection.normalized : Vector2.right;
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos).normalized;
        }
    }
}
