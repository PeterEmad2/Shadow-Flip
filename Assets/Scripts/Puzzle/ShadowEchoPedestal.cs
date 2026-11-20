using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Echo Pedestal: press [E] to record up to 5 seconds of movement.
    /// Flipping realms or finishing the timer spawns a Light Echo Clone that
    /// replays the path and holds down LightOnly / Both floor buttons.
    /// </summary>
    public class ShadowEchoPedestal : MonoBehaviour, IInteractable
    {
        public float recordDuration = 5.0f;
        public float sampleInterval = 0.04f;
        public Sprite echoSprite;

        public bool IsRecording { get; private set; }
        public bool IsPlayingEcho { get; private set; }
        public float RecordRemaining => Mathf.Max(0f, recordDuration - recordElapsed);

        private readonly List<Vector2> recordedPositions = new List<Vector2>();
        private float recordElapsed = 0f;
        private float sampleTimer = 0f;
        private float playElapsed = 0f;

        private PlayerMovement recordingPlayer;
        private logic cachedLogic;
        private GameObject echoClone;
        private SpriteRenderer echoRenderer;

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
            }
        }

        void OnDestroy()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
        }

        void Update()
        {
            if (Time.timeScale == 0f) return;

            if (IsRecording)
            {
                recordElapsed += Time.deltaTime;
                sampleTimer += Time.deltaTime;

                if (sampleTimer >= sampleInterval && recordingPlayer != null)
                {
                    sampleTimer = 0f;
                    recordedPositions.Add(recordingPlayer.GetActiveFormTransform().position);
                }

                if (recordElapsed >= recordDuration)
                {
                    FinishRecordingAndSpawnEcho();
                }
            }
            else if (IsPlayingEcho && echoClone != null && recordedPositions.Count > 0)
            {
                playElapsed += Time.deltaTime;
                int idx = Mathf.Min(recordedPositions.Count - 1, Mathf.FloorToInt(playElapsed / sampleInterval));
                echoClone.transform.position = new Vector3(recordedPositions[idx].x, recordedPositions[idx].y, 0f);
            }
        }

        public void Interact(PlayerMovement player)
        {
            recordingPlayer = player;
            recordedPositions.Clear();
            recordElapsed = 0f;
            sampleTimer = 0f;
            IsRecording = true;
            IsPlayingEcho = false;

            if (echoClone != null) echoClone.SetActive(false);

            Transform origin = player.GetActiveFormTransform();
            if (origin != null) recordedPositions.Add(origin.position);

            PuzzleAudioManager.Instance?.PlayEchoRecord();
        }

        public string GetInteractPrompt()
        {
            return IsRecording ? $"Recording Echo ({RecordRemaining:0.0}s)..." : "[E] Record Light Echo";
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (IsRecording && recordedPositions.Count > 1)
            {
                FinishRecordingAndSpawnEcho();
            }
        }

        private void FinishRecordingAndSpawnEcho()
        {
            IsRecording = false;
            if (recordedPositions.Count == 0) return;

            EnsureEchoClone();
            playElapsed = 0f;
            IsPlayingEcho = true;
            echoClone.transform.position = new Vector3(recordedPositions[0].x, recordedPositions[0].y, 0f);
            echoClone.SetActive(true);
            PuzzleAudioManager.Instance?.PlayEchoRecord();
        }

        private void EnsureEchoClone()
        {
            if (echoClone != null) return;

            echoClone = new GameObject("ShadowEchoClone");
            Vector3 cloneScale = new Vector3(0.45f, 0.45f, 1f);
            if (recordingPlayer != null)
            {
                Transform form = recordingPlayer.GetActiveFormTransform();
                if (form != null && form.localScale.x > 0.05f) cloneScale = form.localScale;
            }
            echoClone.transform.localScale = cloneScale;

            echoRenderer = echoClone.AddComponent<SpriteRenderer>();
            if (echoSprite == null && recordingPlayer != null)
            {
                echoSprite = recordingPlayer.LightspriteDown;
            }
            echoRenderer.sprite = echoSprite;
            echoRenderer.color = new Color(0.55f, 0.95f, 1f, 0.72f);
            echoRenderer.sortingOrder = 9;

            Rigidbody2D rb = echoClone.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            CircleCollider2D col = echoClone.AddComponent<CircleCollider2D>();
            col.radius = 0.66f;
            col.isTrigger = true;

            // Attach Light component (with Jimmy=null) so PressurePlate treats the Echo as a Light form.
            echoClone.AddComponent<Light>();
        }
    }
}
