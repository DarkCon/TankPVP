using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(CollisionFixedSystem))]
public sealed class CollisionFixedSystem : FixedUpdateSystem {
    public float maxActualCollisionDist = 0.01f;

    private Filter filterMoved;
    private Filter filterOldCollisions;
    
    public override void OnAwake() {
        this.filterMoved = this.World.Filter
            .With<MoveComponent>()
            .With<PositionComponent>()
            .With<ObstacleComponent>();

        this.filterOldCollisions = this.World.Filter
            .With<CollisionEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        CheckOldCollisions();
        FindNewCollisions(deltaTime);
    }

    private void CheckOldCollisions() {
        var collisionBag = this.filterOldCollisions.Select<CollisionEventComponent>();
        for (int i = 0, length = this.filterOldCollisions.Length; i < length; ++i) {
            ref var collisionComponent = ref collisionBag.GetComponent(i);
            var entity = this.filterOldCollisions.GetEntity(i);
            
            var isActual = false;
            var otherEntity = collisionComponent.otherEntity;
            if (entity.Has<ObstacleComponent>() && !otherEntity.IsNullOrDisposed() && otherEntity.Has<ObstacleComponent>()) {
                var collider = entity.GetComponent<ObstacleComponent>().collider;
                var otherCollider = otherEntity.GetComponent<ObstacleComponent>().collider;
                var dist = collider.Distance(otherCollider);
                if (dist.isValid && dist.distance < this.maxActualCollisionDist) {
                    collisionComponent.contact.distance = dist;
                    collisionComponent.extrude = -dist.distance;
                    isActual = true;
                }
            }

            if (!isActual) {
                entity.RemoveComponent<CollisionEventComponent>();
            }
        }
    }

    private void FindNewCollisions(float deltaTime) {
        var moveBag = this.filterMoved.Select<MoveComponent>();
        var obstacleBag = this.filterMoved.Select<ObstacleComponent>();
        
        for (int i = 0, length = this.filterMoved.Length; i < length; ++i) {
            ref var moveComponent = ref moveBag.GetComponent(i);
            ref var obstacleComponent = ref obstacleBag.GetComponent(i);
            var entity = filterMoved.GetEntity(i);

            var moveDistance = moveComponent.speed * deltaTime;
            if (PhysicsHelper.GetCollision(obstacleComponent, moveComponent.direction, moveDistance * 2f, out var overlap, CollisionFilter)) {
                entity.SetComponent(new CollisionEventComponent {
                    otherEntity = EntityHelper.FindEntityIn(overlap.other),
                    contact = overlap,
                    extrude = -overlap.distance.distance
                });
            }
        }
    }

    private static bool CollisionFilter(Collider2D collider, Collider2D other) {
        var entity = EntityHelper.FindEntityIn(collider);
        var entityOther = EntityHelper.FindEntityIn(other);
        return !OwnProjectile(entity, entityOther)
               && !OwnProjectile(entityOther, entity);
    }

    private static bool OwnProjectile(IEntity ownerEntity, IEntity projectileEntity) {
        if (ownerEntity == null || projectileEntity == null)
            return false;
        var projectile = projectileEntity.GetComponent<ProjectileComponent>(out var exist);
        return exist && projectile.ownerEntityId == ownerEntity.ID;
    }
}