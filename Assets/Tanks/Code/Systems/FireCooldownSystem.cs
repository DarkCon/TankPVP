using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(FireCooldownSystem))]
public sealed class FireCooldownSystem : UpdateSystem {
    private Filter filterCooldown;
    private Filter filterWantFire;

    public override void OnAwake() {
        var filterLocalControl = this.World.Filter.With<LocalControlComponent>();
        
        this.filterCooldown = filterLocalControl.With<FireCooldownComponent>(); 
        
        this.filterWantFire = filterLocalControl
            .With<TankComponent>()
            .With<DirectionComponent>()
            .With<PositionComponent>()
            .With<WantFireEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        CoolDownUpdate(deltaTime);
        WantFireUpdate();
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

    private void WantFireUpdate() {
        var tankBag = this.filterWantFire.Select<TankComponent>();
        var dirBag = this.filterWantFire.Select<DirectionComponent>();
        var posBag = this.filterWantFire.Select<PositionComponent>();
        for (int i = 0, length = this.filterWantFire.Length; i < length; ++i) {
            var entity = this.filterWantFire.GetEntity(i);
            ref var dirComponent = ref dirBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);

            entity.RemoveComponent<WantFireEventComponent>();
            
            MakeFireEvent(entity, posComponent.position, dirComponent.direction);
            
            ref var tankComponent = ref tankBag.GetComponent(i);
            entity.SetComponent(new FireCooldownComponent {
                time = tankComponent.fireCooldown
            });
        }
    }

    private static void MakeFireEvent(IEntity entity, Vector3 position, Direction direction) {
        var dirVector = DirectionUtils.GetVector(direction);
        position += PhysicsHelper.GetPointOnObstacleEdgeOffset(entity, dirVector);
        entity.SetComponent(new FireEventComponent {
            position = position,
            direction = direction
        });
    }
}