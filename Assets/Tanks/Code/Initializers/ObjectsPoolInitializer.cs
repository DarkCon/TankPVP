using System;
using Morpeh;
using Photon.Pun;
using Tanks;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Initializers/" + nameof(ObjectsPoolInitializer))]
public sealed class ObjectsPoolInitializer : Initializer {
    public EntityProvider[] prefabs;
    
    public override void OnAwake() {
        var pool = ObjectsPool.Main;
        pool.AddPrefabs(prefabs);
        PhotonNetwork.AddCallbackTarget(pool);
    }

    public override void Dispose() {
        var pool = ObjectsPool.Main;
        PhotonNetwork.RemoveCallbackTarget(pool);
        pool.Clean();
    }
}