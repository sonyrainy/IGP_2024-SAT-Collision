// Bullet.cs
// 역할: 총알의 이동 및 속도 관리 및 TimeZone 진입/이탈 시 속도 변경과 속도 기반 데미지 계산을 담당한다.

using UnityEngine;
using UnityEngine.Serialization;

namespace CollisionDetection {
    public class Bullet : MonoBehaviour, ITimeZoneAffectable {
        public const float RightDirection = 1f;
        public const float LeftDirection = -1f;

        [Header("속도")]
        [SerializeField] private float bulletSpeed = 20f;
        [FormerlySerializedAs("increasedSpeed")]
        [Tooltip("TimeZone 안에 있을 때의 속도")]
        [SerializeField] private float timeZoneSpeed = 40f;

        [Header("데미지")]
        [Tooltip("데미지 = 현재 속도 × 배율")]
        [SerializeField] private float damageMultiplier = 0.1f;

        private Rigidbody2D rigidBody;
        private float currentSpeed;
        private bool isInTimeZone = false;
        private float bulletDirection = RightDirection;

        private void Start() {
            rigidBody = GetComponent<Rigidbody2D>();
            currentSpeed = bulletSpeed;
            MoveBullet(currentSpeed);
        }

        public void EnterTimeZone() {
            if (isInTimeZone) return;

            isInTimeZone = true;
            currentSpeed = timeZoneSpeed;
            MoveBullet(currentSpeed);
        }

        public void ExitTimeZone() {
            if (!isInTimeZone) return;

            isInTimeZone = false;
            currentSpeed = bulletSpeed;
            MoveBullet(currentSpeed);
        }

        // Start()의 첫 이동에 쓰이므로 Instantiate 직후 같은 프레임에 호출되어야 한다
        public void SetDirection(float direction) {
            bulletDirection = direction;
        }

        // 빠를수록 데미지가 커진다 → TimeZone을 통과한 총알이 더 강함
        public float CalculateDamage() {
            return rigidBody.velocity.magnitude * damageMultiplier;
        }

        private void MoveBullet(float speed) {
            rigidBody.velocity = new Vector2(bulletDirection * speed, rigidBody.velocity.y);
        }
    }
}
