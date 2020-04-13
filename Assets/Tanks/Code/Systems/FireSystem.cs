using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(FireSystem))]
public sealed class FireSystem : UpdateSystem {
    private Filter filterDoFire;

    public override void OnAwake() {
        this.filterDoFire = this.World.Filter.With<FireEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var tankBag = this.filterDoFire.Select<TankComponent>();
        var fireBag = this.filterDoFire.Select<FireEventComponent>();
        for (int i = 0, length = this.filterDoFire.Length; i < length; ++i) {
            var entity = this.filterDoFire.GetEntity(i);
            var tankComponent = tankBag.GetComponent(i);
            var fireComponent = fireBag.GetComponent(i);

            DoFire(entity, tankComponent.projectile, fireComponent);
        }
    }

    private void DoFire(IEntity ownerEntity, ProjectileComponent projectileComponent, FireEventComponent fireEvent) {
        var projectileEntity = ObjectsPool.Main.Take("Projectile", fireEvent.position);
        
        if (ownerEntity.Has<TeamComponent>())
            projectileComponent.team = ownerEntity.GetComponent<TeamComponent>().team;
        projectileComponent.ownerEntityId = ownerEntity.ID;
        projectileEntity.SetComponent(projectileComponent);
        projectileEntity.SetComponent(new PositionComponent {position = fireEvent.position});
        projectileEntity.SetComponent(new DirectionComponent {direction = fireEvent.direction});
        projectileEntity.SetComponent(new MoveComponent {
            speed = projectileComponent.speed,
            direction = fireEvent.direction
        });
    }
}