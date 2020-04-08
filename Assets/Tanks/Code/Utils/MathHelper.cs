using UnityEngine;

namespace Tanks.Utils {
    public static class MathHelper {
        public static Vector2 Abs(Vector2 v) {
            return new Vector2(
                Mathf.Abs(v.x),
                Mathf.Abs(v.y)
            );
        }
        
        public static Vector3 Abs(Vector3 v) {
            return new Vector3(
                Mathf.Abs(v.x),
                Mathf.Abs(v.y),
                Mathf.Abs(v.z)
            );
        }
        
        public static Vector3 ProjectSize(Vector3 size, Vector3 onNormal) {
            var dotSelf = Vector3.Dot(onNormal, onNormal);
            if ((double) dotSelf < (double) Mathf.Epsilon)
                return Vector3.zero;
            var dotSize = Vector3.Dot(size, onNormal);
            return onNormal * Mathf.Abs(dotSize / dotSelf);
        }
    }
}