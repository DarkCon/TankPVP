using Morpeh;
using Photon.Pun;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(MoveSyncSystem))]
public sealed class MoveSyncSystem : UpdateSystem {
    public float MinDistanceTeleport = 1f;
    
    private Filter filterSyncPosition;
    
    public override void OnAwake() {
        this.filterSyncPosition = this.World.Filter.With<PositionSyncComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var posSyncBag = this.filterSyncPosition.Select<PositionSyncComponent>();
        for (int i = 0, length = this.filterSyncPosition.Length; i < length; ++i) {
            var entity = this.filterSyncPosition.GetEntity(i);
            ref var posSyncComponent = ref posSyncBag.GetComponent(i);
            
            var extrapolatePos = posSyncComponent.position;
            if (entity.Has<MoveComponent>()) {
                ref var moveComponent = ref entity.GetComponent<MoveComponent>();
                var moveVector = DirectionUtils.GetVector(moveComponent.direction);
                var time = (float) PhotonNetwork.Time - posSyncComponent.time;
                extrapolatePos += moveComponent.speed * time * moveVector;
            }

            if (entity.Has<PositionComponent>()) {
                ref var posComponent = ref entity.GetComponent<PositionComponent>();
                var distance = Vector3.Distance(posComponent.position, extrapolatePos);
                
                if (!posSyncComponent.accepted) {
                    posSyncComponent.distanceOnAccepted = distance;
                    posSyncComponent.accepted = true;
                }

                if (distance > this.MinDistanceTeleport) {
                    posComponent.position = extrapolatePos;
                } else {
                    posComponent.position = Vector3.MoveTowards(posComponent.position, extrapolatePos, 
                        distance * deltaTime * PhotonNetwork.SerializationRate);
                }
            } else {
                entity.SetComponent(new PositionComponent {position = extrapolatePos});
            }
        }
    }
}