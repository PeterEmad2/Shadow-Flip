using System.Collections.Generic;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Renders realm-flip chromatic screen flashes and pixel-art sparkle burst particles.
    /// </summary>
    public class PuzzleVFXManager : MonoBehaviour
    {
        public static PuzzleVFXManager Instance { get; private set; }

        private struct PixelSpark
        {
            public GameObject Go;
            public SpriteRenderer Sr;
            public Vector3 Velocity;
            public Color BaseColor;
            public float Life;
            public float Duration;
        }

        private float flipFlashAlpha = 0f;
        private Color flipFlashColor = Color.white;
        private logic cachedLogic;
        private bool lastKnownDark;
        private Texture2D whiteTex;

        private readonly List<PixelSpark> activeSparks = new List<PixelSpark>(64);
        private readonly Queue<PixelSpark> sparkPool = new Queue<PixelSpark>(64);
        private static Sprite pixelSparkSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
            pixelSparkSprite = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInScene()
        {
            if (FindFirstObjectByType<PuzzleVFXManager>() == null)
            {
                GameObject go = new GameObject("PuzzleVFXManager");
                go.AddComponent<PuzzleVFXManager>();
            }
        }

        public static void SpawnPixelBurst(Vector3 worldPos, Color color, int count = 10, float speed = 3.2f)
        {
            if (Instance == null) return;
            Instance.SpawnBurstInternal(worldPos, color, count, speed);
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

            float dt = Time.unscaledDeltaTime;
            for (int i = activeSparks.Count - 1; i >= 0; i--)
            {
                PixelSpark s = activeSparks[i];
                s.Life -= dt;
                if (s.Life <= 0f || s.Go == null)
                {
                    if (s.Go != null)
                    {
                        s.Go.SetActive(false);
                        sparkPool.Enqueue(s);
                    }
                    activeSparks.RemoveAt(i);
                    continue;
                }

                s.Go.transform.position += s.Velocity * dt;
                s.Velocity *= Mathf.Max(0f, 1f - 4.5f * dt);

                float ratio = Mathf.Clamp01(s.Life / s.Duration);
                float scale = ratio > 0.35f ? 0.14f : 0.08f;
                s.Go.transform.localScale = new Vector3(scale, scale, 1f);
                Color c = s.BaseColor;
                c.a = ratio > 0.25f ? 1f : 0.5f;
                if (s.Sr != null) s.Sr.color = c;

                activeSparks[i] = s;
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

            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null)
            {
                SpawnBurstInternal(pm.GetActiveFormTransform().position, flipFlashColor, 14, 4.0f);
            }
            RealmCameraController.TriggerShake(0.08f, 0.12f);
        }

        private void SpawnBurstInternal(Vector3 worldPos, Color color, int count, float speed)
        {
            Sprite sp = GetPixelSparkSprite();
            for (int i = 0; i < count; i++)
            {
                PixelSpark s;
                if (sparkPool.Count > 0)
                {
                    s = sparkPool.Dequeue();
                }
                else
                {
                    GameObject go = new GameObject("PixelSpark");
                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = sp;
                    sr.sortingOrder = 25;
                    s = new PixelSpark { Go = go, Sr = sr };
                }

                if (s.Go == null) continue;
                float angle = (i * (360f / Mathf.Max(1, count)) + Random.Range(-18f, 18f)) * Mathf.Deg2Rad;
                float spd = speed * Random.Range(0.55f, 1.15f);
                s.Go.transform.position = new Vector3(worldPos.x, worldPos.y, -0.05f);
                s.Go.transform.localScale = new Vector3(0.14f, 0.14f, 1f);
                s.Go.SetActive(true);
                s.Velocity = new Vector3(Mathf.Cos(angle) * spd, Mathf.Sin(angle) * spd, 0f);
                s.BaseColor = color;
                s.Duration = Random.Range(0.28f, 0.48f);
                s.Life = s.Duration;
                if (s.Sr != null) s.Sr.color = color;
                activeSparks.Add(s);
            }
        }

        private static Sprite GetPixelSparkSprite()
        {
            if (pixelSparkSprite != null) return pixelSparkSprite;
            Texture2D tex = new Texture2D(2, 2);
            tex.filterMode = FilterMode.Point;
            Color[] cols = { Color.white, Color.white, Color.white, Color.white };
            tex.SetPixels(cols);
            tex.Apply();
            pixelSparkSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
            return pixelSparkSprite;
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
