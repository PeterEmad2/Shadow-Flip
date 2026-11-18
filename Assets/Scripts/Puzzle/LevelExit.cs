using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Level goal trigger. Completes the puzzle when reached by the player and all requirements are met.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        [Header("Requirements (Optional)")]
        public PuzzleDoor[] requiredDoors;
        public LightSensor[] requiredSensors;

        [Header("Progression")]
        public string nextSceneName = "";
        public float loadDelay = 1.5f;

        public bool IsCompleted { get; private set; }

        void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCompleted) return;
            if (other.GetComponent<Light>() == null && other.GetComponent<Dark>() == null) return;
            if (!AreRequirementsMet()) return;

            IsCompleted = true;
            PuzzleAudioManager.Instance?.PlayLevelWin();
            PuzzleHUD hud = FindFirstObjectByType<PuzzleHUD>();
            if (hud != null)
            {
                hud.ShowLevelComplete();
            }

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Invoke(nameof(LoadNextScene), loadDelay);
            }
        }

        private bool AreRequirementsMet()
        {
            if (requiredDoors != null)
            {
                foreach (PuzzleDoor door in requiredDoors)
                {
                    if (door != null && !door.IsOpen) return false;
                }
            }

            if (requiredSensors != null)
            {
                foreach (LightSensor sensor in requiredSensors)
                {
                    if (sensor != null && !sensor.IsActivated) return false;
                }
            }

            return true;
        }

        private void LoadNextScene()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
