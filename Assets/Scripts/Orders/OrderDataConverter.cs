using UnityEngine;
using RecipeAboutLife.Cooking;

namespace RecipeAboutLife.Orders
{
    /// <summary>
    /// OrderData를 Cooking 시스템의 CustomerOrder로 변환
    /// 두 시스템 간의 브리지 역할
    /// </summary>
    public static class OrderDataConverter
    {
        /// <summary>
        /// OrderData를 CustomerOrder로 변환
        /// </summary>
        public static CustomerOrder ToCustomerOrder(OrderData orderData)
        {
            if (orderData == null)
            {
                Debug.LogError("[OrderDataConverter] OrderData가 null입니다!");
                return null;
            }

            CustomerOrder customerOrder = new CustomerOrder();

            // ===== 1. 속재료 변환 =====
            customerOrder.filling = ConvertFilling(orderData.FillingSlot1, orderData.FillingSlot2);

            // ===== 2. 소스 변환 =====
            customerOrder.sauces = new System.Collections.Generic.List<Cooking.SauceRequirement>();

            foreach (var sauce in orderData.SauceRequirements)
            {
                // SauceType 변환
                Cooking.SauceType cookingSauceType = sauce.SauceType == Orders.SauceType.Ketchup
                    ? Cooking.SauceType.Ketchup
                    : Cooking.SauceType.Mustard;

                // SauceAmount 변환
                Cooking.SauceAmount cookingSauceAmount = ConvertSauceAmount(sauce.MinAmount);

                customerOrder.sauces.Add(new Cooking.SauceRequirement(cookingSauceType, cookingSauceAmount));
            }

            // ===== 3. 설탕 변환 =====
            customerOrder.requiresSugar = orderData.NeedSugar;

            Debug.Log($"[OrderDataConverter] 변환 완료: {orderData.OrderName} -> {customerOrder.GetDescription()}");

            return customerOrder;
        }

        /// <summary>
        /// 두 반쪽 재료를 하나의 FillingType으로 변환
        /// </summary>
        private static Cooking.FillingType ConvertFilling(Orders.FillingType slot1, Orders.FillingType slot2)
        {
            // 소시지 + 소시지 = Sausage
            if (slot1 == Orders.FillingType.HalfSausage && slot2 == Orders.FillingType.HalfSausage)
            {
                return Cooking.FillingType.Sausage;
            }

            // 치즈 + 치즈 = Cheese
            if (slot1 == Orders.FillingType.HalfCheese && slot2 == Orders.FillingType.HalfCheese)
            {
                return Cooking.FillingType.Cheese;
            }

            // 소시지 + 치즈 (순서 무관) = Mixed
            return Cooking.FillingType.Mixed;
        }

        /// <summary>
        /// SauceAmount 변환 (Orders -> Cooking)
        /// </summary>
        private static Cooking.SauceAmount ConvertSauceAmount(Orders.SauceAmount orderAmount)
        {
            switch (orderAmount)
            {
                case Orders.SauceAmount.Low:
                    return Cooking.SauceAmount.Low;
                case Orders.SauceAmount.Medium:
                    return Cooking.SauceAmount.Medium;
                case Orders.SauceAmount.High:
                    return Cooking.SauceAmount.High;
                default:
                    return Cooking.SauceAmount.Medium;
            }
        }

        /// <summary>
        /// CustomerOrder를 OrderData로 역변환 (테스트용)
        /// </summary>
        public static OrderData FromCustomerOrder(CustomerOrder customerOrder)
        {
            // ScriptableObject는 런타임에 생성할 수 없으므로
            // 이 메서드는 디버깅/로깅 용도로만 사용
            Debug.LogWarning("[OrderDataConverter] FromCustomerOrder는 테스트 전용입니다.");
            return null;
        }

        /// <summary>
        /// 주문 설명 비교 (디버깅용)
        /// </summary>
        public static void CompareOrders(OrderData orderData, CustomerOrder customerOrder)
        {
            if (orderData == null || customerOrder == null) return;

            Debug.Log("=== Order Comparison ===");
            Debug.Log($"OrderData: {orderData.OrderName}");
            Debug.Log($"  {orderData.GetOrderDescription()}");
            Debug.Log($"CustomerOrder:");
            Debug.Log($"  {customerOrder.GetDescription()}");
        }
    }
}
