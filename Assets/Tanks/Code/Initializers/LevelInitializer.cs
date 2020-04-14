using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Initializers/" + nameof(LevelInitializer))]
public sealed class LevelInitializer : Initializer {
    public override void OnAwake() {
        PrepareEntitiesInScene();
    }

    private static void PrepareEntitiesInScene() {
        foreach (var entityProvider in GameObject.FindObjectsOfType<EntityProvider>()) {
            var entity = entityProvider.Entity;
            if (entity != null) {
                EntityHelper.PrepareNewEntity(entityProvider);
            } else {
                Debug.LogWarning(entityProvider.name);
            }
        }
    }
}