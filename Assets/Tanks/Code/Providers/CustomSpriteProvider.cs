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
        if (ParseSpriteNameId(sprite.name, out var baseName, out var baseId)) {
            return new CustomSpriteDecoder {
                nextFrameOffset = this.nextFrameOffset,
                framesCount = this.framesCount,
                hasDirections = this.hasDirections,
                baseName = baseName,
                baseId = baseId
            };
        } else {
            Debug.LogError("Wrong sprite name for CustomSpriteProvider");
            return base.CreateSpriteDecoder(sprite);
        }
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
}