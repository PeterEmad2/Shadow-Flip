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
        public float inactiveAlpha = 0.2f;
        public Color lightWorldTint = Color.white;
        public Color shadowWorldTint = new Color(0.68f, 0.72f, 1f, 1f);
        public bool disableRenderersInsteadOfFade = false;

        private Collider2D[] colliders;
        private SpriteRenderer[] renderers;
        private Color[] baseColors;
        private logic cachedLogic;

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

            bool activeInTarget = (affinity == WorldAffinity.LightOnly && !targetDarkWorld)
                || (affinity == WorldAffinity.ShadowOnly && targetDarkWorld);
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

        private void HandleWorldFlipped(bool isDarkActive)
        {
            IsActiveInCurrentWorld = affinity == WorldAffinity.Both
                || (affinity == WorldAffinity.LightOnly && !isDarkActive)
                || (affinity == WorldAffinity.ShadowOnly && isDarkActive);

            foreach (Collider2D col in colliders)
            {
                if (col != null) col.enabled = IsActiveInCurrentWorld;
            }

            bool swapsSprites = lightWorldSprite != null && shadowWorldSprite != null;
            if (swapsSprites && renderers.Length > 0 && renderers[0] != null)
            {
                renderers[0].sprite = isDarkActive ? shadowWorldSprite : lightWorldSprite;
            }

            Color worldTint = swapsSprites ? Color.white : (isDarkActive ? shadowWorldTint : lightWorldTint);
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
                    c.a = IsActiveInCurrentWorld ? baseColors[i].a : baseColors[i].a * inactiveAlpha;
                    sr.color = c;
                }
            }
        }
    }
}
