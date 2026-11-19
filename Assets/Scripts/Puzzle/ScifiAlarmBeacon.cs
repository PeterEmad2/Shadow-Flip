using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Wall-mounted facility emergency beacon that switches from dormant (Light Realm)
    /// to an active pulsing red alarm overlay in the Shadow / Umbra Realm.
    /// </summary>
    public class ScifiAlarmBeacon : MonoBehaviour
    {
        [Header("Sprites & Renderers")]
        public SpriteRenderer beaconRenderer;
        public SpriteRenderer glowRenderer;
        public Sprite offSprite;
        public Sprite onSprite;

        [Header("Emergency Glow")]
        public Color glowColor = new Color(1f, 0.24f, 0.22f, 1f);
        public float basePulseSpeed = 4.2f;
        public float minGlowAlpha = 0.25f;
        public float maxGlowAlpha = 0.85f;

        void Awake()
        {
            if (beaconRenderer == null) beaconRenderer = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;

            if (beaconRenderer != null)
            {
                if (isDark && onSprite != null) beaconRenderer.sprite = onSprite;
                else if (!isDark && offSprite != null) beaconRenderer.sprite = offSprite;
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = isDark;
                if (isDark)
                {
                    bool monsterActive = ShadowStalkerController.Instance != null && ShadowStalkerController.Instance.IsMonsterActive;
                    float speed = monsterActive ? basePulseSpeed * 2f : basePulseSpeed;
                    float t = 0.5f + 0.5f * Mathf.Sin(Time.time * speed);
                    Color c = glowColor;
                    c.a = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, t);
                    glowRenderer.color = c;
                }
            }
        }
    }
}
