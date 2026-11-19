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

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            UpdateVisualSprite();
        }

        void Update()
        {
            if (dialogueTimer > 0f)
            {
                dialogueTimer -= Time.deltaTime;
            }
            else if (patrolPoints != null && patrolPoints.Length > 1)
            {
                UpdatePatrol();
            }

            UpdateVisualSprite();
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

            Camera cam = Camera.main;
            if (cam == null) return;

            EnsureGuiStyles();

            Vector3 worldAnchor = transform.position + new Vector3(0f, 1.05f, 0f);
            Vector3 screenPos = cam.WorldToScreenPoint(worldAnchor);
            if (screenPos.z < 0f) return;

            float guiX = screenPos.x;
            float guiY = Screen.height - screenPos.y;

            float boxW = 340f;
            float boxH = 82f;
            float x = Mathf.Round(Mathf.Clamp(guiX - boxW * 0.5f, 12f, Screen.width - boxW - 12f));
            float y = Mathf.Round(Mathf.Clamp(guiY - boxH, 12f, Screen.height - boxH - 12f));

            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            Color ink = new Color(0.07f, 0.06f, 0.11f, 0.96f);
            Color fill = isDark ? new Color(0.15f, 0.11f, 0.28f, 0.96f) : new Color(0.14f, 0.20f, 0.30f, 0.96f);
            Color accent = isDark ? new Color(0.75f, 0.48f, 1f, 1f) : new Color(0.45f, 0.88f, 1f, 1f);

            const float pxStep = 3f;
            Rect box = new Rect(x, y, boxW, boxH);
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(box.x + pxStep, box.y + pxStep, box.width - pxStep * 2f, box.height), whiteTex);
            GUI.DrawTexture(new Rect(box.x, box.y + pxStep * 2f, box.width + pxStep, box.height - pxStep * 3f), whiteTex);

            GUI.color = ink;
            GUI.DrawTexture(new Rect(box.x + pxStep, box.y, box.width - pxStep * 2f, box.height), whiteTex);
            GUI.DrawTexture(new Rect(box.x, box.y + pxStep, box.width, box.height - pxStep * 2f), whiteTex);

            GUI.color = fill;
            GUI.DrawTexture(new Rect(box.x + pxStep, box.y + pxStep, box.width - pxStep * 2f, box.height - pxStep * 2f), whiteTex);
            GUI.color = accent;
            GUI.DrawTexture(new Rect(box.x + pxStep * 2f, box.y + pxStep, box.width - pxStep * 4f, pxStep), whiteTex);

            GUI.Label(new Rect(box.x + 10f, box.y + 8f, box.width - 20f, 20f), npcName, nameStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(box.x + 10f, box.y + 28f, box.width - 20f, 48f), dialogueLines[dialogueIndex], textStyle);
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
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (textStyle == null)
            {
                textStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }
        }
    }
}
