using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(TankKilledSystem))]
public sealed class TankKilledSystem : UpdateSystem {
    private Filter filterKilled;
    
    public override void OnAwake() {
        this.filterKilled = this.World.Filter
            .With<TankComponent>()
            .With<TankKilledEventComponent>()
            .With<LifeComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        HandleKilled();
    }

    private void HandleKilled() {
        var tankBag = this.filterKilled.Select<TankComponent>();
        var killedBag = this.filterKilled.Select<TankKilledEventComponent>();
        var lifeBag = this.filterKilled.Select<LifeComponent>();
        
        for (int i = 0, length = this.filterKilled.Length; i < length; ++i) {
            ref var tankComponent = ref tankBag.GetComponent(i);
            ref var killedComponent = ref killedBag.GetComponent(i);
            ref var lifeComponent = ref lifeBag.GetComponent(i);
            var entity = this.filterKilled.GetEntity(i);

            lifeComponent.lifeCount = killedComponent.lifeCountSpend;
            ObjectsPool.Main.Take("TankBang", killedComponent.position);

            if (lifeComponent.lifeCount > 0) {
                entity.SetComponent(new PositionComponent {position = killedComponent.respawnPosition});
                entity.SetComponent(new HitPointsComponent {hitPoints = tankComponent.maxHitPoints});
                entity.SetComponent(new DirectionComponent {direction = killedComponent.respawnDirection});
                entity.SetComponent(new InvulnerabilityComponent {time = tankComponent.invulnerabilityTime});
            } else {
                entity.SetComponent(new DestroyEventComponent());
            }
        }
    }
}