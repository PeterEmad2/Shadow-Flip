using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Additive color combiner crystal: when hit simultaneously by requiredColorA and requiredColorB,
    /// emits a combined outputColor beam along localOutputDirection.
    /// </summary>
    public class ColorCombiner : MonoBehaviour, IOpticalElement, IInteractable
    {
        [Header("Color Combination")]
        public LightColor requiredColorA = LightColor.Red;
        public LightColor requiredColorB = LightColor.Blue;
        public LightColor outputColor = LightColor.White;

        [Header("Output Direction")]
        public Vector2 localOutputDirection = Vector2.right;
        public bool canRotate = true;
        public float rotationStepDegrees = 90f;

        private int lastHitFrameA = -100;
        private int lastHitFrameB = -100;
        private int lastEmittedFrame = -100;

        public Vector2 GetOutputDirection()
        {
            Vector2 dir = localOutputDirection.sqrMagnitude > 0.001f ? localOutputDirection.normalized : Vector2.right;
            return ((Vector2)transform.TransformDirection(dir)).normalized;
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (hit.Color == requiredColorA) lastHitFrameA = Time.frameCount;
            else if (hit.Color == requiredColorB) lastHitFrameB = Time.frameCount;

            bool hasA = (Time.frameCount - lastHitFrameA) <= 1;
            bool hasB = (Time.frameCount - lastHitFrameB) <= 1;

            if (hasA && hasB && lastEmittedFrame != Time.frameCount)
            {
                lastEmittedFrame = Time.frameCount;
                emitter.EmitChildBeam(transform.position, GetOutputDirection(), outputColor, remainingBounces, hit.Collider);
            }
        }

        public void Interact(PlayerMovement player)
        {
            if (!canRotate) return;
            transform.Rotate(0f, 0f, rotationStepDegrees);
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public string GetInteractPrompt()
        {
            return canRotate ? "[E] Rotate Combiner" : string.Empty;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = LightColorUtility.ToUnityColor(outputColor);
            Vector3 dir = GetOutputDirection();
            Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
        }
    }
}
