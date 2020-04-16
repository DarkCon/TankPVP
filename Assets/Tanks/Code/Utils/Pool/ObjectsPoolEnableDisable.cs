using System.Collections.Generic;
using System.Linq;
using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    public class ObjectsPoolEnableDisable : AbstractPoolWithNetworkSpawn {
        private class PoolElement {
            public bool isInUse;
            public EntityProvider obj;
        }

        private readonly Dictionary<string, List<PoolElement>> objects = new Dictionary<string, List<PoolElement>>();
        private readonly Dictionary<int, PoolElement> entityIdToElementCacheInUse = new Dictionary<int, PoolElement>();
        
        private Transform _poolTransform;
        private Transform PoolTransform {
            get {
                if (this._poolTransform == null) {
                    this._poolTransform = (new GameObject("pool")).transform;
                    this._poolTransform.gameObject.SetActive(false);
                }

                return this._poolTransform;
            }
        }

        public override void AddPrefabs(EntityProvider[] prefabs) {
            foreach (var prefab in prefabs) {
                this.prefabs[prefab.name] = prefab;
                this.objects[prefab.name] = new List<PoolElement>();
            }
        }

        public override void Clean() {
            base.Clean();
            var inUse = this.entityIdToElementCacheInUse.Keys.ToArray();
            this.objects.Clear();
            this.entityIdToElementCacheInUse.Clear();

            var world = World.Default;
            if (world != null) {
                foreach (var entityId in inUse) {
                    var entity = world.GetEntity(entityId);
                    Return(entity, world);
                }
            }

            if (this._poolTransform != null) {
                Object.Destroy(this._poolTransform);
                this._poolTransform = null;
            }
        }

        public override void Return(IEntity entity, World world) {
            if (entity.IsNullOrDisposed())
                return;

            NetworkDestroyIfNeed(entity);
            if (this.entityIdToElementCacheInUse.TryGetValue(entity.ID, out var poolElement)) {
                this.entityIdToElementCacheInUse.Remove(entity.ID);
                poolElement.obj.transform.SetParent(PoolTransform, false);
                poolElement.isInUse = false;
            } else {
                EntityHelper.Destroy(entity, world);
            }
        }

        protected override IEntity TakeLocal(string name, Vector3 position, bool setPosition) {
            PoolElement poolElement = null;
            if (this.objects.TryGetValue(name, out var objsList)) {
                poolElement = objsList.FirstOrDefault(o => !o.isInUse);
                if (poolElement == null && this.prefabs.TryGetValue(name, out var prefab)) {
                    poolElement = new PoolElement {obj = Object.Instantiate(prefab)};   
                    objsList.Add(poolElement);
                }
            }

            if (poolElement == null)
                return null;

            poolElement.isInUse = true;
            poolElement.obj.transform.SetParent(null, false);
            EntityHelper.PrepareNewEntity(poolElement.obj);
            if (setPosition) {
                SafeSetPosition(poolElement.obj, position);
            }

            var entity = poolElement.obj.Entity;
            this.entityIdToElementCacheInUse[entity.ID] = poolElement; 
            return entity;
        }
    }
}