using System.Collections.Generic;
using UnityEngine;

namespace RecipeAboutLife.Orders
{
    /// <summary>
    /// 주문 데이터베이스
    /// 모든 주문 데이터를 관리하고 랜덤으로 제공
    /// </summary>
    [CreateAssetMenu(fileName = "OrderDatabase", menuName = "RecipeAboutLife/OrderDatabase")]
    public class OrderDatabase : ScriptableObject
    {
        [Header("주문 목록")]
        [Tooltip("게임에서 사용할 모든 주문 데이터")]
        public List<OrderData> Orders = new List<OrderData>();

        /// <summary>
        /// 랜덤 주문 가져오기
        /// </summary>
        /// <returns>랜덤으로 선택된 주문 데이터</returns>
        public OrderData GetRandomOrder()
        {
            if (Orders == null || Orders.Count == 0)
            {
                Debug.LogError("[OrderDatabase] 주문 목록이 비어있습니다! OrderDatabase에 OrderData를 추가해주세요.");
                return null;
            }

            int randomIndex = Random.Range(0, Orders.Count);
            OrderData selectedOrder = Orders[randomIndex];

            Debug.Log($"[OrderDatabase] 랜덤 주문 선택: {selectedOrder.OrderName}");
            return selectedOrder;
        }

        /// <summary>
        /// 특정 인덱스의 주문 가져오기 (테스트용)
        /// </summary>
        public OrderData GetOrderAtIndex(int index)
        {
            if (Orders == null || Orders.Count == 0)
            {
                Debug.LogError("[OrderDatabase] 주문 목록이 비어있습니다!");
                return null;
            }

            if (index < 0 || index >= Orders.Count)
            {
                Debug.LogWarning($"[OrderDatabase] 인덱스 범위 초과: {index}. 0~{Orders.Count - 1} 사이여야 합니다.");
                return null;
            }

            return Orders[index];
        }
    }
}
