﻿using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ProjectileHitSystem))]
public sealed class ProjectileHitSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter.With<HitEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var hitBag = this.filter.Select<HitEventComponent>();
        for (int i = 0, length = this.filter.Length; i < length; ++i) {
            var entity = this.filter.GetEntity(i);
            ref var hitEventComponent = ref hitBag.GetComponent(i);

            if (entity.Has<ProjectileComponent>()) {
                HandleProjectileHit(entity, hitEventComponent.otherEntity, hitEventComponent.overlap);
            }
            if (!hitEventComponent.otherEntity.IsNullOrDisposed() && hitEventComponent.otherEntity.Has<ProjectileComponent>()) {
                HandleProjectileHit(hitEventComponent.otherEntity, entity, hitEventComponent.overlap);
            }
        }
    }

    private void HandleProjectileHit(IEntity projectileEntity, IEntity otherEntity, PhysicsHelper.Overlap2D contact) {
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
            } else if (otherEntity.Has<HitPointsComponent>()) {
                if (otherEntity.Has<TeamComponent>() &&
                    otherEntity.GetComponent<TeamComponent>().team == projectileComponent.team) {
                    isFriendlyFire = true;
                } else {
                    ref var hitPointsComponent = ref otherEntity.GetComponent<HitPointsComponent>();
                    hitPointsComponent.hitPoints -= projectileComponent.damage;
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