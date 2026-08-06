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
    }
}
