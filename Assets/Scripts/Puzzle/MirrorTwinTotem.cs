using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Marker component attached to the spawned mirrored twin character so PressurePlates recognize it.
    /// </summary>
    public class MirrorTwinBody : MonoBehaviour
    {
    }

    /// <summary>
    /// Dual-Soul Split Totem: press [E] to manifest or dismiss a mirrored twin across mirrorAxisX
    /// that mirrors Jimmy's movement horizontally and can hold floor buttons across a gap.
    /// </summary>
    public class MirrorTwinTotem : MonoBehaviour, IInteractable
    {
        public float mirrorAxisX = 0f;
        public Sprite twinSprite;

        public bool IsTwinActive => twinGo != null && twinGo.activeSelf;

        private GameObject twinGo;
        private Rigidbody2D twinRb;
        private PlayerMovement activePlayer;
        private logic cachedLogic;

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
            if (twinGo != null) Destroy(twinGo);
        }

        void FixedUpdate()
        {
            if (!IsTwinActive || twinRb == null || activePlayer == null || Time.timeScale == 0f)
            {
                if (twinRb != null) twinRb.linearVelocity = Vector2.zero;
                return;
            }

            Vector3 pPos = activePlayer.GetActiveFormTransform().position;
            Vector2 desired = new Vector2(2f * mirrorAxisX - pPos.x, pPos.y);
            Vector2 delta = desired - twinRb.position;
            if (delta.sqrMagnitude < 0.002f)
            {
                twinRb.linearVelocity = Vector2.zero;
            }
            else
            {
                Vector2 vel = delta / Mathf.Max(Time.fixedDeltaTime, 0.01f);
                const float maxTwinSpeed = 12f;
                if (vel.magnitude > maxTwinSpeed) vel = vel.normalized * maxTwinSpeed;
                twinRb.linearVelocity = vel;
            }
        }

        public void Interact(PlayerMovement player)
        {
            activePlayer = player;
            if (IsTwinActive)
            {
                DismissTwin();
            }
            else
            {
                SpawnTwin(player);
            }
        }

        public string GetInteractPrompt()
        {
            return IsTwinActive ? "[E] Merge Mirror Twin" : "[E] Split Mirror Twin";
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (IsTwinActive)
            {
                DismissTwin();
            }
        }

        private void SpawnTwin(PlayerMovement player)
        {
            Vector3 pPos = player != null ? player.GetActiveFormTransform().position : transform.position;
            Vector3 spawnPos = new Vector3(2f * mirrorAxisX - pPos.x, pPos.y, 0f);

            if (twinGo == null)
            {
                twinGo = new GameObject("MirrorTwinCharacter");
                twinGo.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                SpriteRenderer sr = twinGo.AddComponent<SpriteRenderer>();
                sr.sprite = twinSprite != null ? twinSprite : (player != null ? player.DarkspriteDown : null);
                sr.color = new Color(0.55f, 0.92f, 1f, 0.85f);
                sr.sortingOrder = 10;

                twinRb = twinGo.AddComponent<Rigidbody2D>();
                twinRb.gravityScale = 0f;
                twinRb.freezeRotation = true;

                CircleCollider2D col = twinGo.AddComponent<CircleCollider2D>();
                col.radius = 0.66f;

                twinGo.AddComponent<MirrorTwinBody>();
            }

            twinGo.transform.position = spawnPos;
            twinGo.SetActive(true);
            PuzzleAudioManager.Instance?.PlayEchoRecord();
            PuzzleVFXManager.SpawnPixelBurst(transform.position, new Color(0.62f, 0.88f, 1f, 1f), 10, 3.0f);
            PuzzleVFXManager.SpawnPixelBurst(spawnPos, new Color(0.75f, 0.48f, 1f, 1f), 12, 3.4f);
        }

        private void DismissTwin()
        {
            if (twinGo == null) return;
            PuzzleVFXManager.SpawnPixelBurst(twinGo.transform.position, new Color(0.62f, 0.88f, 1f, 1f), 8, 2.8f);
            twinGo.SetActive(false);
            PuzzleAudioManager.Instance?.PlayButtonClick();
        }
    }
}
