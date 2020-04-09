using System.Text.RegularExpressions;
using Tanks.Sprites;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class CustomSpriteProvider : SpriteProvider {
    [SerializeField] private int nextFrameOffset = 1;
    [SerializeField] private int framesCount = 1;
    [SerializeField] private bool hasDirections;
    
    protected override ISpriteDecoder CreateSpriteDecoder(Sprite sprite) {
        return new CustomSpriteDecoder {
            nextFrameOffset = this.nextFrameOffset,
            framesCount = this.framesCount,
            hasDirections = this.hasDirections
        };
    }
}