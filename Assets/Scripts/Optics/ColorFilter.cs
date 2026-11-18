using UnityEngine;

namespace ShadowFlip.Optics
{
    /// <summary>
    /// Allows matching colors through, tints White light to filterColor, and blocks mismatched colors.
    /// </summary>
    public class ColorFilter : MonoBehaviour, IOpticalElement
    {
        public LightColor filterColor = LightColor.Blue;
        public bool tintWhiteLight = true;

        void Awake()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = LightColorUtility.ToUnityColor(filterColor);
                c.a = 0.75f;
                sr.color = c;
            }
        }

        public void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces)
        {
            LightColor outgoing;
            if (hit.Color == filterColor)
            {
                outgoing = hit.Color;
            }
            else if (tintWhiteLight && hit.Color == LightColor.White)
            {
                outgoing = filterColor;
            }
            else
            {
                return;
            }

            emitter.EmitChildBeam(transform.position, hit.IncomingDirection, outgoing, remainingBounces, hit.Collider);
        }
    }
}
