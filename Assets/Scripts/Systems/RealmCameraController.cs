using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Controls realm-aware 2D camera framing:
    /// - Light Realm: focuses tightly around Jimmy within the arena bounds.
    /// - Umbra / Shadow Realm: zooms out to reveal the entire puzzle map.
    /// Also supports crisp pixel-snapped screen shake.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RealmCameraController : MonoBehaviour
    {
        public static RealmCameraController Instance { get; private set; }

        [Header("References")]
        public PlayerMovement player;
        public logic gameLogic;

        [Header("Realm Framing")]
        public float lightOrthoSize = 4.0f;
        public float umbraOrthoSize = 7.0f;
        public Vector2 arenaHalfExtents = new Vector2(10.9f, 6.5f);
        public float positionLerpSpeed = 8.0f;
        public float zoomLerpSpeed = 6.5f;
        public bool forceFullMapView = false;

        private Camera cam;
        private Vector3 basePosition;
        private float shakeTimer = 0f;
        private float shakeDuration = 0.01f;
        private float shakeIntensity = 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Instance = null;
        }

        public static void TriggerShake(float intensity = 0.16f, float duration = 0.18f)
        {
            if (Instance == null) return;
            if (intensity >= Instance.shakeIntensity || Instance.shakeTimer <= 0f)
            {
                Instance.shakeIntensity = intensity;
                Instance.shakeDuration = Mathf.Max(0.01f, duration);
                Instance.shakeTimer = duration;
            }
        }

        void Awake()
        {
            Instance = this;
            cam = GetComponent<Camera>();
            basePosition = transform.position;
        }

        void Start()
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();

            SnapToTargetImmediate();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void LateUpdate()
        {
            if (cam == null) return;
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (gameLogic == null) gameLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();

            bool isDark = gameLogic != null && gameLogic.isDarkActive;
            bool showFullMap = player == null || isDark || forceFullMapView;
            float fullMapOrtho = Mathf.Max(umbraOrthoSize, (arenaHalfExtents.x + 0.35f) / Mathf.Max(0.5f, cam.aspect));
            float targetOrtho = showFullMap ? fullMapOrtho : lightOrthoSize;

            float zoomT = 1f - Mathf.Exp(-zoomLerpSpeed * Time.unscaledDeltaTime);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrtho, zoomT);

            Vector3 targetPos = ComputeTargetPosition(showFullMap, cam.orthographicSize);
            float posT = 1f - Mathf.Exp(-positionLerpSpeed * Time.unscaledDeltaTime);
            basePosition = Vector3.Lerp(basePosition, targetPos, posT);

            Vector3 finalPos = basePosition;
            if (shakeTimer > 0f)
            {
                shakeTimer = Mathf.Max(0f, shakeTimer - Time.unscaledDeltaTime);
                float falloff = shakeTimer / shakeDuration;
                float mag = shakeIntensity * falloff;
                float ox = Mathf.Round(Random.Range(-mag, mag) * 32f) / 32f;
                float oy = Mathf.Round(Random.Range(-mag, mag) * 32f) / 32f;
                finalPos.x += ox;
                finalPos.y += oy;
            }

            transform.position = finalPos;
        }

        public void SnapToTargetImmediate()
        {
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;

            bool isDark = gameLogic != null && gameLogic.isDarkActive;
            bool showFullMap = player == null || isDark || forceFullMapView;
            float fullMapOrtho = Mathf.Max(umbraOrthoSize, (arenaHalfExtents.x + 0.35f) / Mathf.Max(0.5f, cam.aspect));
            cam.orthographicSize = showFullMap ? fullMapOrtho : lightOrthoSize;
            basePosition = ComputeTargetPosition(showFullMap, cam.orthographicSize);
            transform.position = basePosition;
        }

        private Vector3 ComputeTargetPosition(bool showFullMap, float currentOrtho)
        {
            if (showFullMap || player == null)
            {
                return new Vector3(0f, 0f, -10f);
            }

            Transform activeForm = player.GetActiveFormTransform();
            Vector3 p = activeForm != null ? activeForm.position : Vector3.zero;

            float halfH = currentOrtho;
            float halfW = halfH * Mathf.Max(0.5f, cam.aspect);
            float maxX = Mathf.Max(0f, arenaHalfExtents.x - halfW);
            float maxY = Mathf.Max(0f, arenaHalfExtents.y - halfH);

            float cx = maxX > 0.01f ? Mathf.Clamp(p.x, -maxX, maxX) : 0f;
            float cy = maxY > 0.01f ? Mathf.Clamp(p.y, -maxY, maxY) : 0f;
            return new Vector3(cx, cy, -10f);
        }
    }
}
