using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;
using UnityEngine.Events;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Floor button / pressure plate that activates when pressed by the player or a PushableBlock.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate : MonoBehaviour
    {
        public WorldAffinity acceptedForm = WorldAffinity.Both;
        public bool allowPushableBlocks = true;
        public bool latchOncePressed = false;

        [Header("Visuals")]
        public SpriteRenderer plateRenderer;
        public Sprite unpressedSprite;
        public Sprite pressedSprite;
        public Color idleColor = new Color(0.88f, 0.88f, 0.94f, 1f);
        public Color pressedColor = new Color(0.45f, 1f, 0.65f, 1f);

        public UnityEvent onPressed;
        public UnityEvent onReleased;

        public bool IsPressed { get; private set; }

        private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();
        private bool hasLatched = false;

        void Awake()
        {
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            if (plateRenderer == null) plateRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        void Update()
        {
            if (latchOncePressed && hasLatched) return;

            occupants.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy || !c.enabled);
            SetPressed(occupants.Count > 0);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsValidOccupant(other))
            {
                occupants.Add(other);
                SetPressed(occupants.Count > 0);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (latchOncePressed && hasLatched) return;

            if (occupants.Remove(other))
            {
                SetPressed(occupants.Count > 0);
            }
        }

        private bool IsValidOccupant(Collider2D other)
        {
            if (allowPushableBlocks && other.GetComponent<PushableBlock>() != null)
            {
                return true;
            }

            bool isLightForm = other.GetComponent<Light>() != null;
            bool isDarkForm = other.GetComponent<Dark>() != null;
            if (!isLightForm && !isDarkForm) return false;

            if (acceptedForm == WorldAffinity.Both) return true;
            if (acceptedForm == WorldAffinity.LightOnly) return isLightForm;
            if (acceptedForm == WorldAffinity.ShadowOnly) return isDarkForm;
            return false;
        }

        private void SetPressed(bool pressed)
        {
            if (latchOncePressed && pressed) hasLatched = true;
            if (latchOncePressed && hasLatched) pressed = true;

            if (IsPressed == pressed) return;
            IsPressed = pressed;
            UpdateVisuals();
            if (IsPressed)
            {
                PuzzleAudioManager.Instance?.PlayPlatePress();
                onPressed?.Invoke();
            }
            else
            {
                onReleased?.Invoke();
            }
        }

        private void UpdateVisuals()
        {
            if (plateRenderer == null) return;

            if (unpressedSprite != null && pressedSprite != null)
            {
                plateRenderer.sprite = IsPressed ? pressedSprite : unpressedSprite;
                plateRenderer.color = Color.white;
            }
            else
            {
                plateRenderer.color = IsPressed ? pressedColor : idleColor;
            }
        }
    }
}
