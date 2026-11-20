using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Cel-shaded Main Menu, Pause/Level Map, HUD, Umbra Danger Meter, and 3-Star Victory UI.
    /// Automatically attaches in any scene that lacks a PuzzleHUD.
    /// </summary>
    public class PuzzleHUD : MonoBehaviour
    {
        public PlayerMovement player;
        public logic gameLogic;

        [Header("Progression")]
        public int parFlips = 6;

        [TextArea]
        public string levelHint = "";

        [TextArea]
        public string lessonTitle = "";

        private static readonly string[] LevelScenes =
        {
            "OpticsPuzzle_Tutorial",
            "OpticsPuzzle",
            "OpticsPuzzle_Level2",
            "OpticsPuzzle_Level3",
            "OpticsPuzzle_Level4",
            "OpticsPuzzle_Level5",
            "OpticsPuzzle_Level6",
            "OpticsPuzzle_Level7",
            "OpticsPuzzle_Level8",
            "OpticsPuzzle_Level9",
            "OpticsPuzzle_Level10",
            "OpticsPuzzle_Level11",
            "OpticsPuzzle_Level12"
        };

        private static readonly string[] LevelDisplayNames =
        {
            "Lv 0 (Tutorial): Basics",
            "Lv 1: Chromatic Vault",
            "Lv 2: Eclipse Temple",
            "Lv 3: Prism Sanctum",
            "Lv 4: Umbra Labyrinth",
            "Lv 5: Flip-Clock Matrix",
            "Lv 6: Grandmaster Facility",
            "Lv 7: Quantum Laboratory",
            "Lv 8: Omega Containment",
            "Lv 9: Twin Observatory",
            "Lv 10: Polarized Rail",
            "Lv 11: Reality Anchor Rift",
            "Lv 12: Eclipse Paradox [BOSS]"
        };

        private static int GetCurrentLevelIndex()
        {
            string active = SceneManager.GetActiveScene().name;
            for (int i = 0; i < LevelScenes.Length; i++)
            {
                if (LevelScenes[i] == active) return i;
            }
            return -1;
        }

        private static bool hasShownInitialMainMenu = false;

        private bool levelComplete = false;
        private bool showMainMenu = false;
        private bool isDedicatedMenuScene = false;
        private int earnedStars = 1;
        private int finalFlips = 0;
        private float levelElapsedSeconds = 0f;
        private string menuNotice = "";

        private LightSensor[] cachedSensors;
        private PuzzleDoor[] cachedDoors;
        private PuzzleLever[] cachedLevers;
        private PressurePlate[] cachedPlates;
        private ShadowEchoPedestal[] cachedEchoes;
        private Texture2D whiteTex;
        private GUIStyle badgeStyle;
        private GUIStyle titleStyle;
        private GUIStyle promptStyle;
        private GUIStyle statusStyle;
        private GUIStyle bannerStyle;
        private GUIStyle hintStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            hasShownInitialMainMenu = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInScene()
        {
            if (FindFirstObjectByType<PuzzleHUD>() == null)
            {
                logic l = FindFirstObjectByType<logic>();
                GameObject host = l != null ? l.gameObject : new GameObject("PuzzleHUD");
                host.AddComponent<PuzzleHUD>();
            }
        }

        void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();

            cachedSensors = FindObjectsByType<LightSensor>(FindObjectsSortMode.None);
            cachedDoors = FindObjectsByType<PuzzleDoor>(FindObjectsSortMode.None);
            cachedLevers = FindObjectsByType<PuzzleLever>(FindObjectsSortMode.None);
            cachedPlates = FindObjectsByType<PressurePlate>(FindObjectsSortMode.None);
            cachedEchoes = FindObjectsByType<ShadowEchoPedestal>(FindObjectsSortMode.None);

            string activeScene = SceneManager.GetActiveScene().name;
            isDedicatedMenuScene = activeScene == "MainMenu";
            bool hadLegacyMenu = gameLogic != null && gameLogic.main_menu != null && gameLogic.main_menu.activeSelf;

            if (gameLogic != null && gameLogic.main_menu != null)
            {
                gameLogic.main_menu.SetActive(false);
            }

            if (!hasShownInitialMainMenu || isDedicatedMenuScene || hadLegacyMenu)
            {
                hasShownInitialMainMenu = true;
                OpenMainMenu();
            }
        }

        void Update()
        {
            if (showMainMenu && !isDedicatedMenuScene)
            {
                Time.timeScale = 0f;
            }
            else if (!isDedicatedMenuScene && !levelComplete && Time.timeScale > 0f)
            {
                levelElapsedSeconds += Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.M))
            {
                if (levelComplete) return;
                if (isDedicatedMenuScene)
                {
                    showMainMenu = true;
                    return;
                }

                if (showMainMenu) CloseMainMenu();
                else OpenMainMenu();
            }
        }

        public void OpenMainMenu()
        {
            showMainMenu = true;
            menuNotice = "";
            if (!isDedicatedMenuScene)
            {
                if (gameLogic != null) gameLogic.pause();
                else Time.timeScale = 0f;
            }
        }

        public void CloseMainMenu()
        {
            if (isDedicatedMenuScene) return;
            showMainMenu = false;
            menuNotice = "";
            if (gameLogic != null) gameLogic.Continue();
            else Time.timeScale = 1f;
        }

        public void ShowLevelComplete()
        {
            if (levelComplete) return;
            levelComplete = true;

            ShadowStalkerController stalker = ShadowStalkerController.Instance;
            finalFlips = stalker != null ? stalker.FlipCount : 0;
            bool withinPar = finalFlips <= parFlips;
            bool safeFromMonster = stalker == null || !stalker.HasMonsterSpawnedThisRun || stalker.MonsterWasPetrified;

            earnedStars = 1 + (withinPar ? 1 : 0) + (safeFromMonster ? 1 : 0);
            string sceneName = SceneManager.GetActiveScene().name;
            string key = GetStarsKey(sceneName);
            int best = PlayerPrefs.GetInt(key, 0);
            if (earnedStars > best)
            {
                PlayerPrefs.SetInt(key, earnedStars);
            }

            string timeKey = GetBestTimeKey(sceneName);
            float prevTime = PlayerPrefs.GetFloat(timeKey, -1f);
            if (prevTime < 0f || levelElapsedSeconds < prevTime)
            {
                PlayerPrefs.SetFloat(timeKey, levelElapsedSeconds);
            }
            PlayerPrefs.Save();
        }

        void OnGUI()
        {
            EnsureResources();

            bool isDark = gameLogic != null && gameLogic.isDarkActive;
            Color inkColor = new Color(0.078f, 0.063f, 0.11f, 0.96f);
            Color panelColor = isDark
                ? new Color(0.16f, 0.12f, 0.27f, 0.94f)
                : new Color(0.20f, 0.17f, 0.14f, 0.94f);
            Color accentColor = isDark
                ? new Color(0.486f, 0.322f, 1.0f, 1f)
                : new Color(0.961f, 0.882f, 0.643f, 1f);

            if (showMainMenu)
            {
                DrawMainMenuModal(panelColor, inkColor, accentColor);
                return;
            }

            if (Time.timeScale == 0f && !levelComplete) return;

            string worldLabel = isDark ? "SHADOW REALM" : "LIGHT REALM";

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

            // Top-Left Pixel Panel + [MENU] Button
            int currentLevelIdx = GetCurrentLevelIndex();
            string levelPrefix = currentLevelIdx >= 0 ? $"LEVEL {currentLevelIdx}  |  " : "";
            Rect topPanel = new Rect(16, 14, 620, 94);
            DrawCelPanel(topPanel, panelColor, inkColor, accentColor, 4f);

            GUI.color = accentColor;
            GUI.Label(new Rect(30, 22, 475, 28), levelPrefix + worldLabel, badgeStyle);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(520, 22, 102, 26), "MENU [Esc]"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                OpenMainMenu();
            }

            string doorText = totalDoors == 0
                ? "0/0"
                : $"{openDoors}/{totalDoors}";
            int curFlips = ShadowStalkerController.Instance != null ? ShadowStalkerController.Instance.FlipCount : 0;
            GUI.Label(new Rect(30, 52, 595, 22),
                $"Crystals: {activeSensors}/{totalSensors}  |  Gates: {doorText}  |  Time: {FormatTime(levelElapsedSeconds)}  |  Flips: {curFlips}/{parFlips}",
                statusStyle);
            GUI.Label(new Rect(30, 76, 595, 22), "[WASD] Move   [F] Flip   [E] Interact   [R] Reset   [Esc/Tab] Main Menu", promptStyle);

            DrawUmbraStalkerHUD(isDark, panelColor, inkColor);
            DrawEchoRecordingHUD(panelColor, inkColor);

            // Interact Prompt Pill
            if (player != null && player.NearbyInteractable != null)
            {
                string prompt = player.NearbyInteractable.GetInteractPrompt();
                if (!string.IsNullOrEmpty(prompt))
                {
                    Rect interactRect = new Rect(Screen.width * 0.5f - 175f, Screen.height - 134f, 350f, 36f);
                    DrawCelPanel(interactRect, inkColor, accentColor, accentColor, 3f);
                    GUI.color = accentColor;
                    GUI.Label(interactRect, prompt, bannerStyle);
                    GUI.color = Color.white;
                }
            }

            // Bottom-Center Tutorial / Level Hint Banner
            if (!string.IsNullOrEmpty(levelHint) || !string.IsNullOrEmpty(lessonTitle))
            {
                float w = Mathf.Min(820f, Screen.width - 40f);
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

            // Level Complete 3-Star + Speedrun Modal
            if (levelComplete)
            {
                Rect modalRect = new Rect(Screen.width * 0.5f - 230f, Screen.height * 0.5f - 88f, 460f, 176f);
                DrawCelPanel(modalRect, panelColor, inkColor, accentColor, 5f);
                GUI.color = accentColor;
                string solvedHeader = currentLevelIdx >= 0 ? $"LEVEL {currentLevelIdx} SOLVED!" : "PUZZLE SOLVED!";
                GUI.Label(new Rect(modalRect.x, modalRect.y + 14f, modalRect.width, 34f), solvedHeader, bannerStyle);

                string starStr = new string('★', earnedStars) + new string('☆', 3 - earnedStars);
                GUI.color = new Color(1f, 0.88f, 0.25f, 1f);
                GUI.Label(new Rect(modalRect.x, modalRect.y + 50f, modalRect.width, 32f), starStr, bannerStyle);
                GUI.color = Color.white;
                GUI.Label(new Rect(modalRect.x, modalRect.y + 88f, modalRect.width, 28f),
                    $"Time: {FormatTime(levelElapsedSeconds)}   |   Flips: {finalFlips} (Par {parFlips})", bannerStyle);
                GUI.Label(new Rect(modalRect.x, modalRect.y + 124f, modalRect.width, 26f),
                    "[R] Replay   |   Advancing to next realm...", promptStyle);
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

        private void DrawMainMenuModal(Color panelColor, Color inkColor, Color accentColor)
        {
            // Dim backdrop
            GUI.color = new Color(0.03f, 0.02f, 0.06f, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), whiteTex);
            GUI.color = Color.white;

            float w = 780f;
            float h = 560f;
            Rect modal = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            DrawCelPanel(modal, panelColor, inkColor, accentColor, 5f);

            int totalStars = 0;
            for (int i = 0; i < LevelScenes.Length; i++)
            {
                totalStars += PlayerPrefs.GetInt(GetStarsKey(LevelScenes[i]), 0);
            }

            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x, modal.y + 14f, modal.width, 38f), "SHADOW FLIP", titleStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(modal.x, modal.y + 52f, modal.width, 24f),
                $"PIXEL-ART LIGHT & UMBRA OPTICS PUZZLE   |   ★ {totalStars} / {LevelScenes.Length * 3}", bannerStyle);

            // Top Action Buttons
            string currentScene = SceneManager.GetActiveScene().name;
            string primaryLabel = isDedicatedMenuScene ? "▶ PLAY LEVEL 0 (TUTORIAL)" : "▶ RESUME LEVEL";

            if (GUI.Button(new Rect(modal.x + 24f, modal.y + 86f, 358f, 38f), primaryLabel))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                if (isDedicatedMenuScene) TryLoadLevel("OpticsPuzzle_Tutorial");
                else CloseMainMenu();
            }

            if (GUI.Button(new Rect(modal.x + 398f, modal.y + 86f, 358f, 38f), isDedicatedMenuScene ? "▶ PLAY LEVEL 1" : "↻ RESTART LEVEL"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                if (isDedicatedMenuScene)
                {
                    TryLoadLevel("OpticsPuzzle");
                }
                else
                {
                    showMainMenu = false;
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }

            // 13-Level (0-12) 2-Column Select Grid + Sandbox Button
            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x + 24f, modal.y + 132f, 500f, 22f), "SELECT LEVEL (0 - 12) OR SANDBOX:", statusStyle);
            GUI.color = Color.white;

            for (int i = 0; i < LevelScenes.Length; i++)
            {
                string sceneName = LevelScenes[i];
                int stars = PlayerPrefs.GetInt(GetStarsKey(sceneName), 0);
                float bestTime = PlayerPrefs.GetFloat(GetBestTimeKey(sceneName), -1f);
                string starStr = new string('★', stars) + new string('☆', 3 - stars);
                string timeTag = bestTime > 0f ? $" {FormatTime(bestTime)}" : "";
                string activeTag = sceneName == currentScene ? " *" : "";

                int col = i < 7 ? 0 : 1;
                int row = i < 7 ? i : i - 7;
                float bx = modal.x + (col == 0 ? 24f : 398f);
                float by = modal.y + 158f + row * 41f;
                Rect btnRect = new Rect(bx, by, 358f, 35f);
                if (GUI.Button(btnRect, $" {LevelDisplayNames[i]}{activeTag}  [{starStr}]{timeTag}"))
                {
                    PuzzleAudioManager.Instance?.PlayButtonClick();
                    TryLoadLevel(sceneName);
                }
            }

            // 7th slot in right column: Sandbox Level Editor
            Rect sandboxRect = new Rect(modal.x + 398f, modal.y + 158f + 6 * 41f, 358f, 35f);
            string sandboxTag = currentScene == "OpticsPuzzle_Sandbox" ? " [ACTIVE]" : "";
            if (GUI.Button(sandboxRect, $" ⚙ SANDBOX LEVEL EDITOR{sandboxTag}"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                TryLoadLevel("OpticsPuzzle_Sandbox");
            }

            if (!string.IsNullOrEmpty(menuNotice))
            {
                GUI.color = new Color(1f, 0.55f, 0.45f, 1f);
                GUI.Label(new Rect(modal.x + 24f, modal.y + 458f, modal.width - 48f, 26f), menuNotice, statusStyle);
                GUI.color = Color.white;
            }

            // Footer Buttons
            if (GUI.Button(new Rect(modal.x + 24f, modal.y + h - 42f, 200f, 30f), "Reset Saved Stars & Times"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                for (int i = 0; i < LevelScenes.Length; i++)
                {
                    PlayerPrefs.DeleteKey(GetStarsKey(LevelScenes[i]));
                    PlayerPrefs.DeleteKey(GetBestTimeKey(LevelScenes[i]));
                }
                PlayerPrefs.Save();
                menuNotice = "All saved stars & best times cleared.";
            }

            if (GUI.Button(new Rect(modal.x + modal.width - 184f, modal.y + h - 42f, 160f, 30f), "Quit Game"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                Application.Quit();
            }
        }

        private void TryLoadLevel(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                menuNotice = $"Scene '{sceneName}' not in Build Settings yet. Run: Shadow Flip -> Create All Levels.";
                return;
            }

            showMainMenu = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        private void DrawUmbraStalkerHUD(bool isDark, Color panelColor, Color inkColor)
        {
            ShadowStalkerController stalker = ShadowStalkerController.Instance;
            if (stalker == null) return;
            if (!isDark && stalker.UmbraProgress <= 0.01f && !stalker.IsMonsterActive) return;

            float progress = stalker.UmbraProgress;
            Color violet = new Color(0.58f, 0.35f, 1.0f, 1f);
            Color crimson = new Color(0.96f, 0.22f, 0.36f, 1f);
            Color safeGold = new Color(1f, 0.88f, 0.42f, 1f);
            Color barColor = stalker.IsInSanctuary ? safeGold : Color.Lerp(violet, crimson, progress);
            if (stalker.IsMonsterActive)
            {
                float pulse = 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 10f);
                barColor = Color.Lerp(crimson, Color.white, pulse * 0.35f);
            }

            Rect dangerPanel = new Rect(Screen.width - 336f, 14f, 320f, 64f);
            DrawCelPanel(dangerPanel, panelColor, inkColor, barColor, 4f);

            string label = stalker.IsMonsterActive
                ? (stalker.IsSlowedByLaser ? "LASER PETRIFYING UMBRA!" : "⚠ UMBRA STALKER HUNTING! FLIP [F]!")
                : (stalker.IsInSanctuary ? "SAFE IN SANCTUARY (DRAINING)" : $"UMBRA STALKER AWAKENS: {stalker.RemainingSeconds:0.0}s");

            GUI.color = stalker.IsMonsterActive || stalker.IsInSanctuary ? barColor : Color.white;
            GUI.Label(new Rect(dangerPanel.x + 14f, dangerPanel.y + 10f, dangerPanel.width - 28f, 22f), label, statusStyle);

            Rect track = new Rect(dangerPanel.x + 14f, dangerPanel.y + 38f, dangerPanel.width - 28f, 14f);
            GUI.color = inkColor;
            GUI.DrawTexture(track, whiteTex);
            GUI.color = new Color(0.14f, 0.1f, 0.22f, 1f);
            GUI.DrawTexture(new Rect(track.x + 2f, track.y + 2f, track.width - 4f, track.height - 4f), whiteTex);

            float fillRatio = stalker.IsMonsterActive ? (stalker.IsSlowedByLaser ? stalker.PetrifyProgress : 1f) : progress;
            float fillWidth = Mathf.Floor(((track.width - 4f) * fillRatio) / 4f) * 4f;
            if (fillWidth > 0.5f)
            {
                GUI.color = barColor;
                GUI.DrawTexture(new Rect(track.x + 2f, track.y + 2f, fillWidth, track.height - 4f), whiteTex);
            }
            GUI.color = Color.white;
        }

        private void DrawEchoRecordingHUD(Color panelColor, Color inkColor)
        {
            if (cachedEchoes == null) return;
            for (int i = 0; i < cachedEchoes.Length; i++)
            {
                ShadowEchoPedestal echo = cachedEchoes[i];
                if (echo != null && echo.IsRecording)
                {
                    Color cyan = new Color(0.35f, 0.92f, 1f, 1f);
                    Rect recRect = new Rect(Screen.width - 336f, 86f, 320f, 44f);
                    DrawCelPanel(recRect, panelColor, inkColor, cyan, 4f);
                    GUI.color = cyan;
                    GUI.Label(new Rect(recRect.x + 14f, recRect.y + 10f, recRect.width - 28f, 24f),
                        $"RECORDING LIGHT ECHO: {echo.RecordRemaining:0.0}s", statusStyle);
                    GUI.color = Color.white;
                    break;
                }
            }
        }

        private static string GetStarsKey(string sceneName) => "ShadowFlip_Stars_" + sceneName;
        private static string GetBestTimeKey(string sceneName) => "ShadowFlip_Time_" + sceneName;

        private static string FormatTime(float seconds)
        {
            float clamped = Mathf.Max(0f, seconds);
            int mins = Mathf.FloorToInt(clamped / 60f);
            float secs = clamped - mins * 60f;
            return $"{mins:00}:{secs:00.0}";
        }

        private void DrawCelPanel(Rect rect, Color fill, Color border, Color topStripe, float borderWidth)
        {
            float px = Mathf.Round(rect.x);
            float py = Mathf.Round(rect.y);
            float pw = Mathf.Round(rect.width);
            float ph = Mathf.Round(rect.height);
            float b = Mathf.Max(4f, Mathf.Round(borderWidth));

            // Stepped pixel-art drop shadow (notched corners)
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(px + b * 2f, py + b, pw - b * 2f, ph), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + b * 2f, pw, ph - b * 2f), whiteTex);

            // Pixel-beveled outer ink frame (missing 4 outer corner squares)
            GUI.color = border;
            GUI.DrawTexture(new Rect(px + b, py, pw - b * 2f, ph), whiteTex);
            GUI.DrawTexture(new Rect(px, py + b, pw, ph - b * 2f), whiteTex);

            // Inner panel fill
            GUI.color = fill;
            GUI.DrawTexture(new Rect(px + b, py + b, pw - b * 2f, ph - b * 2f), whiteTex);

            // Crisp 4px accent header stripe + 4x4 corner rivets
            GUI.color = topStripe;
            GUI.DrawTexture(new Rect(px + b * 2f, py + b, pw - b * 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + pw - b - 4f, py + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + ph - b - 4f, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + pw - b - 4f, py + ph - b - 4f, 4f, 4f), whiteTex);

            GUI.color = Color.white;
        }

        private void EnsureResources()
        {
            if (whiteTex == null)
            {
                whiteTex = new Texture2D(1, 1);
                whiteTex.filterMode = FilterMode.Point;
                whiteTex.SetPixel(0, 0, Color.white);
                whiteTex.Apply();
            }

            if (badgeStyle == null)
            {
                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 30,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
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
                    fontSize = 18,
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
