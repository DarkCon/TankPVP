using System.Linq;
using Morpeh;
using Photon.Pun;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.SceneManagement;
using Network = Tanks.Utils.Network;

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
        if (PhotonNetwork.IsConnected) {
            SpawnTanks(true, PhotonNetwork.PlayerList.Select(player => new PlayerInitInfo {
                isLocal = player.IsLocal,
                tankSprite = (string) player.CustomProperties[TanksGame.PLAYER_TANK_SPRITE]
            }).ToArray());
        } else {
            SpawnTanks(false, new[] {new PlayerInitInfo {
                isLocal = true,
                tankSprite = null
            }});
        }
    }

    private void SpawnTanks(bool networkSpawn, PlayerInitInfo[] players) {
        var filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>();
        
        var spawnBag = filterSpawns.Select<SpawnComponent>();
        var posBag = filterSpawns.Select<PositionComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var posComponent = ref posBag.GetComponent(i);
            ref var spawnComponent = ref spawnBag.GetComponent(i);
            var spawnEntity = filterSpawns.GetEntity(i);

            if (spawnComponent.team < players.Length) {
                var player = players[spawnComponent.team];
                if (networkSpawn && !player.isLocal) {
                    continue;
                }
                
                var tankEntity = ObjectsPool.Main.Take("Tank", posComponent.position, networkSpawn);
                
                ref var spriteComponent = ref tankEntity.GetComponent<SpriteComponent>();
                spriteComponent.spriteDecoder.OverrideBaseSpriteByName(player.tankSprite);
                Network.RaiseMyEvent(tankEntity, NetworkEvent.CHANGE_SPRITE, player.tankSprite);
                
                if (spawnEntity.Has<DirectionComponent>()) {
                    tankEntity.SetComponent(spawnEntity.GetComponent<DirectionComponent>());
                }
                if (spawnComponent.isPlayer && player.isLocal) {
                    tankEntity.AddComponent<PlayerControlMarker>();
                }
                tankEntity.SetComponent(new TeamComponent {
                    team = spawnComponent.team
                });
                tankEntity.SetComponent(new LocalControlComponent());
                
                Network.RaiseMyEvent(tankEntity, NetworkEvent.SET_TEAM, spawnComponent.team);
            }
        }
    }
}