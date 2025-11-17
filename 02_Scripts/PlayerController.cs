using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;  
    public float increasedSpeed = 10f;  
    public float currentSpeed;
    public float jumpForce = 20f;
    public float fallMultiplier = 3.5f;
    public float jumpMultiplier = 4.5f;

    public GameObject bulletPrefab;
    public Transform firePoint;

    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private bool isGrounded = false;
    private bool isInTimeZone = false; 

    private float moveInput = 0f;
    private bool jumpRequested = false;
    private bool shootRequested = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentSpeed = moveSpeed;
    }

    void Update()
    {
        // 좌우 이동 입력 읽기
        if (Input.GetKey(KeyCode.D))
        {
            moveInput = 1f;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1f;
        }
        else
        {
            moveInput = 0f;
        }

        // 점프 입력 감지
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            jumpRequested = true;
        }

        // 총알 발사 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shootRequested = true;
        }
    }

    void FixedUpdate()
    {
        // 이동 처리 (실제 Rigidbody 조작)
        Move();

        // 점프 처리
        if (jumpRequested && isGrounded)
        {
            Jump();
            jumpRequested = false; 
        }

        // 중력 배수 적용 (낙하/상승)
        ApplyBetterJumpGravity();

        // 총알 발사 처리
        if (shootRequested)
        {
            Shoot();
            shootRequested = false; 
        }
    }

    void Move()
    {
        // Rigidbody2D에 실제 속도 적용
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // 방향 전환
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce); 
        isGrounded = false;  
    }

    void ApplyBetterJumpGravity()
    {
        // 플레이어가 내려올 때
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier; 
        }
        // 위로 점프 중일 때
        else if (rb.velocity.y > 0)
        {
            rb.gravityScale = jumpMultiplier; 
        }
        else
        {
            rb.gravityScale = 1f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            // 총알을 firePoint 위치에서 생성
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // SATCollisionObject.cs가 제대로 추가되었는지 확인
            SATCollisionObject bulletSAT = bullet.GetComponent<SATCollisionObject>();
            if (bulletSAT == null)
            {
                Debug.LogError("생성된 Bullet에 SATCollisionObject가 없다.");
            }

            // 플레이어가 바라보는 방향에 따라 총알 방향 설정
            float direction = isFacingRight ? 1f : -1f;

            // 총알의 Bullet 스크립트에 방향 전달
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(direction);
            }

            CollisionManager.Instance.RegisterBullet(bulletSAT);
        }
        else
        {
            Debug.LogError("Bullet prefab이나 firePoint가 설정되지 않았다.");
        }
    }

    // TimeZone에 들어갔을 때 속도 증가
    public void EnterTimeZone()
    {
        if (!isInTimeZone)
        {
            isInTimeZone = true;
            currentSpeed = increasedSpeed;  
        }
    }

    // TimeZone을 벗어났을 때 속도 복원
    public void ExitTimeZone()
    {
        if (isInTimeZone)
        {
            isInTimeZone = false;
            currentSpeed = moveSpeed; 
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;  
        transform.localScale = localScale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("GROUND"))
        {
            isGrounded = true; 
        }
    }
}
