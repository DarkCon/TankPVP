using Morpeh;
using Tanks.Sprites;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteProvider : MonoProvider<SpriteComponent> {
    protected override void Initialize() {
        base.Initialize();
        ref var component = ref this.GetData();
        component.spriteDecoder = new TankSpriteDecoder();
        if (!component.spriteDecoder.Init(component.spriteRenderer.sprite)) {
            Debug.LogError("fail init SpriteDecoder!");
        }
    }

    private void Reset() {
        ref var component = ref this.GetData();
        component.spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
}