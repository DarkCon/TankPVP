using Morpeh;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(PlayerSystem))]
public sealed class PlayerSystem : UpdateSystem {
    private Filter filter;
    
    public override void OnAwake() {
        this.filter = this.World.Filter
            .With<PlayerControlMarker>()
            .Without<FreezeControlMarker>()
            .With<TankComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        var moveDirection = GetDirectionFromInput();
        var needFire = GetFireInput();
        foreach (var entity in this.filter) {
            ref var tankComponent = ref entity.GetComponent<TankComponent>(); 
            if (moveDirection != Direction.NONE) {
                entity.SetComponent(new MoveComponent {
                    direction = moveDirection,
                    speed = tankComponent.speed
                });
            } else {
                entity.RemoveComponent<MoveComponent>();
            }

            if (needFire && !entity.Has<FireCooldownComponent>()) {
                entity.SetComponent(new WantFireEventComponent());
            }
        }
    }

    private static Direction GetDirectionFromInput() {
        if (Input.GetKey(KeyCode.W))
            return Direction.UP;
        if (Input.GetKey(KeyCode.S))
            return Direction.DOWN;
        if (Input.GetKey(KeyCode.A))
            return Direction.LEFT;
        if (Input.GetKey(KeyCode.D)) 
            return Direction.RIGHT;
        return Direction.NONE;
    }

    private static bool GetFireInput() {
        return Input.GetKey(KeyCode.Space);
    }
}