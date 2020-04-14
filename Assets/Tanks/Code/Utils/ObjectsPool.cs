using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Morpeh;
using Photon.Pun;
using Photon.Realtime;
using Tanks.Constants;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tanks.Utils {
    public class ObjectsPool : IOnEventCallback {
        private class PoolElement {
            public bool isInUse;
            public EntityProvider obj;
        }
        
        private readonly Dictionary<string, EntityProvider> prefabs = new Dictionary<string, EntityProvider>();
        private readonly Dictionary<string, List<PoolElement>> objects = new Dictionary<string, List<PoolElement>>();
        private readonly Dictionary<int, PoolElement> entityIdToElementCacheInUse = new Dictionary<int, PoolElement>();

        private static ObjectsPool _main;

        public static ObjectsPool Main {
            get {
                if (_main == null) {
                    _main = new ObjectsPool();
                }
                return _main;
            }
        }

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

        public void AddPrefabs(EntityProvider[] prefabs) {
            foreach (var prefab in prefabs) {
                this.prefabs[prefab.name] = prefab;
                this.objects[prefab.name] = new List<PoolElement>();
            }
        }

        public void Clean() {
            this.prefabs.Clear();
            this.objects.Clear();
            if (this._poolTransform != null) {
                Object.Destroy(this._poolTransform);
                this._poolTransform = null;
            }
        }

        public IEntity Take(string name, bool networkSpawn = false) {
            var entity = TakeLocal(name, Vector3.zero, false);
            if (entity != null && networkSpawn)
                NetworkSpawn(name, entity);
            return entity;
        }
        
        public IEntity Take(string name, Vector3 inPosition, bool networkSpawn = false) {
            var entity = TakeLocal(name, inPosition, true);
            if (entity != null && networkSpawn)
                NetworkSpawn(name, entity);
            return entity;
        }

        public void Return(IEntity entity, World world) {
            if (entity.IsNullOrDisposed())
                return;

            if (this.entityIdToElementCacheInUse.TryGetValue(entity.ID, out var poolElement)) {
                this.entityIdToElementCacheInUse.Remove(entity.ID);
                poolElement.obj.transform.SetParent(PoolTransform, false);
                poolElement.isInUse = false;
            } else {
                EntityHelper.Destroy(entity, world);
            }
        } 

        private IEntity TakeLocal(string name, Vector3 position, bool setPosition) {
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

        private static void SafeSetPosition(EntityProvider obj, Vector3 position) {
            var entity = obj.Entity; 
            if (entity.Has<PositionComponent>()) {
                ref var positionComponent = ref entity.GetComponent<PositionComponent>();
                positionComponent.position = position;
            }
            obj.transform.position = position;
        }

        private static void NetworkSpawn(string name, IEntity entity) {
            if (!entity.Has<NetworkViewComponent>()) {
                Debug.LogError("Entity must has NetworkViewComponent for NetworkRespawn");
                return;
            }
            var photonView = entity.GetComponent<NetworkViewComponent>().photonView;

            if (PhotonNetwork.AllocateViewID(photonView)) {
                object[] data = {
                    photonView.ViewID,
                    name,
                    photonView.transform.position
                };
                var raiseEventOptions = new RaiseEventOptions {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCache
                };
                var sendOptions = new SendOptions {
                    Reliability = true
                };

                PhotonNetwork.RaiseEvent((byte) NetworkEvent.MANUAL_INSTANTIATE, data, raiseEventOptions, sendOptions);
            } else {
                Debug.LogError("Failed to allocate a ViewId.");
            }
        }

        public void OnEvent(EventData photonEvent) {
            if (photonEvent.Code == (byte)NetworkEvent.MANUAL_INSTANTIATE) {
                object[] data = (object[]) photonEvent.CustomData;
                var viewId = (int) data[0];
                var name = (string) data[1];
                var position = (Vector3) data[2];

                var entity = TakeLocal(name, position, false);
                if (entity != null) {
                    var photonView = entity.GetComponent<NetworkViewComponent>().photonView;
                    photonView.ViewID = viewId;
                }
            }
        }
    }
}