using System.Linq;
using Morpeh;
using Photon.Pun;
using Tanks.Constants;
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
    
    private struct PlayerInitInfo {
        public string tankSprite;
        public bool isLocal;
    }
    
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
        PlayerInitInfo[] players; 
        if (PhotonNetwork.IsConnected) {
            players = PhotonNetwork.PlayerList.Select(player => new PlayerInitInfo {
                isLocal = player.IsLocal,
                tankSprite = (string) player.CustomProperties[TanksGame.PLAYER_TANK_SPRITE]
            }).ToArray();
        } else {
            players = new[] {new PlayerInitInfo {
                isLocal = true,
                tankSprite = null
            }};
        }
        SpawnTanks(players);
    }

    private void SpawnTanks(PlayerInitInfo[] players) {
        var filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>();
        
        var spawnBag = filterSpawns.Select<SpawnComponent>();
        var posBag = filterSpawns.Select<PositionComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var posComponent = ref posBag.GetComponent(i);
            ref var spawnComponent = ref spawnBag.GetComponent(i);

            if (spawnComponent.team < players.Length) {
                var player = players[spawnComponent.team];
                var tankEntity = EntityHelper.Instantiate(TankPrefab, posComponent.position);
                
                ref var spriteComponent = ref tankEntity.GetComponent<SpriteComponent>();
                spriteComponent.spriteDecoder.OverrideBaseSpriteByName(player.tankSprite);
                
                if (spawnComponent.isPlayer && player.isLocal) {
                    tankEntity.AddComponent<PlayerControlMarker>();
                }
                
                tankEntity.SetComponent(posComponent);
                tankEntity.SetComponent(new TeamComponent {
                    team = spawnComponent.team
                });
            }
        }
    }
}