using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Reflects incoming light beams cleanly. Supports either transform-rotated mirrors
    /// or 4-state isometric sprite mirrors (45 / 135 / 225 / 315 degrees).
    /// </summary>
    public class OpticalMirror : MonoBehaviour, IOpticalElement, IInteractable
    {
        [Header("Reflection")]
        public bool isTwoSided = false;
        public Vector2 localReflectNormal = Vector2.up;

        [Header("4-State Isometric Visuals (45, 135, 225, 315)")]
        public Sprite[] directionalSprites;
        public float logicalAngle = 45f;

        [Header("Rotation Interaction")]
        public bool canRotate = true;
        public float rotationStepDegrees = 90f;
        public bool requireLightWorldToRotate = false;
        public bool requireShadowWorldToRotate = false;

        private SpriteRenderer spriteRenderer;
        private float targetAngle;

        private static readonly Vector2[] IsometricNormals =
        {
            new Vector2(-1f, 1f).normalized,   // 45:  (+1,0) <-> (0,+1)
            new Vector2(-1f, -1f).normalized,  // 135: (+1,0) <-> (0,-1)
            new Vector2(1f, -1f).normalized,   // 225: (-1,0) <-> (0,-1)
            new Vector2(1f, 1f).normalized     // 315: (-1,0) <-> (0,+1)
        };

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (HasIsometricSprites())
            {
                transform.rotation = Quaternion.identity;
                ApplyIsometricVisual();
            }
            else
            {
                targetAngle = transform.eulerAngles.z;
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            Vector2 normal = GetActiveNormal();
            if (isTwoSided && Vector2.Dot(hit.IncomingDirection, normal) > 0f)
            {
                normal = -normal;
            }

            if (Vector2.Dot(hit.IncomingDirection, normal) >= -0.05f)
            {
                return;
            }

            Vector2 reflected = Vector2.Reflect(hit.IncomingDirection, normal).normalized;
            emitter.EmitChildBeam(transform.position, reflected, hit.Color, remainingBounces, hit.Collider);
        }

        public void Interact(PlayerMovement player)
        {
            if (!CanPlayerRotate()) return;
            RotateStep();
            PuzzleAudioManager.Instance?.PlayMirrorRotate();
        }

        public void RotateStep()
        {
            if (HasIsometricSprites())
            {
                logicalAngle = Mathf.Repeat(logicalAngle + rotationStepDegrees, 360f);
                transform.rotation = Quaternion.identity;
                ApplyIsometricVisual();
            }
            else
            {
                targetAngle = Mathf.Repeat(targetAngle + rotationStepDegrees, 360f);
                transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            }
        }

        public string GetInteractPrompt()
        {
            if (!CanPlayerRotate())
            {
                if (canRotate && requireLightWorldToRotate) return "Rotate in Light Realm only";
                if (canRotate && requireShadowWorldToRotate) return "Rotate in Shadow Realm only";
                return string.Empty;
            }
            return "[E] Rotate Mirror";
        }

        public void ApplyIsometricVisual()
        {
            if (!HasIsometricSprites()) return;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
            spriteRenderer.sprite = directionalSprites[GetIsometricStateIndex()];
        }

        private bool HasIsometricSprites()
        {
            return directionalSprites != null && directionalSprites.Length == 4;
        }

        private int GetIsometricStateIndex()
        {
            float wrapped = Mathf.Repeat(logicalAngle, 360f);
            int idx = Mathf.RoundToInt((wrapped - 45f) / 90f);
            return ((idx % 4) + 4) % 4;
        }

        private Vector2 GetActiveNormal()
        {
            if (HasIsometricSprites())
            {
                return IsometricNormals[GetIsometricStateIndex()];
            }
            return transform.TransformDirection(localReflectNormal.normalized);
        }

        private bool CanPlayerRotate()
        {
            if (!canRotate) return false;
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            if (requireLightWorldToRotate && isDark) return false;
            if (requireShadowWorldToRotate && !isDark) return false;
            return true;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 normal = GetActiveNormal();
            Gizmos.DrawLine(transform.position, transform.position + normal * 0.9f);
        }
    }
}
