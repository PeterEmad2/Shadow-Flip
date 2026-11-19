using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Renders realm-flip chromatic screen flashes and world-flip ring bursts for visual juice.
    /// </summary>
    public class PuzzleVFXManager : MonoBehaviour
    {
        public static PuzzleVFXManager Instance { get; private set; }

        private float flipFlashAlpha = 0f;
        private Color flipFlashColor = Color.white;
        private logic cachedLogic;
        private bool lastKnownDark;
        private Texture2D whiteTex;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInScene()
        {
            if (FindFirstObjectByType<PuzzleVFXManager>() == null)
            {
                GameObject go = new GameObject("PuzzleVFXManager");
                go.AddComponent<PuzzleVFXManager>();
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedLogic != null)
            {
                lastKnownDark = cachedLogic.isDarkActive;
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
            }
        }

        void OnDestroy()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (flipFlashAlpha > 0f)
            {
                flipFlashAlpha = Mathf.Max(0f, flipFlashAlpha - Time.unscaledDeltaTime / 0.28f);
            }
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (isDarkActive == lastKnownDark) return;
            lastKnownDark = isDarkActive;
            flipFlashColor = isDarkActive
                ? new Color(0.55f, 0.28f, 1.0f, 1f)
                : new Color(1.0f, 0.90f, 0.45f, 1f);
            flipFlashAlpha = 0.36f;
        }

        void OnGUI()
        {
            if (flipFlashAlpha <= 0.005f) return;

            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.filterMode = FilterMode.Point;
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }

            // Full-screen flash + bold screen-edge vignette border
            Color fill = flipFlashColor;
            fill.a = flipFlashAlpha * 0.45f;
            GUI.color = fill;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);

            Color edge = flipFlashColor;
            edge.a = flipFlashAlpha * 0.95f;
            GUI.color = edge;
            float border = 14f;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, border), whiteTex);
            GUI.DrawTexture(new Rect(0, Screen.height - border, Screen.width, border), whiteTex);
            GUI.DrawTexture(new Rect(0, 0, border, Screen.height), whiteTex);
            GUI.DrawTexture(new Rect(Screen.width - border, 0, border, Screen.height), whiteTex);
            GUI.color = Color.white;
        }
    }
}
