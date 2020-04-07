using UnityEngine;

namespace Tanks.Constants {
    public static class DirectionVector {
        public static Vector3 Get(Direction direction) {
            switch (direction) {
                case Direction.UP:
                    return Vector3.up;
                case Direction.DOWN:
                    return Vector3.down;
                case Direction.LEFT:
                    return Vector3.left;
                case Direction.RIGHT:
                    return Vector3.right;
                default:
                    return Vector3.zero;
            }
        }
    }
}