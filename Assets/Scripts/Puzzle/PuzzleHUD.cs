using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Cel-shaded UI HUD displaying the active realm, sensor/door progress, controls,
    /// interaction prompts, and bottom tutorial/level hint banner.
    /// </summary>
    public class PuzzleHUD : MonoBehaviour
    {
        public PlayerMovement player;
        public logic gameLogic;

        [TextArea]
        public string levelHint = "";

        [TextArea]
        public string lessonTitle = "";

        private bool levelComplete = false;
        private LightSensor[] cachedSensors;
        private PuzzleDoor[] cachedDoors;
        private PuzzleLever[] cachedLevers;
        private PressurePlate[] cachedPlates;
        private Texture2D whiteTex;
        private GUIStyle badgeStyle;
        private GUIStyle promptStyle;
        private GUIStyle statusStyle;
        private GUIStyle bannerStyle;
        private GUIStyle hintStyle;

        void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            cachedSensors = FindObjectsByType<LightSensor>(FindObjectsSortMode.None);
            cachedDoors = FindObjectsByType<PuzzleDoor>(FindObjectsSortMode.None);
            cachedLevers = FindObjectsByType<PuzzleLever>(FindObjectsSortMode.None);
            cachedPlates = FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
        }

        public void ShowLevelComplete()
        {
            levelComplete = true;
        }

        void OnGUI()
        {
            if (Time.timeScale == 0f && !levelComplete) return;
            EnsureResources();

            bool isDark = gameLogic != null && gameLogic.isDarkActive;
            string worldLabel = isDark ? "SHADOW REALM" : "LIGHT REALM";
            Color inkColor = new Color(0.078f, 0.063f, 0.11f, 0.96f);     // #14101C
            Color panelColor = isDark
                ? new Color(0.16f, 0.12f, 0.27f, 0.92f)
                : new Color(0.20f, 0.17f, 0.14f, 0.92f);
            Color accentColor = isDark
                ? new Color(0.486f, 0.322f, 1.0f, 1f)                      // #7C52FF
                : new Color(0.961f, 0.882f, 0.643f, 1f);                   // #F5E1A4

            int totalSensors = cachedSensors != null ? cachedSensors.Length : 0;
            int activeSensors = 0;
            if (cachedSensors != null)
            {
                for (int i = 0; i < cachedSensors.Length; i++)
                {
                    if (cachedSensors[i] != null && cachedSensors[i].IsActivated) activeSensors++;
                }
            }

            int totalDoors = cachedDoors != null ? cachedDoors.Length : 0;
            int openDoors = 0;
            if (cachedDoors != null)
            {
                for (int i = 0; i < cachedDoors.Length; i++)
                {
                    if (cachedDoors[i] != null && cachedDoors[i].IsOpen) openDoors++;
                }
            }

            int totalSwitches = (cachedLevers != null ? cachedLevers.Length : 0)
                              + (cachedPlates != null ? cachedPlates.Length : 0);
            int activeSwitches = 0;
            if (cachedLevers != null)
            {
                for (int i = 0; i < cachedLevers.Length; i++)
                {
                    if (cachedLevers[i] != null && cachedLevers[i].isActivated) activeSwitches++;
                }
            }
            if (cachedPlates != null)
            {
                for (int i = 0; i < cachedPlates.Length; i++)
                {
                    if (cachedPlates[i] != null && cachedPlates[i].IsPressed) activeSwitches++;
                }
            }

            // Top-Left Cel-Shaded Panel
            Rect topPanel = new Rect(16, 14, 580, 94);
            DrawCelPanel(topPanel, panelColor, inkColor, accentColor, 4f);

            GUI.color = accentColor;
            GUI.Label(new Rect(30, 22, 320, 28), worldLabel, badgeStyle);
            GUI.color = Color.white;

            string doorText = totalDoors == 0
                ? "No Gates"
                : (openDoors == totalDoors ? "All Open" : $"{openDoors}/{totalDoors} Open");
            GUI.Label(new Rect(30, 52, 550, 22), $"Crystals: {activeSensors}/{totalSensors}   |   Levers/Buttons: {activeSwitches}/{totalSwitches}   |   Gates: {doorText}", statusStyle);
            GUI.Label(new Rect(30, 76, 550, 22), "[WASD] Move   [F] Flip Realm   [E] Interact   [R] Reset", promptStyle);

            DrawUmbraStalkerHUD(isDark, panelColor, inkColor);

            // Interact Prompt Pill
            if (player != null && player.NearbyInteractable != null)
            {
                string prompt = player.NearbyInteractable.GetInteractPrompt();
                if (!string.IsNullOrEmpty(prompt))
                {
                    Rect interactRect = new Rect(Screen.width * 0.5f - 145f, Screen.height - 134f, 290f, 36f);
                    DrawCelPanel(interactRect, inkColor, accentColor, accentColor, 3f);
                    GUI.color = accentColor;
                    GUI.Label(interactRect, prompt, bannerStyle);
                    GUI.color = Color.white;
                }
            }

            // Bottom-Center Tutorial / Level Hint Banner
            if (!string.IsNullOrEmpty(levelHint) || !string.IsNullOrEmpty(lessonTitle))
            {
                float w = Mathf.Min(780f, Screen.width - 40f);
                Rect hintRect = new Rect((Screen.width - w) * 0.5f, Screen.height - 86f, w, 72f);
                DrawCelPanel(hintRect, panelColor, inkColor, accentColor, 4f);

                if (!string.IsNullOrEmpty(lessonTitle))
                {
                    GUI.color = accentColor;
                    GUI.Label(new Rect(hintRect.x + 16f, hintRect.y + 8f, hintRect.width - 32f, 22f), lessonTitle, statusStyle);
                    GUI.color = Color.white;
                    GUI.Label(new Rect(hintRect.x + 16f, hintRect.y + 30f, hintRect.width - 32f, 36f), levelHint, hintStyle);
                }
                else
                {
                    GUI.Label(new Rect(hintRect.x + 16f, hintRect.y + 14f, hintRect.width - 32f, 44f), levelHint, hintStyle);
                }
            }

            // Level Complete Modal
            if (levelComplete)
            {
                Rect modalRect = new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 62f, 400f, 124f);
                DrawCelPanel(modalRect, panelColor, inkColor, accentColor, 5f);
                GUI.color = accentColor;
                GUI.Label(new Rect(modalRect.x, modalRect.y + 18f, modalRect.width, 40f), "PUZZLE SOLVED!", bannerStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(modalRect.x, modalRect.y + 64f, modalRect.width, 32f), "Advancing... (or press [R] to replay)", hintStyle);
            }

            if (ShadowStalkerController.Instance != null && ShadowStalkerController.Instance.IsPlayerCaught)
            {
                Color crimson = new Color(0.95f, 0.22f, 0.35f, 1f);
                Rect caughtRect = new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 54f, 400f, 108f);
                DrawCelPanel(caughtRect, panelColor, inkColor, crimson, 5f);
                GUI.color = crimson;
                GUI.Label(new Rect(caughtRect.x, caughtRect.y + 20f, caughtRect.width, 36f), "CAUGHT BY THE UMBRA!", bannerStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(caughtRect.x, caughtRect.y + 60f, caughtRect.width, 28f), "Resetting realm...", bannerStyle);
            }
        }

        private void DrawUmbraStalkerHUD(bool isDark, Color panelColor, Color inkColor)
        {
            ShadowStalkerController stalker = ShadowStalkerController.Instance;
            if (stalker == null) return;
            if (!isDark && stalker.UmbraProgress <= 0.01f && !stalker.IsMonsterActive) return;

            float progress = stalker.UmbraProgress;
            Color violet = new Color(0.58f, 0.35f, 1.0f, 1f);
            Color crimson = new Color(0.96f, 0.22f, 0.36f, 1f);
            Color barColor = Color.Lerp(violet, crimson, progress);
            if (stalker.IsMonsterActive)
            {
                float pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 10f);
                barColor = Color.Lerp(crimson, Color.white, pulse * 0.35f);
            }

            Rect dangerPanel = new Rect(Screen.width - 336f, 14f, 320f, 64f);
            DrawCelPanel(dangerPanel, panelColor, inkColor, barColor, 4f);

            string label = stalker.IsMonsterActive
                ? "⚠ UMBRA STALKER HUNTING! FLIP [F]!"
                : $"UMBRA STALKER AWAKENS: {stalker.RemainingSeconds:0.0}s";

            GUI.color = stalker.IsMonsterActive ? barColor : Color.white;
            GUI.Label(new Rect(dangerPanel.x + 14f, dangerPanel.y + 10f, dangerPanel.width - 28f, 22f), label, statusStyle);

            // Progress bar track + fill
            Rect track = new Rect(dangerPanel.x + 14f, dangerPanel.y + 38f, dangerPanel.width - 28f, 14f);
            GUI.color = inkColor;
            GUI.DrawTexture(track, whiteTex);
            GUI.color = new Color(0.14f, 0.1f, 0.22f, 1f);
            GUI.DrawTexture(new Rect(track.x + 2f, track.y + 2f, track.width - 4f, track.height - 4f), whiteTex);

            float fillWidth = (track.width - 4f) * (stalker.IsMonsterActive ? 1f : progress);
            if (fillWidth > 0.5f)
            {
                GUI.color = barColor;
                GUI.DrawTexture(new Rect(track.x + 2f, track.y + 2f, fillWidth, track.height - 4f), whiteTex);
            }
            GUI.color = Color.white;
        }

        private void DrawCelPanel(Rect rect, Color fill, Color border, Color topStripe, float borderWidth)
        {
            // Hard drop shadow for cel-shaded comic depth
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(rect.x + 4f, rect.y + 4f, rect.width, rect.height), whiteTex);

            GUI.color = border;
            GUI.DrawTexture(rect, whiteTex);

            GUI.color = fill;
            GUI.DrawTexture(new Rect(rect.x + borderWidth, rect.y + borderWidth, rect.width - borderWidth * 2f, rect.height - borderWidth * 2f), whiteTex);

            GUI.color = topStripe;
            GUI.DrawTexture(new Rect(rect.x + borderWidth, rect.y + borderWidth, rect.width - borderWidth * 2f, 4f), whiteTex);

            GUI.color = Color.white;
        }

        private void EnsureResources()
        {
            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }

            if (badgeStyle == null)
            {
                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold
                };
            }

            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (statusStyle == null)
            {
                statusStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (bannerStyle == null)
            {
                bannerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (hintStyle == null)
            {
                hintStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }
        }
    }
}
