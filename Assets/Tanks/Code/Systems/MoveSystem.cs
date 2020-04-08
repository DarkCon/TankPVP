using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

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

            var moveVector = DirectionVector.Get(moveComponent.direction);
            var distance = moveComponent.speed * deltaTime;
            
            if (entity.Has<ObstacleComponent>()) {
                ref var obstacleComponent = ref entity.GetComponent<ObstacleComponent>();
                if (PhysicsHelper.CastObstacle(obstacleComponent, moveVector, distance, out var hit)) {
                    distance = hit.distance;
                    CreateCollisionEvent(entity, hit);
                }
            }
            
            posComponent.position += distance * moveVector;
        }
    }

    private static void CreateCollisionEvent(IEntity entity, RaycastHit2D hit) {
        var otherProvider = hit.transform.GetComponent<EntityProvider>() ??
                            hit.transform.GetComponentInParent<EntityProvider>();
        var otherEntity = otherProvider != null ? otherProvider.Entity : null; 
        entity.SetComponent(new HitEventComponent {
            otherEntity = otherEntity,
            hit = hit
        });
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