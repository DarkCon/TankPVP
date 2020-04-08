using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.SceneManagement;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Initializers/" + nameof(LevelInitializer))]
public sealed class LevelInitializer : Initializer {
    public EntityProvider TankPrefab;
    
    public override void OnAwake() {
        PrepareEntitiesInScene();
        SpawnTanks();
    }

    private void PrepareEntitiesInScene() {
        var currentScene = SceneManager.GetActiveScene();
        foreach (var entityProvider in GameObject.FindObjectsOfType<EntityProvider>()) {
            var entity = entityProvider.Entity;
            if (entity != null) {
                EntityHelper.PrepareNewEntity(entityProvider);
            } else {
                Debug.LogWarning(entityProvider.name);
            }
        }
    }

    private void SpawnTanks() {
        var filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>();
        
        var spawnBag = filterSpawns.Select<SpawnComponent>();
        var posBag = filterSpawns.Select<PositionComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var posComponent = ref posBag.GetComponent(i);
            ref var spawnComponent = ref spawnBag.GetComponent(i);

            var tankEntity = EntityHelper.Instantiate(TankPrefab, posComponent.position);
            tankEntity.SetComponent(posComponent);
            if (spawnComponent.isPlayer) {
                tankEntity.AddComponent<PlayerMarker>();
            }
            tankEntity.SetComponent(new TeamComponent {
                team = spawnComponent.team
            });
        }
    }
}