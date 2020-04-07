using Morpeh;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(SpriteSystem))]
public sealed class SpriteSystem : UpdateSystem {
    private Filter filterDirection;
    private Filter filterChangeSprite;
    
    public override void OnAwake() {
        var filterSprite = this.World.Filter.With<SpriteComponent>();

        this.filterDirection = filterSprite.With<DirectionComponent>().Without<ChangeSpriteMarker>();
        this.filterChangeSprite = filterSprite.With<ChangeSpriteMarker>();
    }

    public override void OnUpdate(float deltaTime) {
        UpdateDirections();
        UpdateSprites();
    }

    private void UpdateDirections() {
        var spriteBag = this.filterDirection.Select<SpriteComponent>();
        var dirBag = this.filterDirection.Select<DirectionComponent>();
        for (int i = 0, length = this.filterDirection.Length; i < length; ++i) {
            ref var spriteComponent = ref spriteBag.GetComponent(i);
            ref var dirComponent = ref dirBag.GetComponent(i);
            if (dirComponent.direction != spriteComponent.direction) {
                var entity = this.filterDirection.GetEntity(i);
                entity.AddComponent<ChangeSpriteMarker>();
            }
        }
    }

    private void UpdateSprites() {
        var spriteBag = this.filterChangeSprite.Select<SpriteComponent>();
        for (int i = 0, length = this.filterChangeSprite.Length; i < length; ++i) {
            ref var spriteComponent = ref spriteBag.GetComponent(i);
            var entity = this.filterChangeSprite.GetEntity(i);
            
            var direction = Direction.NONE;
            if (entity.Has<DirectionComponent>()) {
                direction = entity.GetComponent<DirectionComponent>().direction;
            }

            spriteComponent.spriteRenderer.sprite = spriteComponent.spriteDecoder.GetSprite(direction, 0f);
            
            entity.RemoveComponent<ChangeSpriteMarker>();
        }
    }
}