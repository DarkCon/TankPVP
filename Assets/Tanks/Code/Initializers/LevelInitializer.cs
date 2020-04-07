using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Initializers/" + nameof(LevelInitializer))]
public sealed class LevelInitializer : Initializer {
    public EntityProvider TankPrefab;

    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>();
        
        var spawnBag = this.filter.Select<SpawnComponent>();
        var posBag = this.filter.Select<PositionComponent>();
        for (int i = 0, length = this.filter.Length; i < length; ++i) {
            ref var posComponent = ref posBag.GetComponent(i);
            ref var spawnComponent = ref spawnBag.GetComponent(i);

            var tankEntity = Instantiate(TankPrefab).Entity;
            tankEntity.SetComponent(posComponent);
            if (spawnComponent.isPlayer) {
                tankEntity.AddComponent<PlayerMarker>();
            }
        }
    }
}