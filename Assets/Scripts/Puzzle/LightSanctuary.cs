using System.Collections.Generic;
using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Golden crystal sanctuary zone that drains and holds off the Umbra Stalker timer
    /// while the player stands inside it in Shadow/Umbra mode.
    /// </summary>
    public class LightSanctuary : MonoBehaviour
    {
        public float sanctuaryRadius = 2.2f;
        public float drainMultiplier = 2.0f;
        public Color ringColor = new Color(1f, 0.88f, 0.42f, 0.75f);

        private static readonly List<LightSanctuary> ActiveSanctuaries = new List<LightSanctuary>();

        private LineRenderer ringRenderer;

        void Awake()
        {
            BuildRing();
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
                if (s != null && s.gameObject.activeInHierarchy)
                {
                    if (((Vector2)s.transform.position - pos).sqrMagnitude <= s.sanctuaryRadius * s.sanctuaryRadius)
                    {
                        return true;
                    }
                }
            }

            LightSensor[] sensors = FindObjectsByType<LightSensor>(FindObjectsSortMode.None);
            const float sensorRadiusSq = 1.6f * 1.6f;
            for (int i = 0; i < sensors.Length; i++)
            {
                LightSensor sensor = sensors[i];
                if (sensor != null && sensor.gameObject.activeInHierarchy && sensor.IsActivated)
                {
                    if (((Vector2)sensor.transform.position - pos).sqrMagnitude <= sensorRadiusSq)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void BuildRing()
        {
            GameObject ringGo = new GameObject("SanctuaryRing");
            ringGo.transform.SetParent(transform, false);
            ringRenderer = ringGo.AddComponent<LineRenderer>();
            ringRenderer.useWorldSpace = false;
            ringRenderer.loop = true;
            ringRenderer.startWidth = 0.08f;
            ringRenderer.endWidth = 0.08f;
            ringRenderer.sortingOrder = 0;
            ringRenderer.startColor = ringColor;
            ringRenderer.endColor = ringColor;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) ringRenderer.material = new Material(shader);

            const int segments = 40;
            ringRenderer.positionCount = segments;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                ringRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * sanctuaryRadius, Mathf.Sin(angle) * sanctuaryRadius, 0f));
            }
        }
    }
}
