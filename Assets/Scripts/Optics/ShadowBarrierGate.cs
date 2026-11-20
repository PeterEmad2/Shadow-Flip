using ShadowFlip.Puzzle;
using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Forcefield barrier gate controlled directly by an incoming laser beam.
    /// Parent trigger collider receives the laser ray while a child solid collider blocks the player.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ShadowBarrierGate : MonoBehaviour, IOpticalElement
    {
        public bool openWhenLit = true;
        public bool stayOpenOnceLit = false;
        public LightColor requiredColor = LightColor.White;
        public bool acceptAnyColor = true;
        public float fadeSpeed = 10f;

        public bool IsOpen { get; private set; }

        private int lastLitFrame = -100;
        private bool hasOpened = false;
        private BoxCollider2D blockerCollider;
        private SpriteRenderer gateRenderer;
        private Color baseColor = Color.white;

        void Awake()
        {
            BoxCollider2D rayTrigger = GetComponent<BoxCollider2D>();
            rayTrigger.isTrigger = true;

            gateRenderer = GetComponent<SpriteRenderer>();
            if (gateRenderer != null) baseColor = gateRenderer.color;

            GameObject blockerGo = new GameObject("PhysicalBlocker");
            blockerGo.transform.SetParent(transform, false);
            blockerGo.layer = 2; // Ignore Raycast
            blockerCollider = blockerGo.AddComponent<BoxCollider2D>();
            blockerCollider.size = rayTrigger.size;
            blockerCollider.offset = rayTrigger.offset;
            blockerCollider.isTrigger = false;

            ApplyState(!openWhenLit, playAudio: false);
        }

        void Update()
        {
            bool isLit = (Time.frameCount - lastLitFrame) <= 1;
            if (stayOpenOnceLit && isLit)
            {
                hasOpened = true;
            }
            bool nextOpen = (stayOpenOnceLit && hasOpened) || (openWhenLit ? isLit : !isLit);

            if (nextOpen != IsOpen)
            {
                ApplyState(nextOpen, playAudio: Time.timeSinceLevelLoad > 0.1f);
            }

            if (gateRenderer != null)
            {
                Color target = baseColor;
                target.a = IsOpen ? 0.22f : baseColor.a;
                gateRenderer.color = Color.Lerp(gateRenderer.color, target, Time.deltaTime * fadeSpeed);
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            if (LightColorUtility.Matches(hit.Color, requiredColor, acceptAnyColor))
            {
                lastLitFrame = Time.frameCount;
            }
        }

        private void ApplyState(bool open, bool playAudio)
        {
            IsOpen = open;
            if (blockerCollider != null)
            {
                blockerCollider.enabled = !IsOpen;
            }
            if (playAudio)
            {
                PuzzleAudioManager.Instance?.PlayBarrierState();
            }
        }
    }
}
