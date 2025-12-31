using UnityEngine;

namespace RecipeAboutLife.Orders
{
    /// <summary>
    /// 주문 관리자
    /// OrderDatabase에서 주문을 가져와 NPC에게 제공
    /// </summary>
    public class OrderManager : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField]
        [Tooltip("주문 데이터베이스")]
        private OrderDatabase orderDatabase;

        private void Awake()
        {
            if (orderDatabase == null)
            {
                Debug.LogError("[OrderManager] OrderDatabase가 할당되지 않았습니다!");
            }
        }

        /// <summary>
        /// 랜덤 주문 요청
        /// </summary>
        /// <returns>랜덤으로 선택된 주문 데이터</returns>
        public OrderData RequestRandomOrder()
        {
            if (orderDatabase == null)
            {
                Debug.LogError("[OrderManager] OrderDatabase가 없어서 주문을 제공할 수 없습니다!");
                return null;
            }

            OrderData order = orderDatabase.GetRandomOrder();

            if (order != null)
            {
                Debug.Log($"[OrderManager] 주문 제공: {order.OrderName}");
            }

            return order;
        }

        /// <summary>
        /// 특정 인덱스 주문 요청 (테스트용)
        /// </summary>
        public OrderData RequestOrderAtIndex(int index)
        {
            if (orderDatabase == null)
            {
                Debug.LogError("[OrderManager] OrderDatabase가 없습니다!");
                return null;
            }

            return orderDatabase.GetOrderAtIndex(index);
        }
    }
}
