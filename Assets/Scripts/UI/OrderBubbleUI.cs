using UnityEngine;
using TMPro;
using RecipeAboutLife.Orders;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// 주문 말풍선 UI
    /// NPC 오른쪽에 붙어서 주문 내용을 표시
    /// </summary>
    public class OrderBubbleUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField]
        [Tooltip("주문 텍스트를 표시할 TextMeshProUGUI")]
        private TextMeshProUGUI orderText;

        [Header("설정")]
        [SerializeField]
        [Tooltip("NPC로부터의 오프셋 (오른쪽에 배치)")]
        private Vector3 offset = new Vector3(1.5f, 1.0f, 0f);

        [SerializeField]
        [Tooltip("주문 표시 후 자동으로 숨길 시간 (초)")]
        private float autoHideDelay = 1.5f;

        private void Awake()
        {
            if (orderText == null)
            {
                orderText = GetComponentInChildren<TextMeshProUGUI>();
                if (orderText == null)
                {
                    Debug.LogError("[OrderBubbleUI] TextMeshProUGUI를 찾을 수 없습니다!");
                }
            }

            // 처음에는 숨기기
            Hide();
        }

        /// <summary>
        /// 주문 설정 및 표시
        /// </summary>
        /// <param name="order">표시할 주문 데이터</param>
        public void SetOrder(OrderData order)
        {
            if (order == null)
            {
                Debug.LogWarning("[OrderBubbleUI] Order가 null입니다!");
                Hide();
                return;
            }

            if (orderText == null)
            {
                Debug.LogError("[OrderBubbleUI] OrderText가 없어서 주문을 표시할 수 없습니다!");
                return;
            }

            // 주문 텍스트 설정
            orderText.text = order.GetOrderDescription();

            // 표시
            gameObject.SetActive(true);

            // 1.5초 후 자동으로 숨기기
            CancelInvoke(nameof(Hide)); // 기존 Invoke 취소
            Invoke(nameof(Hide), autoHideDelay);
        }

        /// <summary>
        /// 말풍선 숨기기
        /// </summary>
        public void Hide()
        {
            // 기존 Invoke 취소
            CancelInvoke(nameof(Hide));

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 말풍선 표시
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}
