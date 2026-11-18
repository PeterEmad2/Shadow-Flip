using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Splits an incoming light beam into two angled beams, or disperses White light into Red, Green, and Blue.
    /// </summary>
    public class OpticalPrism : MonoBehaviour, IOpticalElement, IInteractable
    {
        public enum PrismMode
        {
            Splitter,
            DisperseRGB
        }

        public PrismMode mode = PrismMode.Splitter;
        public float splitAngle = 90f;
        public bool orientAlongTransform = false;
        public bool canRotate = true;
        public float rotationStepDegrees = 90f;

        private float targetAngle;

        void Awake()
        {
            targetAngle = transform.eulerAngles.z;
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            Vector2 centerOrigin = transform.position;
            Vector2 dir = orientAlongTransform
                ? ((Vector2)transform.right).normalized
                : hit.IncomingDirection.normalized;

            if (mode == PrismMode.DisperseRGB && hit.Color == LightColor.White)
            {
                emitter.EmitChildBeam(centerOrigin, Rotate(dir, -splitAngle), LightColor.Red, remainingBounces, hit.Collider);
                emitter.EmitChildBeam(centerOrigin, dir, LightColor.Green, remainingBounces, hit.Collider);
                emitter.EmitChildBeam(centerOrigin, Rotate(dir, splitAngle), LightColor.Blue, remainingBounces, hit.Collider);
            }
            else
            {
                emitter.EmitChildBeam(centerOrigin, Rotate(dir, -splitAngle), hit.Color, remainingBounces, hit.Collider);
                emitter.EmitChildBeam(centerOrigin, Rotate(dir, splitAngle), hit.Color, remainingBounces, hit.Collider);
            }
        }

        public void Interact(PlayerMovement player)
        {
            if (!canRotate) return;
            targetAngle = Mathf.Repeat(targetAngle + rotationStepDegrees, 360f);
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public string GetInteractPrompt()
        {
            return canRotate ? "[E] Rotate Prism" : string.Empty;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
