using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    // ============================================
    // Enums
    // ============================================
    
    /// <summary>
    /// 요리 단계
    /// </summary>
    public enum CookingStepType
    {
        None,          // 대기 상태
        StickPickup,   // 1. 꼬치 들기
        Ingredient,    // 2. 속재료 끼우기
        Batter,        // 3. 반죽 묻히기
        Frying,        // 4. 튀기기
        Topping,       // 5. 토핑/소스
        Completed      // 6. 완성 (서빙 가능)
    }
    
    /// <summary>
    /// 속재료 타입 (완성품)
    /// </summary>
    public enum FillingType
    {
        Sausage,       // 소시지 핫도그
        Cheese,        // 치즈 핫도그
        Mixed          // 반반 핫도그
    }
    
    /// <summary>
    /// 반쪽 재료 (선택용)
    /// </summary>
    public enum IngredientType
    {
        SausageHalf,   // 반쪽 소시지
        CheeseHalf     // 반쪽 치즈
    }
    
    /// <summary>
    /// 소스 종류
    /// </summary>
    public enum SauceType
    {
        Ketchup,       // 케찹
        Mustard        // 머스타드
    }
    
    /// <summary>
    /// 소스 양
    /// </summary>
    public enum SauceAmount
    {
        Low,           // 적음 (0-33%)
        Medium,        // 보통 (34-66%)
        High           // 많이 (67-100%)
    }
    
    /// <summary>
    /// 튀김 색상 (시간 기반)
    /// 0~3초: Raw
    /// 3~7초: Yellow
    /// 7~9초: Golden (최적!)
    /// 9~11초: Brown
    /// 11초+: Burnt
    /// </summary>
    public enum FryingColor
    {
        Raw,           // 흰색 (0-3초) - 익지 않음
        Yellow,        // 노랑 (3-7초) - 덜 익음
        Golden,        // 황금 (7-9초) - 최적!
        Brown,         // 갈색 (9-11초) - 약간 탐
        Burnt          // 검정 (11초+) - 완전히 탐
    }
    
    // ============================================
    // Data Classes
    // ============================================
    
    /// <summary>
    /// 핫도그 레시피 (제작 중인 핫도그 정보)
    /// </summary>
    [Serializable]
    public class HotdogRecipe
    {
        // === 1단계: 꼬치 ===
        public bool hasStick;
        
        // === 2단계: 속재료 ===
        public FillingType fillingType;
        
        // === 3단계: 반죽 ===
        public float batterAmount;      // 0-100 (80% 이상 최적)
        
        // === 4단계: 튀김 ===
        public float fryingTime;        // 초 단위
        public FryingColor fryingColor; // 색상 상태
        
        // === 5단계: 토핑/소스 ===
        public bool hasSugar;                                  // 설탕 유무
        public List<SauceType> sauces;                         // 뿌린 소스 종류들
        public Dictionary<SauceType, float> sauceAmounts;      // 소스별 양 (0-100)
        
        // === 평가 ===
        public float quality;           // 품질 점수 (0-100)
        public bool matchesOrder;       // 주문과 일치 여부
        
        /// <summary>
        /// 생성자
        /// </summary>
        public HotdogRecipe()
        {
            hasStick = false;
            fillingType = FillingType.Sausage;
            batterAmount = 0f;
            fryingTime = 0f;
            fryingColor = FryingColor.Raw;
            hasSugar = false;
            sauces = new List<SauceType>();
            sauceAmounts = new Dictionary<SauceType, float>();
            quality = 100f;
            matchesOrder = false;
        }
        
        /// <summary>
        /// 보상 계산
        /// 품질 점수와 주문 일치 여부에 따라 보상 결정
        /// </summary>
        /// <param name="config">레시피 설정 (보상 계산에 필요)</param>
        /// <returns>보상 금액</returns>
        public int CalculateReward(RecipeConfigSO config)
        {
            int baseReward = config.baseReward;
            float qualityMultiplier = quality / 100f;
            
            // 주문 일치 시 보너스
            if (matchesOrder)
            {
                qualityMultiplier *= config.orderMatchBonus;
            }
            
            int finalReward = Mathf.RoundToInt(baseReward * qualityMultiplier);
            return Mathf.Max(10, finalReward); // 최소 10원
        }
    }
    
    /// <summary>
    /// 소스 요구사항 (주문에 포함)
    /// </summary>
    [Serializable]
    public class SauceRequirement
    {
        public SauceType type;       // 소스 종류
        public SauceAmount amount;   // 필요 양
        
        public SauceRequirement(SauceType type, SauceAmount amount)
        {
            this.type = type;
            this.amount = amount;
        }
    }
    
    /// <summary>
    /// 손님 주문
    /// </summary>
    [Serializable]
    public class CustomerOrder
    {
        public FillingType filling;                    // 속재료
        public List<SauceRequirement> sauces;          // 소스 요구사항들
        public bool requiresSugar;                     // 설탕 필요 여부
        
        /// <summary>
        /// 생성자
        /// </summary>
        public CustomerOrder()
        {
            filling = FillingType.Sausage;
            sauces = new List<SauceRequirement>();
            requiresSugar = false;
        }
        
        /// <summary>
        /// 랜덤 주문 생성 (테스트용)
        /// </summary>
        public static CustomerOrder GenerateRandom()
        {
            CustomerOrder order = new CustomerOrder();
            
            // 랜덤 속재료
            order.filling = (FillingType)UnityEngine.Random.Range(0, 3);
            
            // 랜덤 소스 (1-2개)
            int sauceCount = UnityEngine.Random.Range(1, 3);
            List<SauceType> availableSauces = new List<SauceType> { SauceType.Ketchup, SauceType.Mustard };
            
            for (int i = 0; i < sauceCount && availableSauces.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, availableSauces.Count);
                SauceType type = availableSauces[index];
                availableSauces.RemoveAt(index);
                
                SauceAmount amount = (SauceAmount)UnityEngine.Random.Range(0, 3);
                order.sauces.Add(new SauceRequirement(type, amount));
            }
            
            // 랜덤 설탕 (50% 확률)
            order.requiresSugar = UnityEngine.Random.value > 0.5f;
            
            return order;
        }
        
        /// <summary>
        /// 주문 설명 (한글)
        /// </summary>
        public string GetDescription()
        {
            string desc = "";
            
            // 속재료
            switch (filling)
            {
                case FillingType.Sausage:
                    desc += "소시지 핫도그";
                    break;
                case FillingType.Cheese:
                    desc += "치즈 핫도그";
                    break;
                case FillingType.Mixed:
                    desc += "반반 핫도그";
                    break;
            }
            
            // 소스
            if (sauces.Count > 0)
            {
                desc += "\n소스: ";
                List<string> sauceDescs = new List<string>();
                
                foreach (var sauce in sauces)
                {
                    string sauceName = sauce.type == SauceType.Ketchup ? "케찹" : "머스타드";
                    string amountName = sauce.amount == SauceAmount.Low ? "적게" :
                                       sauce.amount == SauceAmount.Medium ? "보통" : "많이";
                    sauceDescs.Add($"{sauceName}({amountName})");
                }
                
                desc += string.Join(", ", sauceDescs);
            }
            
            // 설탕
            if (requiresSugar)
            {
                desc += "\n설탕 O";
            }
            
            return desc;
        }
    }
    
    // ============================================
    // Helper Classes
    // ============================================
    
    /// <summary>
    /// 튀김 시스템 헬퍼
    /// </summary>
    public static class FryingHelper
    {
        // 시간 상수
        public const float TIME_RAW_MAX = 3f;       // 0-3초: Raw
        public const float TIME_YELLOW_MAX = 7f;    // 3-7초: Yellow
        public const float TIME_GOLDEN_MAX = 9f;    // 7-9초: Golden (최적!)
        public const float TIME_BROWN_MAX = 11f;    // 9-11초: Brown
        // 11초 이상: Burnt
        
        /// <summary>
        /// 시간에 따른 FryingColor 반환
        /// </summary>
        public static FryingColor GetColorFromTime(float time)
        {
            if (time < TIME_RAW_MAX) return FryingColor.Raw;
            if (time < TIME_YELLOW_MAX) return FryingColor.Yellow;
            if (time < TIME_GOLDEN_MAX) return FryingColor.Golden;
            if (time < TIME_BROWN_MAX) return FryingColor.Brown;
            return FryingColor.Burnt;
        }
        
        /// <summary>
        /// FryingColor에 따른 품질 감점 계산
        /// </summary>
        /// <param name="color">튀김 색상</param>
        /// <param name="shouldDecreaseMental">멘탈 감소 여부</param>
        /// <returns>감점</returns>
        public static float GetPenalty(FryingColor color, out bool shouldDecreaseMental)
        {
            shouldDecreaseMental = false;
            
            switch (color)
            {
                case FryingColor.Raw:
                    shouldDecreaseMental = true;
                    return 40f;
                
                case FryingColor.Yellow:
                    return 15f;
                
                case FryingColor.Golden:
                    return 0f; // 최적!
                
                case FryingColor.Brown:
                    return 15f;
                
                case FryingColor.Burnt:
                    shouldDecreaseMental = true;
                    return 40f;
                
                default:
                    return 0f;
            }
        }
    }
    
    /// <summary>
    /// 소스 검증 헬퍼
    /// </summary>
    public static class SauceHelper
    {
        /// <summary>
        /// 퍼센트를 SauceAmount로 변환
        /// </summary>
        public static SauceAmount CalculateAmount(float percentage)
        {
            if (percentage < 33f) return SauceAmount.Low;
            if (percentage < 67f) return SauceAmount.Medium;
            return SauceAmount.High;
        }
        
        /// <summary>
        /// 레시피의 소스가 주문과 일치하는지 확인
        /// </summary>
        public static bool CheckMatch(Dictionary<SauceType, float> recipeSauces, 
                                       List<SauceRequirement> orderSauces)
        {
            // 소스 개수가 다르면 불일치
            if (recipeSauces.Count != orderSauces.Count)
                return false;
            
            // 각 소스 확인
            foreach (var requirement in orderSauces)
            {
                // 소스 타입이 없으면 불일치
                if (!recipeSauces.ContainsKey(requirement.type))
                    return false;
                
                // 소스 양 확인
                float amount = recipeSauces[requirement.type];
                SauceAmount actualAmount = CalculateAmount(amount);
                
                if (actualAmount != requirement.amount)
                    return false;
            }
            
            return true;
        }
    }
    
    /// <summary>
    /// 속재료 헬퍼
    /// </summary>
    public static class FillingHelper
    {
        /// <summary>
        /// 두 반쪽 재료를 조합하여 FillingType 반환
        /// 순서 무관!
        /// </summary>
        public static FillingType GetFillingType(IngredientType first, IngredientType second)
        {
            // 소시지 + 소시지 = Sausage
            if (first == IngredientType.SausageHalf && second == IngredientType.SausageHalf)
                return FillingType.Sausage;
            
            // 치즈 + 치즈 = Cheese
            if (first == IngredientType.CheeseHalf && second == IngredientType.CheeseHalf)
                return FillingType.Cheese;
            
            // 소시지 + 치즈 (순서 무관) = Mixed
            return FillingType.Mixed;
        }
        
        /// <summary>
        /// 레시피의 속재료가 주문과 일치하는지 확인
        /// </summary>
        public static bool CheckMatch(FillingType recipeType, FillingType orderType)
        {
            return recipeType == orderType;
        }
    }
}