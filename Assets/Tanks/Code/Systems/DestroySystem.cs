using Morpeh;
using Tanks.Constants;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(DestroySystem))]
public sealed class DestroySystem : UpdateSystem {
    private Filter filterKilledTanks;
    private Filter filterDestroy;
    private Filter filterKilledBases;
    
    public override void OnAwake() {
        var filterDestroy = this.World.Filter.With<DestroyEventComponent>();
        
        this.filterDestroy = filterDestroy.Without<BaseComponent>();
        
        this.filterKilledTanks = filterDestroy
            .With<TankComponent>()
            .With<PositionComponent>();
        
        this.filterKilledBases = filterDestroy.With<BaseComponent>().With<SpriteComponent>();
    }

    public override void OnUpdate(float deltaTime) {
        HandleKilledTanks();
        HandleDestroyed();
        HandleKilledBases();
    }

    private void HandleKilledTanks() {
        var posBag = this.filterKilledTanks.Select<PositionComponent>();
        for (int i = 0, length = this.filterKilledTanks.Length; i < length; ++i) {
            var position = posBag.GetComponent(i).position;

            var bangEntity = ObjectsPool.Main.Take("TankBang", position);
        }
    }

    private void HandleDestroyed() {
        foreach (var entity in this.filterDestroy) {
            entity.RemoveComponent<DestroyEventComponent>();
            ObjectsPool.Main.Return(entity, this.World);
        }
    }

    private void HandleKilledBases() {
        var spriteBag = this.filterKilledBases.Select<SpriteComponent>();
        for (int i = 0, length = this.filterKilledBases.Length; i < length; ++i) {
            var spriteComponent = spriteBag.GetComponent(i);
            spriteComponent.spriteRenderer.sprite = spriteComponent.spriteDecoder.GetSprite(Direction.NONE, 1f);
            
            var entity = this.filterKilledBases.GetEntity(i);
            entity.RemoveComponent<DestroyEventComponent>();
        }
    }
}