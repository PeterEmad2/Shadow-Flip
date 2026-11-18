using UnityEngine;

namespace ShadowFlip.Optics
{
    public enum LightColor
    {
        White,
        Red,
        Green,
        Blue,
        Shadow
    }

    public enum WorldAffinity
    {
        Both,
        LightOnly,
        ShadowOnly
    }

    public struct LightBeamHit
    {
        public Vector2 Point;
        public Vector2 Normal;
        public Vector2 IncomingDirection;
        public LightColor Color;
        public Collider2D Collider;
    }

    public interface IOpticalElement
    {
        /// <summary>
        /// Called by a LightBeamEmitter when a ray hits this optical element.
        /// </summary>
        void OnBeamHit(LightBeamHit hit, LightBeamEmitter emitter, int remainingBounces);
    }

    public interface IInteractable
    {
        void Interact(PlayerMovement player);
        string GetInteractPrompt();
    }

    public static class LightColorUtility
    {
        public static Color ToUnityColor(LightColor lightColor)
        {
            switch (lightColor)
            {
                case LightColor.Red: return new Color(1f, 0.25f, 0.25f, 1f);
                case LightColor.Green: return new Color(0.25f, 1f, 0.4f, 1f);
                case LightColor.Blue: return new Color(0.3f, 0.65f, 1f, 1f);
                case LightColor.Shadow: return new Color(0.65f, 0.25f, 0.95f, 1f);
                case LightColor.White:
                default:
                    return new Color(1f, 0.98f, 0.85f, 1f);
            }
        }

        public static bool Matches(LightColor incoming, LightColor required, bool acceptAny = false)
        {
            if (acceptAny) return true;
            if (incoming == required) return true;
            // White light carries Red, Green, and Blue wavelengths.
            if (incoming == LightColor.White &&
                (required == LightColor.Red || required == LightColor.Green || required == LightColor.Blue))
            {
                return true;
            }
            return false;
        }
    }
}
