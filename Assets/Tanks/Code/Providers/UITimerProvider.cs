using Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.UI;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
public sealed class UITimerProvider : MonoProvider<UITimerComponent> {
    private void Reset() {
        ref var component = ref this.GetData();
        component.text = this.GetComponent<Text>();
    }
}