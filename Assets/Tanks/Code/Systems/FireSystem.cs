using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(FireSystem))]
public sealed class FireSystem : UpdateSystem {
    private Filter filterCooldown;
    private Filter filterFire;

    public override void OnAwake() {
        this.filterCooldown = this.World.Filter.With<LocalControlComponent>().With<FireCooldownComponent>();
        
        this.filterFire = this.World.Filter
            .With<TankComponent>()
            .With<DirectionComponent>()
            .With<PositionComponent>()
            .With<FireEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        CoolDownUpdate(deltaTime);
        FireUpdate();
    }

    private void CoolDownUpdate(float deltaTime) {
        var cooldownBag = this.filterCooldown.Select<FireCooldownComponent>();
        for (int i = 0, length = this.filterCooldown.Length; i < length; ++i) {
            ref var cooldownComponent = ref cooldownBag.GetComponent(i);
            cooldownComponent.time -= deltaTime;
            
            if (cooldownComponent.time <= 0f) {
                var entity = this.filterCooldown.GetEntity(i);
                entity.RemoveComponent<FireCooldownComponent>();
            }
        }
    }

    private void FireUpdate() {
        var tankBag = this.filterFire.Select<TankComponent>();
        var dirBag = this.filterFire.Select<DirectionComponent>();
        var posBag = this.filterFire.Select<PositionComponent>();
        for (int i = 0, length = this.filterFire.Length; i < length; ++i) {
            var entity = this.filterFire.GetEntity(i);
            ref var tankComponent = ref tankBag.GetComponent(i);
            var dirComponent = dirBag.GetComponent(i);
            var posComponent = posBag.GetComponent(i); //copy, no ref

            DoFire(entity, tankComponent.projectile, posComponent, dirComponent);

            if (entity.Has<LocalControlComponent>()) {
                entity.SetComponent(new FireCooldownComponent {
                    time = tankComponent.fireCooldown
                });
            }
        }
    }

    private void DoFire(IEntity ownerEntity, ProjectileComponent projectileComponent, PositionComponent posComponent, DirectionComponent dirComponent) {
        
        var dirVector = DirectionVector.Get(dirComponent.direction);
        posComponent.position += PhysicsHelper.GetPointOnObstacleEdgeOffset(ownerEntity, dirVector);
        var projectileEntity = ObjectsPool.Main.Take("Projectile", posComponent.position);
        
        if (ownerEntity.Has<TeamComponent>())
            projectileComponent.team = ownerEntity.GetComponent<TeamComponent>().team;
        projectileComponent.ownerEntityId = ownerEntity.ID;
        projectileEntity.SetComponent(projectileComponent);
        projectileEntity.SetComponent(posComponent);
        projectileEntity.SetComponent(dirComponent);
        projectileEntity.SetComponent(new MoveComponent {
            speed = projectileComponent.speed,
            direction = dirComponent.direction
        });
        
        ownerEntity.SetComponent(new FireAcceptedEventComponent());
    }
}