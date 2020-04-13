using Morpeh;
using Tanks.Constants;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct TankKilledEventComponent : IComponent {
    public int lifeCountSpend;
    public Vector3 position;
    public Vector3 respawnPosition;
    public Direction respawnDirection;
}