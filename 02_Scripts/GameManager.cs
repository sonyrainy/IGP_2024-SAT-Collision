using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject timeZonePrefab;   


    // TimeZone이 유지되는 시간
    public float timeZoneLifetime = 5f;      

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 존재하는 인스턴스가 있으면 파괴
            Destroy(gameObject);  
        }
        else
        {
            //씬 전환 시에도 파괴되지 않도록 함.
            Instance = this;  
            DontDestroyOnLoad(gameObject);
        }
    }

    void Update()
    {
        // 마우스 좌클릭, TimeZone 생성
        if (Input.GetMouseButtonDown(0))
        {
            CreateTimeZone();
        }
    }

    void FixedUpdate()
    {
        // 충돌 처리를 CollisionManager에서 돌아가도록 하기
        CollisionManager.Instance.HandleCollisions();
    }

    // 마우스 클릭 시 TimeZone 생성
    private void CreateTimeZone()
    {
        // 마우스 클릭한 위치를 가져오기
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 2D에서 진행 중이기 때문에 z값을 0으로 설정
        mousePos.z = 0f;  

        // TimeZone 오브젝트 생성
        GameObject timeZone = Instantiate(timeZonePrefab, mousePos, Quaternion.identity);

        // 생성된 TimeZone을 CollisionManager에 등록하기
        SATCollisionObject timeZoneSAT = timeZone.GetComponent<SATCollisionObject>();
        if (timeZoneSAT == null)
        {
            // 생성된 TimeZone에 SATCollisionObject가 없으면 추가하기
            timeZoneSAT = timeZone.AddComponent<SATCollisionObject>();   }

        CollisionManager.Instance.RegisterTimeZone(timeZoneSAT);

        // 일정 시간이 지나면 TimeZone을 삭제
        Destroy(timeZone, timeZoneLifetime);

        // 위 Destroy 만으로는 오브젝트가 null로 보이지만,
        // 충돌 체크하는 루프에서 예외가 발생할 수 있다.
        // 확실하게 TimeZone 정리를 호출하여 정리한다.
        CollisionManager.Instance.RegisterTimeZone(null)
    }
}
