// GameManager.cs
// 역할: 마우스 클릭으로 TimeZone을 생성(일정 시간 후 제거)하고, 충돌 로직을 제외한 게임 흐름 전반의 로직을 담당한다.

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace CollisionDetection {
    public class GameManager : MonoBehaviour {
        private const int LeftMouseButton = 0;

        public static GameManager Instance { get; private set; }

        [Header("TimeZone")]
        [SerializeField] private GameObject timeZonePrefab;
        [Tooltip("생성된 TimeZone이 유지되는 시간(초)")]
        [SerializeField] private float timeZoneLifetime = 5f;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            if (Input.GetMouseButtonDown(LeftMouseButton)) {
                CreateTimeZone();
            }
        }

        private void FixedUpdate() {
            CollisionManager.Instance.HandleCollisions();
        }

        private void CreateTimeZone() {
            Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spawnPosition.z = 0f; // 2D

            GameObject timeZone = Instantiate(timeZonePrefab, spawnPosition, Quaternion.identity);
            if (!timeZone.TryGetComponent(out SatCollisionObject timeZoneCollisionObject)) {
                timeZoneCollisionObject = timeZone.AddComponent<SatCollisionObject>();
            }

            CollisionManager.Instance.RegisterTimeZone(timeZoneCollisionObject);
            StartCoroutine(CoDestroyTimeZoneAfter(timeZone, timeZoneLifetime));
        }

        private IEnumerator CoDestroyTimeZoneAfter(GameObject timeZone, float delay) {
            Assert.IsNotNull(timeZone);

            yield return new WaitForSeconds(delay);

            if (CollisionManager.Instance != null) {
                CollisionManager.Instance.UnregisterTimeZone();
            }
            Destroy(timeZone);
        }
    }
}
