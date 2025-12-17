using System.Collections.Generic;
using UnityEngine;

namespace RecipeAboutLife.Orders
{
    /// <summary>
    /// 속재료 종류 (반쪽 단위)
    /// </summary>
    public enum FillingType
    {
        HalfSausage,  // 반소시지
        HalfCheese    // 반치즈
    }

    /// <summary>
    /// 소스 종류
    /// </summary>
    public enum SauceType
    {
        Ketchup,   // 케찹
        Mustard    // 머스타드
    }

    /// <summary>
    /// 소스 최소량
    /// </summary>
    public enum SauceAmount
    {
        Low,      // 조금
        Medium,   // 중간
        High      // 많이
    }

    /// <summary>
    /// 소스 요구사항 (종류 + 최소량)
    /// </summary>
    [System.Serializable]
    public class SauceRequirement
    {
        public SauceType SauceType;
        public SauceAmount MinAmount;

        public SauceRequirement(SauceType type, SauceAmount amount)
        {
            SauceType = type;
            MinAmount = amount;
        }
    }

    /// <summary>
    /// 핫도그 주문 데이터
    /// </summary>
    [CreateAssetMenu(fileName = "NewOrder", menuName = "RecipeAboutLife/Order")]
    public class OrderData : ScriptableObject
    {
        [Header("주문 정보")]
        [Tooltip("주문 이름 (UI 표시용)")]
        public string OrderName = "새 주문";

        [Header("속재료 (2칸 고정)")]
        [Tooltip("첫 번째 슬롯")]
        public FillingType FillingSlot1 = FillingType.HalfSausage;

        [Tooltip("두 번째 슬롯")]
        public FillingType FillingSlot2 = FillingType.HalfCheese;

        [Header("토핑")]
        [Tooltip("설탕 필요 여부")]
        public bool NeedSugar = false;

        [Header("소스 요구사항")]
        [Tooltip("필요한 소스 종류와 최소량")]
        public List<SauceRequirement> SauceRequirements = new List<SauceRequirement>();

        /// <summary>
        /// 주문 내용을 텍스트로 변환
        /// </summary>
        public string GetOrderDescription()
        {
            string description = "";

            // 속재료
            string slot1 = FillingSlot1 == FillingType.HalfSausage ? "소시지" : "치즈";
            string slot2 = FillingSlot2 == FillingType.HalfSausage ? "소시지" : "치즈";
            description += $"속재료: {slot1}+{slot2}\n";

            // 설탕
            description += $"설탕: {(NeedSugar ? "O" : "X")}\n";

            // 소스
            if (SauceRequirements.Count > 0)
            {
                description += "소스: ";
                List<string> sauceTexts = new List<string>();
                foreach (var req in SauceRequirements)
                {
                    string sauceName = req.SauceType == SauceType.Ketchup ? "케찹" : "머스타드";
                    string amount = req.MinAmount == SauceAmount.Low ? "조금" :
                                   req.MinAmount == SauceAmount.Medium ? "중간" : "많이";
                    sauceTexts.Add($"{sauceName}({amount})");
                }
                description += string.Join(", ", sauceTexts);
            }

            return description;
        }
    }
}

/*
=== 샘플 OrderData 생성 가이드 ===

Unity 에디터에서:
1. Project 창 우클릭
2. Create -> RecipeAboutLife -> Order
3. 아래 조합 예시를 참고해서 10개 정도 만들기

샘플 조합 예시:

1. "소시지 핫도그" - 소시지+소시지, 설탕 O, 케찹(중간)
2. "치즈 핫도그" - 치즈+치즈, 설탕 X, 머스타드(조금)
3. "반반 핫도그" - 소시지+치즈, 설탕 O, 케찹(많이), 머스타드(조금)
4. "더블 소시지" - 소시지+소시지, 설탕 X, 케찹(많이), 머스타드(중간)
5. "더블 치즈" - 치즈+치즈, 설탕 O, 케찹(조금)
6. "심플 핫도그" - 소시지+치즈, 설탕 X, 케찹(중간)
7. "스위트 소시지" - 소시지+소시지, 설탕 O, 머스타드(많이)
8. "치즈 러버" - 치즈+치즈, 설탕 X, 케찹(중간), 머스타드(중간)
9. "미스터리 핫도그" - 소시지+치즈, 설탕 O, 케찹(많이)
10. "클래식" - 소시지+소시지, 설탕 X, 케찹(조금), 머스타드(조금)
*/
