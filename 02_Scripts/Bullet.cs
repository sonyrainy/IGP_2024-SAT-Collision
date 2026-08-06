using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float bulletSpeed = 20f; 

    //TimeZone 내부에서 속도 크기
    public float increasedSpeed = 40f; 
    public float currentSpeed;      

    //TimeZone에 들어가기 전의 속도
    public float originalSpeed;       


    //속도에 따른 데미지 배율
    public float damageMultiplier = 0.1f;
    private Rigidbody2D rb;
    private bool isInTimeZone = false;

    // 총알 발사 방향을 받는 변수
    private float bulletDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
         // 기본 "현재" 속도를 설정..
        currentSpeed = bulletSpeed;  

        // TimeZone에 들어가기 전 속도를 저장
        originalSpeed = bulletSpeed;  

        MoveBullet(currentSpeed);
    }

    void MoveBullet(float speed)
    {
        rb.velocity = new Vector2(bulletDirection * speed, rb.velocity.y);
    }

    // TimeZone에 들어가면 속도 증가
    public void EnterTimeZone()
    {
        if (!isInTimeZone)
        {
            isInTimeZone = true;

             // TimeZone에 들어가기 전 속도 저장
            originalSpeed = currentSpeed; 
            // TimeZone 안에서 속도 증가
            currentSpeed = increasedSpeed;
              // 변경된 속도 적용
            MoveBullet(currentSpeed);    
        }
    }

    // TimeZone을 벗어나면 속도 복원
    public void ExitTimeZone()
    {
        if (isInTimeZone)
        {
            isInTimeZone = false;
            // 속도를 TimeZone에 들어가기 전 상태로 복원
            currentSpeed = originalSpeed;  
            // 복원된 속도 적용
            MoveBullet(currentSpeed);      
        }
    }

    // 플레이어가 바라보는 방향 설정
    public void SetDirection(float direction)
    {
        bulletDirection = direction;
    }

    // 총알의 속도에 따른 데미지 계산 함수
    public float CalculateDamage()
    {
        float speed = rb.velocity.magnitude;  
        return speed * damageMultiplier;     
    }
}
