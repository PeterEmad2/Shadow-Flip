using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Simple frame-cycling sprite animator for pulsing Tesla bulbs, bubbling lab liquids, and glowing panels.
    /// </summary>
    public class LabAnimatedProp : MonoBehaviour
    {
        public Sprite[] frames;
        public float framesPerSecond = 5f;
        public float phaseOffset = 0f;

        private SpriteRenderer sr;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (sr == null || frames == null || frames.Length == 0 || framesPerSecond <= 0f) return;
            int idx = Mathf.FloorToInt((Time.time + phaseOffset) * framesPerSecond) % frames.Length;
            if (idx < 0) idx += frames.Length;
            sr.sprite = frames[idx];
        }
    }
}
