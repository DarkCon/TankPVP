using System.Reflection;
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
            /*if (newEntity == null && newObj.gameObject.activeInHierarchy) {
                newEntity = ForceCreateEntityNow(newObj);
            }*/
            newEntity.SetComponent(new GameObjectComponent {
                obj = newObj.gameObject
            });
            return newEntity;
        }
        
        /*private static IEntity ForceCreateEntityNow(EntityProvider provider) {
            var forceRecreateEntity = provider.GetType().GetMethod("OnEnable",
                BindingFlags.NonPublic | BindingFlags.Instance);
            forceRecreateEntity.Invoke(provider, new object[0]);
            return provider.Entity;
        }*/
        
        public static void Destroy(IEntity entity, World world) {
            if (entity.Has<GameObjectComponent>()) {
                Object.Destroy(entity.GetComponent<GameObjectComponent>().obj);
            }
            world.RemoveEntity(entity);
        }

        public static IEntity FindEntityIn(Component unityComponent) {
            var provider = unityComponent.GetComponent<EntityProvider>() ??
                           unityComponent.transform.GetComponentInParent<EntityProvider>();
            return provider != null ? provider.Entity : null;
        }
    }
}