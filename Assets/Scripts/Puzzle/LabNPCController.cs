using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.Events;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Interactive laboratory NPC (scientist in Light Realm, hazmat tech or alternate variant in Shadow Realm).
    /// Displays cel-shaded speech bubbles on [E], patrols optional waypoints, and can operate remote lab equipment.
    /// </summary>
    public class LabNPCController : MonoBehaviour, IInteractable
    {
        [Header("Identity & Dialogue")]
        public string npcName = "Dr. Vance";
        [TextArea(2, 4)]
        public string[] dialogueLines =
        {
            "Welcome to Sector 7! Press [F] to flip between the Light Lab and the Umbra Lab."
        };
        public float dialogueDuration = 4.8f;

        [Header("Realm Restriction")]
        public WorldAffinity usableInWorld = WorldAffinity.Both;

        [Header("Light Realm Sprites")]
        public Sprite spriteDown;
        public Sprite spriteUp;
        public Sprite spriteLeft;
        public Sprite spriteRight;

        [Header("Shadow Realm Sprites (Optional)")]
        public Sprite shadowSpriteDown;
        public Sprite shadowSpriteUp;
        public Sprite shadowSpriteLeft;
        public Sprite shadowSpriteRight;

        [Header("Patrol (Optional)")]
        public Vector3[] patrolPoints;
        public float patrolSpeed = 1.4f;
        public float waypointWait = 1.2f;

        [Header("Interactive Lab Equipment (Optional)")]
        public OpticalMirror[] assistedMirrors;
        public GameObject[] toggledLabBarriers;
        public bool triggerEquipmentOnce = false;
        public UnityEvent onInteract;

        public static bool IsAnyDialogueActive { get; private set; }
        public static float ActiveBannerBottomY { get; private set; }

        private static readonly HashSet<LabNPCController> ActiveDialogueNpcs = new HashSet<LabNPCController>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsAnyDialogueActive = false;
            ActiveBannerBottomY = 0f;
            ActiveDialogueNpcs.Clear();
        }

        private SpriteRenderer sr;
        private logic cachedLogic;
        private int dialogueIndex = -1;
        private float dialogueTimer = 0f;
        private int patrolIndex = 0;
        private float waitTimer = 0f;
        private Vector2 facingDir = Vector2.down;
        private bool equipmentTriggered = false;

        private Texture2D whiteTex;
        private GUIStyle nameStyle;
        private GUIStyle textStyle;
        private GUIStyle hintStyle;
        private GUIStyle realmBadgeStyle;
        private readonly GUIContent cachedNameContent = new GUIContent();

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            EnsureLabUniformFallbacks();
            UpdateVisualSprite();
        }

        void OnDisable()
        {
            if (dialogueTimer > 0f || ActiveDialogueNpcs.Contains(this))
            {
                dialogueTimer = 0f;
                ActiveDialogueNpcs.Remove(this);
                UpdateDialogueBannerState();
            }
        }

        void Update()
        {
            if (dialogueTimer > 0f)
            {
                dialogueTimer -= Time.deltaTime;
                if (dialogueTimer <= 0f)
                {
                    dialogueTimer = 0f;
                    ActiveDialogueNpcs.Remove(this);
                    UpdateDialogueBannerState();
                }
            }
            else if (patrolPoints != null && patrolPoints.Length > 1)
            {
                UpdatePatrol();
            }

            UpdateVisualSprite();
        }

        private void EnsureLabUniformFallbacks()
        {
            if (spriteDown == null) spriteDown = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Male_Down");
            if (spriteUp == null) spriteUp = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Male_Up");
            if (spriteLeft == null) spriteLeft = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Male_Left");
            if (spriteRight == null) spriteRight = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Male_Right");

            if (shadowSpriteDown == null) shadowSpriteDown = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Hazmat_Down");
            if (shadowSpriteUp == null) shadowSpriteUp = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Hazmat_Up");
            if (shadowSpriteLeft == null) shadowSpriteLeft = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Hazmat_Left");
            if (shadowSpriteRight == null) shadowSpriteRight = Resources.Load<Sprite>("Sprites/Laboratory/LabNPC_Hazmat_Right");
        }

        private static void UpdateDialogueBannerState()
        {
            bool any = ActiveDialogueNpcs.Count > 0;
            IsAnyDialogueActive = any;
            ActiveBannerBottomY = any ? (14f + 148f + 10f) : 0f;
        }

        public void Interact(PlayerMovement player)
        {
            if (!CanInteractInCurrentWorld()) return;

            if (player != null)
            {
                Vector2 delta = (Vector2)(player.GetActiveFormTransform().position - transform.position);
                if (delta.sqrMagnitude > 0.001f) facingDir = delta.normalized;
            }

            if (dialogueLines != null && dialogueLines.Length > 0)
            {
                dialogueIndex = (dialogueIndex + 1) % dialogueLines.Length;
                dialogueTimer = dialogueDuration;
                ActiveDialogueNpcs.Add(this);
                UpdateDialogueBannerState();
            }

            PuzzleAudioManager.Instance?.PlayButtonClick();
            PuzzleVFXManager.SpawnPixelBurst(transform.position + Vector3.up * 0.45f, new Color(0.45f, 0.92f, 1f, 1f), 6, 2.2f);

            if (!triggerEquipmentOnce || !equipmentTriggered)
            {
                equipmentTriggered = true;

                if (assistedMirrors != null)
                {
                    foreach (OpticalMirror m in assistedMirrors)
                    {
                        if (m != null) m.RotateStep();
                    }
                }

                if (toggledLabBarriers != null)
                {
                    foreach (GameObject barrier in toggledLabBarriers)
                    {
                        if (barrier != null) barrier.SetActive(!barrier.activeSelf);
                    }
                }

                onInteract?.Invoke();
            }

            UpdateVisualSprite();
        }

        public string GetInteractPrompt()
        {
            if (!CanInteractInCurrentWorld())
            {
                return usableInWorld == WorldAffinity.LightOnly
                    ? $"Speak to {npcName} in Light Realm"
                    : $"Speak to {npcName} in Shadow Realm";
            }

            bool hasMech = (assistedMirrors != null && assistedMirrors.Length > 0)
                        || (toggledLabBarriers != null && toggledLabBarriers.Length > 0);
            return hasMech
                ? $"[E] Talk to {npcName} (Lab Assist)"
                : $"[E] Talk to {npcName}";
        }

        private logic ResolveLogic()
        {
            if (logic.Instance != null) return logic.Instance;
            if (cachedLogic == null) cachedLogic = FindFirstObjectByType<logic>();
            return cachedLogic;
        }

        private bool CanInteractInCurrentWorld()
        {
            if (usableInWorld == WorldAffinity.Both) return true;
            logic l = ResolveLogic();
            bool isDark = l != null && l.isDarkActive;
            return (usableInWorld == WorldAffinity.ShadowOnly && isDark)
                || (usableInWorld == WorldAffinity.LightOnly && !isDark);
        }

        private void UpdatePatrol()
        {
            if (waitTimer > 0f)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            Vector3 target = patrolPoints[patrolIndex];
            Vector3 diff = target - transform.position;
            float step = patrolSpeed * Time.deltaTime;

            if (diff.magnitude <= step)
            {
                transform.position = target;
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                waitTimer = waypointWait;
            }
            else
            {
                Vector3 dir = diff.normalized;
                facingDir = dir;
                transform.position += dir * step;
            }
        }

        private void UpdateVisualSprite()
        {
            if (sr == null) return;
            logic l = ResolveLogic();
            bool isDark = l != null && l.isDarkActive;

            Sprite down = isDark && shadowSpriteDown != null ? shadowSpriteDown : spriteDown;
            Sprite up = isDark && shadowSpriteUp != null ? shadowSpriteUp : spriteUp;
            Sprite left = isDark && shadowSpriteLeft != null ? shadowSpriteLeft : spriteLeft;
            Sprite right = isDark && shadowSpriteRight != null ? shadowSpriteRight : spriteRight;

            Sprite chosen;
            if (Mathf.Abs(facingDir.x) > Mathf.Abs(facingDir.y))
            {
                chosen = facingDir.x < 0f ? (left != null ? left : down) : (right != null ? right : down);
            }
            else
            {
                chosen = facingDir.y > 0f ? (up != null ? up : down) : down;
            }

            if (chosen != null) sr.sprite = chosen;
        }

        void OnGUI()
        {
            if (dialogueTimer <= 0f || Time.timeScale == 0f) return;
            if (dialogueLines == null || dialogueLines.Length == 0 || dialogueIndex < 0) return;

            EnsureGuiStyles();

            float bannerW = Mathf.Min(980f, Screen.width - 48f);
            float bannerH = 148f;
            float x = Mathf.Round((Screen.width - bannerW) * 0.5f);
            float y = 14f;

            logic l = ResolveLogic();
            bool isDark = l != null && l.isDarkActive;
            Color ink = new Color(0.03f, 0.02f, 0.06f, 0.94f);
            Color fill = isDark ? new Color(0.09f, 0.05f, 0.19f, 0.86f) : new Color(0.06f, 0.08f, 0.14f, 0.86f);
            Color accent = isDark ? new Color(0.76f, 0.48f, 1f, 1f) : new Color(0.38f, 0.92f, 1f, 1f);
            Color innerLine = new Color(accent.r, accent.g, accent.b, 0.25f);

            const float b = 4f;
            Rect box = new Rect(x, y, bannerW, bannerH);

            // Stepped pixel-art drop shadow
            GUI.color = new Color(0f, 0f, 0f, 0.50f);
            GUI.DrawTexture(new Rect(box.x + b * 2f, box.y + b, box.width - b * 2f, box.height), whiteTex);
            GUI.DrawTexture(new Rect(box.x + b, box.y + b * 2f, box.width, box.height - b * 2f), whiteTex);

            // Outer ink border
            GUI.color = ink;
            GUI.DrawTexture(new Rect(box.x + b, box.y, box.width - b * 2f, box.height), whiteTex);
            GUI.DrawTexture(new Rect(box.x, box.y + b, box.width, box.height - b * 2f), whiteTex);

            // Translucent interior fill
            GUI.color = fill;
            GUI.DrawTexture(new Rect(box.x + b, box.y + b, box.width - b * 2f, box.height - b * 2f), whiteTex);

            // Subtle inner hairline border
            GUI.color = innerLine;
            GUI.DrawTexture(new Rect(box.x + b, box.y + box.height - b - 1f, box.width - b * 2f, 1f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + b, box.y + b, 1f, box.height - b * 2f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + box.width - b - 1f, box.y + b, 1f, box.height - b * 2f), whiteTex);

            // Luminous accent top bar + 4 corner rivets
            GUI.color = accent;
            GUI.DrawTexture(new Rect(box.x + b * 2f, box.y + b, box.width - b * 4f, 5f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + b, box.y + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + box.width - b - 4f, box.y + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + b, box.y + box.height - b - 4f, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(box.x + box.width - b - 4f, box.y + box.height - b - 4f, 4f, 4f), whiteTex);

            // Left portrait slot (104 x 104 framed box)
            Rect portraitFrame = new Rect(box.x + 18f, box.y + 22f, 104f, 104f);
            GUI.color = ink;
            GUI.DrawTexture(portraitFrame, whiteTex);
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.45f);
            GUI.DrawTexture(new Rect(portraitFrame.x + 2f, portraitFrame.y + 2f, portraitFrame.width - 4f, portraitFrame.height - 4f), whiteTex);
            GUI.color = isDark ? new Color(0.14f, 0.09f, 0.26f, 0.95f) : new Color(0.10f, 0.13f, 0.20f, 0.95f);
            GUI.DrawTexture(new Rect(portraitFrame.x + 4f, portraitFrame.y + 4f, portraitFrame.width - 8f, portraitFrame.height - 8f), whiteTex);

            Sprite portrait = sr != null && sr.sprite != null ? sr.sprite : (isDark ? shadowSpriteDown : spriteDown);
            if (portrait != null && portrait.texture != null)
            {
                Texture2D tex = portrait.texture;
                Rect tr = portrait.textureRect;
                Rect uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
                Rect avail = new Rect(portraitFrame.x + 8f, portraitFrame.y + 8f, portraitFrame.width - 16f, portraitFrame.height - 16f);
                float aspect = tr.width / Mathf.Max(1f, tr.height);
                float drawW = avail.width;
                float drawH = drawW / Mathf.Max(0.01f, aspect);
                if (drawH > avail.height)
                {
                    drawH = avail.height;
                    drawW = drawH * aspect;
                }
                Rect drawRect = new Rect(
                    Mathf.Round(avail.x + (avail.width - drawW) * 0.5f),
                    Mathf.Round(avail.y + (avail.height - drawH) * 0.5f),
                    Mathf.Round(drawW),
                    Mathf.Round(drawH));
                GUI.color = Color.white;
                GUI.DrawTextureWithTexCoords(drawRect, tex, uv, true);
            }

            // Right text column
            float contentX = box.x + 138f;
            float contentW = box.width - 156f;

            // Header row: Name + Realm Badge + Counter
            string upperName = npcName.ToUpperInvariant();
            cachedNameContent.text = upperName;
            float nameW = Mathf.Min(contentW - 260f, nameStyle.CalcSize(cachedNameContent).x + 10f);

            PixelGuiUtility.DrawShadowedLabel(new Rect(contentX, box.y + 13f, nameW, 28f), upperName, nameStyle, accent, 2f);

            string realmBadgeText = isDark ? "UMBRA LAB" : "LIGHT LAB";
            Rect badgeRect = new Rect(contentX + nameW + 10f, box.y + 16f, 102f, 22f);
            GUI.color = ink;
            GUI.DrawTexture(badgeRect, whiteTex);
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.25f);
            GUI.DrawTexture(new Rect(badgeRect.x + 2f, badgeRect.y + 2f, badgeRect.width - 4f, badgeRect.height - 4f), whiteTex);
            GUI.color = accent;
            GUI.Label(badgeRect, realmBadgeText, realmBadgeStyle);

            string counter = dialogueLines.Length > 1 ? $"[{dialogueIndex + 1}/{dialogueLines.Length}]  [E] Next" : "[E] Repeat";
            GUI.color = new Color(0.86f, 0.91f, 0.98f, 0.95f);
            GUI.Label(new Rect(box.x + box.width - 188f, box.y + 15f, 170f, 24f), counter, hintStyle);

            // Divider hairline below header
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.35f);
            GUI.DrawTexture(new Rect(contentX, box.y + 44f, contentW, 2f), whiteTex);

            // Spacious wrapped dialogue text with 2px drop shadow
            Rect textRect = new Rect(contentX, box.y + 52f, contentW, 84f);
            PixelGuiUtility.DrawShadowedLabel(textRect, dialogueLines[dialogueIndex], textStyle, Color.white, 2f);

            // Bottom progress bar depleting over dialogueDuration
            float progress = Mathf.Clamp01(dialogueTimer / Mathf.Max(0.01f, dialogueDuration));
            Rect barTrack = new Rect(box.x + b + 4f, box.y + box.height - b - 5f, box.width - (b + 4f) * 2f, 4f);
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(barTrack, whiteTex);
            GUI.color = accent;
            GUI.DrawTexture(new Rect(barTrack.x, barTrack.y, Mathf.Round(barTrack.width * progress), barTrack.height), whiteTex);
            GUI.color = Color.white;
        }

        private void EnsureGuiStyles()
        {
            if (whiteTex == null) whiteTex = PixelGuiUtility.MakeSolidTexture(Color.white);

            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleRight
                };
            }

            if (realmBadgeStyle == null)
            {
                realmBadgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
    }
}
