using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Dungeon gate that opens when connected LightSensors, PressurePlates (buttons), and PuzzleLevers are active.
    /// </summary>
    public class PuzzleDoor : MonoBehaviour
    {
        [Header("Inputs")]
        public LightSensor[] requiredSensors;
        public PressurePlate[] requiredPlates;
        public PuzzleLever[] requiredLevers;
        public bool requireAll = true;

        [Header("Behavior & Cel-Shaded Visuals")]
        public bool stayOpenOnceUnlocked = false;
        public float fadeSpeed = 8f;
        public Sprite closedSprite;
        public Sprite openSprite;

        private Collider2D doorCollider;
        private SpriteRenderer doorRenderer;
        private Color closedColor;
        private bool isOpen = false;
        private bool hasUnlocked = false;

        public bool IsOpen => isOpen;

        void Awake()
        {
            doorCollider = GetComponent<Collider2D>();
            doorRenderer = GetComponent<SpriteRenderer>();
            if (doorRenderer != null)
            {
                closedColor = doorRenderer.color;
                if (closedSprite != null) doorRenderer.sprite = closedSprite;
            }
        }

        void Update()
        {
            bool conditionMet = CheckCondition();
            if (stayOpenOnceUnlocked && conditionMet)
            {
                hasUnlocked = true;
            }

            bool nextOpen = stayOpenOnceUnlocked ? hasUnlocked : conditionMet;
            if (nextOpen != isOpen)
            {
                isOpen = nextOpen;
                if (doorCollider != null) doorCollider.enabled = !isOpen;
                if (isOpen) PuzzleAudioManager.Instance?.PlayDoorOpen();
            }

            if (doorRenderer != null)
            {
                if (closedSprite != null && openSprite != null)
                {
                    doorRenderer.sprite = isOpen ? openSprite : closedSprite;
                    doorRenderer.color = closedColor;
                }
                else
                {
                    Color target = closedColor;
                    target.a = isOpen ? 0.15f : closedColor.a;
                    doorRenderer.color = Color.Lerp(doorRenderer.color, target, Time.deltaTime * fadeSpeed);
                }
            }
        }

        private bool CheckCondition()
        {
            int totalInputs = (requiredSensors != null ? requiredSensors.Length : 0)
                            + (requiredPlates != null ? requiredPlates.Length : 0)
                            + (requiredLevers != null ? requiredLevers.Length : 0);
            if (totalInputs == 0) return false;

            int activeCount = 0;

            if (requiredSensors != null)
            {
                foreach (LightSensor sensor in requiredSensors)
                {
                    if (sensor != null && sensor.IsActivated) activeCount++;
                }
            }

            if (requiredPlates != null)
            {
                foreach (PressurePlate plate in requiredPlates)
                {
                    if (plate != null && plate.IsPressed) activeCount++;
                }
            }

            if (requiredLevers != null)
            {
                foreach (PuzzleLever lever in requiredLevers)
                {
                    if (lever != null && lever.isActivated) activeCount++;
                }
            }

            return requireAll ? activeCount == totalInputs : activeCount > 0;
        }
    }
}
