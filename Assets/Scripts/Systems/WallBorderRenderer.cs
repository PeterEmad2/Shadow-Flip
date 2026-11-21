using ShadowFlip.Optics;
using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Legacy wall-border component kept for scene serialization compatibility;
    /// immediately disables and removes any existing outline child borders.
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

        void OnEnable()
        {
            RemoveLegacyBorders();
        }

        public void RefreshBorders(bool force = false)
        {
            RemoveLegacyBorders();
        }

        private void RemoveLegacyBorders()
        {
            DisableAndRemoveChild(OuterChildName);
            DisableAndRemoveChild(InnerChildName);
        }

        private void DisableAndRemoveChild(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null) return;

            GameObject childGo = child.gameObject;
            LineRenderer lr = childGo.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.enabled = false;
                lr.positionCount = 0;
            }
            childGo.SetActive(false);

            if (Application.isPlaying)
            {
                Destroy(childGo);
            }
#if UNITY_EDITOR
            else
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    if (childGo != null) DestroyImmediate(childGo);
                };
            }
#endif
        }
    }
}
