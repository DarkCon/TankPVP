using System;
using Morpeh;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class TransformProvider : MonoProvider<TransformComponent> {
    private void Reset() {
        ref var component = ref this.GetData();
        component.transform = this.transform;
    }
}