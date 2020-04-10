using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(CleanupEventsSystem))]
public sealed class CleanupEventsSystem : UpdateSystem {
    private Filter filterHit;
    private Filter filterFire;
    
    public override void OnAwake() {
        this.filterHit = this.World.Filter.With<HitEventComponent>();
        this.filterFire = this.World.Filter.With<FireEventComponent>().With<FireAcceptedEventComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        foreach (var entity in this.filterHit) {
            entity.RemoveComponent<HitEventComponent>();
        }
        foreach (var entity in this.filterFire) {
            entity.RemoveComponent<FireEventComponent>();
            entity.RemoveComponent<FireAcceptedEventComponent>();
        }
    }
}