using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Morpeh;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using Photon.Realtime;
using Tanks.Constants;
using Tanks.Sprites;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.SceneManagement;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(GameManagerSystem))]
public sealed class GameManagerSystem : UpdateSystem, IInRoomCallbacks {
    public Sprite[] botSprites;

    private const string GAME_START_TIMER = "GameStartTimer";
    
    private struct PlayerInitInfo {
        public string tankSprite;
        public bool isLocal;
        public string name;
        public bool onlyPlayer;
        public bool onlyBots;
    }
    
    private enum GameStage {
        WAIT_ALL_PLAYERS,
        INIT_START_TIMER,
        START_TIMER,
        GAME,
        GAME_OVER
    }

    private GameStage stage;
    private IEntity startTimerEntity;
    private UIGameOverProvider gameOverProvider;
    private float timerNetworkStartedTime = -1f;
    private PlayerInitInfo[] playersInfo;
    
    private Filter filterLocalControlFreezed;
    private Filter filterLocalControlFree;
    private Filter filterTanks;
    private Filter filterBases;

    public override void OnAwake() {
        PhotonNetwork.AddCallbackTarget(this);
        SetLoadedLevel(true);
        
        var filterStartTimer = this.World.Filter.With<UITimerComponent>();
        if (filterStartTimer.Length > 0) {
            this.startTimerEntity = filterStartTimer.First();
        }

        this.gameOverProvider = this.World.Filter
            .With<UIGameOverComponent>().First()
            .GetComponent<GameObjectComponent>().obj.GetComponent<UIGameOverProvider>();
        gameOverProvider.gameObject.SetActive(false);

        var filterLocalControl = this.World.Filter.With<LocalControlComponent>();
        this.filterLocalControlFreezed = filterLocalControl.With<FreezeControlMarker>();
        this.filterLocalControlFree = filterLocalControl.Without<FreezeControlMarker>();

        var filterTeamHP = this.World.Filter
            .With<TeamComponent>()
            .With<HitPointsComponent>();
        this.filterTanks = filterTeamHP.With<TankComponent>();
        this.filterBases = filterTeamHP.With<BaseComponent>();

        this.stage = GameStage.WAIT_ALL_PLAYERS;
    }

    public override void Dispose() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public override void OnUpdate(float deltaTime) {
        switch (stage) {
            case GameStage.WAIT_ALL_PLAYERS:
                if (CheckAllPlayerLoadedLevel()) {
                    InitUnits();
                    FreezeControl();
                    ++stage;
                }
                break;
            case GameStage.INIT_START_TIMER:
                if (InitStartTimer()) {
                    stage = GameStage.START_TIMER;
                } else {
                    UnFreezeControl();
                    stage = GameStage.GAME;
                }
                break;
            case GameStage.START_TIMER:
                if (!UpdateTimer()) {
                    UnFreezeControl();
                    ++stage;
                }
                break;
            case GameStage.GAME:
                if (CheckGameOver(out var teamWinner)) {
                    FreezeControl();
                    ShowGameResult(teamWinner);
                    ++stage;
                };
                break;
        }
    }

#region Init Units
    private void InitUnits() {
        var network = PhotonNetwork.IsConnected;
        
        if (network) {
            this.playersInfo = PhotonNetwork.PlayerList.Select(player => new PlayerInitInfo {
                isLocal = player.IsLocal,
                name = player.NickName,
                tankSprite = (string) player.CustomProperties[TanksGame.PLAYER_TANK_SPRITE]
            }).ToArray();
        } else {
            this.playersInfo = new[] {new PlayerInitInfo {isLocal = true}};
        }

        if (PhotonNetwork.PlayerList.Length < 2) {
            this.playersInfo[0].onlyPlayer = true;
            var playerWithBot = new List<PlayerInitInfo>(this.playersInfo);
            playerWithBot.Add(new PlayerInitInfo {isLocal = true, onlyBots = true});
            SpawnTanks(network, playerWithBot);
        } else {
            SpawnTanks(network, this.playersInfo);
        }
        
        RemoveExtraBases(this.playersInfo);
    }
    
    private void SpawnTanks(bool networkSpawn, IReadOnlyList<PlayerInitInfo> players) {
        var filterSpawns = this.World.Filter
            .With<SpawnComponent>()
            .With<PositionComponent>()
            .With<DirectionComponent>();
        
        var spawnBag = filterSpawns.Select<SpawnComponent>();
        var posBag = filterSpawns.Select<PositionComponent>();
        var dirBag = filterSpawns.Select<DirectionComponent>();
        for (int i = 0, length = filterSpawns.Length; i < length; ++i) {
            ref var spawnComponent = ref spawnBag.GetComponent(i);
            ref var posComponent = ref posBag.GetComponent(i);
            ref var dirComponent = ref dirBag.GetComponent(i);

            if (spawnComponent.team < players.Count) {
                var player = players[spawnComponent.team];
                var isPlayer = !player.onlyBots && spawnComponent.isPlayer;

                if (networkSpawn && !player.isLocal || player.onlyPlayer && !isPlayer) {
                    continue;
                }

                
                var tankPrefabName = isPlayer ? "Tank" : "TankBot";
                var tankEntity = ObjectsPool.Main.Take(tankPrefabName, posComponent.position, networkSpawn);

                if (!player.onlyBots) {
                    ref var spriteComponent = ref tankEntity.GetComponent<SpriteComponent>();
                    var tankSprite = isPlayer ? player.tankSprite : this.botSprites[spawnComponent.team].name;
                    spriteComponent.spriteDecoder.OverrideBaseSpriteByName(tankSprite);
                    NetworkHelper.RaiseMyEventToOthers(tankEntity, NetworkEvent.CHANGE_SPRITE, tankSprite);
                }

                tankEntity.SetComponent(dirComponent);
                
                tankEntity.SetComponent(new TeamComponent {
                    team = spawnComponent.team
                });
                NetworkHelper.RaiseMyEventToOthers(tankEntity, NetworkEvent.SET_TEAM, spawnComponent.team);
                
                if (player.isLocal) {
                    tankEntity.SetComponent(new LocalControlComponent());
                    tankEntity.AddComponent<FreezeControlMarker>();
                    if (isPlayer) {
                        tankEntity.AddComponent<PlayerControlMarker>();
                    } else {
                        tankEntity.AddComponent<BotComponent>();
                    }
                }
            }
        }
    }

    private void RemoveExtraBases(PlayerInitInfo[] players) {
        var teamBag = this.filterBases.Select<TeamComponent>();
        for (int i = 0, length = this.filterBases.Length; i < length; ++i) {
            ref var teamComponent = ref teamBag.GetComponent(i);
            if (teamComponent.team >= players.Length) {
                var entity = this.filterBases.GetEntity(i);
                ObjectsPool.Main.Return(entity, this.World);
            } 
        }
    }

    private void FreezeControl() {
        foreach (var entity in this.filterLocalControlFree) {
            entity.RemoveComponent<MoveComponent>();
            entity.AddComponent<FreezeControlMarker>();
        }
    }

    private void UnFreezeControl() {
        foreach (var entity in this.filterLocalControlFreezed) {
            entity.RemoveComponent<FreezeControlMarker>();
        }
    }

#endregion

#region Start timer
    private bool InitStartTimer() {
        if (startTimerEntity == null) {
            return false;
        }

        if (PhotonNetwork.IsConnected) {
            SyncGameStartTimer((float) PhotonNetwork.Time);
        } else {
            this.startTimerEntity.GetComponent<GameObjectComponent>().obj.SetActive(false);
            return false;
        }

        return true;
    }

    private bool UpdateTimer() {
        if (this.timerNetworkStartedTime < 0f)
            return true;
        
        var time = (float) PhotonNetwork.Time - this.timerNetworkStartedTime;
        var countdown = TanksGame.GAME_START_TIMER - time;
        
        ref var uiTimerComponent = ref this.startTimerEntity.GetComponent<UITimerComponent>();
        uiTimerComponent.text.text = Mathf.Max(0, Mathf.CeilToInt(countdown)).ToString();

        if (countdown <= 0f) {
            this.startTimerEntity.GetComponent<GameObjectComponent>().obj.SetActive(false);
            this.startTimerEntity = null;
            return false;
        }

        return true;
    }

#endregion

#region Game handle
    private bool CheckGameOver(out int teamWinner) {
        teamWinner = -1;
        
        if (IsHasAlive(this.filterBases, out var teamAlive, out var onlyOne, true)) {
            if (onlyOne && this.playersInfo.Length > 1) {
                teamWinner = teamAlive;
                return true;
            }
        } else {
            return true;
        }

        if (IsHasAlive(this.filterTanks, out teamAlive, out onlyOne, false)) {
            if (onlyOne) {
                teamWinner = teamAlive;
                return true;
            }
        } else {
            return true;
        }

        return false;
    }

    private static bool IsHasAlive(Filter filter, out int teamAlive, out bool onlyOne, bool checkHitPoints) {
        var hasAlive = false;
        onlyOne = false;
        teamAlive = -1;
        
        var teamBag = filter.Select<TeamComponent>();
        var hitPointBag = filter.Select<HitPointsComponent>();
        for (int i = 0, length = filter.Length; i < length; ++i) {
            ref var hitPointComponent = ref hitPointBag.GetComponent(i);
            if (!checkHitPoints || hitPointComponent.hitPoints > 0) {
                hasAlive = true;
                var team = teamBag.GetComponent(i).team;
                if (teamAlive < 0 || team == teamAlive) {
                    teamAlive = team;
                    onlyOne = true;
                } else {
                    onlyOne = false;
                    break;
                }
            }
        }

        return hasAlive;
    }

    private void ShowGameResult(int teamWinner) {
        gameOverProvider.gameObject.SetActive(true);
        
        ref var uiGameOverComponent = ref gameOverProvider.Entity.GetComponent<UIGameOverComponent>();

        var hasWinner = teamWinner >= 0 && teamWinner < this.playersInfo.Length;

        uiGameOverComponent.gameOverPnl.SetActive(!hasWinner);
        uiGameOverComponent.winnerPnl.SetActive(hasWinner);
        if (hasWinner) {
            var playerInfo = this.playersInfo[teamWinner];
            if (!string.IsNullOrEmpty(playerInfo.tankSprite)) {
                uiGameOverComponent.imgWinner.sprite = SpritesCache.FindInLoaded(playerInfo.tankSprite);
                uiGameOverComponent.imgWinner.gameObject.SetActive(true);
            } else {
                uiGameOverComponent.imgWinner.gameObject.SetActive(false);
            }
            if (this.playersInfo.Length > 1) {
                uiGameOverComponent.txtWinnerName.text = playerInfo.name;   
            }
        }
        SyncGameStartTimer(-1f);
        SetLoadedLevel(false);
        
        uiGameOverComponent.btnNext.onClick.AddListener(OnBtnNext);
    }

    private void OnBtnNext() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
#endregion

#region Network handle
    private static void SyncGameStartTimer(float time) {
        if (PhotonNetwork.IsMasterClient) {
            var props = new Hashtable {
                {GAME_START_TIMER, time}
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }
    
    private static void SetLoadedLevel(bool loaded) {
        var props = new Hashtable {
            {TanksGame.PLAYER_LOADED_LEVEL, loaded}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private static bool CheckAllPlayerLoadedLevel() {
        foreach (var p in PhotonNetwork.PlayerList) {
            if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_LOADED_LEVEL, out var playerLoadedLevel)) {
                if ((bool) playerLoadedLevel) {
                    continue;
                }
            }
            return false;
        }
        return true;
    }

    public void OnPlayerEnteredRoom(Player newPlayer) { }

    public void OnPlayerLeftRoom(Player otherPlayer) { }

    public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        if (propertiesThatChanged.TryGetValue(GAME_START_TIMER, out var startTimeFromProps)) {
            this.timerNetworkStartedTime = (float)startTimeFromProps;
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) { }

    public void OnMasterClientSwitched(Player newMasterClient) { }

#endregion
    
}