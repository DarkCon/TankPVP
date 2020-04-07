using System;
using Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class ObstacleProvider : MonoProvider<ObstacleComponent> {
    private void Reset() {
        ref var component = ref this.GetData();
        component.collider = this.GetComponent<BoxCollider2D>();
    }
}