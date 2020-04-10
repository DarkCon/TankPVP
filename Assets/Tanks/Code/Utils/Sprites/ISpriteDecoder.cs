using Tanks.Constants;
using UnityEngine;

namespace Tanks.Sprites {
    public interface ISpriteDecoder {
        bool Init(Sprite baseSprite);
        Sprite GetSprite(Direction direction, float animationNormalized);
        void OverrideBaseSpriteByName(string tankSprite);
    }
}