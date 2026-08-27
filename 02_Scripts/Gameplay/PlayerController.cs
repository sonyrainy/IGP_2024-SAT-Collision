// PlayerController.cs
// 역할: 플레이어의 이동/점프/총알 발사 등 기본 조작과, TimeZone 진입/이탈 시 이동 속도 변경 로직을 담당한다.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace CollisionDetection {
    public class PlayerController : MonoBehaviour, ITimeZoneAffectable {
        private const string GroundTag = "GROUND";
        private const float DefaultGravityScale = 1f;

        [Header("이동")]
        [SerializeField] private float moveSpeed = 5f;
        [FormerlySerializedAs("increasedSpeed")]
        [Tooltip("TimeZone 안에 있을 때의 이동 속도")]
        [SerializeField] private float timeZoneSpeed = 10f;

        [Header("점프")]
        [SerializeField] private float jumpForce = 20f;
        [Tooltip("하강 중 중력 배율 (클수록 빨리 떨어짐)")]
        [SerializeField] private float fallMultiplier = 3.5f;
        [Tooltip("상승 중 중력 배율")]
        [SerializeField] private float jumpMultiplier = 4.5f;

        [Header("발사")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;

        private Rigidbody2D rigidBody;
        private float currentSpeed;
        private bool isFacingRight = true;
        private bool isGrounded = false;
        private bool isInTimeZone = false;

        // Update에서 읽은 입력을 FixedUpdate에서 소비 (입력 프레임과 물리 프레임 분리)
        private float moveInput = 0f;
        private bool isJumpRequested = false;
        private bool isShootRequested = false;

        private void Start() {
            rigidBody = GetComponent<Rigidbody2D>();
            currentSpeed = moveSpeed;
        }

        private void Update() {
            if (Input.GetKey(KeyCode.D)) {
                moveInput = 1f;
            } else if (Input.GetKey(KeyCode.A)) {
                moveInput = -1f;
            } else {
                moveInput = 0f;
            }

            if (Input.GetKeyDown(KeyCode.W) && isGrounded) {
                isJumpRequested = true;
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                isShootRequested = true;
            }
        }

        private void FixedUpdate() {
            Move();

            if (isJumpRequested && isGrounded) {
                Jump();
                isJumpRequested = false;
            }

            ApplyBetterJumpGravity();

            if (isShootRequested) {
                Shoot();
                isShootRequested = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            Assert.IsNotNull(collision);
            if (collision.gameObject.CompareTag(GroundTag)) {
                isGrounded = true;
            }
        }

        public void EnterTimeZone() {
            if (isInTimeZone) return;

            isInTimeZone = true;
            currentSpeed = timeZoneSpeed;
        }

        public void ExitTimeZone() {
            if (!isInTimeZone) return;

            isInTimeZone = false;
            currentSpeed = moveSpeed;
        }

        private void Move() {
            rigidBody.velocity = new Vector2(moveInput * currentSpeed, rigidBody.velocity.y);

            if (moveInput > 0 && !isFacingRight) {
                Flip();
            } else if (moveInput < 0 && isFacingRight) {
                Flip();
            }
        }

        private void Jump() {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            isGrounded = false;
        }

        // 상승/하강에 서로 다른 중력 배율을 적용해 점프 조작감을 개선
        private void ApplyBetterJumpGravity() {
            if (rigidBody.velocity.y < 0) {
                rigidBody.gravityScale = fallMultiplier;
            } else if (rigidBody.velocity.y > 0) {
                rigidBody.gravityScale = jumpMultiplier;
            } else {
                rigidBody.gravityScale = DefaultGravityScale;
            }
        }

        private void Shoot() {
            if (bulletPrefab == null || firePoint == null) {
                Debug.LogError("bulletPrefab 또는 firePoint가 설정되지 않았다.");
                return;
            }

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // Bullet.Start()가 첫 이동에 방향을 쓰므로, 생성 이후 같은 프레임에 넘겨야 한다
            if (bullet.TryGetComponent(out Bullet bulletScript)) {
                bulletScript.SetDirection(isFacingRight ? Bullet.RightDirection : Bullet.LeftDirection);
            }

            if (!bullet.TryGetComponent(out SatCollisionObject bulletCollisionObject)) {
                Debug.LogError("Bullet 프리팹에 SatCollisionObject가 없다.");
            }
            CollisionManager.Instance.RegisterBullet(bulletCollisionObject);
        }

        private void Flip() {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }
}
