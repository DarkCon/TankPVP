using Morpeh;
using Tanks.Sprites;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteProvider : MonoProvider<SpriteComponent> {
    protected override void Initialize() {
        base.Initialize();
        ref var component = ref this.GetData();
        var sprite = component.spriteRenderer.sprite;
        if (sprite != null)
            component.spriteDecoder = CreateSpriteDecoder(sprite);
        if (sprite == null || !component.spriteDecoder.Init(sprite)) {
            Debug.LogError("fail init SpriteDecoder!");
        }
    }

    protected virtual ISpriteDecoder CreateSpriteDecoder(Sprite sprite) {
        return new EmptySpriteDecoder();
    }

    private void Reset() {
        ref var component = ref this.GetData();
        component.spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
}