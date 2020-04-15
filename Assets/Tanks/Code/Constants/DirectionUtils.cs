using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tanks.Constants {
    public static class DirectionUtils {
        public static Vector3 GetVector(Direction direction) {
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
        
        public static Direction GetDirection(Vector2 vector) {
            if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) {
                if (vector.x < 0f)
                    return Direction.LEFT;
                return Direction.RIGHT;
            } else {
                if (vector.y > 0f)
                    return Direction.UP;
                return Direction.DOWN;
            }
        }

        public static Direction GetOpposite(Direction direction) {
            switch (direction) {
                case Direction.UP:
                    return Direction.DOWN;
                case Direction.DOWN:
                    return Direction.UP;
                case Direction.LEFT:
                    return Direction.RIGHT;
                case Direction.RIGHT:
                    return Direction.LEFT;
                default:
                    return Direction.NONE;
            }
        }

        public static bool IsClose(Direction d1, Direction d2) {
            return d1 != GetOpposite(d2);
        }

        public static IEnumerable<Direction> Enumerate() {
            yield return Direction.UP;
            yield return Direction.DOWN;
            yield return Direction.LEFT;
            yield return Direction.RIGHT;
        }
    }
}