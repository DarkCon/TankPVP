using Morpeh;
using Tanks.Constants;
using UnityEngine;

namespace Tanks.Utils {
    public static class PhysicsHelper {
        public delegate bool OverlapFilter(Collider2D collider, Collider2D other);
        public struct Collision {
            public Collider2D other;
            public ColliderDistance2D distance;
            public Direction direction;
        }
        
        private static RaycastHit2D[] raycastResultCache = new RaycastHit2D[1];
        private static Collider2D[] collidersResutlCache = new Collider2D[5];

        public static bool GetCollision(ObstacleComponent obstacleComponent, Direction dir, float extend,
            out Collision collision, OverlapFilter filter = null) 
        {
            var dirVector = DirectionUtils.GetVector(dir);
            var contactFilter = new ContactFilter2D();

            var collider = obstacleComponent.collider;
            var pos = collider.transform.position;
            var size = collider.size + (Vector2)dirVector * extend;
            
            var count = Physics2D.OverlapBox(pos, size, 0f, contactFilter, collidersResutlCache);
            if (count > 0) {
                collision = new Collision();
                for (int i = 0; i < count; ++i) {
                    var otherCollider = collidersResutlCache[i];
                    if (otherCollider != collider && (filter == null || filter(collider, otherCollider))) {
                        var dist = collider.Distance(otherCollider);
                        if (dist.isValid) {
                            var dirToCollider = GetDirectionToCollider(otherCollider, pos, dir);
                            if (!DirectionUtils.IsOpposite(dir, dirToCollider)
                            && collision.other == null || dist.distance < collision.distance.distance) {
                                collision.other = otherCollider;
                                collision.distance = dist;
                                collision.direction = dirToCollider;
                            }
                        }
                    }
                }

                if (collision.other == null)
                    return false;

                return true;
            }

            collision = default;
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
            if (dirToCollider == Direction.NONE)
                return moveDir;
            
            return dirToCollider;
        }
    }
}