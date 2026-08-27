// CollisionManager.cs
// 역할: 충돌 가능한 오브젝트(Player/Enemy/TimeZone/Bullet)를 등록/관리하고,
// 매 물리 프레임 충돌 여부를 판정해 그 결과(TimeZone 진입/이탈, 피격)를 각 오브젝트에 전달한다.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace CollisionDetection {
    public class CollisionManager : MonoBehaviour {
        public static CollisionManager Instance { get; private set; }

        [Header("충돌 대상 (인스펙터에서 할당)")]
        [FormerlySerializedAs("playerSAT")]
        [SerializeField] private SatCollisionObject playerCollisionObject;
        [FormerlySerializedAs("enemySAT")]
        [SerializeField] private SatCollisionObject enemyCollisionObject;

        [Header("충돌 대상 (런타임에 Register로 등록)")]
        [Tooltip("GameManager가 생성 시 등록, 만료 시 해제")]
        [FormerlySerializedAs("timeZoneSAT")]
        [SerializeField] private SatCollisionObject timeZoneCollisionObject;
        [Tooltip("PlayerController가 발사 시 등록, 적에 맞으면 제거")]
        [FormerlySerializedAs("bullets")]
        [SerializeField] private List<SatCollisionObject> bulletCollisionObjects = new List<SatCollisionObject>();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterTimeZone(SatCollisionObject timeZone) {
            Assert.IsNotNull(timeZone);
            timeZoneCollisionObject = timeZone;
        }

        public void UnregisterTimeZone() {
            timeZoneCollisionObject = null;
        }

        public void RegisterBullet(SatCollisionObject bullet) {
            Assert.IsNotNull(bullet);
            if (!bulletCollisionObjects.Contains(bullet)) {
                bulletCollisionObjects.Add(bullet);
            }
        }

        // FixedUpdate 주기로 호출되어야 함 (Rigidbody 속도 갱신과 동기화)
        public void HandleCollisions() {
            UpdateTimeZoneState(playerCollisionObject);

            // 적에 맞은 총알은 리스트에서 제거되므로 역순 순회
            for (int i = bulletCollisionObjects.Count - 1; i >= 0; i--) {
                SatCollisionObject bullet = bulletCollisionObjects[i];
                UpdateTimeZoneState(bullet);

                if (!TryHitEnemy(bullet)) continue;

                Destroy(bullet.gameObject);
                bulletCollisionObjects.RemoveAt(i);
            }
        }

        // target은 인스펙터 미할당 등으로 null일 수 있으며, 그 경우 아무것도 하지 않는다
        private void UpdateTimeZoneState(SatCollisionObject target) {
            if (timeZoneCollisionObject == null || target == null) return;
            if (!target.TryGetComponent(out ITimeZoneAffectable affected)) return;

            if (IsColliding(target, timeZoneCollisionObject)) {
                affected.EnterTimeZone();
            } else {
                affected.ExitTimeZone();
            }
        }

        // 명중 시 데미지만 주고 true 반환. 총알 제거는 호출한 쪽에서 한다
        private bool TryHitEnemy(SatCollisionObject bullet) {
            Assert.IsNotNull(bullet);

            if (enemyCollisionObject == null) return false;
            if (!IsColliding(bullet, enemyCollisionObject)) return false;
            if (!bullet.TryGetComponent(out Bullet bulletScript)) return false;
            if (!enemyCollisionObject.TryGetComponent(out Enemy enemy)) return false;

            enemy.TakeDamage(bulletScript.CalculateDamage());
            return true;
        }

        private static bool IsColliding(SatCollisionObject first, SatCollisionObject second) {
            Assert.IsNotNull(first);
            Assert.IsNotNull(second);

            Vector2[] verticesA = first.GetVertices();
            Vector2[] verticesB = second.GetVertices();
            if (verticesA == null || verticesB == null) return false;

            return PolygonCollision.Intersects(verticesA, verticesB);
        }
    }
}
