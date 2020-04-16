using Morpeh;
using Tanks.Utils;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct CollisionEventComponent : IComponent {
    public IEntity otherEntity;
    public PhysicsHelper.Collision contact;
    public float extrude;
}