using System.Collections.Generic;
using System.IO;
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

        private static readonly string[] BuiltInLevelScenes =
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

        private static readonly string[] BuiltInLevelDisplayNames =
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

        private static readonly List<string> ActiveLevelScenes = new List<string>(24);
        private static readonly List<string> ActiveLevelDisplayNames = new List<string>(24);

        private static void RefreshLevelCatalog()
        {
            ActiveLevelScenes.Clear();
            ActiveLevelDisplayNames.Clear();

            for (int i = 0; i < BuiltInLevelScenes.Length; i++)
            {
                ActiveLevelScenes.Add(BuiltInLevelScenes[i]);
                ActiveLevelDisplayNames.Add(BuiltInLevelDisplayNames[i]);
            }

            int buildCount = SceneManager.sceneCountInBuildSettings;
            for (int i = 0; i < buildCount; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                if (string.IsNullOrEmpty(scenePath)) continue;

                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (string.IsNullOrEmpty(sceneName)
                    || sceneName == "MainMenu"
                    || sceneName == "OpticsPuzzle_Sandbox"
                    || ActiveLevelScenes.Contains(sceneName))
                {
                    continue;
                }

                ActiveLevelScenes.Add(sceneName);
                ActiveLevelDisplayNames.Add($"Lv {ActiveLevelScenes.Count - 1}: {sceneName}");
            }
        }

        private static int GetCurrentLevelIndex()
        {
            if (ActiveLevelScenes.Count == 0) RefreshLevelCatalog();
            string active = SceneManager.GetActiveScene().name;
            for (int i = 0; i < ActiveLevelScenes.Count; i++)
            {
                if (ActiveLevelScenes[i] == active) return i;
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
        private Vector2 menuScrollPos;

        private int lastStatSensors = -1;
        private int lastStatTotalSensors = -1;
        private int lastStatDoors = -1;
        private int lastStatTotalDoors = -1;
        private int lastStatFlips = -1;
        private int lastStatTenths = -1;
        private string cachedStatsText = "";
        private readonly GUIContent tempGuiContent = new GUIContent();

        private LightSensor[] cachedSensors;
        private PuzzleDoor[] cachedDoors;
        private ShadowEchoPedestal[] cachedEchoes;
        private Texture2D whiteTex;
        private Texture2D btnNormalTex;
        private Texture2D btnHoverTex;
        private Texture2D btnActiveTex;
        private Texture2D btnPrimaryTex;
        private Texture2D btnPrimaryHoverTex;
        private GUIStyle badgeStyle;
        private GUIStyle realmPillStyle;
        private GUIStyle titleStyle;
        private GUIStyle promptStyle;
        private GUIStyle statusStyle;
        private GUIStyle bannerStyle;
        private GUIStyle hintStyle;
        private GUIStyle helperTextStyle;
        private GUIStyle buttonStyle;
        private GUIStyle primaryButtonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            hasShownInitialMainMenu = false;
            ActiveLevelScenes.Clear();
            ActiveLevelDisplayNames.Clear();
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
            RefreshLevelCatalog();
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();

            cachedSensors = FindObjectsByType<LightSensor>(FindObjectsSortMode.None);
            cachedDoors = FindObjectsByType<PuzzleDoor>(FindObjectsSortMode.None);
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

            bool hasNearby = player != null && player.NearbyInteractable != null;
            if (!hasNearby)
            {
                showControlsHelper = false;
            }

            if (Input.GetKeyDown(KeyCode.Q) && !showMainMenu && !levelComplete && !PixelSandboxEditor.IsInBuildMode)
            {
                if (hasNearby)
                {
                    showControlsHelper = !showControlsHelper;
                    PuzzleAudioManager.Instance?.PlayButtonClick();
                }
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
            Color inkColor = new Color(0.06f, 0.05f, 0.10f, 0.96f);
            Color panelColor = isDark
                ? new Color(0.13f, 0.09f, 0.24f, 0.92f)
                : new Color(0.16f, 0.14f, 0.13f, 0.92f);
            Color accentColor = isDark
                ? new Color(0.62f, 0.44f, 1.0f, 1f)
                : new Color(0.961f, 0.882f, 0.56f, 1f);

            if (showMainMenu)
            {
                DrawMainMenuModal(panelColor, inkColor, accentColor);
                return;
            }

            if (Time.timeScale == 0f && !levelComplete) return;

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

            // Compact Top-Left HUD Card (yields neatly below the bigger Top-of-Screen NPC Dialogue Banner)
            int currentLevelIdx = GetCurrentLevelIndex();
            string levelLabel = currentLevelIdx >= 0 ? $"LEVEL {currentLevelIdx}" : "SANDBOX";
            string realmPillText = isDark ? "● UMBRA" : "● LIGHT";
            float topY = LabNPCController.IsAnyDialogueActive ? LabNPCController.ActiveBannerBottomY : 14f;
            Rect topPanel = new Rect(16f, topY, 450f, 88f);
            DrawCelPanel(topPanel, panelColor, inkColor, accentColor, 4f);

            // Realm Pill Badge
            Rect realmPill = new Rect(28f, topY + 11f, 92f, 22f);
            GUI.color = inkColor;
            GUI.DrawTexture(realmPill, whiteTex);
            GUI.color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.24f);
            GUI.DrawTexture(new Rect(realmPill.x + 2f, realmPill.y + 2f, realmPill.width - 4f, realmPill.height - 4f), whiteTex);
            GUI.color = accentColor;
            GUI.Label(realmPill, realmPillText, realmPillStyle);

            // Level Title + Custom Styled MENU Button
            GUI.color = Color.white;
            GUI.Label(new Rect(128f, topY + 8f, 200f, 26f), levelLabel, badgeStyle);

            if (GUI.Button(new Rect(348f, topY + 9f, 104f, 24f), "MENU [Esc]", buttonStyle))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                OpenMainMenu();
            }

            // Clean Spaced Stats Row + Subtle Footer (rebuild text only on state or tenth-of-second change)
            int curFlips = ShadowStalkerController.Instance != null ? ShadowStalkerController.Instance.FlipCount : 0;
            int tenths = Mathf.FloorToInt(Mathf.Max(0f, levelElapsedSeconds) * 10f);
            if (activeSensors != lastStatSensors || totalSensors != lastStatTotalSensors
                || openDoors != lastStatDoors || totalDoors != lastStatTotalDoors
                || curFlips != lastStatFlips || tenths != lastStatTenths)
            {
                lastStatSensors = activeSensors;
                lastStatTotalSensors = totalSensors;
                lastStatDoors = openDoors;
                lastStatTotalDoors = totalDoors;
                lastStatFlips = curFlips;
                lastStatTenths = tenths;
                string doorText = totalDoors == 0 ? "0/0" : $"{openDoors}/{totalDoors}";
                cachedStatsText = $"Crystals {activeSensors}/{totalSensors}   ·   Gates {doorText}   ·   {FormatTime(levelElapsedSeconds)}   ·   Flips {curFlips}/{parFlips}";
            }
            GUI.color = new Color(0.95f, 0.96f, 0.99f, 1f);
            GUI.Label(new Rect(28f, topY + 38f, 424f, 22f), cachedStatsText, statusStyle);

            GUI.color = new Color(0.76f, 0.80f, 0.88f, 0.88f);
            GUI.Label(new Rect(28f, topY + 60f, 424f, 20f),
                "[F] Flip   ·   [E] Interact   ·   [Q] Nearby Help   ·   [R] Reset",
                promptStyle);
            GUI.color = Color.white;

            DrawUmbraStalkerHUD(isDark, panelColor, inkColor, topY);
            DrawEchoRecordingHUD(panelColor, inkColor, topY);

            // Top-of-Screen [Q] Nearby Interaction Helper (shown ONLY when Q is pressed while close to an interactable)
            if (showControlsHelper && player != null && player.NearbyInteractable != null)
            {
                DrawNearbyHelperBanner(panelColor, inkColor, accentColor, topY);
            }

            // Bottom-Center Translucent Tutorial / Level Hint Banner
            if (!string.IsNullOrEmpty(levelHint) || !string.IsNullOrEmpty(lessonTitle))
            {
                float w = Mathf.Min(820f, Screen.width - 40f);
                Rect hintRect = new Rect(Mathf.Round((Screen.width - w) * 0.5f), Screen.height - 84f, w, 70f);
                Color hintBackdrop = isDark
                    ? new Color(0.10f, 0.07f, 0.19f, 0.86f)
                    : new Color(0.12f, 0.11f, 0.10f, 0.86f);
                DrawCelPanel(hintRect, hintBackdrop, inkColor, accentColor, 4f);

                if (!string.IsNullOrEmpty(lessonTitle))
                {
                    DrawShadowedLabel(new Rect(hintRect.x + 16f, hintRect.y + 8f, hintRect.width - 32f, 22f), lessonTitle, statusStyle, accentColor);
                    DrawShadowedLabel(new Rect(hintRect.x + 16f, hintRect.y + 30f, hintRect.width - 32f, 34f), levelHint, hintStyle, Color.white);
                }
                else
                {
                    DrawShadowedLabel(new Rect(hintRect.x + 16f, hintRect.y + 14f, hintRect.width - 32f, 42f), levelHint, hintStyle, Color.white);
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

            if (ActiveLevelScenes.Count == 0) RefreshLevelCatalog();

            int totalStars = 0;
            for (int i = 0; i < ActiveLevelScenes.Count; i++)
            {
                totalStars += PlayerPrefs.GetInt(GetStarsKey(ActiveLevelScenes[i]), 0);
            }

            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x, modal.y + 12f, modal.width, 34f), "SHADOW FLIP", titleStyle);
            GUI.color = Color.white;
            GUI.Label(new Rect(modal.x, modal.y + 46f, modal.width, 22f),
                $"PIXEL-ART LIGHT & UMBRA OPTICS PUZZLE   |   ★ {totalStars} / {ActiveLevelScenes.Count * 3}", bannerStyle);

            // Top Action Buttons
            string currentScene = SceneManager.GetActiveScene().name;
            string primaryLabel = isDedicatedMenuScene ? "▶ PLAY LEVEL 0 (TUTORIAL)" : "▶ RESUME LEVEL";
            float halfW = (modal.width - 64f) * 0.5f;

            if (GUI.Button(new Rect(modal.x + 24f, modal.y + 76f, halfW, 34f), primaryLabel, primaryButtonStyle))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                if (isDedicatedMenuScene) TryLoadLevel("OpticsPuzzle_Tutorial");
                else CloseMainMenu();
            }

            if (GUI.Button(new Rect(modal.x + modal.width - 24f - halfW, modal.y + 76f, halfW, 34f), isDedicatedMenuScene ? "▶ PLAY LEVEL 1" : "↻ RESTART LEVEL", primaryButtonStyle))
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

            // Dynamic Scrollable 2-Column Level Select Grid (all built-in + custom scenes) + Sandbox
            int maxIndex = Mathf.Max(0, ActiveLevelScenes.Count - 1);
            GUI.color = accentColor;
            GUI.Label(new Rect(modal.x + 24f, modal.y + 118f, modal.width - 48f, 20f), $"SELECT LEVEL (0 - {maxIndex}) OR SANDBOX:", statusStyle);
            GUI.color = Color.white;

            float gridTop = modal.y + 142f;
            float availH = modal.height - 196f;
            int totalSlots = ActiveLevelScenes.Count + 1; // +1 for Sandbox
            int rowsPerCol = Mathf.Max(1, Mathf.CeilToInt(totalSlots / 2f));
            float rowStep = Mathf.Max(31f, Mathf.Min(36f, availH / rowsPerCol));
            float btnH = Mathf.Max(26f, rowStep - 5f);
            float contentH = rowsPerCol * rowStep;
            bool needsScroll = contentH > availH + 1f;
            float colW = needsScroll ? (modal.width - 80f) * 0.5f : halfW;

            Rect scrollOuter = new Rect(modal.x + 24f, gridTop, modal.width - 48f, availH);
            Rect scrollView = new Rect(0f, 0f, modal.width - (needsScroll ? 64f : 48f), contentH);
            menuScrollPos = GUI.BeginScrollView(scrollOuter, menuScrollPos, scrollView, false, needsScroll);

            for (int i = 0; i < ActiveLevelScenes.Count; i++)
            {
                string sceneName = ActiveLevelScenes[i];
                int stars = PlayerPrefs.GetInt(GetStarsKey(sceneName), 0);
                float bestTime = PlayerPrefs.GetFloat(GetBestTimeKey(sceneName), -1f);
                string starStr = new string('★', stars) + new string('☆', 3 - stars);
                string timeTag = bestTime > 0f ? $" {FormatTime(bestTime)}" : "";
                string activeTag = sceneName == currentScene ? " *" : "";

                int col = i < rowsPerCol ? 0 : 1;
                int row = i < rowsPerCol ? i : i - rowsPerCol;
                float bx = col == 0 ? 0f : scrollView.width - colW;
                float by = row * rowStep;
                Rect btnRect = new Rect(bx, by, colW, btnH);
                if (GUI.Button(btnRect, $" {ActiveLevelDisplayNames[i]}{activeTag}  [{starStr}]{timeTag}", buttonStyle))
                {
                    PuzzleAudioManager.Instance?.PlayButtonClick();
                    TryLoadLevel(sceneName);
                }
            }

            // Final slot: Sandbox Level Editor
            int sCol = ActiveLevelScenes.Count < rowsPerCol ? 0 : 1;
            int sRow = ActiveLevelScenes.Count < rowsPerCol ? ActiveLevelScenes.Count : ActiveLevelScenes.Count - rowsPerCol;
            Rect sandboxRect = new Rect(sCol == 0 ? 0f : scrollView.width - colW, sRow * rowStep, colW, btnH);
            string sandboxTag = currentScene == "OpticsPuzzle_Sandbox" ? " [ACTIVE]" : "";
            if (GUI.Button(sandboxRect, $" ⚙ SANDBOX LEVEL EDITOR{sandboxTag}", buttonStyle))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                TryLoadLevel("OpticsPuzzle_Sandbox");
            }

            GUI.EndScrollView();

            if (!string.IsNullOrEmpty(menuNotice))
            {
                GUI.color = new Color(1f, 0.55f, 0.45f, 1f);
                GUI.Label(new Rect(modal.x + 24f, modal.y + h - 72f, modal.width - 48f, 24f), menuNotice, statusStyle);
                GUI.color = Color.white;
            }

            // Footer Buttons
            if (GUI.Button(new Rect(modal.x + 24f, modal.y + h - 42f, 212f, 30f), "Reset Saved Stars & Times", buttonStyle))
            {
                PuzzleAudioManager.Instance?.PlayButtonClick();
                for (int i = 0; i < ActiveLevelScenes.Count; i++)
                {
                    PlayerPrefs.DeleteKey(GetStarsKey(ActiveLevelScenes[i]));
                    PlayerPrefs.DeleteKey(GetBestTimeKey(ActiveLevelScenes[i]));
                }
                PlayerPrefs.Save();
                menuNotice = "All saved stars & best times cleared.";
            }

            if (GUI.Button(new Rect(modal.x + modal.width - 184f, modal.y + h - 42f, 160f, 30f), "Quit Game", buttonStyle))
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

        private void DrawNearbyHelperBanner(Color panelColor, Color inkColor, Color accentColor, float topY)
        {
            if (player == null || player.NearbyInteractable == null) return;
            string rawPrompt = player.NearbyInteractable.GetInteractPrompt();
            bool allowed = player.IsInteractableAllowedInCurrentRealm(player.NearbyInteractable, out string lockMessage);
            if (!allowed && string.IsNullOrEmpty(lockMessage) && string.IsNullOrEmpty(rawPrompt)) return;
            if (allowed && string.IsNullOrEmpty(rawPrompt)) return;

            bool showLocked = !allowed && !string.IsNullOrEmpty(lockMessage);
            string shown = showLocked ? lockMessage : rawPrompt;
            Color pillCol = showLocked ? new Color(1f, 0.72f, 0.35f, 1f) : accentColor;

            tempGuiContent.text = shown;
            float textWidth = helperTextStyle.CalcSize(tempGuiContent).x;
            float width = Mathf.Clamp(textWidth + 92f, 340f, 560f);
            float height = 48f;

            float helperX = Mathf.Round((Screen.width - width) * 0.5f);
            float helperY;
            float freeLeft = 478f;
            float freeRight = Screen.width - 348f;
            if (Screen.width >= 1220f && (freeRight - freeLeft) >= width)
            {
                helperY = topY;
                helperX = Mathf.Round(Mathf.Clamp(helperX, freeLeft, freeRight - width));
            }
            else
            {
                helperY = topY + 98f;
            }

            Rect box = new Rect(helperX, helperY, width, height);
            DrawCelPanel(box, panelColor, inkColor, pillCol, 4f);

            Rect qPill = new Rect(box.x + 12f, box.y + 11f, 38f, 26f);
            GUI.color = inkColor;
            GUI.DrawTexture(qPill, whiteTex);
            GUI.color = new Color(pillCol.r, pillCol.g, pillCol.b, 0.25f);
            GUI.DrawTexture(new Rect(qPill.x + 2f, qPill.y + 2f, qPill.width - 4f, qPill.height - 4f), whiteTex);
            DrawShadowedLabel(qPill, "[Q]", realmPillStyle, pillCol);

            Rect textRect = new Rect(box.x + 58f, box.y + 6f, box.width - 70f, box.height - 12f);
            DrawShadowedLabel(textRect, shown, helperTextStyle, pillCol);
        }

        private void DrawShadowedLabel(Rect rect, string text, GUIStyle style, Color textColor)
        {
            PixelGuiUtility.DrawShadowedLabel(rect, text, style, textColor);
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
            PixelGuiUtility.DrawPixelPanel(rect, whiteTex, fill, border, topStripe, borderWidth);
        }

        void OnDestroy()
        {
            if (whiteTex != null) Destroy(whiteTex);
            if (btnNormalTex != null) Destroy(btnNormalTex);
            if (btnHoverTex != null) Destroy(btnHoverTex);
            if (btnActiveTex != null) Destroy(btnActiveTex);
            if (btnPrimaryTex != null) Destroy(btnPrimaryTex);
            if (btnPrimaryHoverTex != null) Destroy(btnPrimaryHoverTex);
        }

        private void EnsureResources()
        {
            if (whiteTex == null) whiteTex = PixelGuiUtility.MakeSolidTexture(Color.white);
            if (btnNormalTex == null) btnNormalTex = PixelGuiUtility.MakeSolidTexture(new Color(0.16f, 0.14f, 0.25f, 0.96f));
            if (btnHoverTex == null) btnHoverTex = PixelGuiUtility.MakeSolidTexture(new Color(0.26f, 0.22f, 0.40f, 0.98f));
            if (btnActiveTex == null) btnActiveTex = PixelGuiUtility.MakeSolidTexture(new Color(0.36f, 0.28f, 0.56f, 1f));
            if (btnPrimaryTex == null) btnPrimaryTex = PixelGuiUtility.MakeSolidTexture(new Color(0.27f, 0.21f, 0.46f, 0.98f));
            if (btnPrimaryHoverTex == null) btnPrimaryHoverTex = PixelGuiUtility.MakeSolidTexture(new Color(0.39f, 0.29f, 0.66f, 1f));

            if (badgeStyle == null)
            {
                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (realmPillStyle == null)
            {
                realmPillStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
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
                    fontSize = 12,
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
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                };
            }

            if (helperTextStyle == null)
            {
                helperTextStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 17,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    clipping = TextClipping.Clip
                };
                buttonStyle.normal.background = btnNormalTex;
                buttonStyle.normal.textColor = new Color(0.92f, 0.94f, 0.99f, 1f);
                buttonStyle.hover.background = btnHoverTex;
                buttonStyle.hover.textColor = new Color(1f, 0.92f, 0.65f, 1f);
                buttonStyle.active.background = btnActiveTex;
                buttonStyle.active.textColor = Color.white;
                buttonStyle.border = new RectOffset(2, 2, 2, 2);
                buttonStyle.padding = new RectOffset(8, 8, 2, 2);
            }

            if (primaryButtonStyle == null)
            {
                primaryButtonStyle = new GUIStyle(buttonStyle)
                {
                    fontSize = 14
                };
                primaryButtonStyle.normal.background = btnPrimaryTex;
                primaryButtonStyle.normal.textColor = new Color(1f, 0.92f, 0.65f, 1f);
                primaryButtonStyle.hover.background = btnPrimaryHoverTex;
                primaryButtonStyle.hover.textColor = Color.white;
            }
        }
    }
}
