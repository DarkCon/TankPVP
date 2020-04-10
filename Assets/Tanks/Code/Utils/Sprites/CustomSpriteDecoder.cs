using System.Linq;
using System.Text.RegularExpressions;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public class CustomSpriteDecoder : BaseSpriteDecoder {
        public int nextFrameOffset = 29;
        public int framesCount = 2;
        public bool hasDirections;
        private string baseName;
        private int baseId;

        private SpriteDescription[] sprites;

        private struct SpriteDescription {
            public Direction direction;
            public int frame;
            public Sprite sprite;
        }

        public override bool Init(Sprite baseSprite) {
            if (!base.Init(baseSprite))
                return false;

            baseSprite = this.baseSprite;

            if (!ParseSpriteNameId(baseSprite.name, out this.baseName, out this.baseId))
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

        private static bool ParseSpriteNameId(string spriteName, out string baseName, out int baseId) {
            baseName = spriteName;
            baseId = 0;
            var match = Regex.Match(spriteName, @"(\D*)(\d+)");
            if (match.Success) {
                baseName = match.Groups[1].Value;
                baseId = int.Parse(match.Groups[2].Value);
                return true;
            }

            return false;
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

        public override Sprite GetSprite(Direction direction, float animationNormalized) {
            var frame = Mathf.Min(Mathf.FloorToInt(animationNormalized * framesCount), framesCount - 1);
            return GetSpriteFrame(direction, frame);
        }
        
        public Sprite GetSpriteFrame(Direction direction, int frameId) {
            return sprites.First(s => s.direction == direction && s.frame == frameId).sprite;
        }
    }
}