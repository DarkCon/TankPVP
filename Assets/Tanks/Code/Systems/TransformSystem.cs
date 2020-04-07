using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(TransformSystem))]
public sealed class TransformSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter
            .With<PositionComponent>()
            .With<TransformComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var posBag = this.filter.Select<PositionComponent>();
        var unitViewBag = this.filter.Select<TransformComponent>();

        for (int i = 0, length = this.filter.Length; i < length; ++i) {
            ref var posComponent = ref posBag.GetComponent(i);
            ref var unitViewComponent = ref unitViewBag.GetComponent(i);
            unitViewComponent.transform.position = posComponent.position;
        }
    }
}