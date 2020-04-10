﻿using Morpeh;
using Tanks.Sprites;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteProvider : MonoProvider<SpriteComponent> {
    private ISpriteDecoder spriteDecoderCache;
    
    protected override void Initialize() {
        base.Initialize();
        ref var component = ref this.GetData();
        if (this.spriteDecoderCache == null) {
            var sprite = component.spriteRenderer.sprite;
            if (sprite != null)
                this.spriteDecoderCache = CreateSpriteDecoder(sprite);
            if (sprite == null || !this.spriteDecoderCache.Init(sprite)) {
                Debug.LogError("fail init SpriteDecoder!");
            }
        }

        component.spriteDecoder = this.spriteDecoderCache;
    }

    protected virtual ISpriteDecoder CreateSpriteDecoder(Sprite sprite) {
        return new BaseSpriteDecoder();
    }

    private void Reset() {
        ref var component = ref this.GetData();
        component.spriteRenderer = this.GetComponent<SpriteRenderer>();
    }
}