// ITimeZoneAffectable.cs
// 역할: TimeZone 안에 들어가면 영향을 받는 오브젝트(Player, Bullet)의 공통 인터페이스.
// TimeZone 진입/이탈 처리에서 CollisionManager는 이 인터페이스만 사용한다.

namespace CollisionDetection {
    public interface ITimeZoneAffectable {
        // 두 메서드 모두 매 물리 프레임 반복 호출돼도 안전해야 한다 (이미 같은 상태면 무시)
        void EnterTimeZone();
        void ExitTimeZone();
    }
}
