// Enemy.cs
// 역할: 적의 체력을 관리하고, 데미지를 입거나 체력이 0 이하가 되어 죽는 로직을 담당한다.

using UnityEngine;

namespace CollisionDetection {
    public class Enemy : MonoBehaviour {
        [SerializeField] private float health = 100f;

        public void TakeDamage(float damage) {
            health -= damage;
            Debug.Log($"Enemy가 {damage}의 데미지를 받았고, 현재 적의 체력은 {health}이다.");

            if (health <= 0) {
                Die();
            }
        }

        private void Die() {
            Debug.Log("적의 HP가 0 이하가 되었다.");
            Destroy(gameObject);
        }
    }
}
