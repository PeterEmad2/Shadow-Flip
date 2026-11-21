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

        void Awake()
        {
            // Enforce continuous hold-only behavior on all pressure plates.
            latchOncePressed = false;
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
            if (plateRenderer == null) plateRenderer = GetComponent<SpriteRenderer>();
            UpdateVisuals();
        }

        void Update()
        {
            occupants.RemoveWhere(c => c == null || !c.gameObject.activeInHierarchy || !c.enabled || !IsValidOccupant(c));
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

        void OnTriggerStay2D(Collider2D other)
        {
            if (IsValidOccupant(other))
            {
                if (occupants.Add(other)) SetPressed(occupants.Count > 0);
            }
            else
            {
                if (occupants.Remove(other)) SetPressed(occupants.Count > 0);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (occupants.Remove(other))
            {
                SetPressed(occupants.Count > 0);
            }
        }

        private bool IsValidOccupant(Collider2D other)
        {
            if (other == null) return false;
            if (allowPushableBlocks && other.GetComponent<PushableBlock>() != null)
            {
                return true;
            }

            logic l = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            bool globalDark = l != null && l.isDarkActive;
            bool effectiveDark = SplitRealmZone.GetEffectiveDarkState(transform.position, globalDark);

            bool isMirrorTwin = other.GetComponent<MirrorTwinBody>() != null;
            if (isMirrorTwin)
            {
                return acceptedForm == WorldAffinity.Both
                    || (acceptedForm == WorldAffinity.LightOnly && (!effectiveDark || RealityAnchorLantern.IsPositionAnchoredInLight(transform.position)))
                    || (acceptedForm == WorldAffinity.ShadowOnly && effectiveDark);
            }

            bool isLightForm = other.GetComponent<Light>() != null;
            bool isDarkForm = other.GetComponent<Dark>() != null;
            if (!isLightForm && !isDarkForm) return false;

            if (acceptedForm == WorldAffinity.Both) return true;
            if (acceptedForm == WorldAffinity.LightOnly)
            {
                return !effectiveDark || isLightForm || RealityAnchorLantern.IsPositionAnchoredInLight(transform.position);
            }
            if (acceptedForm == WorldAffinity.ShadowOnly)
            {
                return effectiveDark || isDarkForm;
            }
            return false;
        }

        private void SetPressed(bool pressed)
        {
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
