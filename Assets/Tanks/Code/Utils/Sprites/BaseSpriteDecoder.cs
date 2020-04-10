using System.Linq;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public class BaseSpriteDecoder : ISpriteDecoder {
        protected Sprite baseSprite;
        private string overrideSpriteByName; 
        
        public virtual bool Init(Sprite baseSprite) {
            if (baseSprite == null)
                return false;

            var neededSpriteName = baseSprite.name;
            if (!string.IsNullOrEmpty(this.overrideSpriteByName)) {
                neededSpriteName = this.overrideSpriteByName;
            }

            if (this.baseSprite != null && this.baseSprite.name == neededSpriteName)
                return true;

            if (baseSprite.name != neededSpriteName) {
                baseSprite = SpritesCache.GetAllFor(baseSprite.texture.name)
                    .First(s => s.name == neededSpriteName);
            }
                
            this.baseSprite = baseSprite;
            return true;
        }

        public virtual Sprite GetSprite(Direction direction, float animationNormalized) {
            return this.baseSprite;
        }

        public void OverrideBaseSpriteByName(string spriteName) {
            this.overrideSpriteByName = spriteName;
            if (this.baseSprite != null)
                Init(this.baseSprite);
        }
    }
}