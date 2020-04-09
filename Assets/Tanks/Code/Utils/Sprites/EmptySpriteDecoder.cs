using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public class EmptySpriteDecoder : ISpriteDecoder {
        private Sprite sprite;
        
        public bool Init(Sprite baseSprite) {
            this.sprite = baseSprite;
            return baseSprite != null;
        }

        public Sprite GetSprite(Direction direction, float animationNormalized) {
            return this.sprite;
        }
    }
}