namespace Tanks.Constants {
    public enum NetworkEvent : byte {
        MANUAL_INSTANTIATE,
        CHANGE_SPRITE,
        SET_TEAM,
        SET_HITPOINTS,
        FIRE,
        TANK_KILLED,
        DESTROY,
        UNKNOWN
    }
}