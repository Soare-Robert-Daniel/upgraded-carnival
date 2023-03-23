using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace GameEntities
{
    [Serializable]
    public class EntityAssets
    {
        [SerializeField] private List<Texture2D> textures;
        private Sprite renderedSprite;

        public Sprite RenderedSprite
        {
            get
            {
                if (renderedSprite == null)
                {
                    Debug.Log("Create sprite");
                    renderedSprite = SpriteCombiner.CombineTextures(textures);
                }
                return renderedSprite;
            }
        }

        public List<Texture2D> Textures
        {
            get => textures;
            set => textures = value;
        }
    }
}