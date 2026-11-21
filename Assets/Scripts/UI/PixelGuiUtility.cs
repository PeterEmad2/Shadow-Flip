using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Shared pixel-art IMGUI helpers for solid 1x1 textures, shadowed labels, and cel-shaded panels.
    /// </summary>
    public static class PixelGuiUtility
    {
        public static Texture2D MakeSolidTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.filterMode = FilterMode.Point;
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static void DrawShadowedLabel(Rect rect, string text, GUIStyle style, Color textColor, float offset = 1.5f)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);
            GUI.color = textColor;
            GUI.Label(rect, text, style);
            GUI.color = Color.white;
        }

        public static void DrawPixelPanel(Rect rect, Texture2D whiteTex, Color fill, Color border, Color topStripe, float borderWidth = 4f)
        {
            if (whiteTex == null) return;

            float px = Mathf.Round(rect.x);
            float py = Mathf.Round(rect.y);
            float pw = Mathf.Round(rect.width);
            float ph = Mathf.Round(rect.height);
            float b = Mathf.Max(4f, Mathf.Round(borderWidth));

            // Stepped pixel-art drop shadow (notched corners)
            GUI.color = new Color(0f, 0f, 0f, 0.48f);
            GUI.DrawTexture(new Rect(px + b * 2f, py + b, pw - b * 2f, ph), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + b * 2f, pw, ph - b * 2f), whiteTex);

            // Pixel-beveled outer ink frame (missing 4 outer corner squares)
            GUI.color = border;
            GUI.DrawTexture(new Rect(px + b, py, pw - b * 2f, ph), whiteTex);
            GUI.DrawTexture(new Rect(px, py + b, pw, ph - b * 2f), whiteTex);

            // Inner panel fill
            GUI.color = fill;
            GUI.DrawTexture(new Rect(px + b, py + b, pw - b * 2f, ph - b * 2f), whiteTex);

            // Subtle 1px inner sheen & bottom rim
            GUI.color = new Color(1f, 1f, 1f, 0.10f);
            GUI.DrawTexture(new Rect(px + b, py + b + 4f, pw - b * 2f, 1f), whiteTex);
            GUI.color = new Color(0f, 0f, 0f, 0.22f);
            GUI.DrawTexture(new Rect(px + b, py + ph - b - 1f, pw - b * 2f, 1f), whiteTex);

            // Crisp 4px accent header stripe + 4x4 corner rivets
            GUI.color = topStripe;
            GUI.DrawTexture(new Rect(px + b * 2f, py + b, pw - b * 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + pw - b - 4f, py + b, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + b, py + ph - b - 4f, 4f, 4f), whiteTex);
            GUI.DrawTexture(new Rect(px + pw - b - 4f, py + ph - b - 4f, 4f, 4f), whiteTex);

            GUI.color = Color.white;
        }
    }
}
