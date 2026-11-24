using UnityEngine;

namespace ShadowFlip.Puzzle
{
    /// <summary>
    /// Shared runtime render assets to prevent duplicate Material allocations across beams, walls, and halos.
    /// </summary>
    public static class SharedRenderResources
    {
        private static Material spriteDefaultMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            spriteDefaultMaterial = null;
        }

        public static Material SpriteDefaultMaterial
        {
            get
            {
                if (spriteDefaultMaterial == null)
                {
                    Shader spriteShader = Shader.Find("Sprites/Default");
                    if (spriteShader != null)
                    {
                        spriteDefaultMaterial = new Material(spriteShader)
                        {
                            name = "Shared_SpritesDefault"
                        };
                    }
                }
                return spriteDefaultMaterial;
            }
        }
    }
}
