using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public static CollisionManager Instance { get; private set; }

    [SerializeField] private SATCollisionObject timeZoneSAT; 
    [SerializeField] private SATCollisionObject enemySAT;  
    [SerializeField] private SATCollisionObject playerSAT;  
    [SerializeField] private List<SATCollisionObject> bullets = new List<SATCollisionObject>();  
    
        void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    
    public void RegisterTimeZone(SATCollisionObject timeZone)
    {
        timeZoneSAT = timeZone;
    }

  
    public void RegisterPlayer(SATCollisionObject player)
    {
        playerSAT = player;
    }

 
    public void RegisterEnemy(SATCollisionObject enemy)
    {
        enemySAT = enemy;
    }

    
    public void RegisterBullet(SATCollisionObject bulletSAT)
    {
        if (!bullets.Contains(bulletSAT))
        {
            bullets.Add(bulletSAT);
        }
    }

    // 충돌 처리 함수
    public void HandleCollisions()
    {
        // Player와 TimeZone의 충돌 감지
        if (timeZoneSAT != null && playerSAT != null)
        {
            PlayerController playerScript = playerSAT.GetComponent<PlayerController>();

            if (CheckCollision(playerSAT, timeZoneSAT))
            {
                // TimeZone 안에 있을 때 속도 증가
                playerScript.EnterTimeZone();  
            }
            else
            {
                // TimeZone을 벗어났을 때 속도 복원
                playerScript.ExitTimeZone();   
            }
        }

        // 발사된 총알(들)과 TimeZone 및 enemy의 충돌 감지
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            SATCollisionObject bullet = bullets[i];
            Bullet bulletScript = bullet.GetComponent<Bullet>();

            // TimeZone과 Bullet의 충돌 감지
            if (timeZoneSAT != null && bulletScript != null)
            {
                if (CheckCollision(bullet, timeZoneSAT))
                {
                    // TimeZone 안에 있을 때 속도 증가되도록 함수 불러오기
                    bulletScript.EnterTimeZone();  
                }
                else
                {
                    // TimeZone을 벗어났을 때 속도 복원
                    bulletScript.ExitTimeZone();   
                }
            }

            // enemy와 bullet의 충돌 감지
            if (enemySAT != null && CheckCollision(bullet, enemySAT))
            {
                Enemy enemyScript = enemySAT.GetComponent<Enemy>();
                if (bulletScript != null && enemyScript != null)
                {
                    float damage = bulletScript.CalculateDamage();
                    enemyScript.TakeDamage(damage);
                    Debug.Log($"총알이 적에게 {damage}만큼의 데미지를 입혔습니다.");

                    Destroy(bullet.gameObject);
                    bullets.RemoveAt(i);
                }
            }
        }
    }

    // AABB와 SAT를 활용한 충돌 감지 로직
    public bool CheckCollision(SATCollisionObject objA, SATCollisionObject objB)
    {
        Vector2[] verticesA = objA.GetVertices();
        Vector2[] verticesB = objB.GetVertices();

        if (verticesA == null || verticesB == null)
        {
            //Debug.LogError("꼭짓점 정보를 불러오지 못했습니다!");
            return false;
        }

        // 1. AABB 충돌 가능성 검사
        var aabbA = CalculateAABB(verticesA);
        var aabbB = CalculateAABB(verticesB);

        if (!IsAABBColliding(aabbA.min, aabbA.max, aabbB.min, aabbB.max))
        {
            Debug.Log("AABB 충돌 없음");
            return false;
        }

        // 2. AABB 충돌 가능성이 있으면 SAT 알고리즘으로 정밀 검사
        Debug.Log("AABB 충돌 발생, SAT 검사 시작");
        return SATAlgorithm(verticesA, verticesB);
    }

    // AABB 충돌 감지 함수
    private bool IsAABBColliding(Vector2 minA, Vector2 maxA, Vector2 minB, Vector2 maxB)
    {
        if (maxA.x < minB.x || minA.x > maxB.x)
            return false;

        if (maxA.y < minB.y || minA.y > maxB.y)
            return false;

        return true;
    }

    // AABB 경계 계산 함수
    private (Vector2 min, Vector2 max) CalculateAABB(Vector2[] vertices)
    {
        Vector2 min = vertices[0];
        Vector2 max = vertices[0];

        foreach (var vertex in vertices)
        {
            if (vertex.x < min.x) min.x = vertex.x;
            if (vertex.y < min.y) min.y = vertex.y;
            if (vertex.x > max.x) max.x = vertex.x;
            if (vertex.y > max.y) max.y = vertex.y;
        }

        return (min, max);
    }

    // SAT 알고리즘을 사용하여 충돌 여부 확인
    private bool SATAlgorithm(Vector2[] verticesA, Vector2[] verticesB)
    {
        Vector2[] axes = GetAxes(verticesA, verticesB);

        foreach (Vector2 axis in axes)
        {
            (float minA, float maxA) = Project(verticesA, axis);
            (float minB, float maxB) = Project(verticesB, axis);

            Debug.Log($"축 위치: {axis}, A가 투영된 범위: {minA} ~ {maxA}, B가 투영된 범위: {minB} ~ {maxB}");

            if (maxA < minB || minA > maxB)
            {
                Debug.Log("조건에 맞지 않아 충돌 X로 판단(즉, 분리 축 발견)");
                return false;
            }
        }

        Debug.Log("충돌 발생");
        return true;
    }

    // 두 오브젝트의 변에 대해 법선 벡터(축)를 구함
    private Vector2[] GetAxes(Vector2[] verticesA, Vector2[] verticesB)
    {
        List<Vector2> axes = new List<Vector2>();

        // 첫 번째 오브젝트의 변에 대해 축(법선 벡터) 구하기
        for (int i = 0; i < verticesA.Length; i++)
        {
            Vector2 edge = verticesA[(i + 1) % verticesA.Length] - verticesA[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
            axes.Add(normal);
        }

        // 두 번째 오브젝트의 변에 대해 축(법선 벡터) 구하기
        for (int i = 0; i < verticesB.Length; i++)
        {
            Vector2 edge = verticesB[(i + 1) % verticesB.Length] - verticesB[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
            axes.Add(normal);
        }

        return axes.ToArray();
    }

    // 꼭짓점을 축에 투영하고 최소, 최대 값을 반환
    private (float, float) Project(Vector2[] vertices, Vector2 axis)
    {
        float min = Vector2.Dot(vertices[0], axis);
        float max = min;

        for (int i = 1; i < vertices.Length; i++)
        {
            float projection = Vector2.Dot(vertices[i], axis);
            if (projection < min)
            {
                min = projection;
            }
            if (projection > max)
            {
                max = projection;
            }
        }

        return (min, max);
    }
}
