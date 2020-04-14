using Morpeh;
using Photon.Pun;
using Tanks.Constants;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(PhotonView))]
public sealed class NetworkViewProvider : MonoProvider<NetworkViewComponent>, IPunObservable {
    [System.Serializable]
    public struct WhatSync {
        public bool Position;
        public bool Direction;
        public bool Move;
    }

    [SerializeField] private WhatSync sync;
    
    private void Reset() {
        ref var component = ref this.GetData();
        var photonView = this.GetComponent<PhotonView>();
        component.photonView = photonView;
        photonView.ObservedComponents.RemoveAll( c => c == null);
        if (!photonView.ObservedComponents.Contains(this))
            photonView.ObservedComponents.Add(this);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        var entity = this.Entity;
        
        if (stream.IsWriting) {
            if (sync.Position) {
                stream.SendNext(entity.GetComponent<PositionComponent>().position);
                stream.SendNext((float) PhotonNetwork.Time);
            }
            if (sync.Direction) {
                stream.SendNext((int)entity.GetComponent<DirectionComponent>().direction);
            }
            if (sync.Move) {
                if (entity.Has<MoveComponent>()) {
                    stream.SendNext(true);
                    stream.SendNext((int) entity.GetComponent<MoveComponent>().direction);
                    stream.SendNext(entity.GetComponent<MoveComponent>().speed);
                } else {
                    stream.SendNext(false);
                }
            }
        } else {
            if (sync.Position) {
                entity.SetComponent(new PositionSyncComponent {
                    position = (Vector3) stream.ReceiveNext(),
                    time = (float) stream.ReceiveNext()
                });
            }
            if (sync.Direction) {
                entity.SetComponent(new  DirectionComponent { direction = (Direction) stream.ReceiveNext()});
            }
            if (sync.Move) {
                if ((bool) stream.ReceiveNext()) {
                    entity.SetComponent(new MoveComponent {
                        direction = (Direction) stream.ReceiveNext(),
                        speed = (float) stream.ReceiveNext()
                    });
                } else {
                    entity.RemoveComponent<MoveComponent>();
                }
            }
        }
    }
}