using System;
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(SpriteSystem))]
public sealed class SpriteSystem : UpdateSystem {
    private Filter filterDirection;
    private Filter filterAnimate;
    private Filter filterAnimateOnMovingStarted;
    private Filter filterAnimateOnMovingStopped;
    private Filter filterChangeSprite;

    public override void OnAwake() {
        var filterSprite = this.World.Filter.With<SpriteComponent>();

        this.filterDirection = filterSprite.With<DirectionComponent>().Without<ChangeSpriteMarker>();
        this.filterChangeSprite = filterSprite.With<ChangeSpriteMarker>();
        this.filterAnimate = filterSprite.With<AnimateSpriteComponent>();

        var filterAnimateOnMoving = filterSprite.With<AnimateOnMovingComponent>();
        this.filterAnimateOnMovingStarted = filterAnimateOnMoving.With<MoveComponent>().Without<AnimateSpriteComponent>();
        this.filterAnimateOnMovingStopped = filterAnimateOnMoving.Without<MoveComponent>().With<AnimateSpriteComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        UpdateDirections();
        UpdateSprites();
        UpdateAnimations(deltaTime);
        UpdateAnimationsOnMoving();
    }

    private void UpdateDirections() {
        var spriteBag = this.filterDirection.Select<SpriteComponent>();
        var dirBag = this.filterDirection.Select<DirectionComponent>();
        for (int i = 0, length = this.filterDirection.Length; i < length; ++i) {
            ref var spriteComponent = ref spriteBag.GetComponent(i);
            ref var dirComponent = ref dirBag.GetComponent(i);
            
            if (dirComponent.direction != spriteComponent.direction) {
                var entity = this.filterDirection.GetEntity(i);
                entity.SetComponent(new ChangeSpriteMarker());
            }
        }
    }

    private void UpdateAnimationsOnMoving() {
        foreach (var entity in this.filterAnimateOnMovingStopped) {
            entity.RemoveComponent<AnimateSpriteComponent>();
        }

        var moveBag = this.filterAnimateOnMovingStarted.Select<MoveComponent>();
        var animateOnMoveBag = this.filterAnimateOnMovingStarted.Select<AnimateOnMovingComponent>();
        for (int i = 0, length = this.filterAnimateOnMovingStarted.Length; i < length; ++i) {
            ref var moveComponent = ref moveBag.GetComponent(i);
            if (moveComponent.speed > 0f) {
                ref var animateOnMoveComponent = ref animateOnMoveBag.GetComponent(i);
                var entity = this.filterAnimateOnMovingStarted.GetEntity(i);

                ref var animateComponent = ref entity.AddComponent<AnimateSpriteComponent>();
                animateComponent.loop = true;
                animateComponent.duration = animateOnMoveComponent.distance / moveComponent.speed;

                entity.SetComponent(new ChangeSpriteMarker());
            }
        }
    }

    private void UpdateAnimations(float deltaTime) {
        var animateBag = this.filterAnimate.Select<AnimateSpriteComponent>();
        for (int i = 0, length = this.filterAnimate.Length; i < length; ++i) {
            ref var animateComponent = ref animateBag.GetComponent(i);
            var entity = this.filterAnimate.GetEntity(i);
            
            if (!animateComponent.loop && animateComponent.time >= animateComponent.duration) {
                if (animateComponent.destroyOnEnd)
                    entity.SetComponent(new DestroyEventComponent());
                else
                    entity.RemoveComponent<AnimateSpriteComponent>();
            } else {
                animateComponent.time += deltaTime;
                if (animateComponent.loop)
                    animateComponent.time %= animateComponent.duration;
                entity.SetComponent(new ChangeSpriteMarker());
            }
        }
    }

    private void UpdateSprites() {
        var spriteBag = this.filterChangeSprite.Select<SpriteComponent>();
        for (int i = 0, length = this.filterChangeSprite.Length; i < length; ++i) {
            ref var spriteComponent = ref spriteBag.GetComponent(i);
            var entity = this.filterChangeSprite.GetEntity(i);
            
            var direction = spriteComponent.direction;
            var animationNormalized = 0f;
            if (entity.Has<DirectionComponent>()) {
                direction = spriteComponent.direction = entity.GetComponent<DirectionComponent>().direction;
            }
            if (entity.Has<AnimateSpriteComponent>()) {
                ref var animateComponent = ref entity.GetComponent<AnimateSpriteComponent>();
                animationNormalized = animateComponent.time / animateComponent.duration;
            }

            try {
                spriteComponent.spriteRenderer.sprite = spriteComponent.spriteDecoder.GetSprite(direction, animationNormalized);
            } catch (Exception e) {
                var obj = entity.GetComponent<GameObjectComponent>().obj;
                Debug.LogError("fuck");
            }

            
            
            entity.RemoveComponent<ChangeSpriteMarker>();
        }
    }
}