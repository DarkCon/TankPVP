using ExitGames.Client.Photon;
using Morpeh;
using Photon.Pun;
using Photon.Realtime;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Utils {
    public static class Network {
        public static void RaiseMasterEvent(IEntity entity, NetworkEvent ev, params object[] addData) {
            RaiseEvent(false, entity, ev, addData);
        }

        public static void RaiseMyEvent(IEntity entity, NetworkEvent ev, params object[] addData) {
            RaiseEvent(true, entity, ev, addData);
        }
        
        private static void RaiseEvent(bool onlyIfMyEntity, IEntity entity, NetworkEvent ev, params object[] addData) {
            if (!entity.Has<NetworkViewComponent>()) {
                Debug.LogError("Entity must has NetworkViewComponent for RaiseEvents");
                return;
            }
            var photonView = entity.GetComponent<NetworkViewComponent>().photonView;
            if (onlyIfMyEntity && !photonView.IsMine)
                return;
            
            object[] data = {
                photonView.ViewID,
                addData
            };
            var raiseEventOptions = new RaiseEventOptions {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache
            };
            var sendOptions = new SendOptions {
                Reliability = true
            };
            PhotonNetwork.RaiseEvent((byte) ev, data, raiseEventOptions, sendOptions);
        }

        public static bool FindEventTarget(EventData photonEvent, out IEntity entity, out object[] data) {
            data = (object[]) photonEvent.CustomData;
            var viewId = (int) data[0];
            var photonView = PhotonNetwork.GetPhotonView(viewId);
            if (photonView != null) {
                var provider = photonView.GetComponent<EntityProvider>();
                if (provider != null) {
                    entity = provider.Entity;
                    if (entity != null) {
                        data = (object[]) data[1];
                        return true;
                    }
                }
            }

            entity = null;
            return false;
        }

        public static bool CanUseMasterLogic() {
            return !PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient;
        }
    }
}