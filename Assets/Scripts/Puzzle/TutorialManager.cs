using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Guides the player step-by-step through Optics, Realm Flipping, Shadow Mirrors, and Ruin Blocks
    /// in the Tutorial level with animated cel-shaded target markers.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Core References")]
        public PlayerMovement player;
        public logic gameLogic;
        public PuzzleHUD hud;

        [Header("Lesson Triggers")]
        public LightSensor tutorialSensor1;
        public PuzzleLever tutorialLever1;
        public PuzzleDoor tutorialDoor1;
        public float crossXThreshold = -1.5f;
        public LightSensor tutorialSensor2;
        public PuzzleDoor tutorialDoor2;

        [Header("Stage Target Markers (0=Mirror1, 1=PhaseWall, 2=ShadowMirror, 3=RuinBlock, 4=Exit, 5=Lever1)")]
        public Transform[] stageTargets;

        private int currentStage = 0;
        private Texture2D boxTex;
        private GUIStyle markerStyle;

        void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (hud == null) hud = FindFirstObjectByType<PuzzleHUD>();
            ApplyStageText();
        }

        void Update()
        {
            int nextStage = currentStage;

            if (currentStage == 0)
            {
                bool sensorLit = tutorialSensor1 != null && tutorialSensor1.IsActivated;
                bool leverPulled = tutorialLever1 == null || tutorialLever1.isActivated;
                if (sensorLit && leverPulled)
                {
                    nextStage = 1;
                }
            }
            else if (currentStage == 1)
            {
                bool isDark = gameLogic != null && gameLogic.isDarkActive;
                bool crossed = player != null && player.GetActiveFormTransform().position.x > crossXThreshold;
                if (isDark && crossed)
                {
                    nextStage = 2;
                }
            }
            else if (currentStage == 2)
            {
                if (tutorialSensor2 != null && tutorialSensor2.IsActivated)
                {
                    nextStage = 3;
                }
            }
            else if (currentStage == 3)
            {
                if (tutorialDoor2 != null && tutorialDoor2.IsOpen)
                {
                    nextStage = 4;
                }
            }

            if (nextStage != currentStage)
            {
                currentStage = nextStage;
                ApplyStageText();
            }
        }

        private void ApplyStageText()
        {
            if (hud == null) return;

            switch (currentStage)
            {
                case 0:
                    hud.lessonTitle = "Lesson 1/4: Crystal Optics & Levers";
                    hud.levelHint = "Rotate the Angled Mirror Block [E] to light the Crystal, then pull the Dungeon Lever [E] to open the Gate.";
                    break;
                case 1:
                    hud.lessonTitle = "Lesson 2/4: Realm Flipping";
                    hud.levelHint = "A Light-Realm Stone Wall blocks the path! Press [F] to flip into the Shadow Realm and phase right through it.";
                    break;
                case 2:
                    hud.lessonTitle = "Lesson 3/4: Shadow Realm Mirrors";
                    hud.levelHint = "Some mirrors only exist in the Shadow Realm. Walk to the Shadow Mirror and press [E] to power the second Receiver Crystal.";
                    break;
                case 3:
                    hud.lessonTitle = "Lesson 4/4: Ruin Blocks & Vaults";
                    hud.levelHint = "Push the Ruined Pedestal Block onto the Floor Receiver Plate to unlock the Final Dungeon Gate!";
                    break;
                default:
                    hud.lessonTitle = "Tutorial Complete!";
                    hud.levelHint = "The Final Dungeon Gate is open! Step into the glowing Exit Portal to begin Level 1.";
                    break;
            }
        }

        void OnGUI()
        {
            if (Time.timeScale == 0f) return;
            Camera cam = Camera.main;
            if (cam == null || stageTargets == null || currentStage >= stageTargets.Length) return;

            int targetIdx = currentStage;
            string label = GetMarkerLabel(currentStage);
            if (currentStage == 0 && tutorialSensor1 != null && tutorialSensor1.IsActivated && tutorialLever1 != null && !tutorialLever1.isActivated && stageTargets.Length > 5)
            {
                targetIdx = 5;
                label = "▼ PULL LEVER [E]";
            }

            Transform target = stageTargets[targetIdx];
            if (target == null) return;

            EnsureStyles();

            float bob = Mathf.Sin(Time.time * 4.5f) * 0.14f;
            Vector3 screenPos = cam.WorldToScreenPoint(target.position + new Vector3(0f, 1.15f + bob, 0f));
            if (screenPos.z <= 0f) return;

            float guiY = Screen.height - screenPos.y;
            float width = 165f;
            float height = 28f;
            Rect r = new Rect(Mathf.Round(screenPos.x - width * 0.5f), Mathf.Round(guiY - height), width, height);

            GUI.color = new Color(0.08f, 0.06f, 0.11f, 0.95f);
            GUI.DrawTexture(r, boxTex);
            GUI.color = new Color(1f, 0.90f, 0.45f, 1f);
            GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), boxTex);
            GUI.color = new Color(0.08f, 0.06f, 0.11f, 1f);
            GUI.Label(r, label, markerStyle);
            GUI.color = Color.white;
        }

        private static string GetMarkerLabel(int stage)
        {
            switch (stage)
            {
                case 0: return "▼ PRESS [E]";
                case 1: return "▼ FLIP [F] & CROSS";
                case 2: return "▼ PRESS [E]";
                case 3: return "▼ PUSH BLOCK";
                default: return "▼ ENTER EXIT";
            }
        }

        private void EnsureStyles()
        {
            if (boxTex == null)
            {
                boxTex = new Texture2D(1, 1);
                boxTex.filterMode = FilterMode.Point;
                boxTex.SetPixel(0, 0, Color.white);
                boxTex.Apply();
            }

            if (markerStyle == null)
            {
                markerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
    }
}
