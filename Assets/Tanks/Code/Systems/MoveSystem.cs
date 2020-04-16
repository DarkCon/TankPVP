using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.UI;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(MoveSystem))]
public sealed class MoveSystem : UpdateSystem {
    private Filter filterPos;
    private Filter filterDirection;
    
    public override void OnAwake() {
        var filterMove = this.World.Filter.With<MoveComponent>();
        
        this.filterPos = filterMove.With<PositionComponent>();
        this.filterDirection = filterMove.With<DirectionComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        UpdateDirections();
        UpdatePositions(deltaTime);
    }

    private void UpdatePositions(float deltaTime) {
        var moveBag = this.filterPos.Select<MoveComponent>();
        var posBag = this.filterPos.Select<PositionComponent>();

        for (int i = 0, length = this.filterPos.Length; i < length; ++i) {
            ref var moveComponent = ref moveBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);
            var entity = filterPos.GetEntity(i);

            if (entity.Has<CollisionEventComponent>()) {
                ref var collisionComponent = ref entity.GetComponent<CollisionEventComponent>();

                var otherMoveExist = collisionComponent.otherEntity != null 
                                     && collisionComponent.otherEntity.Has<MoveComponent>();
                var otherMoveDirection = otherMoveExist
                    ? collisionComponent.otherEntity.GetComponent<MoveComponent>().direction
                    : Direction.NONE;
                
                if (collisionComponent.extrude >= 0f 
                    && (!otherMoveExist || DirectionUtils.IsOpposite(otherMoveDirection, moveComponent.direction))) {
                    var extrude = Mathf.Min(collisionComponent.extrude, moveComponent.speed * deltaTime);
                    posComponent.position -= (Vector3) collisionComponent.contact.distance.normal * extrude;
                    collisionComponent.extrude -= extrude;
                }
                if (collisionComponent.contact.direction == moveComponent.direction)
                    continue;
            }

            var moveVector = DirectionUtils.GetVector(moveComponent.direction);
            var distance = moveComponent.speed * deltaTime;
            
            posComponent.position += distance * moveVector;
        }
    }

    private void UpdateDirections() {
        var moveBag = this.filterDirection.Select<MoveComponent>();
        var dirBag = this.filterDirection.Select<DirectionComponent>();

        for (int i = 0, length = this.filterDirection.Length; i < length; ++i) {
            ref var moveComponent = ref moveBag.GetComponent(i);
            ref var dirComponent = ref dirBag.GetComponent(i);

            dirComponent.direction = moveComponent.direction;
        }
    }
}