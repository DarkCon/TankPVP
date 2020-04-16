using Morpeh;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(HideSystem))]
public sealed class HideSystem : UpdateSystem {
    private Filter filterHide;
    
    public override void OnAwake() {
        this.filterHide = this.World.Filter
            .With<HiddenComponent>()
            .With<LocalControlComponent>()
            .With<PositionComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var hideBag = this.filterHide.Select<HiddenComponent>();
        var posBag = this.filterHide.Select<PositionComponent>();
        for (int i = 0, length = this.filterHide.Length; i < length; ++i) {
            ref var hiddenComponent = ref hideBag.GetComponent(i);
            
            if (!hiddenComponent.isHided) {
                ref var posComponent = ref posBag.GetComponent(i);
                posComponent.position = TanksGame.HIDE_OBJECT_POSITION;
                
                var entity = this.filterHide.GetEntity(i);
                entity.RemoveComponent<MoveComponent>();
                entity.RemoveComponent<BotMoveCooldownComponent>();
                entity.SetComponent(new FreezeControlMarker());
                hiddenComponent.isHided = true;
            }
        }
    }
}