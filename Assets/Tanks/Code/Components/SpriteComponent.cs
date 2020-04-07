using Morpeh;
using Tanks.Constants;
using Tanks.Sprites;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct SpriteComponent : IComponent {
    public SpriteRenderer spriteRenderer;
    public Direction direction;
    [HideInInspector] public ISpriteDecoder spriteDecoder;
}