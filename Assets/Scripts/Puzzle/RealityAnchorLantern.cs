using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Reality-Anchor Lantern: when charged by any beam, projects a Light-Realm bubble
    /// inside the Shadow Realm so LightOnly buttons, sensors, and emitters can operate
    /// locally while shielding Jimmy from Umbra decay.
    /// </summary>
    public class RealityAnchorLantern : MonoBehaviour, IOpticalElement
    {
        public float anchorRadius = 3.6f;
        public float holdDuration = 5.0f;

        [Header("Sprites")]
        public Sprite offSprite;
        public Sprite onSprite;

        public bool IsActive => remaining > 0f;
        public float RemainingSeconds => remaining;

        private static readonly List<RealityAnchorLantern> ActiveAnchors = new List<RealityAnchorLantern>();

        private SpriteRenderer sr;
        private LineRenderer ringRenderer;
        private float remaining = 0f;
        private int lastHitFrame = -100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ActiveAnchors.Clear();
        }

        public static bool IsPositionAnchoredInLight(Vector2 pos)
        {
            for (int i = 0; i < ActiveAnchors.Count; i++)
            {
                RealityAnchorLantern a = ActiveAnchors[i];
                if (a != null && a.gameObject.activeInHierarchy && a.IsActive)
                {
                    if (((Vector2)a.transform.position - pos).sqrMagnitude <= a.anchorRadius * a.anchorRadius)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null && offSprite == null) offSprite = sr.sprite;
            BuildRing();
        }

        void OnEnable()
        {
            if (!ActiveAnchors.Contains(this)) ActiveAnchors.Add(this);
        }

        void OnDisable()
        {
            ActiveAnchors.Remove(this);
        }

        void Update()
        {
            bool directlyLit = (Time.frameCount - lastHitFrame) <= 1;
            if (!directlyLit && remaining > 0f)
            {
                remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            }

            bool active = IsActive;
            if (sr != null)
            {
                if (active && onSprite != null) sr.sprite = onSprite;
                else if (!active && offSprite != null) sr.sprite = offSprite;
            }
            if (ringRenderer != null)
            {
                ringRenderer.enabled = active;
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            bool wasActive = IsActive;
            lastHitFrame = Time.frameCount;
            remaining = holdDuration;

            if (!wasActive)
            {
                PuzzleAudioManager.Instance?.PlaySensorState(true);
                PuzzleVFXManager.SpawnPixelBurst(transform.position, new Color(0.45f, 0.95f, 1f, 1f), 10, 3.2f);
            }

            // Let beams shine cleanly through the crystalline lantern lens to downstream crystals.
            Vector2 dir = hit.IncomingDirection.normalized;
            int keepBounces = Mathf.Min(emitter.maxBounces, remainingBounces + 1);
            emitter.EmitChildBeam(hit.Point + dir * 0.02f, dir, hit.Color, keepBounces, hit.Collider);
        }

        private void BuildRing()
        {
            GameObject ringGo = new GameObject("RealityAnchorRing");
            ringGo.transform.SetParent(transform, false);
            ringRenderer = ringGo.AddComponent<LineRenderer>();
            ringRenderer.useWorldSpace = false;
            ringRenderer.loop = true;
            ringRenderer.numCornerVertices = 0;
            ringRenderer.numCapVertices = 0;
            ringRenderer.startWidth = 0.09f;
            ringRenderer.endWidth = 0.09f;
            ringRenderer.sortingOrder = 0;
            Color c = new Color(0.42f, 0.95f, 1f, 0.85f);
            ringRenderer.startColor = c;
            ringRenderer.endColor = c;

            if (SharedRenderResources.SpriteDefaultMaterial != null)
            {
                ringRenderer.sharedMaterial = SharedRenderResources.SpriteDefaultMaterial;
            }

            const int segments = 16;
            ringRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float px = Mathf.Round(Mathf.Cos(angle) * anchorRadius * 16f) / 16f;
                float py = Mathf.Round(Mathf.Sin(angle) * anchorRadius * 16f) / 16f;
                ringRenderer.SetPosition(i, new Vector3(px, py, 0f));
            }
            ringRenderer.enabled = false;
        }
    }
}
