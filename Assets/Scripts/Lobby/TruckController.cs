using UnityEngine;
using System.Collections;

namespace RecipeAboutLife.Lobby
{
    /// <summary>
    /// 트럭 - 핀 위치로 이동
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class TruckController : MonoBehaviour
    {
        [Header("스프라이트")]
        [SerializeField] private Sprite truckSprite;

        [Header("이동 설정")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float arrivalDistance = 0.1f;

        private SpriteRenderer spriteRenderer;
        private bool isMoving = false;

        public bool IsMoving => isMoving;
        public System.Action OnArrived;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (truckSprite != null)
            {
                spriteRenderer.sprite = truckSprite;
            }
        }

        public void MoveToPosition(Vector3 targetPosition)
        {
            if (isMoving) return;
            StartCoroutine(MoveCoroutine(targetPosition));
        }

        private IEnumerator MoveCoroutine(Vector3 target)
        {
            isMoving = true;
            Debug.Log($"[Truck] 이동 시작: {target}");

            while (Vector3.Distance(transform.position, target) > arrivalDistance)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = target;
            isMoving = false;
            Debug.Log("[Truck] 도착!");
            OnArrived?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (spriteRenderer != null && truckSprite != null)
                spriteRenderer.sprite = truckSprite;
        }
#endif
    }
}
