// PolygonCollision.cs
// 역할: 볼록 다각형 두 개의 충돌 여부를 AABB(가능성 검사) → SAT(정밀 검사) 순서로 판정하는 계산 클래스이다.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace CollisionDetection {
    public static class PolygonCollision {
        public static bool Intersects(Vector2[] verticesA, Vector2[] verticesB) {
            Assert.IsNotNull(verticesA);
            Assert.IsNotNull(verticesB);

            // 1) AABB: 감싸는 사각형조차 안 겹치면 충돌 x (비용 낮은 연산)
            if (!IsAabbOverlapping(verticesA, verticesB)) return false;

            // 2) SAT: 사각형이 겹칠 때만 정밀 검사
            return !HasSeparatingAxis(verticesA, verticesB);
        }

        // AABB (Axis-Aligned Bounding Box)

        private static bool IsAabbOverlapping(Vector2[] verticesA, Vector2[] verticesB) {
            Assert.IsNotNull(verticesA);
            Assert.IsNotNull(verticesB);

            AxisAlignedBoundingBox boxA = CalculateAabb(verticesA);
            AxisAlignedBoundingBox boxB = CalculateAabb(verticesB);
            return boxA.Overlaps(boxB);
        }

        private static AxisAlignedBoundingBox CalculateAabb(Vector2[] vertices) {
            Assert.IsNotNull(vertices);

            Vector2 min = vertices[0];
            Vector2 max = vertices[0];

            foreach (Vector2 vertex in vertices) {
                min = Vector2.Min(min, vertex);
                max = Vector2.Max(max, vertex);
            }

            return new AxisAlignedBoundingBox(min, max);
        }

        // SAT (Separating Axis Theorem)
        // 두 도형을 완전히 갈라놓는 축(분리축)이 하나라도 있으면 true = 충돌 아님
        private static bool HasSeparatingAxis(Vector2[] verticesA, Vector2[] verticesB) {
            Assert.IsNotNull(verticesA);
            Assert.IsNotNull(verticesB);

            foreach (Vector2 axis in GetAxes(verticesA, verticesB)) {
                Projection projectionA = Project(verticesA, axis);
                Projection projectionB = Project(verticesB, axis);
                if (!projectionA.Overlaps(projectionB)) return true;
            }

            return false;
        }

        // 검사할 후보 축 목록(두 도형의 모든 변 법선)
        private static List<Vector2> GetAxes(Vector2[] verticesA, Vector2[] verticesB) {
            Assert.IsNotNull(verticesA);
            Assert.IsNotNull(verticesB);

            var axes = new List<Vector2>(verticesA.Length + verticesB.Length);
            AddEdgeNormals(verticesA, axes);
            AddEdgeNormals(verticesB, axes);
            return axes;
        }

        private static void AddEdgeNormals(Vector2[] vertices, List<Vector2> axes) {
            Assert.IsNotNull(vertices);
            Assert.IsNotNull(axes);

            for (int i = 0; i < vertices.Length; i++) {
                Vector2 edge = vertices[(i + 1) % vertices.Length] - vertices[i]; // 꼭짓점 i → i+1 변
                Vector2 normal = new Vector2(-edge.y, edge.x).normalized;         // (x, y) → (-y, x) = 90° 회전
                axes.Add(normal);
            }
        }

        private static Projection Project(Vector2[] vertices, Vector2 axis) {
            Assert.IsNotNull(vertices);

            float min = Vector2.Dot(vertices[0], axis);
            float max = min;

            for (int i = 1; i < vertices.Length; i++) {
                float value = Vector2.Dot(vertices[i], axis);
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            return new Projection(min, max);
        }

        private readonly struct AxisAlignedBoundingBox {
            public readonly Vector2 min;
            public readonly Vector2 max;

            public AxisAlignedBoundingBox(Vector2 min, Vector2 max) {
                this.min = min;
                this.max = max;
            }

            public bool Overlaps(AxisAlignedBoundingBox other) {
                bool isOverlappingX = max.x >= other.min.x && min.x <= other.max.x;
                bool isOverlappingY = max.y >= other.min.y && min.y <= other.max.y;
                return isOverlappingX && isOverlappingY;
            }
        }

        private readonly struct Projection {
            public readonly float min;
            public readonly float max;

            public Projection(float min, float max) {
                this.min = min;
                this.max = max;
            }

            public bool Overlaps(Projection other) {
                return max >= other.min && min <= other.max;
            }
        }
    }
}
