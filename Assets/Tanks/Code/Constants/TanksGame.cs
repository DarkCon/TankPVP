using UnityEngine;

namespace Tanks.Constants {
    public static class TanksGame {
        public const float GAME_START_TIMER = 3f;
        public static readonly Vector3 HIDE_OBJECT_POSITION = Vector2.one * 9000f;

        public const string PLAYER_TANK_SPRITE = "TankSprite";
        public const string PLAYER_READY = "IsPlayerReady";
        public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
    }
}