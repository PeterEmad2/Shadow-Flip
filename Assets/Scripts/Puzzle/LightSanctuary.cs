using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Tesla Coil safe zone that drains and holds off the Umbra Stalker timer
    /// while the player stands inside its smaller electrical perimeter in Umbra mode.
    /// Only Tesla safe zones protect the player from the Umbra Stalker.
    /// </summary>
    public class LightSanctuary : MonoBehaviour
    {
        public bool isTeslaSafeZone = true;
        public float sanctuaryRadius = 1.4f;
        public float drainMultiplier = 2.0f;
        public Color ringColor = new Color(1f, 0.88f, 0.42f, 0.85f);

        private static readonly List<LightSanctuary> ActiveSanctuaries = new List<LightSanctuary>();

        private LineRenderer ringRenderer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ActiveSanctuaries.Clear();
        }

        void Awake()
        {
            BuildRing();
        }

        void Start()
        {
            SyncRingGeometry();
        }

        void OnEnable()
        {
            if (!ActiveSanctuaries.Contains(this)) ActiveSanctuaries.Add(this);
        }

        void OnDisable()
        {
            ActiveSanctuaries.Remove(this);
        }

        public static bool IsPositionInSanctuary(Vector2 pos)
        {
            for (int i = 0; i < ActiveSanctuaries.Count; i++)
            {
                LightSanctuary s = ActiveSanctuaries[i];
                if (s != null && s.isTeslaSafeZone && s.gameObject.activeInHierarchy)
                {
                    if (((Vector2)s.transform.position - pos).sqrMagnitude <= s.sanctuaryRadius * s.sanctuaryRadius)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SyncRingGeometry()
        {
            if (ringRenderer == null) return;

            float invX = 1f / Mathf.Max(0.1f, Mathf.Abs(transform.lossyScale.x));
            float invY = 1f / Mathf.Max(0.1f, Mathf.Abs(transform.lossyScale.y));
            const int segments = 24;
            ringRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float px = Mathf.Round(Mathf.Cos(angle) * sanctuaryRadius * invX * 16f) / 16f;
                float py = Mathf.Round(Mathf.Sin(angle) * sanctuaryRadius * invY * 16f) / 16f;
                ringRenderer.SetPosition(i, new Vector3(px, py, 0f));
            }
        }

        private void BuildRing()
        {
            GameObject ringGo = new GameObject("SanctuaryRing");
            ringGo.transform.SetParent(transform, false);
            ringRenderer = ringGo.AddComponent<LineRenderer>();
            ringRenderer.useWorldSpace = false;
            ringRenderer.loop = true;
            ringRenderer.numCornerVertices = 0;
            ringRenderer.numCapVertices = 0;
            ringRenderer.startWidth = 0.09f;
            ringRenderer.endWidth = 0.09f;
            ringRenderer.sortingOrder = 0;
            ringRenderer.startColor = ringColor;
            ringRenderer.endColor = ringColor;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) ringRenderer.material = new Material(shader);

            SyncRingGeometry();
        }
    }
}
