using UnityEngine;
using RecipeAboutLife.Cooking;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 재료 끼우기 단계
    /// 
    /// 동작:
    /// - IngredientPopup 열기
    /// - 사용자가 재료 2개 선택 (소시지/치즈)
    /// - FillingType 결정 (Sausage, Cheese, Mixed)
    /// - 레시피에 저장
    /// 
    /// Phase 2.3 완료
    /// </summary>
    public class IngredientStep : ICookingStep
    {
        private RecipeConfigSO config;
        private HotdogRecipe currentRecipe;
        private IngredientPopup ingredientPopup;
        private bool isComplete = false;
        
        public IngredientStep(RecipeConfigSO recipeConfig)
        {
            this.config = recipeConfig;
        }
        
        public void Enter(HotdogRecipe recipe)
        {
            currentRecipe = recipe;
            isComplete = false;
            
            Debug.Log("[IngredientStep] ===== ENTER =====");
            
            // PopupManager에서 IngredientPopup 가져오기
            ingredientPopup = PopupManager.Instance.GetPopup<IngredientPopup>();
            
            if (ingredientPopup == null)
            {
                Debug.LogError("[IngredientStep] IngredientPopup not found in PopupManager!");
                
                // Fallback: Scene에서 직접 찾기 (Unity 6 방식)
                ingredientPopup = Object.FindFirstObjectByType<IngredientPopup>(FindObjectsInactive.Include);
                
                if (ingredientPopup == null)
                {
                    Debug.LogError("[IngredientStep] IngredientPopup not found in Scene!");
                    return;
                }
            }
            
            // 이벤트 구독
            ingredientPopup.OnIngredientsCompleted += OnIngredientsSelected;
            
            // 팝업 열기
            ingredientPopup.Open();
            
            Debug.Log("[IngredientStep] Popup opened, waiting for user input...");
        }
        
        /// <summary>
        /// 매 프레임 업데이트 (사용 안 함)
        /// </summary>
        public void Update()
        {
            // 이 단계에서는 Update 불필요
        }
        
        /// <summary>
        /// 재료 2개가 선택됨
        /// </summary>
        private void OnIngredientsSelected(FillingType first, FillingType second)
        {
            Debug.Log($"[IngredientStep] 🎯 Ingredients selected: {first}, {second}");
            
            // FillingType 계산 (직접 구현)
            FillingType finalFilling = CalculateFillingType(first, second);
            
            Debug.Log($"[IngredientStep] Final FillingType: {finalFilling}");
            
            // CookingManager에 전달
            CookingManager.Instance.ProcessCurrentStep(finalFilling);
        }
        
        /// <summary>
        /// 두 재료를 조합하여 최종 FillingType 계산
        /// </summary>
        private FillingType CalculateFillingType(FillingType first, FillingType second)
        {
            // 둘 다 소시지 → Sausage
            if (first == FillingType.Sausage && second == FillingType.Sausage)
            {
                return FillingType.Sausage;
            }
            
            // 둘 다 치즈 → Cheese
            if (first == FillingType.Cheese && second == FillingType.Cheese)
            {
                return FillingType.Cheese;
            }
            
            // 하나는 소시지, 하나는 치즈 → Mixed
            return FillingType.Mixed;
        }
        
        public bool Process(object data, ref float quality)
        {
            Debug.Log($"[IngredientStep] ===== PROCESS ===== Data: {data}");
            
            if (data is FillingType fillingType)
            {
                // 레시피에 저장
                currentRecipe.fillingType = fillingType;
                
                Debug.Log($"[IngredientStep] ✅ FillingType saved to recipe: {fillingType}");
                
                // 품질 유지 (재료 선택은 품질에 영향 없음)
                // quality는 그대로 유지
                
                isComplete = true;
                return true;
            }
            
            Debug.LogWarning($"[IngredientStep] Invalid data type: {data?.GetType().Name ?? "null"}");
            return false;
        }
        
        public void Exit()
        {
            Debug.Log("[IngredientStep] ===== EXIT =====");
            
            // 이벤트 구독 해제
            if (ingredientPopup != null)
            {
                ingredientPopup.OnIngredientsCompleted -= OnIngredientsSelected;
                
                // 팝업 닫기 (이미 닫혀있을 수도 있음)
                if (ingredientPopup.IsOpen)
                {
                    ingredientPopup.Close();
                }
            }
            
            Debug.Log($"[IngredientStep] Step complete! FillingType: {currentRecipe.fillingType}");
        }
        
        public bool IsStepComplete()
        {
            return isComplete;
        }
    }
}