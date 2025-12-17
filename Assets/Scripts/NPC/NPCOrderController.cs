using UnityEngine;
using RecipeAboutLife.Orders;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// NPC 주문 컨트롤러
    /// NPC가 정지했을 때 주문을 받아서 말풍선 UI에 표시
    /// </summary>
    public class NPCOrderController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField]
        [Tooltip("주문 관리자")]
        private OrderManager orderManager;

        [SerializeField]
        [Tooltip("말풍선 UI")]
        private OrderBubbleUI orderBubbleUI;

        [Header("상태")]
        private OrderData currentOrder;
        private bool hasOrder = false;

        private void Awake()
        {
            if (orderManager == null)
            {
                orderManager = FindObjectOfType<OrderManager>();
                if (orderManager == null)
                {
                    Debug.LogError("[NPCOrderController] OrderManager를 찾을 수 없습니다!");
                }
            }

            if (orderBubbleUI == null)
            {
                orderBubbleUI = GetComponentInChildren<OrderBubbleUI>();
                if (orderBubbleUI == null)
                {
                    Debug.LogWarning("[NPCOrderController] OrderBubbleUI를 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// NPC가 정지했을 때 호출 - 주문 요청
        /// </summary>
        public void OnNPCStopped()
        {
            if (hasOrder)
            {
                Debug.LogWarning("[NPCOrderController] 이미 주문을 받았습니다.");
                return;
            }

            RequestOrder();
        }

        /// <summary>
        /// 주문 요청
        /// </summary>
        private void RequestOrder()
        {
            if (orderManager == null)
            {
                Debug.LogError("[NPCOrderController] OrderManager가 없어서 주문을 받을 수 없습니다!");
                return;
            }

            currentOrder = orderManager.RequestRandomOrder();

            if (currentOrder != null)
            {
                hasOrder = true;
                DisplayOrder();
                Debug.Log($"[NPCOrderController] 주문 받음: {currentOrder.OrderName}");
            }
            else
            {
                Debug.LogError("[NPCOrderController] 주문을 받지 못했습니다!");
            }
        }

        /// <summary>
        /// 말풍선 UI에 주문 표시
        /// </summary>
        private void DisplayOrder()
        {
            if (orderBubbleUI == null)
            {
                Debug.LogWarning("[NPCOrderController] OrderBubbleUI가 없어서 주문을 표시할 수 없습니다!");
                return;
            }

            orderBubbleUI.SetOrder(currentOrder);
        }

        /// <summary>
        /// 현재 주문 가져오기
        /// </summary>
        public OrderData GetCurrentOrder()
        {
            return currentOrder;
        }

        /// <summary>
        /// 주문 초기화
        /// </summary>
        public void ClearOrder()
        {
            currentOrder = null;
            hasOrder = false;

            if (orderBubbleUI != null)
            {
                orderBubbleUI.Hide();
            }
        }

    }
}
