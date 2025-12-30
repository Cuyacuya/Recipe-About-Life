using UnityEngine;
using RecipeAboutLife.Orders;

namespace RecipeAboutLife.Utilities
{
    /// <summary>
    /// 검증 결과 구조체
    /// </summary>
    public struct ValidationResult
    {
        public bool isMatch;            // 전체 주문 일치 여부
        public float quality;           // 0-100 품질 점수
        public bool ingredientMatch;    // 재료 일치
        public bool toppingMatch;       // 토핑 일치
        public bool sauceMatch;         // 소스 일치
        public string details;          // 상세 피드백
    }

    /// <summary>
    /// 주문 검증 유틸리티
    /// HotdogData와 OrderData를 비교하여 일치 여부와 품질 점수 계산
    /// </summary>
    public static class OrderValidator
    {
        /// <summary>
        /// 핫도그 데이터와 주문 데이터 비교
        /// </summary>
        public static ValidationResult Validate(Cooking.HotdogData hotdog, OrderData order)
        {
            if (hotdog == null || order == null)
            {
                Debug.LogError("[OrderValidator] Null data provided!");
                return new ValidationResult
                {
                    isMatch = false,
                    quality = 0f,
                    details = "Invalid data"
                };
            }

            // 재료 검증 (40점)
            bool ingredientMatch = ValidateIngredients(hotdog, order);

            // 토핑 검증 (20점)
            bool toppingMatch = ValidateToppings(hotdog, order);

            // 소스 검증 (20점)
            bool sauceMatch = ValidateSauces(hotdog, order);

            // 조리 품질 (20점)
            float cookingQuality = CalculateCookingQuality(hotdog);

            // 총 품질 점수 계산
            float totalQuality = 0f;
            if (ingredientMatch) totalQuality += 40f;
            if (toppingMatch) totalQuality += 20f;
            if (sauceMatch) totalQuality += 20f;
            totalQuality += cookingQuality;

            // 전체 일치 여부
            bool overallMatch = ingredientMatch && toppingMatch && sauceMatch;

            // 상세 피드백 생성
            string details = GenerateDetails(ingredientMatch, toppingMatch, sauceMatch, cookingQuality);

            Debug.Log($"[OrderValidator] Validation Result:\n" +
                     $"  Ingredient: {ingredientMatch}\n" +
                     $"  Topping: {toppingMatch}\n" +
                     $"  Sauce: {sauceMatch}\n" +
                     $"  Cooking Quality: {cookingQuality}/20\n" +
                     $"  Total Quality: {totalQuality}/100\n" +
                     $"  Overall Match: {overallMatch}");

            return new ValidationResult
            {
                isMatch = overallMatch,
                quality = totalQuality,
                ingredientMatch = ingredientMatch,
                toppingMatch = toppingMatch,
                sauceMatch = sauceMatch,
                details = details
            };
        }

        /// <summary>
        /// 재료 검증
        /// HotdogData의 filling1/2 (string)와 OrderData의 FillingSlot1/2 (enum) 비교
        /// </summary>
        private static bool ValidateIngredients(Cooking.HotdogData hotdog, OrderData order)
        {
            // OrderData FillingType enum → string 변환
            string expectedFill1 = ConvertFillingTypeToString(order.FillingSlot1);
            string expectedFill2 = ConvertFillingTypeToString(order.FillingSlot2);

            // 순서 무관 비교 (Slot1=Sausage, Slot2=Cheese 주문에 Cheese+Sausage로 만들어도 OK)
            bool exactMatch = (hotdog.filling1 == expectedFill1 && hotdog.filling2 == expectedFill2);
            bool reverseMatch = (hotdog.filling1 == expectedFill2 && hotdog.filling2 == expectedFill1);

            bool match = exactMatch || reverseMatch;

            Debug.Log($"[OrderValidator] Ingredient Check:\n" +
                     $"  Expected: {expectedFill1} + {expectedFill2}\n" +
                     $"  Got: {hotdog.filling1} + {hotdog.filling2}\n" +
                     $"  Match: {match}");

            return match;
        }

        /// <summary>
        /// FillingType enum → string 변환
        /// </summary>
        private static string ConvertFillingTypeToString(FillingType fillingType)
        {
            switch (fillingType)
            {
                case FillingType.HalfSausage:
                    return "Sausage";
                case FillingType.HalfCheese:
                    return "Cheese";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 토핑 검증
        /// </summary>
        private static bool ValidateToppings(Cooking.HotdogData hotdog, OrderData order)
        {
            bool match = hotdog.hasSugar == order.NeedSugar;

            Debug.Log($"[OrderValidator] Topping Check:\n" +
                     $"  Expected Sugar: {order.NeedSugar}\n" +
                     $"  Got Sugar: {hotdog.hasSugar}\n" +
                     $"  Match: {match}");

            return match;
        }

        /// <summary>
        /// 소스 검증
        /// 주의: HotdogData는 소스의 존재 여부만 추적 (hasKetchup, hasMustard)
        ///       소스 양(amount)은 검증 불가능
        /// </summary>
        private static bool ValidateSauces(Cooking.HotdogData hotdog, OrderData order)
        {
            // 주문에 소스가 없으면 항상 통과
            if (order.SauceRequirements == null || order.SauceRequirements.Count == 0)
            {
                Debug.Log("[OrderValidator] No sauce requirements - Pass");
                return true;
            }

            // 각 소스 요구사항 확인
            foreach (var req in order.SauceRequirements)
            {
                if (req.SauceType == SauceType.Ketchup && !hotdog.hasKetchup)
                {
                    Debug.Log($"[OrderValidator] Missing required Ketchup!");
                    return false;
                }

                if (req.SauceType == SauceType.Mustard && !hotdog.hasMustard)
                {
                    Debug.Log($"[OrderValidator] Missing required Mustard!");
                    return false;
                }

                // 소스 양은 HotdogData에 없으므로 검증 불가 (제한사항)
            }

            Debug.Log("[OrderValidator] Sauce Check: All required sauces present");
            return true;
        }

        /// <summary>
        /// 조리 품질 계산 (최대 20점)
        /// 반죽 단계와 튀김 상태를 기준으로 점수 부여
        /// </summary>
        private static float CalculateCookingQuality(Cooking.HotdogData hotdog)
        {
            float score = 0f;

            // 반죽 단계 (5점)
            // Stage 2-3이 적정
            if (hotdog.batterStage == 2 || hotdog.batterStage == 3)
            {
                score += 5f;
            }
            else if (hotdog.batterStage == 1)
            {
                score += 3f; // 너무 적음
            }

            // 튀김 상태 (15점)
            // Golden이 완벽, Yellow/Brown도 괜찮음
            switch (hotdog.fryingState)
            {
                case Cooking.FryingState.Golden:
                    score += 15f;  // 완벽
                    break;
                case Cooking.FryingState.Yellow:
                    score += 10f;  // 괜찮음
                    break;
                case Cooking.FryingState.Brown:
                    score += 5f;   // 약간 타버림
                    break;
                case Cooking.FryingState.Raw:
                    score += 0f;   // 덜 익음
                    break;
                case Cooking.FryingState.Burnt:
                    score += 0f;   // 탐
                    break;
            }

            Debug.Log($"[OrderValidator] Cooking Quality:\n" +
                     $"  Batter Stage: {hotdog.batterStage}\n" +
                     $"  Frying State: {hotdog.fryingState}\n" +
                     $"  Score: {score}/20");

            return score;
        }

        /// <summary>
        /// 상세 피드백 메시지 생성
        /// </summary>
        private static string GenerateDetails(bool ingredientMatch, bool toppingMatch, bool sauceMatch, float cookingQuality)
        {
            string details = "";

            if (!ingredientMatch)
                details += "재료가 다릅니다. ";

            if (!toppingMatch)
                details += "토핑이 다릅니다. ";

            if (!sauceMatch)
                details += "소스가 부족합니다. ";

            if (cookingQuality < 10f)
                details += "조리가 완벽하지 않습니다. ";

            if (string.IsNullOrEmpty(details))
                details = "완벽합니다!";

            return details.Trim();
        }
    }
}
