using Morpeh;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(MoveSystem))]
public sealed class MoveSystem : UpdateSystem {
    private Filter filterPos;
    private Filter filterDirection;
    
    private readonly RaycastHit2D[] raycastResult = new RaycastHit2D[1];
    private ContactFilter2D contactFilter = new ContactFilter2D(); 
    
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
                var angle = (Vector2.SignedAngle(Vector2.right, moveVector) + 180f) % 360f;
                this.contactFilter.SetNormalAngle(angle - 1f, angle + 1f);
                if (obstacleComponent.collider.Cast(moveVector, this.contactFilter, this.raycastResult, distance) > 0) {
                    var result = this.raycastResult[0];
                    distance = result.distance;
                }
            }
            
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