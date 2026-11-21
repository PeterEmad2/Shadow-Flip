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
            "OpticsPuzzle_Level12",
            "OpticsPuzzle_Level13"
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
            "Lv 12: Eclipse Paradox [BOSS]",
            "Lv 13: Event Horizon [BLACK HOLE]"
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
        private bool showControlsHelper = false;
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

            if (Input.GetKeyDown(KeyCode.Q) && !showMainMenu && !levelComplete && !PixelSandboxEditor.IsInBuildMode)
            {
                showControlsHelper = !showControlsHelper;
                PuzzleAudioManager.Instance?.PlayButtonClick();
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

            // Top-Left Pixel Panel + [MENU] Button (yields neatly below Top-of-Screen NPC Dialogue Banner)
            int currentLevelIdx = GetCurrentLevelIndex();
            string levelPrefix = currentLevelIdx >= 0 ? $"LEVEL {currentLevelIdx}  |  " : "";
            float topY = LabNPCController.IsAnyDialogueActive ? 116f : 14f;
            Rect topPanel = new Rect(16, topY, 620, 94);
            DrawCelPanel(topPanel, panelColor, inkColor, accentColor, 4f);

            GUI.color = accentColor;
            GUI.Label(new Rect(30, topY + 8f, 475, 28), levelPrefix + worldLabel, badgeStyle);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(520, topY + 8f, 102, 26), "MENU [Esc]"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                OpenMainMenu();
            }

            string doorText = totalDoors == 0
                ? "0/0"
                : $"{openDoors}/{totalDoors}";
            int curFlips = ShadowStalkerController.Instance != null ? ShadowStalkerController.Instance.FlipCount : 0;
            GUI.Label(new Rect(30, topY + 38f, 595, 22),
                $"Crystals: {activeSensors}/{totalSensors}  |  Gates: {doorText}  |  Time: {FormatTime(levelElapsedSeconds)}  |  Flips: {curFlips}/{parFlips}",
                statusStyle);
            GUI.Label(new Rect(30, topY + 62f, 595, 22), "[WASD] Move   [F] Flip   [E] Interact   [Q] Help   [R] Reset   [Esc] Menu", promptStyle);

            DrawUmbraStalkerHUD(isDark, panelColor, inkColor, topY);
            DrawEchoRecordingHUD(panelColor, inkColor, topY);

            // Interact Prompt Pill (blocks manual interaction in Umbra Mode)
            if (player != null && player.NearbyInteractable != null)
            {
                string rawPrompt = player.NearbyInteractable.GetInteractPrompt();
                if (!string.IsNullOrEmpty(rawPrompt))
                {
                    bool lockedInDark = player.IsDarkModeActive;
                    string shown = lockedInDark ? "Light Mode Required to Interact ([F] Flip)" : rawPrompt;
                    Color pillCol = lockedInDark ? new Color(1f, 0.72f, 0.35f, 1f) : accentColor;
                    Rect interactRect = new Rect(Screen.width * 0.5f - 205f, Screen.height - 134f, 410f, 36f);
                    DrawCelPanel(interactRect, inkColor, pillCol, pillCol, 3f);
                    GUI.color = pillCol;
                    GUI.Label(interactRect, shown, bannerStyle);
                    GUI.color = Color.white;
                }
            }

            // [Q] Toggle Controls & Interaction Helper Overlay
            if (showControlsHelper)
            {
                DrawControlsHelperOverlay(inkColor, accentColor, isDark);
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

            float w = Mathf.Min(780f, Screen.width - 20f);
            float h = Mathf.Min(548f, Screen.height - 16f);
            Rect modal = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            DrawCelPanel(modal, panelColor, inkColor, accentColor, 5f);

            int totalStars = 0;
            for (int i = 0; i < LevelScenes.Length; i++)
            {
                totalStars += PlayerPrefs.GetInt(GetStarsKey(LevelScenes[i]), 0);
            }

            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x, modal.y + 12f, modal.width, 34f), "SHADOW FLIP", titleStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(modal.x, modal.y + 46f, modal.width, 22f),
                $"PIXEL-ART LIGHT & UMBRA OPTICS PUZZLE   |   ★ {totalStars} / {LevelScenes.Length * 3}", bannerStyle);

            // Top Action Buttons
            string currentScene = SceneManager.GetActiveScene().name;
            string primaryLabel = isDedicatedMenuScene ? "▶ PLAY LEVEL 0 (TUTORIAL)" : "▶ RESUME LEVEL";
            float halfW = (modal.width - 64f) * 0.5f;

            if (GUI.Button(new Rect(modal.x + 24f, modal.y + 76f, halfW, 34f), primaryLabel))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                if (isDedicatedMenuScene) TryLoadLevel("OpticsPuzzle_Tutorial");
                else CloseMainMenu();
            }

            if (GUI.Button(new Rect(modal.x + modal.width - 24f - halfW, modal.y + 76f, halfW, 34f), isDedicatedMenuScene ? "▶ PLAY LEVEL 1" : "↻ RESTART LEVEL"))
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

            // 14-Level (0-13) + Sandbox 2-Column Select Grid (8 left, 7 right)
            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x + 24f, modal.y + 118f, modal.width - 48f, 20f), "SELECT LEVEL (0 - 13) OR SANDBOX:", statusStyle);
            GUI.color = Color.white;

            float gridTop = modal.y + 142f;
            float rowStep = Mathf.Min(36f, (modal.height - 196f) / 8f);
            float btnH = Mathf.Max(26f, rowStep - 5f);

            for (int i = 0; i < LevelScenes.Length; i++)
            {
                string sceneName = LevelScenes[i];
                int stars = PlayerPrefs.GetInt(GetStarsKey(sceneName), 0);
                float bestTime = PlayerPrefs.GetFloat(GetBestTimeKey(sceneName), -1f);
                string starStr = new string('★', stars) + new string('☆', 3 - stars);
                string timeTag = bestTime > 0f ? $" {FormatTime(bestTime)}" : "";
                string activeTag = sceneName == currentScene ? " *" : "";

                int col = i < 8 ? 0 : 1;
                int row = i < 8 ? i : i - 8;
                float bx = col == 0 ? modal.x + 24f : modal.x + modal.width - 24f - halfW;
                float by = gridTop + row * rowStep;
                Rect btnRect = new Rect(bx, by, halfW, btnH);
                if (GUI.Button(btnRect, $" {LevelDisplayNames[i]}{activeTag}  [{starStr}]{timeTag}"))
                {
                    PuzzleAudioManager.Instance?.PlayButtonClick();
                    TryLoadLevel(sceneName);
                }
            }

            // Right column slot 7 (row 6): Sandbox Level Editor
            Rect sandboxRect = new Rect(modal.x + modal.width - 24f - halfW, gridTop + 6 * rowStep, halfW, btnH);
            string sandboxTag = currentScene == "OpticsPuzzle_Sandbox" ? " [ACTIVE]" : "";
            if (GUI.Button(sandboxRect, $" ⚙ SANDBOX LEVEL EDITOR{sandboxTag}"))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                TryLoadLevel("OpticsPuzzle_Sandbox");
            }

            if (!string.IsNullOrEmpty(menuNotice))
            {
                GUI.color = new Color(1f, 0.55f, 0.45f, 1f);
                GUI.Label(new Rect(modal.x + 24f, modal.y + h - 72f, modal.width - 48f, 24f), menuNotice, statusStyle);
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

        private void DrawUmbraStalkerHUD(bool isDark, Color panelColor, Color inkColor, float topY = 14f)
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

            Rect dangerPanel = new Rect(Screen.width - 336f, topY, 320f, 64f);
            DrawCelPanel(dangerPanel, panelColor, inkColor, barColor, 4f);

            string label = stalker.IsMonsterActive
                ? (stalker.IsSlowedByLaser ? "LASER PETRIFYING UMBRA!" : "⚠ UMBRA STALKER HUNTING! FLIP [F]!")
                : (stalker.IsInSanctuary ? "SAFE IN TESLA ZONE (DRAINING)" : $"UMBRA STALKER AWAKENS: {stalker.RemainingSeconds:0.0}s");

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

        private void DrawEchoRecordingHUD(Color panelColor, Color inkColor, float topY = 14f)
        {
            if (cachedEchoes == null) return;
            for (int i = 0; i < cachedEchoes.Length; i++)
            {
                ShadowEchoPedestal echo = cachedEchoes[i];
                if (echo != null && echo.IsRecording)
                {
                    Color cyan = new Color(0.35f, 0.92f, 1f, 1f);
                    Rect recRect = new Rect(Screen.width - 336f, topY + 72f, 320f, 44f);
                    DrawCelPanel(recRect, panelColor, inkColor, cyan, 4f);
                    GUI.color = cyan;
                    GUI.Label(new Rect(recRect.x + 14f, recRect.y + 10f, recRect.width - 28f, 24f),
                        $"RECORDING LIGHT ECHO: {echo.RecordRemaining:0.0}s", statusStyle);
                    GUI.color = Color.white;
                    break;
                }
            }
        }

        private void DrawControlsHelperOverlay(Color inkColor, Color accentColor, bool isDark)
        {
            float w = Mathf.Min(560f, Screen.width - 40f);
            float h = 252f;
            float x = Mathf.Round((Screen.width - w) * 0.5f);
            float y = Mathf.Round((Screen.height - h) * 0.5f);
            Rect box = new Rect(x, y, w, h);

            Color translucentDark = isDark
                ? new Color(0.08f, 0.05f, 0.16f, 0.90f)
                : new Color(0.06f, 0.08f, 0.12f, 0.90f);
            DrawCelPanel(box, translucentDark, inkColor, accentColor, 4f);

            GUI.color = accentColor;
            GUI.Label(new Rect(box.x + 16f, box.y + 10f, box.width - 32f, 28f),
                "CONTROLS & INTERACTION HELPER   [Q to Close]", bannerStyle);
            GUI.color = Color.white;

            GUI.Label(new Rect(box.x + 22f, box.y + 44f, box.width - 44f, 22f), "[WASD / Arrows]   Move Jimmy", statusStyle);
            GUI.Label(new Rect(box.x + 22f, box.y + 68f, box.width - 44f, 22f), "[F]   Flip Light Realm <-> Umbra (Dark) Realm", statusStyle);
            GUI.Label(new Rect(box.x + 22f, box.y + 92f, box.width - 44f, 22f), "[E]   Interact (Mirrors, Levers, NPCs — Light Mode Only)", statusStyle);
            GUI.Label(new Rect(box.x + 22f, box.y + 116f, box.width - 44f, 22f), "[R]   Reset Level     |     [Esc / Tab / M]   Main Menu", statusStyle);
            GUI.Label(new Rect(box.x + 22f, box.y + 140f, box.width - 44f, 22f), "[Q]   Toggle this Helper", statusStyle);

            GUI.color = accentColor;
            GUI.DrawTexture(new Rect(box.x + 18f, box.y + 168f, box.width - 36f, 2f), whiteTex);
            GUI.Label(new Rect(box.x + 22f, box.y + 176f, box.width - 44f, 22f), "CURRENT CONTEXT INTERACTION:", statusStyle);

            string contextLine;
            Color contextCol;
            if (player != null && player.NearbyInteractable != null)
            {
                string prompt = player.NearbyInteractable.GetInteractPrompt();
                if (string.IsNullOrEmpty(prompt))
                {
                    contextLine = "Nearby object has no manual interaction right now.";
                    contextCol = new Color(0.82f, 0.85f, 0.90f, 1f);
                }
                else if (player.IsDarkModeActive)
                {
                    contextLine = $"LOCKED IN UMBRA MODE — Press [F] to return to Light Mode.\nAvailable in Light Mode: {prompt}";
                    contextCol = new Color(1f, 0.72f, 0.35f, 1f);
                }
                else
                {
                    contextLine = $"Ready: {prompt}";
                    contextCol = new Color(0.45f, 1f, 0.72f, 1f);
                }
            }
            else
            {
                contextLine = isDark
                    ? "Umbra Mode: pushing blocks & [E] interactions are frozen. Phase past Light Walls or charge Capacitors!"
                    : "Walk near a mirror, lever, polarizer, NPC, or pedestal to see its [E] action.";
                contextCol = Color.white;
            }

            GUI.color = contextCol;
            GUI.Label(new Rect(box.x + 22f, box.y + 198f, box.width - 44f, 46f), contextLine, hintStyle);
            GUI.color = Color.white;
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
