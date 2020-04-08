using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    public static class EntityHelper {
        public static IEntity Instantiate(EntityProvider entityProvider) {
            var newObj = Object.Instantiate(entityProvider);
            return PrepareNewEntity(newObj);
        }
        
        public static IEntity Instantiate(EntityProvider entityProvider, Vector3 position) {
            var newObj = Object.Instantiate(entityProvider, position, entityProvider.transform.rotation);
            return PrepareNewEntity(newObj);
        }

        public static IEntity PrepareNewEntity(EntityProvider newObj) {
            var newEntity = newObj.Entity;
            newEntity.SetComponent(new GameObjectComponent {
                obj = newObj.gameObject
            });
            return newEntity;
        }
        
        public static void Destroy(IEntity entity, World world) {
            if (entity.Has<GameObjectComponent>()) {
                Object.Destroy(entity.GetComponent<GameObjectComponent>().obj);
            }
            world.RemoveEntity(entity);
        }
    }
}