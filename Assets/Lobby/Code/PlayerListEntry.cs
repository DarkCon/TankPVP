using System;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Tanks.Constants;
using Tanks.Sprites;

namespace Lobby {
    public class PlayerListEntry : MonoBehaviour {
        [Header("UI References")] public Text PlayerNameText;

        public Image PlayerTankImage;
        public Button PlayerReadyButton;
        public Image PlayerReadyImage;

        [Header("Tank sprite decoder props")] 
        [SerializeField] private int tankSpritesCount;
        [SerializeField] private int nextTankSpriteOffset;

        private int ownerId;
        private bool isPlayerReady;
        private CustomSpriteDecoder spriteDecoder;

#region UNITY

        private void Awake() {
            spriteDecoder = new CustomSpriteDecoder {
                framesCount = tankSpritesCount,
                nextFrameOffset = nextTankSpriteOffset,
                hasDirections = false
            };
            spriteDecoder.Init(PlayerTankImage.sprite);
        }

        public void OnEnable() {
            PlayerNumbering.OnPlayerNumberingChanged += OnPlayerNumberingChanged;
        }

        public void Start() {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId) {
                PlayerReadyButton.gameObject.SetActive(false);
            } else {
                Hashtable initialProps = new Hashtable() {
                    {TanksGame.PLAYER_READY, isPlayerReady},
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
                PhotonNetwork.LocalPlayer.SetScore(0);

                PlayerReadyButton.onClick.AddListener(() => {
                    isPlayerReady = !isPlayerReady;
                    SetPlayerReady(isPlayerReady);

                    Hashtable props = new Hashtable() {{TanksGame.PLAYER_READY, isPlayerReady}};
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    if (PhotonNetwork.IsMasterClient) {
                        FindObjectOfType<LobbyMainPanel>().LocalPlayerPropertiesUpdated();
                    }
                });
            }
        }

        public void OnDisable() {
            PlayerNumbering.OnPlayerNumberingChanged -= OnPlayerNumberingChanged;
        }

        #endregion

        public void Initialize(int playerId, string playerName) {
            ownerId = playerId;
            PlayerNameText.text = playerName;
        }

        private void OnPlayerNumberingChanged() {
            foreach (Player p in PhotonNetwork.PlayerList) {
                if (p.ActorNumber == ownerId) {
                    var tankFrame = Mathf.Clamp(p.GetPlayerNumber(), 0, tankSpritesCount);
                    var tankSprite = spriteDecoder.GetSpriteFrame(Direction.NONE, tankFrame);
                    PlayerTankImage.sprite = tankSprite;
                    p.SetCustomProperties(new Hashtable { 
                        {TanksGame.PLAYER_TANK_SPRITE, tankSprite.name}
                    });
                }
            }
        }

        public void SetPlayerReady(bool playerReady) {
            PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
            PlayerReadyImage.enabled = playerReady;
        }
    }
}