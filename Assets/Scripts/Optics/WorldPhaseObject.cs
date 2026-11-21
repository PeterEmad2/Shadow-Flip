using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Controls visibility, collision, and cel-shaded Light/Shadow Realm sprite swapping
    /// based on whether the player is in the Light Realm or Shadow Realm.
    /// </summary>
    public class WorldPhaseObject : MonoBehaviour
    {
        public WorldAffinity affinity = WorldAffinity.Both;

        [Header("Cel-Shaded Realm Sprites (Optional)")]
        public Sprite lightWorldSprite;
        public Sprite shadowWorldSprite;

        [Range(0f, 1f)]
        public float inactiveAlpha = 0.16f;
        public Color lightWorldTint = Color.white;
        public Color shadowWorldTint = new Color(0.68f, 0.72f, 1f, 1f);
        public bool disableRenderersInsteadOfFade = false;

        private Collider2D[] colliders;
        private SpriteRenderer[] renderers;
        private Color[] baseColors;
        private logic cachedLogic;
        private LineRenderer phaseBorder;
        private bool isPhaseWall;

        public bool IsActiveInCurrentWorld { get; private set; } = true;

        void Awake()
        {
            colliders = GetComponentsInChildren<Collider2D>(true);
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                baseColors[i] = renderers[i].color;
            }

            DetectAndStyleWall();
        }

        void OnEnable()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
                HandleWorldFlipped(cachedLogic.isDarkActive);
            }
            else
            {
                HandleWorldFlipped(false);
            }
        }

        void OnDisable()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
        }

        public void RefreshState()
        {
            bool darkActive = cachedLogic != null && cachedLogic.isDarkActive;
            HandleWorldFlipped(darkActive);
        }

        public bool WouldBlockPosition(Vector2 worldPos, float radius, bool targetDarkWorld)
        {
            if (affinity == WorldAffinity.Both || IsActiveInCurrentWorld) return false;

            bool effectiveTargetDark = SplitRealmZone.GetEffectiveDarkState(transform.position, targetDarkWorld);
            bool activeInTarget = (affinity == WorldAffinity.LightOnly && !effectiveTargetDark)
                || (affinity == WorldAffinity.ShadowOnly && effectiveTargetDark);
            if (!activeInTarget) return false;

            bool hasSolidCollider = false;
            float expandedRadius = radius * 0.78f;
            if (colliders != null)
            {
                foreach (Collider2D col in colliders)
                {
                    if (col == null || col.isTrigger) continue;
                    hasSolidCollider = true;

                    bool prevEnabled = col.enabled;
                    col.enabled = true;
                    Bounds cb = col.bounds;
                    col.enabled = prevEnabled;

                    if (cb.size.sqrMagnitude > 0.001f)
                    {
                        cb.Expand(expandedRadius * 2f);
                        Vector3 probe = new Vector3(worldPos.x, worldPos.y, cb.center.z);
                        if (cb.Contains(probe))
                        {
                            return true;
                        }
                    }
                }
            }
            if (!hasSolidCollider || renderers == null) return false;

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr == null) continue;
                Bounds expanded = sr.bounds;
                expanded.Expand(expandedRadius * 2f);
                Vector3 probe = new Vector3(worldPos.x, worldPos.y, expanded.center.z);
                if (expanded.Contains(probe))
                {
                    return true;
                }
            }

            return false;
        }

        private void HandleWorldFlipped(bool globalDarkActive)
        {
            bool effectiveDark = SplitRealmZone.GetEffectiveDarkState(transform.position, globalDarkActive);

            IsActiveInCurrentWorld = affinity == WorldAffinity.Both
                || (affinity == WorldAffinity.LightOnly && !effectiveDark)
                || (affinity == WorldAffinity.ShadowOnly && effectiveDark);

            foreach (Collider2D col in colliders)
            {
                if (col != null) col.enabled = IsActiveInCurrentWorld;
            }

            bool swapsSprites = lightWorldSprite != null && shadowWorldSprite != null;
            if (swapsSprites && renderers.Length > 0 && renderers[0] != null)
            {
                renderers[0].sprite = effectiveDark ? shadowWorldSprite : lightWorldSprite;
            }

            Color worldTint = swapsSprites ? Color.white : (effectiveDark ? shadowWorldTint : lightWorldTint);
            if (isPhaseWall)
            {
                worldTint = affinity == WorldAffinity.LightOnly
                    ? new Color(0.62f, 0.48f, 1.0f, 1f)
                    : new Color(0.76f, 0.42f, 1.0f, 1f);
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                if (sr == null) continue;

                if (disableRenderersInsteadOfFade)
                {
                    sr.enabled = IsActiveInCurrentWorld;
                }
                else
                {
                    sr.enabled = true;
                    Color c = baseColors[i] * worldTint;
                    float activeAlpha = isPhaseWall ? 0.78f : baseColors[i].a;
                    c.a = IsActiveInCurrentWorld ? activeAlpha : baseColors[i].a * inactiveAlpha;
                    sr.color = c;
                }
            }

            if (phaseBorder != null)
            {
                Color bCol = affinity == WorldAffinity.LightOnly
                    ? new Color(0.55f, 0.85f, 1f, IsActiveInCurrentWorld ? 0.92f : 0.35f)
                    : new Color(0.82f, 0.42f, 1f, IsActiveInCurrentWorld ? 0.92f : 0.25f);
                phaseBorder.startColor = bCol;
                phaseBorder.endColor = bCol;
            }
        }

        private void DetectAndStyleWall()
        {
            if (affinity == WorldAffinity.Both) return;

            bool hasSolidWallCollider = false;
            BoxCollider2D wallBox = null;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && !colliders[i].isTrigger)
                {
                    hasSolidWallCollider = true;
                    wallBox = colliders[i] as BoxCollider2D;
                    break;
                }
            }

            bool isProp = GetComponent<OpticalMirror>() != null || GetComponent<OpticalPrism>() != null
                || GetComponent<LightSensor>() != null || GetComponent<PuzzleDoor>() != null
                || GetComponent<PushableBlock>() != null || GetComponent<ColorFilter>() != null
                || GetComponent<PortalMirror>() != null || GetComponent<ColorCombiner>() != null
                || GetComponent<BeamRelayCrystal>() != null || GetComponent<OneWayPolarizer>() != null
                || GetComponent<OpticalMemoryPrism>() != null || GetComponent<RealityAnchorLantern>() != null
                || GetComponent<BeamMagneticRail>() != null || GetComponent<BlackHoleLens>() != null;

            if (!hasSolidWallCollider || isProp || wallBox == null) return;

            isPhaseWall = true;

            int layerIdx = affinity == WorldAffinity.LightOnly
                ? LayerMask.NameToLayer("LightWorld")
                : LayerMask.NameToLayer("UmbraWorld");
            if (layerIdx >= 0) gameObject.layer = layerIdx;

            GameObject borderGo = new GameObject("PhaseWallOutline");
            borderGo.transform.SetParent(transform, false);
            phaseBorder = borderGo.AddComponent<LineRenderer>();
            phaseBorder.useWorldSpace = false;
            phaseBorder.loop = true;
            phaseBorder.numCornerVertices = 0;
            phaseBorder.numCapVertices = 0;
            phaseBorder.startWidth = 0.08f;
            phaseBorder.endWidth = 0.08f;
            phaseBorder.sortingOrder = 5;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) phaseBorder.material = new Material(shader);

            Vector2 half = wallBox.size * 0.5f;
            Vector2 off = wallBox.offset;
            phaseBorder.positionCount = 4;
            phaseBorder.SetPosition(0, new Vector3(off.x - half.x, off.y - half.y, 0f));
            phaseBorder.SetPosition(1, new Vector3(off.x + half.x, off.y - half.y, 0f));
            phaseBorder.SetPosition(2, new Vector3(off.x + half.x, off.y + half.y, 0f));
            phaseBorder.SetPosition(3, new Vector3(off.x - half.x, off.y + half.y, 0f));
        }
    }
}
