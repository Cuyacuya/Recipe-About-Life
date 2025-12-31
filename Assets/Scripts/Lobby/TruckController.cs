using UnityEngine;
using System.Collections;

namespace RecipeAboutLife.Lobby
{
    /// <summary>
    /// 트럭 컨트롤러 - 오른쪽으로 이동
    /// </summary>
    public class TruckController : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField] private float moveDistance = 10f;
        [SerializeField] private float moveDuration = 1f;

        private bool isMoving = false;
        public bool IsMoving => isMoving;

        /// <summary>
        /// 오른쪽으로 이동 시작
        /// </summary>
        public void MoveRight()
        {
            if (isMoving) return;
            StartCoroutine(MoveRightCoroutine());
        }

        /// <summary>
        /// 오른쪽으로 이동 (거리, 시간 지정)
        /// </summary>
        public void MoveRight(float distance, float duration)
        {
            if (isMoving) return;
            moveDistance = distance;
            moveDuration = duration;
            StartCoroutine(MoveRightCoroutine());
        }

        private IEnumerator MoveRightCoroutine()
        {
            isMoving = true;

            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + Vector3.right * moveDistance;
            float elapsed = 0f;

            Debug.Log($"[Truck] 이동 시작: {startPos} → {targetPos}");

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / moveDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            isMoving = false;

            Debug.Log("[Truck] 이동 완료!");
        }
    }
}
