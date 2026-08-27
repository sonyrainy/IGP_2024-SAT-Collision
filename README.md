**인디게임제작(Indie Game Production)_Tutorial 제작 프로젝트입니다.**

>**SAT와 AABB 이론을 활용해 충돌처리를 자체적으로 구현하였습니다.**

+) 01_Scenes, 02_Scripts, 03_Prefabs 폴더만 업로드하였습니다.

---

### 1. 프로젝트(게임) 특징
>- **기본 조작**: wasd를 이용한 이동 및 점프, spacebar를 통한 공격
>
>+) 점프 및 하강 시, 중력가속도를 다르게 적용하여 조작감을 향상시키려고 하였습니다.
>
>- **클릭을 이용한 영역(TimeZone) 생성**: 마우스 클릭으로 화면 상 영역을 생성하여 그 내부에 들어오는 오브젝트의 시간이 빠르게 흐르도록(속도 증가) 설정
>
>+) 속도를 증가시킨 총알로 적을 공격하면 데미지 증가

→ **다각형의 충돌 가능성 검사(AABB) 후, 충돌 정밀 검사(SAT)를 진행하는 방식으로 충돌 로직을 구현하였습니다.**

<br>

<div align="center">
    <img src="https://github.com/user-attachments/assets/307b1f6f-ff42-48be-9dd0-0578f2ae3736" alt="image" width="320" height="145">
</div>

<br>

### 1.5. Vertex 추출 방식

- **2D Sprite**
  
```
void Start()
    {
        // SpriteRenderer를 통해 Sprite를 가져오기
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;

        // Sprite의 꼭짓점(Vertex) 배열을 가져오기
        Vector2[] spriteVertices = sprite.vertices;
    }

```

<br>


- **Mesh**
```
    private Mesh mesh;
    private Vector3[] vertices;
    void Start()
    {
        // MeshFilter에서 Mesh를 가져오기
        mesh = GetComponent<MeshFilter>().mesh;

        // Mesh의 꼭짓점(Vertex) 배열을 가져오기
        vertices = mesh.vertices;
    }

```

<br>


- **PolygonCollider2D ✔**
```
    private PolygonCollider2D polygonCollider;

    // 꼭짓점 배열을 월드 좌표계로 변환하여 반환 (PolygonCollider2D가 없으면 null)
    public Vector2[] GetVertices() {
        if (polygonCollider == null) return null;

        return polygonCollider.points
            .Select(point => (Vector2)transform.TransformPoint(point))
            .ToArray();
    }
```

→ PolygonCollider2D를 통해 vertex를 추출하여 충돌 처리 구현에 활용하였습니다.

<br>

### 2. AABB(Axis – Aligned Bounding Box), SAT(Seperating Axis Theorem)
#### - AABB란?
>- 두 물체(직사각형))의 x, y 축에으로 투영되는 최대, 최소 거리 값을 계산 → 두 물체 간의 축 범위가 겹치는지 검사 → 두 축(x축, y축)에서 모두 겹치는 것이 확인되면 충돌으로 판단한다.
>- 사각형이 아닌 다른 다각형 오브젝트의 경우, 각각의 vertex가 x축으로 가는 x값의 최댓값, y축으로 가는 y값의 최댓값을 구해서 오브젝트를 감싸는 임의의 사각형을 생성 후,
그 두 사각형을 비교하여 AABB를 검사한다.

<br>


<div align="center">
    <img src="https://github.com/user-attachments/assets/a9fe2347-d6eb-4946-878c-7ae64f99d39d" alt="image" width="400" height="300">
</div>

<br>

#### - SAT란?
>- SAT는 두 개의 볼록 다각형이 충돌하지 않으면, 두 다각형을 완전히 분리하는 축이 존재하는 것을 원리로 작동한다.
>- 만약 모든 가능한 축에서 두 다각형의 꼭짓점이 “내적” 즉, “투영”된(도형이 빛을 통해 투영된 것)이 겹치면 충돌이 발생한 것으로 판단한다.
>- 각 다각형의 변에 대해 법선 벡터를 계산하고, 그 벡터를 축으로 사용한다. 두 다각형의 모든 변에 대해 이러한 분리 축을 생성한 후, 그 축에 두 다각형을 투영(내적)한다. 내적된 부분이 겹치지 않으면 두 다각형은 충돌하지 않는 것으로 간주된다.
>- 모든 축에서 투영(내적)이 겹치는 부분이 있다면, 두 다각형은 충돌한 것으로 판단한다.

<br>

<div align="center">
    <img src="https://github.com/user-attachments/assets/acc8956e-4fae-4486-a204-233aebbe9ae0" alt="image" width="550" height="270">
</div>

<br>

### 3. 스크립트 구성 및 역할

```
02_Scripts/
├── Core/        GameManager.cs
├── Collision/   CollisionManager.cs, PolygonCollision.cs, SatCollisionObject.cs
└── Gameplay/    ITimeZoneAffectable.cs, PlayerController.cs, Bullet.cs, Enemy.cs
```

- **GameManager.cs**: TimeZone을 생성한다. 충돌 로직을 제외한 게임 흐름에 대한 게임 로직을 담당(예정)한다.
- **CollisionManager.cs**: 충돌할 수 있는 오브젝트를 관리하고, 매 물리 프레임 충돌 여부를 판정해 그 결과(TimeZone 진입/이탈, 피격)를 각 오브젝트에 전달한다.
- **PolygonCollision.cs**: AABB(Axis-Aligned Bounding Box)로 충돌 가능성을 판단한 후, SAT(Separating Axis Theorem) 알고리즘으로 정밀한 충돌 여부를 검사(계산)하는 순수 계산 클래스. Unity 오브젝트에 의존하지 않는다.
- **SatCollisionObject.cs**: 물체의 PolygonCollider2D에서 꼭짓점을 가져와 충돌 연산에 필요한 오브젝트의 정보를 CollisionManager에 넘겨준다.
- **ITimeZoneAffectable.cs**: TimeZone의 영향을 받는 오브젝트(Player, Bullet)의 공통 인터페이스. TimeZone 진입/이탈 처리에서 CollisionManager는 이 인터페이스만 사용한다.
- **Bullet.cs**: 총알의 이동과 속도 관리 및 총알이 TimeZone과 충돌했을 때, 충돌을 빠져나갔을 때의 로직을 담당한다.
- **PlayerController.cs**: 플레이어의 이동, 점프, 총알 발사 등의 기본적인 제어를 담당한다. 플레이어가 TimeZone 안에 있을 때의 로직을 담당한다.
- **Enemy.cs**: 적의 체력 관리 및 데미지를 입거나, 죽는 로직을 담당한다.

→ 상세 구조도(의존 관계·호출 흐름·충돌 파이프라인): [02_Scripts/ScriptStructure.md](02_Scripts/ScriptStructure.md)

<br>

최종 영상: https://youtu.be/k8w_UgUpgv8

---


+) 참고: [AABB](https://developer.mozilla.org/en-US/docs/Games/Techniques/3D_collision_detection), [SAT_1](https://www.sevenson.com.au/programming/sat/), [SAT_2](https://programmerart.weebly.com/separating-axis-theorem.html), [SAT_3](https://www.youtube.com/watch?v=dn0hUgsok9M)

