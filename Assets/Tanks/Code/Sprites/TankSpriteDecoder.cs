using System.Linq;
using System.Text.RegularExpressions;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public class TankSpriteDecoder : ISpriteDecoder {
        private const int NEXT_FRAME_OFFSET = 29;
        private const int FRAME_COUNTS = 2;

        private SpriteDescription[] sprites;

        private struct SpriteDescription {
            public Direction direction;
            public int frame;
            public Sprite sprite;
        }

        public bool Init(Sprite baseSprite) {
            if (baseSprite == null) 
                return false;

            if (!ParseSpriteName(baseSprite.name, out var baseName, out var baseId))
                return false;
            
            var allSprites = SpritesCache.GetAllFor(baseSprite.texture.name);
            if (allSprites == null)
                return false;

            this.sprites = new SpriteDescription[FRAME_COUNTS * 4];
            var i = 0;
            for (int dirId = 0; dirId < 4; ++dirId) {
                var direction = DecodeDirectionFromId(dirId);
                for (int frame = 0; frame < FRAME_COUNTS; ++frame) {
                    var id = baseId + dirId + NEXT_FRAME_OFFSET * frame;
                    var targetName = baseName + id;
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

        private static bool ParseSpriteName(string spriteName, out string baseName, out int baseId) {
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

        public Sprite GetSprite(Direction direction, float animation) {
            var frame = Mathf.FloorToInt(Mathf.Clamp01(animation) * FRAME_COUNTS);
            return sprites.First(s => s.direction == direction && s.frame == frame).sprite;
        }
    }
}