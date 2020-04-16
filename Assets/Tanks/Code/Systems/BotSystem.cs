using System;
using System.Collections.Generic;
using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(BotSystem))]
public sealed class BotSystem : FixedUpdateSystem {
    private const int RANDOM_STEPS = 5;

    public float moveCooldown = 1f;
    
    private struct BarrierInfo {
        public bool hasBarrier;
        public bool isDestroyable;
    }
    
    private Filter filterBot;
    private Filter filterBotCooldown;
    private Filter filterTargetBases;
    private Filter filterTargetTanks;

    private readonly Dictionary<int, List<IEntity>> currentBasesTargets = new Dictionary<int, List<IEntity>>();
    private readonly ISet<int> deadTeamsCache = new HashSet<int>();
    private readonly Direction[] directionInPriorityCache = new Direction[3];
    private readonly RaycastHit2D[] raycastResultCache = new RaycastHit2D[1];

    public override void OnAwake() {
        this.filterBot = this.World.Filter
            .With<BotComponent>()
            .With<LocalControlComponent>()
            .Without<FreezeControlMarker>()
            .With<TankComponent>()
            .With<TeamComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>();

        this.filterBotCooldown = this.filterBot.With<BotMoveCooldownComponent>();

        var filterTarget = this.World.Filter
            .With<TeamComponent>()
            .With<HitPointsComponent>()
            .With<PositionComponent>()
            .Without<InvulnerabilityComponent>(); 
        
        this.filterTargetBases = filterTarget
            .With<BaseComponent>();
        this.filterTargetTanks = filterTarget
            .With<TankComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        UpdateAvailableTargets();
        UpdateBots();
        UpdateBotsCooldown(deltaTime);
    }

    private void UpdateAvailableTargets() {
        this.deadTeamsCache.Clear();
        foreach (var pair in this.currentBasesTargets) {
            this.deadTeamsCache.Add(pair.Key);
            pair.Value.Clear();
        }

        var teamBag = this.filterTargetBases.Select<TeamComponent>();
        var hitPointsBag = this.filterTargetBases.Select<HitPointsComponent>();

        for (int i = 0, length = this.filterTargetBases.Length; i < length; ++i) {
            if (hitPointsBag.GetComponent(i).hitPoints > 0) {
                var team = teamBag.GetComponent(i).team;
                var entity = this.filterTargetBases.GetEntity(i);
                
                if (!this.currentBasesTargets.TryGetValue(team, out var list)) {
                     list = new List<IEntity>();
                     currentBasesTargets[team] = list;
                }
                
                list.Add(entity);
                deadTeamsCache.Remove(team);
            }
        }

        foreach (var deadTeam in this.deadTeamsCache) {
            currentBasesTargets.Remove(deadTeam);
        }
    }

    private void UpdateBotsCooldown(float deltaTime) {
        var moveCooldownBag = this.filterBotCooldown.Select<BotMoveCooldownComponent>();

        for (int i = 0, length = this.filterBotCooldown.Length; i < length; ++i) {
            ref var moveCooldownComponent = ref moveCooldownBag.GetComponent(i);
            moveCooldownComponent.time -= deltaTime;
            if (moveCooldownComponent.time <= 0f) {
                var entity = this.filterBotCooldown.GetEntity(i);
                entity.RemoveComponent<BotMoveCooldownComponent>();
            }
        }
    }

    private void UpdateBots() {
        var teamBag = this.filterBot.Select<TeamComponent>();
        var botBag = this.filterBot.Select<BotComponent>();
        var tankBag = this.filterBot.Select<TankComponent>();
        var posBag = this.filterBot.Select<PositionComponent>();
        var dirBag = this.filterBot.Select<DirectionComponent>();
        for (int i = 0, length = this.filterBot.Length; i < length; ++i) {
            ref var botComponent = ref botBag.GetComponent(i);
            ref var tankComponent = ref tankBag.GetComponent(i);
            var team = teamBag.GetComponent(i).team;
            var pos = posBag.GetComponent(i).position;
            var dir = dirBag.GetComponent(i).direction;
            var entity = this.filterBot.GetEntity(i);
            
            SearchTarget(team, ref botComponent);
            
            var barrierInfo = DetectBarrier(entity, team, pos, dir);
            
            if (!entity.Has<BotMoveCooldownComponent>())
                dir = Move(entity, ref botComponent, ref barrierInfo, pos, dir, tankComponent.speed, team);
            
            Fire(entity, barrierInfo, pos, dir, tankComponent.projectile.speed, team);
        }
    }

    private bool SearchTarget(int team, ref BotComponent botComponent) {
        if (!botComponent.target.IsNullOrDisposed() 
            && botComponent.target.GetComponent<HitPointsComponent>().hitPoints > 0) {
            return true;
        }

        var enemyTeam = -1;
        for (int i = 0; i < RANDOM_STEPS; ++i) {
            if (this.currentBasesTargets.Keys.TryRandom(out var otherTeam)) {
                if (otherTeam != team) {
                    enemyTeam = otherTeam;
                    break;
                }
            } else {
                break;
            }
        }

        if (enemyTeam >= 0) {
            botComponent.target = this.currentBasesTargets[enemyTeam].Random();
            return true;
        }

        return false;
    }

    private BarrierInfo DetectBarrier(IEntity entity, int team, Vector3 pos, Direction dir) {
        var info = new BarrierInfo();
        if (entity.Has<CollisionEventComponent>()) {
            ref var collisionComponent = ref entity.GetComponent<CollisionEventComponent>();
            if (collisionComponent.contact.direction == dir) {
                info.hasBarrier = true;
                info.isDestroyable = IsOnFireLine(collisionComponent.contact.other, pos, dir)
                                     && IsCanDestroy(collisionComponent.otherEntity, team);

                if (info.hasBarrier && info.isDestroyable) {
                    entity.RemoveComponent<MoveComponent>();
                    entity.SetComponent(new BotMoveCooldownComponent {time = this.moveCooldown});
                }
            }
        }
        return info;
    }

    private Direction Move(IEntity entity, ref BotComponent botComponent, ref BarrierInfo barrierInfo, 
        Vector3 currPos, Direction currDir, float speed, int team) 
    {
        var newDir = Direction.NONE;
        var targetDir = Direction.NONE;
        if (botComponent.target != null) {
            var targetPos = botComponent.target.GetComponent<PositionComponent>().position;
            targetDir = DirectionUtils.GetDirection(targetPos - currPos);
        }
        
        if (entity.Has<MoveComponent>()) {
            if (barrierInfo.hasBarrier && !barrierInfo.isDestroyable || !DirectionUtils.IsClose(currDir, targetDir)) {
                GetDirectionsPriority(currDir, targetDir, this.directionInPriorityCache);
                for (int i = 0; i < this.directionInPriorityCache.Length; ++i) {
                    var dir = this.directionInPriorityCache[i];
                    if (dir != botComponent.prevDir) {
                        var newBarrierInfo = GetBarrierForMove(entity, currPos, dir, team);
                        if (!newBarrierInfo.hasBarrier || newBarrierInfo.isDestroyable) {
                            newDir = dir;
                            barrierInfo = newBarrierInfo;
                            break;
                        }
                    }
                }
            }
        } else {
            newDir = targetDir;
        }

        if (newDir != Direction.NONE) {
            botComponent.prevDir = currDir;
            entity.SetComponent(new MoveComponent {direction = newDir, speed = speed});
            entity.SetComponent(new BotMoveCooldownComponent {time = this.moveCooldown});
            return newDir;
        }
        return currDir;
    }

    private void Fire(IEntity entity, BarrierInfo barrierInfo, Vector3 currPos, Direction currDir, float projectileSpeed, int team) {
        if (entity.Has<FireCooldownComponent>())
            return;
        
        var needFire = false;
        if (barrierInfo.hasBarrier) {
            needFire = barrierInfo.isDestroyable;
        } else {
            var dirVector = DirectionUtils.GetVector(currDir);
            var rayCastPos = currPos + PhysicsHelper.GetPointOnObstacleEdgeOffset(entity, dirVector);
            if (Physics2D.Raycast(rayCastPos, dirVector, new ContactFilter2D(), this.raycastResultCache, projectileSpeed) > 0) {
                var otherEntity = EntityHelper.FindEntityIn(this.raycastResultCache[0].collider);
                if (IsCanDestroy(otherEntity, team)) {
                    needFire = true;
                }
            }

            if (!needFire) {
                var posBag = this.filterTargetTanks.Select<PositionComponent>();
                for (int i = 0, length = this.filterTargetTanks.Length; i < length && !needFire; ++i) {
                    var otherPos = posBag.GetComponent(i).position;
                    var otherEntity = this.filterTargetTanks.GetEntity(i);
                    if (DirectionUtils.GetDirection(otherPos - currPos) == currDir 
                        && Vector2.Distance(currPos, otherPos) < projectileSpeed 
                        && IsCanDestroy(otherEntity, team, false)) {
                        needFire = true;
                    }
                }
            }
        }

        if (needFire) {
            entity.SetComponent(new WantFireEventComponent {});
        }
    }

    private static void GetDirectionsPriority(Direction currDir, Direction targetDir, Direction[] directions) {
        int i = 0;
        if (targetDir != currDir)
            directions[i++] = targetDir;
        var notCloseDir = currDir;
        foreach (var dir in DirectionUtils.Enumerate()) {
            if (dir == currDir || dir == targetDir)
                continue;
            if (DirectionUtils.IsClose(dir, targetDir)) {
                directions[i++] = dir;
            } else {
                notCloseDir = dir;
            }
        }
        if (i < directions.Length) {
            directions[i] = notCloseDir;
        }
    }

    private static BarrierInfo GetBarrierForMove(IEntity entity, Vector3 pos, Direction dir, int team) {
        var info = new BarrierInfo();
        
        var obstacleComponent = entity.GetComponent<ObstacleComponent>();
        var dirVector = DirectionUtils.GetVector(dir);
        var maxDistance = MathHelper.ProjectSize(obstacleComponent.collider.size, dirVector).magnitude;
        if (PhysicsHelper.CastObstacle(obstacleComponent, dirVector, maxDistance, out var hit) 
            && PhysicsHelper.GetDirectionToCollider(hit.collider, obstacleComponent.collider.transform.position, dir) == dir) {
            info.hasBarrier = true;
            var barrierEntity = EntityHelper.FindEntityIn(hit.collider);
            info.isDestroyable = IsOnFireLine(hit.collider, pos, dir) && IsCanDestroy(barrierEntity, team);
        }
        return info;
    }

    private static bool IsCanDestroy(IEntity targetEntity, int byTeam, bool alsoCheckProjectile = true) {
        if (targetEntity.IsNullOrDisposed())
            return false;
        if (targetEntity.Has<BotDontFireComponent>() &&
            targetEntity.GetComponent<BotDontFireComponent>().team == byTeam)
            return false;
        if (targetEntity.Has<TeamComponent>() &&
            targetEntity.GetComponent<TeamComponent>().team == byTeam)
            return false;
        
        return (targetEntity.Has<HitPointsComponent>() &&
                targetEntity.GetComponent<HitPointsComponent>().hitPoints > 0)
               || 
               alsoCheckProjectile && targetEntity.Has<ProjectileComponent>() && 
               targetEntity.GetComponent<ProjectileComponent>().team != byTeam;
    }

    private static bool IsOnFireLine(Collider2D collider, Vector2 fromPos, Direction fromDir) {
        var closestOnEdge = collider.ClosestPoint(fromPos);
        return Vector2.Angle(DirectionUtils.GetVector(fromDir), closestOnEdge - fromPos) < 0.5f;
    }
}