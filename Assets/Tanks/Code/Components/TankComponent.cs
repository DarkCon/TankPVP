﻿using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct TankComponent : IComponent {
    public float speed;
    public float fireCooldown;
    public int maxHitPoints;
    public float invulnerabilityTime;
    public ProjectileComponent projectile;
}