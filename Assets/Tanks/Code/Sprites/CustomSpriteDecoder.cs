using System.Linq;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public class CustomSpriteDecoder : ISpriteDecoder {
        public int nextFrameOffset = 29;
        public int framesCount = 2;
        public bool hasDirections;
        public string baseName;
        public int baseId;

        private SpriteDescription[] sprites;

        private struct SpriteDescription {
            public Direction direction;
            public int frame;
            public Sprite sprite;
        }

        public bool Init(Sprite baseSprite) {
            if (baseSprite == null) 
                return false;

            var allSprites = SpritesCache.GetAllFor(baseSprite.texture.name);
            if (allSprites == null)
                return false;

            var directionsCount = hasDirections ? 4 : 1;
            this.sprites = new SpriteDescription[this.framesCount * directionsCount];
            var i = 0;
            for (int dirId = 0; dirId < directionsCount; ++dirId) {
                var direction = hasDirections ? DecodeDirectionFromId(dirId) : Direction.NONE;
                for (int frame = 0; frame < this.framesCount; ++frame) {
                    var id = this.baseId + dirId + this.nextFrameOffset * frame;
                    var targetName = this.baseName + id;
                    var sprite = allSprites.FirstOrDefault(s => s.name == targetName);
                    if (sprite == null) 
                        return false;
                    
                    sprites[i++] = new SpriteDescription {
                        direction = direction,
                        frame = frame,
                        sprite = sprite
                    };
                }
            }

            return true;
        }

        private static Direction DecodeDirectionFromId(int dirId) {
            switch (dirId) {
                case 0: return Direction.UP;
                case 1: return Direction.RIGHT;
                case 2: return Direction.DOWN;
                case 3: return Direction.LEFT;
                default: return Direction.NONE;
            }
        }

        public Sprite GetSprite(Direction direction, float animationNormalized) {
            var frame = Mathf.Min(Mathf.FloorToInt(animationNormalized * framesCount), framesCount - 1);
            return sprites.First(s => s.direction == direction && s.frame == frame).sprite;
        }
    }
}