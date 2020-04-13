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
    private struct PlayerInitInfo {
        public string tankSprite;
        public bool isLocal;
    }
    
    public override void OnAwake() {
        PrepareEntitiesInScene();
        SpawnTanks();
    }

    private static void PrepareEntitiesInScene() {
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
        var network = PhotonNetwork.IsConnected;
        PlayerInitInfo[] players; 
        if (network) {
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
        SpawnTanks(network, players);
        RemoveExtraBases(players);
    }

    private void SpawnTanks(bool networkSpawn, PlayerInitInfo[] players) {
        var filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>();
        
        var spawnBag = filterSpawns.Select<SpawnComponent>();
        var posBag = filterSpawns.Select<PositionComponent>();
        var dirBag = filterSpawns.Select<DirectionComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var spawnComponent = ref spawnBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);
            ref var dirComponent = ref dirBag.GetComponent(i);
            var spawnEntity = filterSpawns.GetEntity(i);

            if (spawnComponent.team < players.Length) {
                var player = players[spawnComponent.team];
                if (networkSpawn && !player.isLocal) {
                    continue;
                }
                
                var tankEntity = ObjectsPool.Main.Take("Tank", posComponent.position, networkSpawn);
                
                ref var spriteComponent = ref tankEntity.GetComponent<SpriteComponent>();
                spriteComponent.spriteDecoder.OverrideBaseSpriteByName(player.tankSprite);
                NetworkHelper.RaiseMyEventToOthers(tankEntity, NetworkEvent.CHANGE_SPRITE, player.tankSprite);
                
                
                tankEntity.SetComponent(dirComponent);
                tankEntity.SetComponent(new LifeComponent {lifeCount = TanksGame.PLAYER_LIFE_COUNT});
                
                tankEntity.SetComponent(new TeamComponent {
                    team = spawnComponent.team
                });
                NetworkHelper.RaiseMyEventToOthers(tankEntity, NetworkEvent.SET_TEAM, spawnComponent.team);
                
                if (player.isLocal) {
                    tankEntity.SetComponent(new LocalControlComponent());
                    if (spawnComponent.isPlayer) {
                        tankEntity.AddComponent<PlayerControlMarker>();
                    }
                }
            }
        }
    }

    private void RemoveExtraBases(PlayerInitInfo[] players) {
        var filterSpawns = this.World.Filter
            .With<BaseComponent>()
            .With<TeamComponent>();
        
        var teamBag = filterSpawns.Select<TeamComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var teamComponent = ref teamBag.GetComponent(i);
            if (teamComponent.team >= players.Length) {
                var entity = filterSpawns.GetEntity(i);
                ObjectsPool.Main.Return(entity, this.World);
            } 
        }
    }
}