using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    public class ObjectsPoolFake : AbstractPoolWithNetworkSpawn {
        public override void Return(IEntity entity, World world) {
            if (entity.IsNullOrDisposed())
                return;
            
            CleanNetworkViewIdIfNeed(entity);
            EntityHelper.Destroy(entity, world);
        }

        protected override IEntity TakeLocal(string name, Vector3 position, bool setPosition) {
            if (this.prefabs.TryGetValue(name, out var prefab)) {
                var obj = Object.Instantiate(prefab);   
                EntityHelper.PrepareNewEntity(obj);
                if (setPosition) {
                    SafeSetPosition(obj, position);
                }
                return obj.Entity;
            }

            return null;
        }
    }
}