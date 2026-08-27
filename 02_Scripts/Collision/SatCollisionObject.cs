// SatCollisionObject.cs
// 역할: 오브젝트의 PolygonCollider2D에서 꼭짓점을 월드 좌표로 추출하여, 충돌 연산에 필요한 정보를 CollisionManager에 제공한다.

using System.Linq;
using UnityEngine;

namespace CollisionDetection {
    public class SatCollisionObject : MonoBehaviour {
        private PolygonCollider2D polygonCollider;

        private void Start() {
            polygonCollider = GetComponent<PolygonCollider2D>();
        }

        public Vector2[] GetVertices() {
            if (polygonCollider == null) return null;

            return polygonCollider.points
                .Select(point => (Vector2)transform.TransformPoint(point))
                .ToArray();
        }
    }
}
