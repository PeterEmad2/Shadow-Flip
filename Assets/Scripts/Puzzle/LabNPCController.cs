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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            IsAnyDialogueActive = false;
        }

        private SpriteRenderer sr;
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

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            EnsureLabUniformFallbacks();
            UpdateVisualSprite();
        }

        void OnDisable()
        {
            if (dialogueTimer > 0f)
            {
                dialogueTimer = 0f;
                RefreshAnyDialogueFlag();
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
                }
            }
            else if (patrolPoints != null && patrolPoints.Length > 1)
            {
                UpdatePatrol();
            }

            RefreshAnyDialogueFlag();
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

        private static void RefreshAnyDialogueFlag()
        {
            LabNPCController[] npcs = FindObjectsByType<LabNPCController>(FindObjectsSortMode.None);
            bool any = false;
            for (int i = 0; i < npcs.Length; i++)
            {
                if (npcs[i] != null && npcs[i].dialogueTimer > 0f)
                {
                    any = true;
                    break;
                }
            }
            IsAnyDialogueActive = any;
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

        private bool CanInteractInCurrentWorld()
        {
            if (usableInWorld == WorldAffinity.Both) return true;
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
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
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
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

            float bannerW = Mathf.Min(820f, Screen.width - 40f);
            float bannerH = 98f;
            float x = Mathf.Round((Screen.width - bannerW) * 0.5f);
            float y = 12f;

            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            Color ink = new Color(0.03f, 0.02f, 0.06f, 0.88f);
            Color fill = isDark ? new Color(0.08f, 0.05f, 0.16f, 0.78f) : new Color(0.05f, 0.07f, 0.12f, 0.78f);
            Color accent = isDark ? new Color(0.76f, 0.48f, 1f, 1f) : new Color(0.38f, 0.92f, 1f, 1f);

            const float pxStep = 3f;
            Rect box = new Rect(x, y, bannerW, bannerH);
            GUI.color = ink;
            GUI.DrawTexture(new Rect(box.x + pxStep, box.y, box.width - pxStep * 2f, box.height), whiteTex);
            GUI.DrawTexture(new Rect(box.x, box.y + pxStep, box.width, box.height - pxStep * 2f), whiteTex);

            GUI.color = fill;
            GUI.DrawTexture(new Rect(box.x + pxStep, box.y + pxStep, box.width - pxStep * 2f, box.height - pxStep * 2f), whiteTex);
            GUI.color = accent;
            GUI.DrawTexture(new Rect(box.x + pxStep * 2f, box.y + pxStep, box.width - pxStep * 4f, 4f), whiteTex);

            GUI.color = accent;
            GUI.Label(new Rect(box.x + 18f, box.y + 10f, box.width - 170f, 26f), npcName.ToUpperInvariant(), nameStyle);

            GUI.color = new Color(0.82f, 0.88f, 0.96f, 0.9f);
            string counter = dialogueLines.Length > 1 ? $"[{dialogueIndex + 1}/{dialogueLines.Length}]  [E] Next" : "[E] Repeat";
            GUI.Label(new Rect(box.x + box.width - 155f, box.y + 12f, 138f, 22f), counter, hintStyle);

            // Drop shadow + crisp pure white dialogue text for high legibility over any background
            Rect textRect = new Rect(box.x + 18f, box.y + 38f, box.width - 36f, 52f);
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(textRect.x + 1.5f, textRect.y + 1.5f, textRect.width, textRect.height), dialogueLines[dialogueIndex], textStyle);
            GUI.color = Color.white;
            GUI.Label(textRect, dialogueLines[dialogueIndex], textStyle);
        }

        private void EnsureGuiStyles()
        {
            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.filterMode = FilterMode.Point;
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }

            if (nameStyle == null)
            {
                nameStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperRight
                };
            }
        }
    }
}
