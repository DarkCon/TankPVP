using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(DestroySystem))]
public sealed class DestroySystem : UpdateSystem {
    private Filter filterKilledTanks;
    private Filter filterDestroy;
    
    public override void OnAwake() {
        this.filterDestroy = this.World.Filter.With<DestroyEventComponent>();
        
        this.filterKilledTanks = this.filterDestroy.With<TankComponent>().With<PositionComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        HandleKilledTanks();
        HandleDestroyed();
    }

    private void HandleKilledTanks() {
        var posBag = this.filterKilledTanks.Select<PositionComponent>();
        for (int i = 0, length = this.filterKilledTanks.Length; i < length; ++i) {
            var position = posBag.GetComponent(i).position;

            var bangEntity = ObjectsPool.Main.Take("TankBang", position);
        }
    }

    private void HandleDestroyed() {
        foreach (var entity in this.filterDestroy) {
            entity.RemoveComponent<DestroyEventComponent>();
            ObjectsPool.Main.Return(entity, this.World);
        }
    }
}