using System;
using System.Collections.Generic;
using UnityEngine;
using RecipeAboutLife.Events;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 요리 시스템 조율자 (Orchestrator)
    /// 
    /// 역할:
    /// - 6단계 요리 프로세스 조율
    /// - 드래그 앤 드롭 공통 처리
    /// - 품질 최종 계산
    /// - 이벤트 발생 (다른 시스템과 통신)
    /// 
    /// 책임 범위:
    /// - ✅ 요리 시스템만 관리
    /// - ❌ 손님, 멘탈, 스테이지 등은 이벤트로만 통신
    /// 
    /// Phase 1 완료 (기본 구조)
    /// Phase 2 대기: Step 클래스 구현
    /// </summary>
    public class CookingManager : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================
        
        private static CookingManager _instance;
        public static CookingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CookingManager>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration (ScriptableObject)
        // ==========================================
        
        [Header("Configuration")]
        [SerializeField] private RecipeConfigSO recipeConfig;

        // ==========================================
        // Current State
        // ==========================================
        
        [Header("Current State (Debug)")]
        [SerializeField] private CookingStepType currentStep = CookingStepType.None;
        [SerializeField] private HotdogRecipe currentRecipe;
        [SerializeField] private CustomerOrder currentOrder;
        [SerializeField] private float recipeQuality = 100f;

        // ==========================================
        // Step System
        // ==========================================
        
        private Dictionary<CookingStepType, ICookingStep> cookingSteps;
        private ICookingStep currentStepInstance;

        // ==========================================
        // Drag System
        // ==========================================
        
        [Header("Drag System")]
        private GameObject currentDragObject;
        private Vector3 dragStartPosition;
        private bool isDragging = false;
        private bool isServingDrag = false;  // 서빙용 드래그 구분

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // Singleton 설정
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 초기화
            cookingSteps = new Dictionary<CookingStepType, ICookingStep>();
            InitializeCookingSteps();
        }

        private void Start()
        {
            ValidateConfiguration();
        }

        // ==========================================
        // Initialization
        // ==========================================

        /// <summary>
        /// 각 요리 단계 클래스 등록
        /// TODO Phase 2: Step 클래스 생성 후 여기에 등록
        /// </summary>
        private void InitializeCookingSteps()
        {
            // TODO Phase 2: Step 클래스 등록
            // cookingSteps[CookingStepType.StickPickup] = new StickPickupStep();
            // cookingSteps[CookingStepType.Ingredient] = new IngredientStep();
            // cookingSteps[CookingStepType.Batter] = new BatterStep();
            // cookingSteps[CookingStepType.Frying] = new FryingStep();
            // cookingSteps[CookingStepType.Topping] = new ToppingStep();
            // cookingSteps[CookingStepType.Completed] = new CompletionStep();

            Debug.Log("CookingManager: Steps initialized (placeholder)");
        }

        /// <summary>
        /// 설정 검증
        /// </summary>
        private void ValidateConfiguration()
        {
            if (recipeConfig == null)
            {
                Debug.LogError("CookingManager: RecipeConfigSO is not assigned! Please assign it in the Inspector.");
            }
        }

        // ==========================================
        // Public API: Cooking Flow
        // ==========================================

        /// <summary>
        /// 요리 시작 (외부에서 주문을 주입받음)
        /// CustomerManager가 손님 도착 시 호출
        /// </summary>
        public void StartCooking(CustomerOrder order)
        {
            if (order == null)
            {
                Debug.LogError("CookingManager: Cannot start cooking with null order!");
                return;
            }

            currentOrder = order;
            currentRecipe = new HotdogRecipe();
            recipeQuality = 100f;

            ChangeStep(CookingStepType.StickPickup);

            Debug.Log($"CookingManager: Started cooking. Order: {order.GetDescription()}");
        }

        /// <summary>
        /// 현재 단계 처리
        /// </summary>
        public void ProcessCurrentStep(object data)
        {
            if (currentStepInstance == null)
            {
                Debug.LogWarning("CookingManager: No active step to process!");
                return;
            }

            bool stepCompleted = currentStepInstance.Process(data, ref recipeQuality);

            if (stepCompleted)
            {
                // 다음 단계로 진행
                CookingStepType nextStep = GetNextStep(currentStep);
                
                if (nextStep == CookingStepType.Completed)
                {
                    CompleteCooking();
                }
                else
                {
                    ChangeStep(nextStep);
                }
            }
        }

        /// <summary>
        /// 요리 완성 및 제공
        /// </summary>
        private void CompleteCooking()
        {
            // 최종 품질 계산
            float finalQuality = CalculateFinalQuality();
            currentRecipe.quality = finalQuality;

            // 주문 일치 확인
            bool matchesOrder = CheckOrderMatch();
            currentRecipe.matchesOrder = matchesOrder;

            // 보상 계산
            int reward = currentRecipe.CalculateReward(recipeConfig);

            Debug.Log($"CookingManager: Recipe completed! Quality: {finalQuality:F1}, Matches: {matchesOrder}, Reward: {reward}");

            // ===== 이벤트 발생 (외부 시스템과 통신) =====
            GameEvents.TriggerRecipeCompleted(currentRecipe);
            GameEvents.TriggerQualityCalculated(finalQuality);
            
            // 효과음 요청
            GameEvents.TriggerSFXRequested(recipeConfig.sfxRecipeComplete);

            // 상태 초기화
            ChangeStep(CookingStepType.None);
        }

        /// <summary>
        /// 요리 폐기 (실수 시)
        /// </summary>
        public void DiscardRecipe()
        {
            Debug.Log("CookingManager: Recipe discarded!");

            // ===== 이벤트 발생 =====
            GameEvents.TriggerRecipeDiscarded();
            GameEvents.TriggerMistakeMade();  // 멘탈 감소
            GameEvents.TriggerSFXRequested(recipeConfig.sfxDiscard);

            // 새로 시작
            ResetCookingState();
            
            if (currentOrder != null)
            {
                StartCooking(currentOrder);
            }
        }

        /// <summary>
        /// 요리 상태 초기화
        /// </summary>
        private void ResetCookingState()
        {
            currentRecipe = new HotdogRecipe();
            recipeQuality = 100f;
            currentStep = CookingStepType.None;
            currentStepInstance = null;
        }

        // ==========================================
        // Step Management
        // ==========================================

        /// <summary>
        /// 단계 전환
        /// </summary>
        private void ChangeStep(CookingStepType newStep)
        {
            // 이전 단계 종료
            if (currentStepInstance != null)
            {
                currentStepInstance.Exit();
            }

            currentStep = newStep;

            // 새 단계 시작
            if (newStep != CookingStepType.None && cookingSteps.ContainsKey(newStep))
            {
                currentStepInstance = cookingSteps[newStep];
                currentStepInstance.Enter(currentRecipe);
            }
            else
            {
                currentStepInstance = null;
            }

            // ===== 이벤트 발생 =====
            GameEvents.TriggerCookingStepChanged(newStep);

            Debug.Log($"CookingManager: Changed to step {newStep}");
        }

        /// <summary>
        /// 다음 단계 결정
        /// </summary>
        private CookingStepType GetNextStep(CookingStepType current)
        {
            switch (current)
            {
                case CookingStepType.None:
                    return CookingStepType.StickPickup;
                case CookingStepType.StickPickup:
                    return CookingStepType.Ingredient;
                case CookingStepType.Ingredient:
                    return CookingStepType.Batter;
                case CookingStepType.Batter:
                    return CookingStepType.Frying;
                case CookingStepType.Frying:
                    return CookingStepType.Topping;
                case CookingStepType.Topping:
                    return CookingStepType.Completed;
                default:
                    return CookingStepType.None;
            }
        }

        // ==========================================
        // Drag & Drop System
        // ==========================================

        /// <summary>
        /// 드래그 시작 (일반)
        /// </summary>
        public void StartDrag(GameObject hotdog)
        {
            if (hotdog == null) return;

            currentDragObject = hotdog;
            dragStartPosition = hotdog.transform.position;
            isDragging = true;
            isServingDrag = false;

            Debug.Log($"CookingManager: Started dragging {hotdog.name}");
        }

        /// <summary>
        /// 드래그 시작 (서빙용)
        /// </summary>
        public void StartServingDrag(GameObject hotdog)
        {
            if (hotdog == null) return;

            currentDragObject = hotdog;
            dragStartPosition = hotdog.transform.position;
            isDragging = true;
            isServingDrag = true;

            Debug.Log($"CookingManager: Started serving drag {hotdog.name}");
        }

        /// <summary>
        /// 드래그 업데이트
        /// </summary>
        public void UpdateDrag(Vector3 worldPosition)
        {
            if (!isDragging || currentDragObject == null) return;

            currentDragObject.transform.position = worldPosition;
        }

        /// <summary>
        /// 드래그 종료
        /// </summary>
        public void EndDrag()
        {
            if (!isDragging) return;

            if (isServingDrag)
            {
                EndServingDrag(currentDragObject.transform.position);
            }
            else
            {
                EndNormalDrag(currentDragObject.transform.position);
            }

            isDragging = false;
            isServingDrag = false;
        }

        /// <summary>
        /// 일반 드래그 종료 (요리 단계 간 이동)
        /// </summary>
        private void EndNormalDrag(Vector3 dropPosition)
        {
            // TODO Phase 3: 드롭 영역 검증
            bool validDrop = ValidateDropZone(dropPosition);

            if (validDrop)
            {
                Debug.Log("CookingManager: Valid drop!");
                // 다음 단계로 진행 등...
            }
            else
            {
                // 원래 위치로 복귀
                ReturnToOriginalPosition();
            }
        }

        /// <summary>
        /// 서빙 드래그 종료 (손님에게 제공)
        /// </summary>
        private void EndServingDrag(Vector3 dropPosition)
        {
            // TODO Phase 3: 손님 영역 체크
            bool servedToCustomer = TryServeToCustomer(dropPosition);

            if (servedToCustomer)
            {
                Debug.Log("CookingManager: Served to customer!");
                CompleteCooking();
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }

        /// <summary>
        /// 드롭 영역 검증
        /// TODO Phase 3: CookingStation 구현 후 실제 검증
        /// </summary>
        private bool ValidateDropZone(Vector3 position)
        {
            // TODO Phase 3: Raycast로 CookingStation 찾기
            return false;
        }

        /// <summary>
        /// 손님에게 서빙 시도
        /// TODO Phase 3: CustomerServingZone 구현 후 실제 검증
        /// </summary>
        private bool TryServeToCustomer(Vector3 position)
        {
            // TODO Phase 3: 손님 영역 체크
            return false;
        }

        /// <summary>
        /// 원래 위치로 복귀
        /// </summary>
        private void ReturnToOriginalPosition()
        {
            if (currentDragObject != null)
            {
                // TODO Phase 3: DraggableHotdog 컴포넌트 사용
                currentDragObject.transform.position = dragStartPosition;
                Debug.Log("CookingManager: Returned to original position");
            }
        }

        // ==========================================
        // Quality Calculation
        // ==========================================

        /// <summary>
        /// 최종 품질 계산
        /// </summary>
        private float CalculateFinalQuality()
        {
            float quality = recipeQuality;  // 100에서 시작

            // 1. 속재료 체크
            if (!FillingHelper.CheckMatch(currentRecipe.fillingType, currentOrder.filling))
            {
                quality -= recipeConfig.penaltyWrongFilling;
                RegisterMistake("속재료가 주문과 다릅니다!");
            }

            // 2. 튀김 상태 체크
            bool causesMentalDecrease;
            float fryingPenalty = recipeConfig.GetFryingPenalty(currentRecipe.fryingColor, out causesMentalDecrease);
            quality -= fryingPenalty;
            
            if (causesMentalDecrease)
            {
                RegisterMistake($"튀김 상태가 좋지 않습니다: {currentRecipe.fryingColor}");
            }

            // 3. 반죽 체크
            float batterPenalty = recipeConfig.GetBatterPenalty(currentRecipe.batterAmount);
            quality -= batterPenalty;

            // 4. 소스 체크
            if (!SauceHelper.CheckMatch(currentRecipe.sauceAmounts, currentOrder.sauces))
            {
                quality -= recipeConfig.penaltyWrongSauce;
                RegisterMistake("소스가 주문과 다릅니다!");
            }

            // 5. 설탕 체크
            if (currentRecipe.hasSugar != currentOrder.requiresSugar)
            {
                quality -= recipeConfig.penaltyWrongSugar;
            }

            // 최소 0점
            quality = Mathf.Max(0f, quality);

            return quality;
        }

        /// <summary>
        /// 주문 일치 확인
        /// </summary>
        private bool CheckOrderMatch()
        {
            bool fillingMatch = FillingHelper.CheckMatch(currentRecipe.fillingType, currentOrder.filling);
            bool sauceMatch = SauceHelper.CheckMatch(currentRecipe.sauceAmounts, currentOrder.sauces);
            bool sugarMatch = (currentRecipe.hasSugar == currentOrder.requiresSugar);
            
            // 튀김은 Raw/Burnt만 아니면 OK
            bool fryingOK = (currentRecipe.fryingColor != FryingColor.Raw && 
                           currentRecipe.fryingColor != FryingColor.Burnt);

            return fillingMatch && sauceMatch && sugarMatch && fryingOK;
        }

        /// <summary>
        /// 실수 등록
        /// </summary>
        private void RegisterMistake(string message)
        {
            Debug.LogWarning($"CookingManager: Mistake - {message}");

            // ===== 이벤트 발생 (MentalManager가 구독) =====
            GameEvents.TriggerMistakeMade();
            
            // 알림 표시
            GameEvents.TriggerShowNotification(message);
            
            // 효과음
            GameEvents.TriggerSFXRequested(recipeConfig.sfxMistake);
        }

        // ==========================================
        // Public Accessors
        // ==========================================

        public HotdogRecipe GetCurrentRecipe() => currentRecipe;
        public CustomerOrder GetCurrentOrder() => currentOrder;
        public CookingStepType GetCurrentStep() => currentStep;
        public float GetCurrentQuality() => recipeQuality;
        public bool IsCooking() => currentStep != CookingStepType.None;
        public bool IsDragging() => isDragging;

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Start Random Cooking")]
        private void TestStartCooking()
        {
            CustomerOrder testOrder = CustomerOrder.GenerateRandom();
            StartCooking(testOrder);
        }

        [ContextMenu("Test: Complete Current Recipe")]
        private void TestCompleteCooking()
        {
            if (IsCooking())
            {
                CompleteCooking();
            }
            else
            {
                Debug.Log("No active cooking to complete!");
            }
        }

        [ContextMenu("Test: Discard Recipe")]
        private void TestDiscardRecipe()
        {
            DiscardRecipe();
        }
#endif
    }
}