using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class SpriteCombiner
    {
        public SpriteCombiner()
        {

        }

        public static Sprite CombineTextures(List<Texture2D> textures)
        {
            var tex = new Texture2D(512, 512);
            var rect = new Rect(0, 0, 512, 512);
            var pivot = new Vector2(0.5f, 0.5f);

            foreach (var texture in textures)
            {
                for (var i = 0; i < tex.width; i++)
                {
                    for (var j = 0; j < tex.height; j++)
                    {
                        var pixel = texture.GetPixel(i, j);
                        if (pixel.a != 0)
                        {
                            tex.SetPixel(i, j, pixel);
                        }
                    }
                }
            }

            tex.Apply();

            return Sprite.Create(tex, rect, pivot, 256);
        }
    }
}