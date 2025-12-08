using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 요리 시스템 설정 데이터 (ScriptableObject)
    /// Inspector에서 수정 가능하며, 코드와 데이터를 분리
    /// 
    /// 생성 방법:
    /// 1. Project 창에서 우클릭
    /// 2. Create > Recipe About Life > Recipe Config
    /// 3. CookingManager에 할당
    /// </summary>
    [CreateAssetMenu(fileName = "RecipeConfig", menuName = "Recipe About Life/Recipe Config")]
    public class RecipeConfigSO : ScriptableObject
    {
        [Header("Quality Penalties")]
        [Tooltip("속재료가 주문과 다를 때 감점")]
        public float penaltyWrongFilling = 30f;
        
        [Tooltip("소스가 주문과 다를 때 감점")]
        public float penaltyWrongSauce = 30f;
        
        [Tooltip("설탕이 주문과 다를 때 감점")]
        public float penaltyWrongSugar = 10f;
        
        [Tooltip("반죽이 부족할 때 감점")]
        public float penaltyPoorBatter = 10f;

        [Header("Frying Times (seconds)")]
        [Tooltip("Raw 구간 최대 시간")]
        public float timeRawMax = 3f;
        
        [Tooltip("Yellow 구간 최대 시간")]
        public float timeYellowMax = 7f;
        
        [Tooltip("Golden 구간 최대 시간 (최적!)")]
        public float timeGoldenMax = 9f;
        
        [Tooltip("Brown 구간 최대 시간")]
        public float timeBrownMax = 11f;

        [Header("Frying Penalties")]
        [Tooltip("Raw 상태 감점")]
        public float penaltyRaw = 40f;
        
        [Tooltip("Yellow 상태 감점")]
        public float penaltyYellow = 15f;
        
        [Tooltip("Brown 상태 감점")]
        public float penaltyBrown = 15f;
        
        [Tooltip("Burnt 상태 감점")]
        public float penaltyBurnt = 40f;

        [Header("Batter Settings")]
        [Tooltip("최적 반죽량 (%)")]
        [Range(0f, 100f)]
        public float optimalBatterAmount = 80f;
        
        [Tooltip("최대 반죽량 (%)")]
        [Range(0f, 100f)]
        public float maxBatterAmount = 100f;

        [Header("Rewards")]
        [Tooltip("기본 보상 (원)")]
        public int baseReward = 100;
        
        [Tooltip("주문 일치 시 보너스 배율")]
        public float orderMatchBonus = 1.5f;

        [Header("Mental System")]
        [Tooltip("실수 시 멘탈 감소량 (%)")]
        [Range(0f, 100f)]
        public float mentalDecreasePerMistake = 15f;

        [Header("Sound Effects")]
        [Tooltip("효과음 이름들")]
        public string sfxStepComplete = "StepComplete";
        public string sfxMistake = "Mistake";
        public string sfxDiscard = "Discard";
        public string sfxRecipeComplete = "RecipeComplete";
        public string sfxServe = "Serve";

        // ===== Helper 메서드 =====

        /// <summary>
        /// 튀김 시간에서 색상 계산
        /// </summary>
        public FryingColor GetColorFromTime(float time)
        {
            if (time < timeRawMax) return FryingColor.Raw;
            if (time < timeYellowMax) return FryingColor.Yellow;
            if (time < timeGoldenMax) return FryingColor.Golden;
            if (time < timeBrownMax) return FryingColor.Brown;
            return FryingColor.Burnt;
        }

        /// <summary>
        /// 튀김 색상에 따른 감점 계산
        /// </summary>
        public float GetFryingPenalty(FryingColor color, out bool causesMentalDecrease)
        {
            causesMentalDecrease = false;

            switch (color)
            {
                case FryingColor.Raw:
                    causesMentalDecrease = true;
                    return penaltyRaw;
                    
                case FryingColor.Yellow:
                    return penaltyYellow;
                    
                case FryingColor.Golden:
                    return 0f;  // 최적!
                    
                case FryingColor.Brown:
                    return penaltyBrown;
                    
                case FryingColor.Burnt:
                    causesMentalDecrease = true;
                    return penaltyBurnt;
                    
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 반죽량에 따른 감점 계산
        /// </summary>
        public float GetBatterPenalty(float batterAmount)
        {
            if (batterAmount >= optimalBatterAmount)
                return 0f;
            
            return penaltyPoorBatter;
        }
    }
}