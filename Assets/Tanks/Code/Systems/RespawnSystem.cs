using System;
using System.Collections.Generic;
using System.Linq;
using Morpeh;
using Photon.Pun;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(RespawnSystem))]
public sealed class RespawnSystem : UpdateSystem {
    public float BotRespawnTime = 2f;

    private Filter filterHiddenPlayers;
    private Filter filterHiddenBots;
    private Filter filterSpawns;
    private Filter filterSpawnsFree;
    private Filter filterRespawnCooldown;
    
    public override void OnAwake() {
        var filterHidden = this.World.Filter
            .With<TankComponent>()
            .With<LocalControlComponent>()
            .With<HiddenComponent>()
            .With<TeamComponent>()
            .Without<FindedSpawnMarker>();
        
        this.filterHiddenPlayers = filterHidden.With<PlayerControlMarker>();
        
        this.filterHiddenBots = filterHidden.With<BotComponent>();
        
        this.filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>()
            .With<LocalControlComponent>()
            .Without<FreezeControlMarker>();

        this.filterRespawnCooldown = this.filterSpawns.With<RespawnCooldownComponent>();
        
        this.filterSpawnsFree = this.filterSpawns.Without<RespawnCooldownComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        RespawnPlayers();
        RespawnBots();
        UpdateRespawnCooldown(deltaTime);
    }

    private static void Respawn(IEntity entity, IEntity entitySpawn, bool addEmptyMove) {
        var posComponent = entitySpawn.GetComponent<PositionComponent>();
        var dirComponent = entitySpawn.GetComponent<DirectionComponent>();
        ref var tankComponent = ref entity.GetComponent<TankComponent>();
        
        entity.SetComponent(posComponent);
        entity.SetComponent(dirComponent);
        
        entity.SetComponent(new HitPointsComponent {hitPoints = tankComponent.maxHitPoints});
        NetworkHelper.RaiseMyEventToOthers(entity, NetworkEvent.SET_HITPOINTS, tankComponent.maxHitPoints);
        
        entity.SetComponent(new InvulnerabilityComponent {time = tankComponent.invulnerabilityTime});
        NetworkHelper.RaiseMyEventToOthers(entity, NetworkEvent.SET_INVULNERABILITY, 
            (float) PhotonNetwork.Time, 
            tankComponent.invulnerabilityTime);
        
        if (addEmptyMove && !entity.Has<MoveComponent>()) { 
            entity.SetComponent(new MoveComponent { //for collisionSystem
                speed = 0f,
                direction = dirComponent.direction
            });
        }
        entity.RemoveComponent<HiddenComponent>();
        entity.RemoveComponent<FreezeControlMarker>();
        entity.RemoveComponent<FindedSpawnMarker>();
    }

    private void RespawnPlayers() {
        foreach (var entity in filterHiddenPlayers) {
            var team = entity.GetComponent<TeamComponent>().team;
            var entitySpawn = FindFreeSpawn(entity, team, true, true);
            if (entitySpawn != null)
                Respawn(entity, entitySpawn, true);
        }
    }

    private void RespawnBots() {
        if (Mathf.Min(this.filterHiddenBots.Length, this.filterSpawnsFree.Length) == 0)
            return;
        
        var botEntity = this.filterHiddenBots.First();
        var team = botEntity.GetComponent<TeamComponent>().team;
        var respawnTime = this.BotRespawnTime + this.filterRespawnCooldown
                              .Where(e => e.GetComponent<SpawnComponent>().team == team)
                              .Sum(entity => entity.GetComponent<RespawnCooldownComponent>().time);
        
        var entitySpawn = FindFreeSpawn(botEntity, team, false);
        if (entitySpawn != null && !entitySpawn.Has<RespawnCooldownComponent>()) {
            ref var respawnCooldown = ref entitySpawn.AddComponent<RespawnCooldownComponent>();
            ref var posComponent = ref entitySpawn.GetComponent<PositionComponent>();
            respawnCooldown.time = respawnTime;
            respawnCooldown.spawningView = ObjectsPool.Main.Take("SpawningView", posComponent.position, true);
            respawnCooldown.spawnEntity = botEntity;
        }
    }

    private void UpdateRespawnCooldown(float deltaTime) {
        var cooldownBag = this.filterRespawnCooldown.Select<RespawnCooldownComponent>();
        for (int i = 0, length = this.filterRespawnCooldown.Length; i < length; ++i) {
            ref var cooldownComponent = ref cooldownBag.GetComponent(i);
            cooldownComponent.time -= deltaTime;
            if (cooldownComponent.time <= 0f) {
                var entitySpawn = this.filterRespawnCooldown.GetEntity(i);
                
                Respawn(cooldownComponent.spawnEntity, entitySpawn, false);
                
                cooldownComponent.spawningView.SetComponent(new DestroyEventComponent());
                entitySpawn.RemoveComponent<RespawnCooldownComponent>();
            }
        }
    }

    private IEntity FindFreeSpawn(IEntity entity, int team, bool isPlayer, bool anywayReturnSomething = false) {
        bool MyTeam(IEntity e) => e.GetComponent<SpawnComponent>().team == team;
        int TypeOrder(IEntity e) => e.GetComponent<SpawnComponent>().isPlayer == isPlayer ? 0 : 1;
        
        var mySpawns = this.filterSpawnsFree.Where(MyTeam);
        if (anywayReturnSomething)
            mySpawns = mySpawns.Concat(this.filterRespawnCooldown.Where(MyTeam));

        IEntity spawn = null;
        if (entity.Has<ObstacleComponent>()) {
            ref var obstacleComponent = ref entity.GetComponent<ObstacleComponent>();
            foreach (var entitySpawn in mySpawns.OrderBy(TypeOrder)) {
                if (spawn == null && anywayReturnSomething) 
                    spawn = entitySpawn;
                ref var posComponent = ref entitySpawn.GetComponent<PositionComponent>();
                if (!PhysicsHelper.GetCollision(obstacleComponent, posComponent.position, Direction.NONE, 0f,
                    out _, FilterCollisionAllExceptProjectiles)) {
                    spawn = entitySpawn;
                    break;
                }
            }
        }
        
        if (spawn != null) {
            entity.SetComponent(new FindedSpawnMarker());
        }
        return spawn;
    }

    private static bool FilterCollisionAllExceptProjectiles(Collider2D collider, Collider2D other) {
        var otherEntity = EntityHelper.FindEntityIn(other);
        return otherEntity == null || !otherEntity.Has<ProjectileComponent>();
    }
}