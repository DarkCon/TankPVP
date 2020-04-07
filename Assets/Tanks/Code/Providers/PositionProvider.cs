using System;
using Morpeh;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class PositionProvider : MonoProvider<PositionComponent> {
    protected override void Initialize() {
        base.Initialize();
        ResetPositionToTransform();
    }

    private void ResetPositionToTransform() {
        ref var component = ref this.GetData();
        component.position = this.transform.position;
    }
}