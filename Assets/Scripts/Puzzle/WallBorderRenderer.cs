using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Draws a high-contrast double-layered dark ink + realm-colored outline border
    /// around every map wall so walls and phase gates stand out unmistakably.
    /// </summary>
    [ExecuteAlways]
    public class WallBorderRenderer : MonoBehaviour
    {
        public Vector2 wallSize = Vector2.zero;
        public WorldAffinity affinity = WorldAffinity.Both;

        [Header("Border Styling")]
        public float outerWidth = 0.14f;
        public float innerWidth = 0.055f;
        public int sortingOrder = 6;

        private const string OuterChildName = "_OuterInkBorder";
        private const string InnerChildName = "_InnerRealmBorder";

        private static Material sharedBorderMat;

        private LineRenderer outerLine;
        private LineRenderer innerLine;
        private logic cachedLogic;
        private Vector2 lastSize = Vector2.zero;
        private bool lastDarkState = false;
        private bool hasInitializedState = false;

        void OnEnable()
        {
            SyncAffinityFromPhaseObject();
            EnsureLines();
            HookLogic(true);
            RefreshBorders(force: true);
        }

        void OnDisable()
        {
            HookLogic(false);
        }

        void Update()
        {
            Vector2 currentSize = ResolveSize();
            bool currentDark = GetIsDark();

            if (!hasInitializedState || currentSize != lastSize || currentDark != lastDarkState)
            {
                RefreshBorders(force: false);
            }
        }

        public void RefreshBorders(bool force = false)
        {
            EnsureLines();
            if (outerLine == null || innerLine == null) return;

            SyncAffinityFromPhaseObject();
            Vector2 size = ResolveSize();
            if (size.x <= 0.01f || size.y <= 0.01f) return;

            bool isDark = GetIsDark();
            lastSize = size;
            lastDarkState = isDark;
            hasInitializedState = true;

            Vector2 half = size * 0.5f;
            float inset = 0.015f;
            float hx = Mathf.Max(0.05f, half.x - inset);
            float hy = Mathf.Max(0.05f, half.y - inset);

            Vector3[] outerPts =
            {
                new Vector3(-hx, -hy, -0.02f),
                new Vector3(-hx,  hy, -0.02f),
                new Vector3( hx,  hy, -0.02f),
                new Vector3( hx, -hy, -0.02f)
            };
            Vector3[] innerPts =
            {
                new Vector3(-hx, -hy, -0.03f),
                new Vector3(-hx,  hy, -0.03f),
                new Vector3( hx,  hy, -0.03f),
                new Vector3( hx, -hy, -0.03f)
            };

            outerLine.startWidth = outerWidth;
            outerLine.endWidth = outerWidth;
            outerLine.SetPositions(outerPts);

            innerLine.startWidth = innerWidth;
            innerLine.endWidth = innerWidth;
            innerLine.SetPositions(innerPts);

            bool activeInRealm = affinity == WorldAffinity.Both
                || (affinity == WorldAffinity.LightOnly && !isDark)
                || (affinity == WorldAffinity.ShadowOnly && isDark);

            Color inkCol = new Color(0.043f, 0.039f, 0.078f, activeInRealm ? 0.98f : 0.30f);
            Color rimCol;
            if (affinity == WorldAffinity.LightOnly)
            {
                rimCol = new Color(1.0f, 0.878f, 0.40f, activeInRealm ? 1f : 0.32f);
            }
            else if (affinity == WorldAffinity.ShadowOnly)
            {
                rimCol = new Color(0.816f, 0.40f, 1f, activeInRealm ? 1f : 0.32f);
            }
            else
            {
                rimCol = isDark
                    ? new Color(0.651f, 0.459f, 1f, 1f)
                    : new Color(0.847f, 0.949f, 1f, 1f);
            }

            outerLine.startColor = inkCol;
            outerLine.endColor = inkCol;
            innerLine.startColor = rimCol;
            innerLine.endColor = rimCol;
        }

        private void HandleWorldFlipped(bool isDarkActive)
        {
            RefreshBorders(force: true);
        }

        private void HookLogic(bool subscribe)
        {
            if (subscribe)
            {
                cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
                if (cachedLogic != null)
                {
                    cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
                    cachedLogic.OnWorldFlipped += HandleWorldFlipped;
                }
            }
            else if (cachedLogic != null)
            {
                cachedLogic.OnWorldFlipped -= HandleWorldFlipped;
            }
        }

        private void SyncAffinityFromPhaseObject()
        {
            WorldPhaseObject phase = GetComponent<WorldPhaseObject>();
            if (phase != null)
            {
                affinity = phase.affinity;
            }
        }

        private bool GetIsDark()
        {
            if (cachedLogic == null) cachedLogic = logic.Instance != null ? logic.Instance : FindFirstObjectByType<logic>();
            return cachedLogic != null && cachedLogic.isDarkActive;
        }

        private Vector2 ResolveSize()
        {
            if (wallSize.x > 0.01f && wallSize.y > 0.01f) return wallSize;

            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null && box.size.x > 0.01f && box.size.y > 0.01f)
            {
                return box.size;
            }

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.drawMode != SpriteDrawMode.Simple && sr.size.x > 0.01f && sr.size.y > 0.01f)
                {
                    return sr.size;
                }
                if (sr.sprite != null)
                {
                    return sr.sprite.bounds.size;
                }
            }

            return Vector2.one;
        }

        private void EnsureLines()
        {
            Material mat = GetBorderMaterial();
            if (outerLine == null) outerLine = GetOrCreateLine(OuterChildName, sortingOrder, mat);
            if (innerLine == null) innerLine = GetOrCreateLine(InnerChildName, sortingOrder + 1, mat);
        }

        private LineRenderer GetOrCreateLine(string childName, int order, Material mat)
        {
            Transform existing = transform.Find(childName);
            GameObject child = existing != null ? existing.gameObject : new GameObject(childName);
            if (existing == null)
            {
                child.transform.SetParent(transform, false);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
            }

            LineRenderer lr = child.GetComponent<LineRenderer>();
            if (lr == null) lr = child.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.positionCount = 4;
            lr.numCornerVertices = 2;
            lr.sortingOrder = order;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            if (mat != null && lr.sharedMaterial == null)
            {
                lr.sharedMaterial = mat;
            }
            return lr;
        }

        private static Material GetBorderMaterial()
        {
            if (sharedBorderMat != null) return sharedBorderMat;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                sharedBorderMat = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
            }
            return sharedBorderMat;
        }
    }
}
