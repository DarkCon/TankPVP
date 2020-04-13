using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(PlayerLifeSystem))]
public sealed class PlayerLifeSystem : UpdateSystem {
    private Filter filterLifeDestroyed;
    private Filter filterSpawn;
    
    public override void OnAwake() {
        this.filterLifeDestroyed = this.World.Filter
            .With<TeamComponent>()
            .With<LifeComponent>()
            .With<DestroyEventComponent>()
            .With<PositionComponent>();

        this.filterSpawn = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        if (!NetworkHelper.CanUseMasterLogic())
            return;
        
        var lifeBag = this.filterLifeDestroyed.Select<LifeComponent>();
        var posBag = this.filterLifeDestroyed.Select<PositionComponent>();
        var teamBag = this.filterLifeDestroyed.Select<TeamComponent>();
        
        var spawnBag = this.filterSpawn.Select<SpawnComponent>();
        var spawnPosBag = this.filterSpawn.Select<PositionComponent>();
        var spawnDirBag = this.filterSpawn.Select<DirectionComponent>();
        
        for (int i = 0, length = this.filterLifeDestroyed.Length; i < length; ++i) {
            ref var lifeComponent = ref lifeBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);
            ref var teamComponent = ref teamBag.GetComponent(i);
            var entity = this.filterLifeDestroyed.GetEntity(i);
            
            var tankKilledComponent = new TankKilledEventComponent {
                lifeCountSpend = lifeComponent.lifeCount - 1,
                position = posComponent.position 
            };

            for (int j = 0, spawnLength = this.filterSpawn.Length; j < spawnLength; ++j) {
                ref var spawnComponent = ref spawnBag.GetComponent(j);
                if (spawnComponent.isPlayer && spawnComponent.team == teamComponent.team) {
                    tankKilledComponent.respawnPosition = spawnPosBag.GetComponent(j).position;
                    tankKilledComponent.respawnDirection = spawnDirBag.GetComponent(j).direction;
                    break;
                }
            }
            
            entity.SetComponent(tankKilledComponent);
            entity.RemoveComponent<DestroyEventComponent>();
        }
    }
}