using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Emits a 2D cel-shaded dual-layer optical beam that reflects off mirrors,
    /// splits through prisms, passes through color filters, and powers LightSensors.
    /// </summary>
    public class LightBeamEmitter : MonoBehaviour
    {
        [Header("Beam Properties")]
        public LightColor beamColor = LightColor.White;
        public WorldAffinity activeInWorld = WorldAffinity.Both;
        public float maxDistance = 30f;
        public int maxBounces = 12;
        public float beamWidth = 0.22f;
        public LayerMask hitMask = ~0;
        public Vector2 localEmitDirection = Vector2.right;

        [Header("Cel-Shaded Pedestal Visuals")]
        public SpriteRenderer emitterRenderer;
        public Sprite activeEmitterSprite;
        public Sprite inactiveEmitterSprite;

        [Header("Rendering")]
        public Material beamMaterial;
        public int sortingOrder = 5;

        private struct BeamLinePair
        {
            public LineRenderer OuterGlow;
            public LineRenderer InnerCore;
        }

        private readonly List<BeamLinePair> linePool = new List<BeamLinePair>();
        private int activeLineCount = 0;
        private Material fallbackMaterial;

        void Awake()
        {
            if (emitterRenderer == null) emitterRenderer = GetComponent<SpriteRenderer>();
            if (beamMaterial == null)
            {
                Shader spriteShader = Shader.Find("Sprites/Default");
                if (spriteShader != null)
                {
                    fallbackMaterial = new Material(spriteShader);
                }
            }
        }

        void OnDisable()
        {
            HideUnusedLines(0);
        }

        void LateUpdate()
        {
            activeLineCount = 0;
            bool active = IsEmitterActive();
            UpdateEmitterVisual(active);

            if (active)
            {
                Vector2 dir = transform.TransformDirection(localEmitDirection.normalized);
                CastSegment(transform.position, dir, beamColor, maxBounces);
            }

            HideUnusedLines(activeLineCount);
        }

        public void EmitChildBeam(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider = null)
        {
            if (remainingBounces <= 0 || direction.sqrMagnitude < 0.0001f) return;
            CastSegment(origin, direction.normalized, color, remainingBounces, ignoreCollider);
        }

        internal void RegisterSensorHit(LightSensor sensor, LightColor color)
        {
            if (sensor == null) return;
            sensor.NotifyBeamHit(color);
        }

        private void UpdateEmitterVisual(bool active)
        {
            if (emitterRenderer == null) return;
            if (active && activeEmitterSprite != null)
            {
                emitterRenderer.sprite = activeEmitterSprite;
            }
            else if (!active && inactiveEmitterSprite != null)
            {
                emitterRenderer.sprite = inactiveEmitterSprite;
            }
        }

        private bool IsEmitterActive()
        {
            if (activeInWorld == WorldAffinity.Both) return true;
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            return (activeInWorld == WorldAffinity.ShadowOnly && isDark)
                || (activeInWorld == WorldAffinity.LightOnly && !isDark);
        }

        private void CastSegment(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider = null)
        {
            if (remainingBounces < 0) return;

            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance, hitMask);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            RaycastHit2D chosenHit = default;
            IOpticalElement chosenOptical = null;
            bool foundHit = false;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D h = hits[i];
                if (h.collider == null || h.collider == ignoreCollider) continue;

                // Player bodies do not block light beams.
                if (h.collider.GetComponent<Light>() != null || h.collider.GetComponent<Dark>() != null) continue;

                IOpticalElement optical = h.collider.GetComponent<IOpticalElement>();
                if (optical == null) optical = h.collider.GetComponentInParent<IOpticalElement>();

                // Non-optical trigger colliders (e.g., PressurePlate, LevelExit) do not block beams.
                if (h.collider.isTrigger && optical == null) continue;

                chosenHit = h;
                chosenOptical = optical;
                foundHit = true;
                break;
            }

            if (!foundHit)
            {
                DrawSegment(origin, origin + direction * maxDistance, color);
                return;
            }

            Vector2 endPoint = chosenOptical != null
                ? (Vector2)chosenHit.collider.transform.position
                : chosenHit.point;

            DrawSegment(origin, endPoint, color);

            if (chosenOptical == null || remainingBounces <= 0) return;

            LightBeamHit beamHit = new LightBeamHit
            {
                Point = endPoint,
                Normal = chosenHit.normal,
                IncomingDirection = direction,
                Color = color,
                Collider = chosenHit.collider
            };
            chosenOptical.OnBeamHit(beamHit, this, remainingBounces - 1);
        }

        private void DrawSegment(Vector2 start, Vector2 end, LightColor color)
        {
            BeamLinePair pair = GetOrCreateLinePair(activeLineCount++);
            Color outerColor = LightColorUtility.ToUnityColor(color);
            Vector3 p0 = new Vector3(start.x, start.y, 0f);
            Vector3 p1 = new Vector3(end.x, end.y, 0f);

            pair.OuterGlow.enabled = true;
            pair.OuterGlow.positionCount = 2;
            pair.OuterGlow.SetPosition(0, p0);
            pair.OuterGlow.SetPosition(1, p1);
            pair.OuterGlow.startWidth = beamWidth;
            pair.OuterGlow.endWidth = beamWidth;
            pair.OuterGlow.startColor = outerColor;
            pair.OuterGlow.endColor = outerColor;

            float coreWidth = beamWidth * 0.41f;
            pair.InnerCore.enabled = true;
            pair.InnerCore.positionCount = 2;
            pair.InnerCore.SetPosition(0, p0);
            pair.InnerCore.SetPosition(1, p1);
            pair.InnerCore.startWidth = coreWidth;
            pair.InnerCore.endWidth = coreWidth;
            pair.InnerCore.startColor = Color.white;
            pair.InnerCore.endColor = Color.white;
        }

        private BeamLinePair GetOrCreateLinePair(int index)
        {
            while (linePool.Count <= index)
            {
                Material mat = beamMaterial != null ? beamMaterial : fallbackMaterial;

                GameObject outerGo = new GameObject($"BeamOuter_{linePool.Count}");
                outerGo.transform.SetParent(transform, false);
                LineRenderer outer = outerGo.AddComponent<LineRenderer>();
                outer.useWorldSpace = true;
                outer.numCapVertices = 4;
                outer.sortingOrder = sortingOrder;
                outer.material = mat;

                GameObject coreGo = new GameObject($"BeamCore_{linePool.Count}");
                coreGo.transform.SetParent(transform, false);
                LineRenderer core = coreGo.AddComponent<LineRenderer>();
                core.useWorldSpace = true;
                core.numCapVertices = 4;
                core.sortingOrder = sortingOrder + 1;
                core.material = mat;

                linePool.Add(new BeamLinePair { OuterGlow = outer, InnerCore = core });
            }
            return linePool[index];
        }

        private void HideUnusedLines(int usedCount)
        {
            for (int i = usedCount; i < linePool.Count; i++)
            {
                if (linePool[i].OuterGlow != null) linePool[i].OuterGlow.enabled = false;
                if (linePool[i].InnerCore != null) linePool[i].InnerCore.enabled = false;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = LightColorUtility.ToUnityColor(beamColor);
            Vector3 dir = transform.TransformDirection(localEmitDirection.normalized);
            Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
            Gizmos.DrawSphere(transform.position, 0.12f);
        }
    }
}
