using Morpeh;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Utils {
    public static class PhysicsHelper {
        public struct Overlap2D {
            public Collider2D other;
            public ColliderDistance2D distance;
        }
        
        private static RaycastHit2D[] raycastResultCache = new RaycastHit2D[1];
        private static Collider2D[] collidersResutlCache = new Collider2D[4];

        public static bool GetOverlapForward(ObstacleComponent obstacleComponent, Vector3 dirVector, out Overlap2D overlap) {
            var contactFilter = CreateContactFilterForward(dirVector);

            var collider = obstacleComponent.collider; 
            var count = collider.OverlapCollider(contactFilter, collidersResutlCache);
            if (count > 0) {
                overlap = new Overlap2D{
                    other = collidersResutlCache[0],
                    distance =  obstacleComponent.collider.Distance(collidersResutlCache[0])
                };
                var minDistanceToB = Vector3.Distance(overlap.distance.pointB, collider.transform.position);
                for (int i = 1; i < count; ++i) {
                    var dist = obstacleComponent.collider.Distance(collidersResutlCache[i]);
                    var distToB = Vector3.Distance(dist.pointB, collider.transform.position);  
                    if (distToB < minDistanceToB) {
                        overlap.distance = dist;
                        overlap.other = collidersResutlCache[i];
                        minDistanceToB = distToB;
                    }
                }

                var rect = new Rect {
                    size = collider.size * 1.1f,
                    center = collider.transform.position,
                };
                if (!rect.Contains(overlap.distance.pointA)) //collider not updated;
                    return false;

                return true;
            }

            overlap = default;
            return false;
        }
        
        public static bool CastObstacle(ObstacleComponent obstacleComponent, Vector3 dirVector, float maxDistance, out RaycastHit2D hit) {
            var contactFilter = CreateContactFilterForward(dirVector);
            
            if (obstacleComponent.collider.Cast(dirVector, contactFilter, raycastResultCache, maxDistance) > 0) {
                hit = raycastResultCache[0];
                return true;
            }
            
            hit = new RaycastHit2D();
            return false;
        }

        private static ContactFilter2D CreateContactFilterForward(Vector3 dirVector) {
            var angle = (Vector2.SignedAngle(Vector2.right, dirVector) + 180f) % 360f;
            return new ContactFilter2D {
                minNormalAngle = angle - 1f,
                maxNormalAngle = angle + 1,
                useNormalAngle = true
            };
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

        public static Direction GetDirectionToCollider(Collider2D collider, Vector2 fromPosition, Direction moveDir) {
            var posOnEdge = collider.ClosestPoint(fromPosition);
            var dirToCollider = DirectionUtils.GetDirection(posOnEdge - fromPosition);
            if (!DirectionUtils.IsClose(dirToCollider, moveDir))
                dirToCollider = moveDir;
            
            return dirToCollider;
        }
    }
}