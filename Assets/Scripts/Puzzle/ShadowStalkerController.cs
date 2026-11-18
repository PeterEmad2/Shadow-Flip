using System.Collections;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Spawns an animated cel-shaded Umbra Stalker monster if the player stays too long
    /// in Shadow/Umbra mode. Flipping back to Light mode banishes it, standing in a Light
    /// Sanctuary drains its timer, and holding it in a laser beam petrifies it into a stone block.
    /// </summary>
    public class ShadowStalkerController : MonoBehaviour
    {
        public static ShadowStalkerController Instance { get; private set; }

        [Header("Timing")]
        public float umbraSafeDuration = 12f;
        public float lightCooldownRate = 2.5f;

        [Header("Monster Behavior")]
        public float chaseSpeed = 3.5f;
        public float laserSlowMultiplier = 0.38f;
        public float spawnDistance = 5.0f;
        public float catchRadius = 0.62f;

        [Header("Laser Petrification")]
        public bool canBePetrifiedByLaser = true;
        public float petrifyExposureTime = 1.1f;
        public Sprite petrifiedSprite;

        [Header("Cel-Shaded Visuals")]
        public Sprite monsterFrame1;
        public Sprite monsterFrame2;

        public float UmbraProgress => Mathf.Clamp01(umbraTimer / Mathf.Max(0.1f, umbraSafeDuration));
        public float RemainingSeconds => Mathf.Max(0f, umbraSafeDuration - umbraTimer);
        public bool IsMonsterActive { get; private set; }
        public bool IsPlayerCaught { get; private set; }
        public bool IsSlowedByLaser { get; private set; }
        public bool IsInSanctuary { get; private set; }
        public int FlipCount { get; private set; }
        public bool HasMonsterSpawnedThisRun { get; private set; }
        public bool MonsterWasPetrified { get; private set; }
        public float PetrifyProgress => Mathf.Clamp01(laserExposureTimer / Mathf.Max(0.1f, petrifyExposureTime));

        private float umbraTimer = 0f;
        private float laserExposureTimer = 0f;
        private int lastWarnSecond = -1;
        private bool lastTrackedDark;
        private logic cachedLogic;
        private PlayerMovement cachedPlayer;
        private LightBeamEmitter[] cachedEmitters;
        private LevelExit[] cachedExits;

        private GameObject monsterObj;
        private SpriteRenderer monsterRenderer;
        private Transform visualChild;
        private Vector3 basePosition;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInScene()
        {
            if (FindFirstObjectByType<ShadowStalkerController>() == null)
            {
                GameObject go = new GameObject("ShadowStalkerController");
                go.AddComponent<ShadowStalkerController>();
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            if (monsterFrame1 == null) monsterFrame1 = Resources.Load<Sprite>("Sprites/UmbraMonster_1");
            if (monsterFrame2 == null) monsterFrame2 = Resources.Load<Sprite>("Sprites/UmbraMonster_2");
            if (petrifiedSprite == null) petrifiedSprite = Resources.Load<Sprite>("Sprites/Monster_Petrified");
            if (monsterFrame1 == null) monsterFrame1 = CreateFallbackSprite();
            if (monsterFrame2 == null) monsterFrame2 = monsterFrame1;
        }

        void Start()
        {
            cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            cachedPlayer = FindFirstObjectByType<PlayerMovement>();
            cachedEmitters = FindObjectsByType<LightBeamEmitter>(FindObjectsSortMode.None);
            cachedExits = FindObjectsByType<LevelExit>(FindObjectsSortMode.None);

            if (cachedLogic != null)
            {
                lastTrackedDark = cachedLogic.isDarkActive;
                cachedLogic.OnWorldFlipped += HandleWorldFlipped;
            }
        }

        void OnDestroy()
        {
            if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
            if (Instance == this) Instance = null;
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            if (isDarkActive == lastTrackedDark) return;
            lastTrackedDark = isDarkActive;
            FlipCount++;
        }

        void Update()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            if (cachedPlayer == null) cachedPlayer = FindFirstObjectByType<PlayerMovement>();
            if (cachedLogic == null || cachedPlayer == null) return;
            if (Time.timeScale == 0f || IsPlayerCaught || IsAnyExitCompleted()) return;

            if (cachedLogic.isDarkActive)
            {
                UpdateUmbraMode();
            }
            else
            {
                UpdateLightMode();
            }
        }

        private void UpdateUmbraMode()
        {
            Transform playerTransform = cachedPlayer.GetActiveFormTransform();
            if (playerTransform == null) return;

            IsInSanctuary = LightSanctuary.IsPositionInSanctuary(playerTransform.position);

            if (!IsMonsterActive)
            {
                if (IsInSanctuary)
                {
                    umbraTimer = Mathf.Max(0f, umbraTimer - Time.deltaTime * lightCooldownRate);
                    lastWarnSecond = -1;
                    return;
                }

                umbraTimer += Time.deltaTime;

                float remaining = RemainingSeconds;
                if (remaining > 0f && remaining <= 3.0f)
                {
                    int secBucket = Mathf.CeilToInt(remaining);
                    if (secBucket != lastWarnSecond)
                    {
                        lastWarnSecond = secBucket;
                        PuzzleAudioManager.Instance?.PlayMonsterWarn();
                    }
                }

                if (umbraTimer >= umbraSafeDuration)
                {
                    umbraTimer = umbraSafeDuration;
                    SpawnMonster(playerTransform.position);
                }
                return;
            }

            UpdateMonsterChase(playerTransform.position);
        }

        private void UpdateLightMode()
        {
            IsInSanctuary = false;
            if (IsMonsterActive)
            {
                IsMonsterActive = false;
                IsSlowedByLaser = false;
                laserExposureTimer = 0f;
                if (monsterObj != null) monsterObj.SetActive(false);
                umbraTimer = 0f;
                lastWarnSecond = -1;
            }
            else
            {
                umbraTimer = Mathf.Max(0f, umbraTimer - Time.deltaTime * lightCooldownRate);
                lastWarnSecond = -1;
            }
        }

        private void SpawnMonster(Vector3 playerPos)
        {
            EnsureMonsterObject();

            Vector2 spawnPos = PickValidSpawnPosition(playerPos);
            basePosition = new Vector3(spawnPos.x, spawnPos.y, 0f);
            monsterObj.transform.position = basePosition;
            monsterObj.SetActive(true);

            IsMonsterActive = true;
            HasMonsterSpawnedThisRun = true;
            IsSlowedByLaser = false;
            laserExposureTimer = 0f;
            PuzzleAudioManager.Instance?.PlayMonsterSpawn();
        }

        private void UpdateMonsterChase(Vector3 playerPos)
        {
            if (monsterObj == null) return;

            if (monsterRenderer != null)
            {
                int frame = Mathf.FloorToInt(Time.time * 5f) % 2;
                monsterRenderer.sprite = frame == 0 ? monsterFrame1 : monsterFrame2;
                monsterRenderer.flipX = playerPos.x < basePosition.x;
            }

            if (visualChild != null)
            {
                visualChild.localPosition = new Vector3(0f, Mathf.Sin(Time.time * 6f) * 0.12f, 0f);
            }

            IsSlowedByLaser = CheckNearLaserBeam(basePosition);
            if (IsSlowedByLaser)
            {
                laserExposureTimer += Time.deltaTime;
                if (canBePetrifiedByLaser && laserExposureTimer >= petrifyExposureTime)
                {
                    PetrifyMonsterIntoBlock();
                    return;
                }
            }
            else
            {
                laserExposureTimer = Mathf.Max(0f, laserExposureTimer - Time.deltaTime * 0.5f);
            }

            if (monsterRenderer != null)
            {
                monsterRenderer.color = IsSlowedByLaser
                    ? Color.Lerp(new Color(0.65f, 0.95f, 1f, 1f), new Color(0.85f, 0.85f, 0.9f, 1f), PetrifyProgress)
                    : Color.white;
            }

            float speed = chaseSpeed * (IsSlowedByLaser ? laserSlowMultiplier : 1f);
            Vector2 nextPos = Vector2.MoveTowards(basePosition, playerPos, speed * Time.deltaTime);
            basePosition = new Vector3(nextPos.x, nextPos.y, 0f);
            monsterObj.transform.position = basePosition;

            if (Vector2.Distance(basePosition, playerPos) <= catchRadius)
            {
                OnPlayerCaught();
            }
        }

        private void PetrifyMonsterIntoBlock()
        {
            GameObject statue = new GameObject("PetrifiedUmbraStatue");
            statue.transform.position = basePosition;
            statue.transform.localScale = new Vector3(0.95f, 0.95f, 1f);

            SpriteRenderer sr = statue.AddComponent<SpriteRenderer>();
            sr.sprite = petrifiedSprite != null ? petrifiedSprite : monsterFrame1;
            sr.sortingOrder = 4;

            BoxCollider2D col = statue.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.9f);

            statue.AddComponent<PushableBlock>();

            if (monsterObj != null) monsterObj.SetActive(false);
            IsMonsterActive = false;
            IsSlowedByLaser = false;
            MonsterWasPetrified = true;
            laserExposureTimer = 0f;
            umbraTimer = 0f;
            lastWarnSecond = -1;

            PuzzleAudioManager.Instance?.PlayMonsterPetrify();
        }

        private void OnPlayerCaught()
        {
            if (IsPlayerCaught) return;
            IsPlayerCaught = true;
            PuzzleAudioManager.Instance?.PlayMonsterCatch();
            StartCoroutine(RestartAfterDelay());
        }

        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.85f);
            if (cachedLogic != null)
            {
                cachedLogic.RestartLevel();
            }
        }

        private Vector2 PickValidSpawnPosition(Vector2 playerPos)
        {
            Vector2 best = ClampArena(playerPos + Vector2.right * spawnDistance);
            float bestDist = 0f;

            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.PI * 0.25f;
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 candidate = ClampArena(playerPos + dir * spawnDistance);
                float d = Vector2.Distance(candidate, playerPos);
                if (d >= spawnDistance * 0.8f)
                {
                    return candidate;
                }
                if (d > bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }

            return best;
        }

        private static Vector2 ClampArena(Vector2 pos)
        {
            return new Vector2(Mathf.Clamp(pos.x, -8.8f, 8.8f), Mathf.Clamp(pos.y, -4.6f, 4.6f));
        }

        private bool CheckNearLaserBeam(Vector2 point)
        {
            if (cachedEmitters == null) return false;

            const float slowRadiusSq = 0.6f * 0.6f;
            for (int i = 0; i < cachedEmitters.Length; i++)
            {
                LightBeamEmitter emitter = cachedEmitters[i];
                if (emitter == null || !emitter.gameObject.activeInHierarchy) continue;

                LineRenderer[] lines = emitter.GetComponentsInChildren<LineRenderer>(false);
                for (int j = 0; j < lines.Length; j++)
                {
                    LineRenderer lr = lines[j];
                    if (lr == null || !lr.enabled || lr.positionCount < 2) continue;

                    Vector2 a = lr.GetPosition(0);
                    Vector2 b = lr.GetPosition(1);
                    if (DistanceToSegmentSq(point, a, b) <= slowRadiusSq)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static float DistanceToSegmentSq(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float lenSq = ab.sqrMagnitude;
            if (lenSq < 0.0001f) return (p - a).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
            Vector2 proj = a + ab * t;
            return (p - proj).sqrMagnitude;
        }

        private bool IsAnyExitCompleted()
        {
            if (cachedExits == null) return false;
            for (int i = 0; i < cachedExits.Length; i++)
            {
                if (cachedExits[i] != null && cachedExits[i].IsCompleted) return true;
            }
            return false;
        }

        private void EnsureMonsterObject()
        {
            if (monsterObj != null) return;

            monsterObj = new GameObject("UmbraStalker");
            GameObject visGo = new GameObject("Visual");
            visGo.transform.SetParent(monsterObj.transform, false);
            visualChild = visGo.transform;

            monsterRenderer = visGo.AddComponent<SpriteRenderer>();
            monsterRenderer.sprite = monsterFrame1;
            monsterRenderer.sortingOrder = 15;
            monsterObj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            monsterObj.SetActive(false);
        }

        private static Sprite CreateFallbackSprite()
        {
            Texture2D tex = new Texture2D(16, 16);
            Color c = new Color(0.55f, 0.2f, 0.85f, 1f);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++) tex.SetPixel(x, y, c);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
