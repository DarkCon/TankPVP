using Morpeh;
using UnityEngine;

namespace Tanks.Utils {
    public static class PhysicsHelper {
        private static RaycastHit2D[] raycastResultCache = new RaycastHit2D[1];
        
        public static bool CastObstacle(ObstacleComponent obstacleComponent, Vector3 dirVector, float maxDistance, out RaycastHit2D hit) {
            var angle = (Vector2.SignedAngle(Vector2.right, dirVector) + 180f) % 360f;
            var contactFilter = new ContactFilter2D {
                minNormalAngle = angle - 1f,
                maxNormalAngle = angle + 1,
                useNormalAngle = true
            };
            
            if (obstacleComponent.collider.Cast(dirVector, contactFilter, raycastResultCache, maxDistance) > 0) {
                hit = raycastResultCache[0];
                return true;
            }
            
            hit = new RaycastHit2D();
            return false;
        }
        
        public static Vector3 GetPointOnObstacleEdgeOffset(IEntity entity, Vector3 dirVector) {
            if (entity.Has<ObstacleComponent>()) {
                ref var obstacleComponent = ref entity.GetComponent<ObstacleComponent>();
                return  GetPointOnObstacleEdgeOffset(obstacleComponent, dirVector);
            }
            return Vector3.zero;
        }

        public static Vector3 GetPointOnObstacleEdgeOffset(Component component, Vector3 dirVector) {
            if (component.TryGetComponent(out ObstacleProvider obstacleProvider)) {
                ref var obstacleComponent = ref obstacleProvider.GetData();
                return  GetPointOnObstacleEdgeOffset(obstacleComponent, dirVector);
            }
            return Vector3.zero;
        }
        
        public static Vector3 GetPointOnObstacleEdgeOffset(ObstacleComponent obstacleComponent, Vector3 dirVector) {
            var size = obstacleComponent.collider.size * 0.5f;
            var offset = MathHelper.ProjectSize(size, dirVector);
            return offset;
        }
    }
}