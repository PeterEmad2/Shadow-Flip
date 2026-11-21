using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Umbra Warden Boss (Level 12): guarded sequentially by Red, Green, and Blue chromatic shields.
    /// Shatter all 3 shields with matching laser beams to petrify the Warden and unlock the final exit.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class UmbraWardenBoss : MonoBehaviour, IOpticalElement
    {
        [Header("Sprites")]
        public Sprite frame1;
        public Sprite frame2;
        public Sprite petrifiedSprite;

        [Header("Shield Stages")]
        public float shieldHoldTime = 0.55f;
        public float shieldRadius = 1.35f;

        private readonly LightColor[] shieldOrder = { LightColor.Red, LightColor.Green, LightColor.Blue };

        public int ShieldStage => shieldStage;
        public bool IsDefeated => shieldStage >= shieldOrder.Length;

        private SpriteRenderer sr;
        private LineRenderer shieldRing;
        private int shieldStage = 0;
        private float breakProgress = 0f;
        private int lastMatchHitFrame = -100;
        private Vector3 startPos;
        private Texture2D whiteTex;
        private GUIStyle bossStyle;

        void Awake()
        {
            startPos = transform.position;
            sr = GetComponent<SpriteRenderer>();
            if (sr != null && frame1 == null) frame1 = sr.sprite;
            BuildShieldRing();
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            if (IsDefeated)
            {
                if (shieldRing != null) shieldRing.enabled = false;
                if (sr != null)
                {
                    if (petrifiedSprite != null) sr.sprite = petrifiedSprite;
                    sr.color = new Color(0.85f, 0.88f, 0.94f, 1f);
                }
                return;
            }

            // Gentle menacing levitation
            transform.position = startPos + new Vector3(0f, Mathf.Sin(Time.time * 3.5f) * 0.14f, 0f);

            if (sr != null && frame1 != null && frame2 != null)
            {
                int idx = Mathf.FloorToInt(Time.time * 4f) % 2;
                sr.sprite = idx == 0 ? frame1 : frame2;
            }

            if ((Time.frameCount - lastMatchHitFrame) > 2)
            {
                breakProgress = Mathf.Max(0f, breakProgress - Time.deltaTime * 0.8f);
            }

            if (shieldRing != null)
            {
                shieldRing.enabled = true;
                Color shieldCol = LightColorUtility.ToUnityColor(shieldOrder[shieldStage]);
                shieldRing.startColor = shieldCol;
                shieldRing.endColor = shieldCol;
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (IsDefeated) return;

            LightColor required = shieldOrder[shieldStage];
            if (!LightColorUtility.Matches(hit.Color, required, false)) return;

            lastMatchHitFrame = Time.frameCount;
            breakProgress += Time.deltaTime;
            if (breakProgress >= shieldHoldTime)
            {
                Color burstCol = LightColorUtility.ToUnityColor(required);
                breakProgress = 0f;
                shieldStage++;

                PuzzleAudioManager.Instance?.PlayMonsterPetrify();
                PuzzleVFXManager.SpawnPixelBurst(transform.position, burstCol, 20, 4.5f);
                RealmCameraController.TriggerShake(0.24f, 0.25f);
            }
        }

        void OnGUI()
        {
            if (Time.timeScale == 0f) return;
            EnsureStyles();

            float w = 310f;
            float h = 42f;
            float barY = LabNPCController.IsAnyDialogueActive
                ? LabNPCController.ActiveBannerBottomY
                : (Screen.width < 1380f ? 114f : 14f);
            Rect r = new Rect(Mathf.Round((Screen.width - w) * 0.5f), barY, w, h);

            GUI.color = new Color(0.06f, 0.05f, 0.10f, 0.95f);
            GUI.DrawTexture(r, whiteTex);

            Color accent = IsDefeated
                ? new Color(0.45f, 1f, 0.72f, 1f)
                : LightColorUtility.ToUnityColor(shieldOrder[shieldStage]);

            GUI.color = new Color(0.16f, 0.12f, 0.26f, 0.95f);
            GUI.DrawTexture(new Rect(r.x + 3f, r.y + 3f, r.width - 6f, r.height - 6f), whiteTex);

            GUI.color = accent;
            string label = IsDefeated
                ? "UMBRA WARDEN PETRIFIED!"
                : $"WARDEN SHIELD {shieldStage + 1}/3: {shieldOrder[shieldStage].ToString().ToUpper()} BEAM";
            GUI.Label(new Rect(r.x + 6f, r.y + 4f, r.width - 12f, 20f), label, bossStyle);

            Rect barTrack = new Rect(r.x + 12f, r.y + 26f, r.width - 24f, 10f);
            GUI.color = new Color(0.08f, 0.06f, 0.12f, 1f);
            GUI.DrawTexture(barTrack, whiteTex);

            float shieldsLeft = IsDefeated ? 0f : (3 - shieldStage - (breakProgress / shieldHoldTime)) / 3f;
            float fillW = Mathf.Floor(barTrack.width * Mathf.Clamp01(shieldsLeft) / 4f) * 4f;
            if (fillW > 1f)
            {
                GUI.color = accent;
                GUI.DrawTexture(new Rect(barTrack.x, barTrack.y, fillW, barTrack.height), whiteTex);
            }
            GUI.color = Color.white;
        }

        private void BuildShieldRing()
        {
            GameObject ringGo = new GameObject("WardenShieldRing");
            ringGo.transform.SetParent(transform, false);
            shieldRing = ringGo.AddComponent<LineRenderer>();
            shieldRing.useWorldSpace = false;
            shieldRing.loop = true;
            shieldRing.numCornerVertices = 0;
            shieldRing.numCapVertices = 0;
            shieldRing.startWidth = 0.11f;
            shieldRing.endWidth = 0.11f;
            shieldRing.sortingOrder = 16;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) shieldRing.material = new Material(shader);

            const int segments = 16;
            shieldRing.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float px = Mathf.Round(Mathf.Cos(angle) * shieldRadius * 16f) / 16f;
                float py = Mathf.Round(Mathf.Sin(angle) * shieldRadius * 16f) / 16f;
                shieldRing.SetPosition(i, new Vector3(px, py, 0f));
            }
        }

        private void EnsureStyles()
        {
            if (whiteTex == null) whiteTex = PixelGuiUtility.MakeSolidTexture(Color.white);
            if (bossStyle == null)
            {
                bossStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
    }
}
