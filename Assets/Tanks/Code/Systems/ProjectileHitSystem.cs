using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ProjectileHitSystem))]
public sealed class ProjectileHitSystem : UpdateSystem {
    private Filter filter;
    private bool canUseMasterLogic;
    
    public override void OnAwake() {
        this.filter = this.World.Filter
            .With<CollisionEventComponent>()
            .With<ProjectileComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        this.canUseMasterLogic = NetworkHelper.CanUseMasterLogic();
        
        var collisionBag = this.filter.Select<CollisionEventComponent>();
        for (int i = 0, length = this.filter.Length; i < length; ++i) {
            var entity = this.filter.GetEntity(i);
            ref var collisionComponent = ref collisionBag.GetComponent(i);

            //if (entity.Has<ProjectileComponent>()) {
            HandleProjectileHit(entity, collisionComponent.otherEntity, collisionComponent.contact);
                /*}
                if (!hitEventComponent.otherEntity.IsNullOrDisposed() && hitEventComponent.otherEntity.Has<ProjectileComponent>()) {
                    HandleProjectileHit(hitEventComponent.otherEntity, entity, hitEventComponent.overlap);
                }*/
            entity.RemoveComponent<CollisionEventComponent>();
        }
    }

    private void HandleProjectileHit(IEntity projectileEntity, IEntity otherEntity, PhysicsHelper.Collision contact) {
        ref var projectileComponent = ref projectileEntity.GetComponent<ProjectileComponent>();
        
        if (!otherEntity.IsNullOrDisposed() && projectileComponent.ownerEntityId == otherEntity.ID)
            return;
        
        var otherIsProjectile = false;
        var isFriendlyFire = false;
        
        if (!otherEntity.IsNullOrDisposed()) {
            if (projectileComponent.ownerEntityId == otherEntity.ID)
                return;
            
            if (otherEntity.Has<ProjectileComponent>()) {
                otherIsProjectile = true;
                otherEntity.SetComponent(new DestroyEventComponent());
            } else if (otherEntity.Has<HitPointsComponent>() && !otherEntity.Has<InvulnerabilityComponent>()) {
                if (otherEntity.Has<TeamComponent>() &&
                    otherEntity.GetComponent<TeamComponent>().team == projectileComponent.team) {
                    isFriendlyFire = true;
                } else if (this.canUseMasterLogic) {
                    ref var hitPointsComponent = ref otherEntity.GetComponent<HitPointsComponent>();
                    hitPointsComponent.hitPoints = Mathf.Max(0, hitPointsComponent.hitPoints - projectileComponent.damage);
                    NetworkHelper.RaiseMasterEventToOthers(otherEntity, NetworkEvent.SET_HITPOINTS, hitPointsComponent.hitPoints);
                    if (hitPointsComponent.hitPoints <= 0) {
                        otherEntity.SetComponent(new DestroyEventComponent());
                    }
                }
            }
        }
        
        projectileEntity.SetComponent(new DestroyEventComponent());
        if (!otherIsProjectile && !isFriendlyFire) {
            MakeBangEffect(contact.distance.pointB);
        }
    }

    private void MakeBangEffect(Vector2 position) {
        var bangEntity = ObjectsPool.Main.Take("ProjectileBang", position);
    }
}