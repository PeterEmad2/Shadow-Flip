using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Controls an opposite-realm split zone along splitAxisX (Level 11):
    /// Objects/beams/receivers on the opposite side of splitAxisX from the player
    /// see the inverted realm state (!globalIsDark).
    /// </summary>
    public class SplitRealmZone : MonoBehaviour
    {
        public static SplitRealmZone ActiveZone { get; private set; }

        [Header("Split Divider")]
        public float splitAxisX = 0f;

        [Header("Opposite-Side Ambient Visuals")]
        public Color umbraProjectionTint = new Color(0.14f, 0.08f, 0.28f, 0.42f);
        public Color lightProjectionTint = new Color(1.0f, 0.96f, 0.78f, 0.22f);

        private PlayerMovement player;
        private logic gameLogic;
        private bool lastPlayerOnRight;
        private bool lastDarkState;
        private SpriteRenderer westOverlay;
        private SpriteRenderer eastOverlay;
        private Texture2D solidTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ActiveZone = null;
        }

        public static bool GetEffectiveDarkState(Vector2 worldPos, bool globalIsDark)
        {
            if (ActiveZone == null || !ActiveZone.isActiveAndEnabled)
            {
                return globalIsDark;
            }

            bool playerOnRight = ActiveZone.IsPlayerOnRightSide();
            bool targetOnRight = worldPos.x > ActiveZone.splitAxisX;
            return targetOnRight != playerOnRight ? !globalIsDark : globalIsDark;
        }

        void Awake()
        {
            ActiveZone = this;
            BuildHalfArenaOverlays();
        }

        void Start()
        {
            player = FindFirstObjectByType<PlayerMovement>();
            gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            lastPlayerOnRight = IsPlayerOnRightSide();
            lastDarkState = gameLogic != null && gameLogic.isDarkActive;

            if (gameLogic != null)
            {
                gameLogic.OnWorldFlipped += HandleWorldFlipped;
            }

            SyncSplitState(forceRefresh: true);
        }

        void OnDestroy()
        {
            if (gameLogic != null)
            {
                gameLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
            if (ActiveZone == this) ActiveZone = null;
        }

        void Update()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();

            bool playerOnRight = IsPlayerOnRightSide();
            bool isDark = gameLogic != null && gameLogic.isDarkActive;

            if (playerOnRight != lastPlayerOnRight || isDark != lastDarkState)
            {
                lastPlayerOnRight = playerOnRight;
                lastDarkState = isDark;
                SyncSplitState(forceRefresh: true);
            }
        }

        public bool IsPlayerOnRightSide()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (player == null) return false;
            Transform t = player.GetActiveFormTransform();
            return t != null && t.position.x > splitAxisX;
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            lastDarkState = isDarkActive;
            SyncSplitState(forceRefresh: true);
        }

        private void SyncSplitState(bool forceRefresh)
        {
            bool isDark = gameLogic != null && gameLogic.isDarkActive;
            bool playerOnRight = IsPlayerOnRightSide();

            if (westOverlay != null)
            {
                // Opposite side gets contrasted visual overlay; player side stays clean.
                bool westOpposite = playerOnRight;
                westOverlay.enabled = westOpposite;
                if (westOpposite)
                {
                    bool westIsDark = !isDark;
                    westOverlay.color = westIsDark ? umbraProjectionTint : lightProjectionTint;
                }
            }

            if (eastOverlay != null)
            {
                bool eastOpposite = !playerOnRight;
                eastOverlay.enabled = eastOpposite;
                if (eastOpposite)
                {
                    bool eastIsDark = !isDark;
                    eastOverlay.color = eastIsDark ? umbraProjectionTint : lightProjectionTint;
                }
            }

            if (forceRefresh)
            {
                WorldPhaseObject[] phases = FindObjectsByType<WorldPhaseObject>(FindObjectsSortMode.None);
                for (int i = 0; i < phases.Length; i++)
                {
                    if (phases[i] != null) phases[i].RefreshState();
                }
            }
        }

        private void BuildHalfArenaOverlays()
        {
            solidTex = new Texture2D(2, 2);
            solidTex.filterMode = FilterMode.Point;
            Color[] cols = { Color.white, Color.white, Color.white, Color.white };
            solidTex.SetPixels(cols);
            solidTex.Apply();

            Sprite whiteSprite = Sprite.Create(solidTex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);

            westOverlay = CreateHalfOverlay("WestRealmProjectionOverlay", whiteSprite, new Vector3(splitAxisX - 5.5f, 0f, 0f), new Vector3(11f, 13f, 1f));
            eastOverlay = CreateHalfOverlay("EastRealmProjectionOverlay", whiteSprite, new Vector3(splitAxisX + 5.5f, 0f, 0f), new Vector3(11f, 13f, 1f));
        }

        private SpriteRenderer CreateHalfOverlay(string childName, Sprite sprite, Vector3 pos, Vector3 scale)
        {
            GameObject go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -19;
            sr.enabled = false;
            return sr;
        }
    }
}
