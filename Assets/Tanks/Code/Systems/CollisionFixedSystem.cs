using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(CollisionFixedSystem))]
public sealed class CollisionFixedSystem : FixedUpdateSystem {
    private Filter filterMoved;
    
    public override void OnAwake() {
        this.filterMoved = this.World.Filter
            .With<MoveComponent>()
            .With<PositionComponent>()
            .With<ObstacleComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var moveBag = this.filterMoved.Select<MoveComponent>();
        var posBag = this.filterMoved.Select<PositionComponent>();
        var obstacleBag = this.filterMoved.Select<ObstacleComponent>();
        
        for (int i = 0, length = this.filterMoved.Length; i < length; ++i) {
            ref var moveComponent = ref moveBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);
            ref var obstacleComponent = ref obstacleBag.GetComponent(i);
            var entity = filterMoved.GetEntity(i);

            var moveVector = DirectionVector.Get(moveComponent.direction);

            if (PhysicsHelper.GetOverlapForward(obstacleComponent, moveVector, out var overlap)
                && CreateCollisionEvent(entity, overlap)) {
                var extrude = Mathf.Min(Mathf.Abs(overlap.distance.distance), moveComponent.speed * deltaTime * 2f);
                posComponent.position -= (Vector3) overlap.distance.normal * extrude;
            }
        }
    }

    private static bool CreateCollisionEvent(IEntity entity, PhysicsHelper.Overlap2D overlap) {
        var otherProvider = overlap.other.GetComponent<EntityProvider>() ??
                            overlap.other.transform.GetComponentInParent<EntityProvider>();
        var otherEntity = otherProvider != null ? otherProvider.Entity : null;
        
        if (otherEntity != null && otherEntity.Has<ProjectileComponent>() && entity.Has<TankComponent>()
            && otherEntity.GetComponent<ProjectileComponent>().ownerEntityId == entity.ID) {
            return false;
        }
        
        entity.SetComponent(new HitEventComponent {
            otherEntity = otherEntity,
            overlap = overlap
        });
        return true;
    }
}