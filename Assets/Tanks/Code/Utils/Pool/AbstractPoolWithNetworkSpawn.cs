using System.Collections.Generic;
using ExitGames.Client.Photon;
using Morpeh;
using Photon.Pun;
using Photon.Realtime;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Utils {
    public abstract class AbstractPoolWithNetworkSpawn : IObjectsPool, IOnEventCallback {
        protected readonly Dictionary<string, EntityProvider> prefabs = new Dictionary<string, EntityProvider>();

        public virtual void AddPrefabs(EntityProvider[] prefabs) {
            foreach (var prefab in prefabs) {
                this.prefabs[prefab.name] = prefab;
            }
        }

        public virtual void Clean() {
            this.prefabs.Clear();
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

        public abstract void Return(IEntity entity, World world);
        
        protected static void SafeSetPosition(EntityProvider obj, Vector3 position) {
            var entity = obj.Entity; 
            if (entity.Has<PositionComponent>()) {
                ref var positionComponent = ref entity.GetComponent<PositionComponent>();
                positionComponent.position = position;
            }
            obj.transform.position = position;
        }

        protected abstract IEntity TakeLocal(string name, Vector3 position, bool setPosition);

        protected static void NetworkDestroyIfNeed(IEntity entity) {
            if (entity.Has<NetworkViewComponent>()) {
                var photonView = entity.GetComponent<NetworkViewComponent>().photonView;
                if (photonView != null) {
                    if (photonView.ViewID != 0 && photonView.IsMine) {
                        NetworkHelper.RaiseMyEventToOthers(entity, NetworkEvent.DESTROY);
                    }
                    PhotonNetwork.LocalCleanPhotonView(photonView);
                    photonView.ViewID = 0;
                }
            }
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
                photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
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

                var entity = TakeLocal(name, position, true);
                if (entity != null) {
                    var photonView = entity.GetComponent<NetworkViewComponent>().photonView;
                    photonView.ViewID = viewId;
                }
            }
        }
    }
}