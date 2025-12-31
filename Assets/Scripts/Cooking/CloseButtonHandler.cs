using UnityEngine;
using UnityEngine.Events;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// Canvas 없이 월드 스페이스에서 클릭을 감지하는 버튼 핸들러
    /// CloseButton 오브젝트에 부착
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CloseButtonHandler : MonoBehaviour
    {
        [Header("=== Click Event ===")]
        [Tooltip("클릭 시 실행할 이벤트")]
        public UnityEvent OnClicked;

        [Header("=== Visual Feedback ===")]
        [Tooltip("클릭 시 스케일 변화")]
        public bool useScaleEffect = true;
        public float clickScale = 0.9f;
        public float scaleSpeed = 10f;

        private Vector3 originalScale;
        private bool isPressed = false;

        private void Awake()
        {
            originalScale = transform.localScale;

            // Collider2D 확인
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                // BoxCollider2D 자동 추가
                col = gameObject.AddComponent<BoxCollider2D>();
                Debug.Log("[CloseButtonHandler] BoxCollider2D 자동 추가됨");
            }
        }

        private void Update()
        {
            // 스케일 복원 애니메이션
            if (useScaleEffect && !isPressed)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleSpeed);
            }
        }

        private void OnMouseDown()
        {
            isPressed = true;
            
            if (useScaleEffect)
            {
                transform.localScale = originalScale * clickScale;
            }

            Debug.Log("[CloseButtonHandler] 버튼 클릭 감지!");
        }

        private void OnMouseUp()
        {
            if (isPressed)
            {
                isPressed = false;

                // 클릭 이벤트 발생
                OnClicked?.Invoke();
                Debug.Log("[CloseButtonHandler] OnClicked 이벤트 발생!");
            }
        }

        private void OnMouseExit()
        {
            // 마우스가 버튼 영역을 벗어나면 클릭 취소
            isPressed = false;
        }
    }
}
