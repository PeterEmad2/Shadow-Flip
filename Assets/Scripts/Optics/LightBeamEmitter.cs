using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ShadowFlip.Puzzle;

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
        public Collider2D selfIgnoreCollider;

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

        private struct CachedColliderInfo
        {
            public Collider2D Collider;
            public bool IgnoreAlways;
            public IOpticalElement Optical;
        }

        private readonly List<BeamLinePair> linePool = new List<BeamLinePair>();
        private readonly List<Vector3> curvedPathBuffer = new List<Vector3>(128);
        private int activeLineCount = 0;
        private logic cachedLogic;

        // Shared, growable hit buffer & per-collider component cache to eliminate per-ray GC and GetComponent churn.
        private static RaycastHit2D[] HitBuffer = new RaycastHit2D[32];
        private static readonly Dictionary<int, CachedColliderInfo> ColliderCache = new Dictionary<int, CachedColliderInfo>(128);
        private static bool sceneHookRegistered = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            HitBuffer = new RaycastHit2D[32];
            ColliderCache.Clear();
            if (!sceneHookRegistered)
            {
                sceneHookRegistered = true;
                SceneManager.sceneUnloaded += _ => ColliderCache.Clear();
            }
        }

        void Awake()
        {
            if (emitterRenderer == null) emitterRenderer = GetComponent<SpriteRenderer>();
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
                CastBeam(transform.position, dir, beamColor, maxBounces, selfIgnoreCollider);
            }

            HideUnusedLines(activeLineCount);
        }

        public void EmitChildBeam(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider = null)
        {
            if (remainingBounces <= 0 || direction.sqrMagnitude < 0.0001f) return;
            CastBeam(origin, direction.normalized, color, remainingBounces, ignoreCollider);
        }

        private void CastBeam(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider)
        {
            if (BlackHoleLens.HasAnyActiveBlackHoles())
            {
                CastCurvedSegment(origin, direction, color, remainingBounces, ignoreCollider);
            }
            else
            {
                CastSegment(origin, direction, color, remainingBounces, ignoreCollider);
            }
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
            logic l = logic.Instance != null ? logic.Instance : cachedLogic;
            if (l == null) l = cachedLogic = FindFirstObjectByType<logic>();
            bool globalDark = l != null && l.isDarkActive;
            bool isDark = SplitRealmZone.GetEffectiveDarkState(transform.position, globalDark);
            if (activeInWorld == WorldAffinity.LightOnly && isDark && RealityAnchorLantern.IsPositionAnchoredInLight(transform.position))
            {
                return true;
            }
            return (activeInWorld == WorldAffinity.ShadowOnly && isDark)
                || (activeInWorld == WorldAffinity.LightOnly && !isDark);
        }

        private static CachedColliderInfo GetOrCacheColliderInfo(Collider2D col)
        {
            int id = col.GetInstanceID();
            if (ColliderCache.TryGetValue(id, out CachedColliderInfo cached) && cached.Collider == col)
            {
                return cached;
            }

            bool ignoreAlways = col.GetComponent<Light>() != null
                || col.GetComponent<Dark>() != null
                || col.GetComponent<MirrorTwinBody>() != null
                || col.GetComponent<BlackHoleLens>() != null
                || col.GetComponentInParent<BlackHoleLens>() != null;

            IOpticalElement optical = null;
            if (!ignoreAlways)
            {
                optical = col.GetComponent<IOpticalElement>();
                if (optical == null) optical = col.GetComponentInParent<IOpticalElement>();
            }

            CachedColliderInfo info = new CachedColliderInfo
            {
                Collider = col,
                IgnoreAlways = ignoreAlways,
                Optical = optical
            };
            ColliderCache[id] = info;
            return info;
        }

        private bool TryPickFirstHit(RaycastHit2D[] hits, int hitCount, Collider2D ignoreCollider, out RaycastHit2D chosenHit, out IOpticalElement chosenOptical)
        {
            chosenHit = default;
            chosenOptical = null;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D h = hits[i];
                if (h.collider == null || h.collider == ignoreCollider || h.collider == selfIgnoreCollider) continue;

                CachedColliderInfo info = GetOrCacheColliderInfo(h.collider);
                if (info.IgnoreAlways) continue;

                // Non-optical trigger colliders (e.g., PressurePlate, LevelExit) do not block beams.
                if (h.collider.isTrigger && info.Optical == null) continue;

                chosenHit = h;
                chosenOptical = info.Optical;
                return true;
            }

            return false;
        }

        private int RaycastSorted(Vector2 origin, Vector2 dir, float dist)
        {
            ContactFilter2D filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = hitMask,
                useTriggers = Physics2D.queriesHitTriggers
            };

            int n;
            while (true)
            {
                n = Physics2D.Raycast(origin, dir, filter, HitBuffer, dist);
                if (n < HitBuffer.Length) break;
                HitBuffer = new RaycastHit2D[HitBuffer.Length * 2];
            }

            // Insertion sort by hit distance (small n, zero allocations).
            for (int i = 1; i < n; i++)
            {
                RaycastHit2D key = HitBuffer[i];
                int j = i - 1;
                while (j >= 0 && HitBuffer[j].distance > key.distance)
                {
                    HitBuffer[j + 1] = HitBuffer[j];
                    j--;
                }
                HitBuffer[j + 1] = key;
            }
            return n;
        }

        private void CastSegment(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider = null)
        {
            if (remainingBounces < 0) return;

            int hitCount = RaycastSorted(origin, direction, maxDistance);

            if (!TryPickFirstHit(HitBuffer, hitCount, ignoreCollider, out RaycastHit2D chosenHit, out IOpticalElement chosenOptical))
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

        private void CastCurvedSegment(Vector2 origin, Vector2 direction, LightColor color, int remainingBounces, Collider2D ignoreCollider = null)
        {
            if (remainingBounces < 0 || direction.sqrMagnitude < 0.0001f) return;

            const float ds = 0.14f;
            Vector2 pos = origin;
            Vector2 dir = direction.normalized;
            float traveled = 0f;
            Collider2D activeIgnore = ignoreCollider;

            curvedPathBuffer.Clear();
            curvedPathBuffer.Add(new Vector3(pos.x, pos.y, 0f));
            Vector2 lastAddedPos = pos;
            Vector2 lastAddedDir = dir;

            while (traveled < maxDistance)
            {
                // Apply gravitational bending / event-horizon capture from every active Black Hole.
                for (int i = 0; i < BlackHoleLens.ActiveBlackHoles.Count; i++)
                {
                    BlackHoleLens bh = BlackHoleLens.ActiveBlackHoles[i];
                    if (bh == null || !bh.IsFieldActive) continue;

                    Vector2 toCenter = (Vector2)bh.transform.position - pos;
                    float dist = toCenter.magnitude;
                    if (dist <= bh.eventHorizonRadius)
                    {
                        curvedPathBuffer.Add(new Vector3(pos.x, pos.y, 0f));
                        bh.NotifyBeamAbsorbed(color, pos);
                        DrawPolyline(curvedPathBuffer, color);
                        return;
                    }

                    if (dist < bh.gravityRadius && dist > 0.0001f)
                    {
                        Vector2 radial = toCenter / dist;
                        Vector2 perp = radial - dir * Vector2.Dot(radial, dir);
                        float fade = 1f - (dist / bh.gravityRadius);
                        float pull = (bh.EffectiveGravityStrength / Mathf.Max(dist * dist, 0.35f)) * (fade * fade);
                        dir = (dir + perp * (pull * ds)).normalized;
                    }
                }

                float step = Mathf.Min(ds, maxDistance - traveled);
                if (traveled > 1.15f) activeIgnore = null;

                int hitCount = RaycastSorted(pos, dir, step);

                if (TryPickFirstHit(HitBuffer, hitCount, activeIgnore, out RaycastHit2D chosenHit, out IOpticalElement chosenOptical))
                {
                    Vector2 endPoint = chosenOptical != null
                        ? (Vector2)chosenHit.collider.transform.position
                        : chosenHit.point;

                    curvedPathBuffer.Add(new Vector3(endPoint.x, endPoint.y, 0f));
                    DrawPolyline(curvedPathBuffer, color);

                    if (chosenOptical == null || remainingBounces <= 0) return;

                    LightBeamHit beamHit = new LightBeamHit
                    {
                        Point = endPoint,
                        Normal = chosenHit.normal,
                        IncomingDirection = dir,
                        Color = color,
                        Collider = chosenHit.collider
                    };
                    chosenOptical.OnBeamHit(beamHit, this, remainingBounces - 1);
                    return;
                }

                pos += dir * step;
                traveled += step;

                if (Vector2.Dot(dir, lastAddedDir) < 0.9994f || (pos - lastAddedPos).sqrMagnitude >= 0.20f)
                {
                    curvedPathBuffer.Add(new Vector3(pos.x, pos.y, 0f));
                    lastAddedPos = pos;
                    lastAddedDir = dir;
                }
            }

            Vector3 finalPt = new Vector3(pos.x, pos.y, 0f);
            if ((curvedPathBuffer[curvedPathBuffer.Count - 1] - finalPt).sqrMagnitude > 0.0001f)
            {
                curvedPathBuffer.Add(finalPt);
            }
            DrawPolyline(curvedPathBuffer, color);
        }

        private void DrawSegment(Vector2 start, Vector2 end, LightColor color)
        {
            BeamLinePair pair = GetOrCreateLinePair(activeLineCount++);
            Color outerColor = LightColorUtility.ToUnityColor(color);
            Vector3 p0 = new Vector3(start.x, start.y, 0f);
            Vector3 p1 = new Vector3(end.x, end.y, 0f);

            ConfigureBeamLine(pair.OuterGlow, beamWidth, outerColor, 2);
            pair.OuterGlow.SetPosition(0, p0);
            pair.OuterGlow.SetPosition(1, p1);

            ConfigureBeamLine(pair.InnerCore, beamWidth * 0.41f, Color.white, 2);
            pair.InnerCore.SetPosition(0, p0);
            pair.InnerCore.SetPosition(1, p1);
        }

        private void DrawPolyline(List<Vector3> points, LightColor color)
        {
            if (points == null || points.Count < 2) return;

            BeamLinePair pair = GetOrCreateLinePair(activeLineCount++);
            Color outerColor = LightColorUtility.ToUnityColor(color);
            int count = points.Count;

            ConfigureBeamLine(pair.OuterGlow, beamWidth, outerColor, count);
            ConfigureBeamLine(pair.InnerCore, beamWidth * 0.41f, Color.white, count);

            for (int i = 0; i < count; i++)
            {
                pair.OuterGlow.SetPosition(i, points[i]);
                pair.InnerCore.SetPosition(i, points[i]);
            }
        }

        private static void ConfigureBeamLine(LineRenderer lr, float width, Color color, int vertexCount)
        {
            lr.enabled = true;
            lr.positionCount = vertexCount;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.startColor = color;
            lr.endColor = color;
        }

        private BeamLinePair GetOrCreateLinePair(int index)
        {
            while (linePool.Count <= index)
            {
                Material mat = beamMaterial != null ? beamMaterial : SharedRenderResources.SpriteDefaultMaterial;

                GameObject outerGo = new GameObject($"BeamOuter_{linePool.Count}");
                outerGo.transform.SetParent(transform, false);
                LineRenderer outer = outerGo.AddComponent<LineRenderer>();
                outer.useWorldSpace = true;
                outer.numCapVertices = 0;
                outer.numCornerVertices = 0;
                outer.sortingOrder = sortingOrder;
                if (mat != null) outer.sharedMaterial = mat;

                GameObject coreGo = new GameObject($"BeamCore_{linePool.Count}");
                coreGo.transform.SetParent(transform, false);
                LineRenderer core = coreGo.AddComponent<LineRenderer>();
                core.useWorldSpace = true;
                core.numCapVertices = 0;
                core.numCornerVertices = 0;
                core.sortingOrder = sortingOrder + 1;
                if (mat != null) core.sharedMaterial = mat;

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
