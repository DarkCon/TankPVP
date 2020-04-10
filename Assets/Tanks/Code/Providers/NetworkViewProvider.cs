using System;
using Morpeh;
using Photon.Pun;
using Tanks.Utils;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(PhotonView))]
public sealed class NetworkViewProvider : MonoProvider<NetworkViewComponent>, IPunObservable {
    private void Reset() {
        ref var component = ref this.GetData();
        var photonView = this.GetComponent<PhotonView>();
        component.photonView = photonView;
        photonView.ObservedComponents.RemoveAll( c => c == null);
        if (!photonView.ObservedComponents.Contains(this))
            photonView.ObservedComponents.Add(this);
        
        photonView.RefreshRpcMonoBehaviourCache();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        var entity = this.Entity;
        
        if (stream.IsWriting) {
            stream.SendNext(entity.GetComponent<PositionComponent>());
        } else {
            entity.SetComponent((PositionComponent) stream.ReceiveNext());
        }
    }
}