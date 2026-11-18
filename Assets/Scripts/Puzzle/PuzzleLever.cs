using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.Events;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Interactive dungeon lever pulled with [E]. Can open doors, toggle barriers, and remotely rotate mirrors.
    /// </summary>
    public class PuzzleLever : MonoBehaviour, IInteractable
    {
        public bool isActivated = false;
        public WorldAffinity usableInWorld = WorldAffinity.Both;

        [Header("Visuals")]
        public SpriteRenderer leverRenderer;
        public Sprite leverOffSprite;
        public Sprite leverOnSprite;

        [Header("Linked Mechanisms")]
        public OpticalMirror[] controlledMirrors;
        public GameObject[] toggledObjects;

        public UnityEvent onToggled;

        void Awake()
        {
            if (leverRenderer == null) leverRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        public void Interact(PlayerMovement player)
        {
            if (!CanUseInCurrentWorld()) return;

            isActivated = !isActivated;
            UpdateVisuals();
            PuzzleAudioManager.Instance?.PlayLeverPull();

            if (controlledMirrors != null)
            {
                foreach (OpticalMirror mirror in controlledMirrors)
                {
                    if (mirror != null) mirror.RotateStep();
                }
            }

            if (toggledObjects != null)
            {
                foreach (GameObject obj in toggledObjects)
                {
                    if (obj != null) obj.SetActive(!obj.activeSelf);
                }
            }

            onToggled?.Invoke();
        }

        public string GetInteractPrompt()
        {
            if (!CanUseInCurrentWorld())
            {
                return usableInWorld == WorldAffinity.LightOnly
                    ? "Pull in Light Realm only"
                    : "Pull in Shadow Realm only";
            }
            return "[E] Pull Lever";
        }

        private bool CanUseInCurrentWorld()
        {
            if (usableInWorld == WorldAffinity.Both) return true;
            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool isDark = l != null && l.isDarkActive;
            return (usableInWorld == WorldAffinity.ShadowOnly && isDark)
                || (usableInWorld == WorldAffinity.LightOnly && !isDark);
        }

        private void UpdateVisuals()
        {
            if (leverRenderer == null) return;
            if (leverOffSprite != null && leverOnSprite != null)
            {
                leverRenderer.sprite = isActivated ? leverOnSprite : leverOffSprite;
                leverRenderer.color = Color.white;
            }
            else
            {
                leverRenderer.color = isActivated
                    ? new Color(0.45f, 1f, 0.65f, 1f)
                    : new Color(0.75f, 0.75f, 0.8f, 1f);
            }
        }
    }
}
