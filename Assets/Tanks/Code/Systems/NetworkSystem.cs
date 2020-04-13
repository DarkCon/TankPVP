using ExitGames.Client.Photon;
using Morpeh;
using Photon.Pun;
using Photon.Realtime;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(NetworkSystem))]
public sealed class NetworkSystem : UpdateSystem, IOnEventCallback {
    private Filter filterDestroy;
    private Filter filterFire;
    
    public override void OnAwake() {
        PhotonNetwork.AddCallbackTarget(this);
        var filterSync = this.World.Filter.With<NetworkViewComponent>();
        this.filterDestroy = filterSync.With<DestroyEventComponent>();
        this.filterFire = filterSync.With<FireEventComponent>();
    }

    public override void Dispose() {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.Dispose();
    }

    public override void OnUpdate(float deltaTime) {
        var fireBag = this.filterFire.Select<FireEventComponent>();
        for (int i = 0, length = this.filterFire.Length; i < length; ++i) {
            var entity = this.filterFire.GetEntity(i);
            ref var fireComponent = ref fireBag.GetComponent(i);
            NetworkHelper.RaiseMyEventToOthers(entity, NetworkEvent.FIRE, 
                fireComponent.position, (int) fireComponent.direction);
        }
        
        if (PhotonNetwork.IsMasterClient) {
            foreach (var entity in this.filterDestroy) {
                NetworkHelper.RaiseMasterEventToOthers(entity, NetworkEvent.DESTROY);
            }
        }
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code <= (byte) NetworkEvent.MANUAL_INSTANTIATE 
            || photonEvent.Code >= (byte) NetworkEvent.UNKNOWN)
            return;

        if (NetworkHelper.FindEventTarget(photonEvent, out var entity, out var data)) {
            var ev = (NetworkEvent) photonEvent.Code;
            switch (ev) {
                case NetworkEvent.CHANGE_SPRITE:
                    ref var spriteComponent = ref entity.GetComponent<SpriteComponent>();
                    spriteComponent.spriteDecoder.OverrideBaseSpriteByName((string) data[0]);
                    break;
                case NetworkEvent.SET_TEAM:
                    entity.SetComponent(new TeamComponent { team = (int) data[0]});
                    break;
                case NetworkEvent.SET_HITPOINTS:
                    entity.SetComponent(new HitPointsComponent {hitPoints = (int) data[0]});
                    break;
                case NetworkEvent.FIRE:
                    entity.SetComponent(new FireEventComponent { 
                        position = (Vector3) data[0], 
                        direction = (Direction) data[1]}
                    );
                    break;
                case NetworkEvent.DESTROY:
                    entity.SetComponent(new DestroyEventComponent());
                    break;
            }
        }
        
    }
}