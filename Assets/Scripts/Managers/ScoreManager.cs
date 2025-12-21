using System.Collections.Generic;
using UnityEngine;
using RecipeAboutLife.Events;
using RecipeAboutLife.Cooking;

namespace RecipeAboutLife.Managers
{
    /// <summary>
    /// 점수 및 재화 관리 시스템
    /// 5명의 NPC에게 음식을 제공하고 재화를 획득
    /// 목표 재화 달성 시 플레이어 대화 잠금 해제
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================

        private static ScoreManager _instance;
        public static ScoreManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<ScoreManager>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration
        // ==========================================

        [Header("목표 설정")]
        [SerializeField]
        [Tooltip("대화 잠금 해제에 필요한 최소 재화")]
        private int targetTotalReward = 400;

        [SerializeField]
        [Tooltip("스테이지당 NPC 수")]
        private int maxNPCCount = 5;

        // ==========================================
        // Current State
        // ==========================================

        [Header("현재 상태 (Debug)")]
        [SerializeField]
        private int totalReward = 0;

        [SerializeField]
        private int currentNPCIndex = 0;

        [SerializeField]
        private bool isDialogueUnlocked = false;

        [SerializeField]
        private List<NPCRewardData> npcRewards = new List<NPCRewardData>();

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// 재화가 변경되었을 때 발생 (UI 업데이트용)
        /// </summary>
        public System.Action<int> OnTotalRewardChanged;

        /// <summary>
        /// NPC에게 재화를 지급했을 때 발생
        /// </summary>
        public System.Action<int, int> OnNPCRewarded; // (npcIndex, reward)

        /// <summary>
        /// 대화가 잠금 해제되었을 때 발생
        /// </summary>
        public System.Action OnDialogueUnlocked;

        /// <summary>
        /// 스테이지가 완료되었을 때 발생
        /// </summary>
        public System.Action<bool> OnStageCompleted; // (success)

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

            InitializeNPCRewards();
        }

        private void OnEnable()
        {
            // 요리 완료 이벤트 구독
            GameEvents.OnRecipeCompleted += OnRecipeCompleted;
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            GameEvents.OnRecipeCompleted -= OnRecipeCompleted;
        }

        // ==========================================
        // Initialization
        // ==========================================

        /// <summary>
        /// NPC 보상 데이터 초기화
        /// </summary>
        private void InitializeNPCRewards()
        {
            npcRewards.Clear();

            for (int i = 0; i < maxNPCCount; i++)
            {
                npcRewards.Add(new NPCRewardData
                {
                    npcIndex = i + 1,
                    reward = 0,
                    quality = 0,
                    orderMatched = false,
                    isServed = false
                });
            }

            Debug.Log($"[ScoreManager] Initialized {maxNPCCount} NPC reward slots");
        }

        // ==========================================
        // Recipe Completion Handler
        // ==========================================

        /// <summary>
        /// 요리 완료 이벤트 처리
        /// CookingManager에서 발생한 이벤트를 받아서 재화 지급
        /// </summary>
        private void OnRecipeCompleted(HotdogRecipe recipe)
        {
            if (currentNPCIndex >= maxNPCCount)
            {
                Debug.LogWarning("[ScoreManager] 이미 모든 NPC에게 음식을 제공했습니다!");
                return;
            }

            // 보상 계산 (RecipeConfigSO 필요)
            RecipeConfigSO config = FindRecipeConfig();
            if (config == null)
            {
                Debug.LogError("[ScoreManager] RecipeConfigSO를 찾을 수 없습니다!");
                return;
            }

            int reward = recipe.CalculateReward(config);

            // NPC 보상 데이터 저장
            NPCRewardData npcData = npcRewards[currentNPCIndex];
            npcData.reward = reward;
            npcData.quality = recipe.quality;
            npcData.orderMatched = recipe.matchesOrder;
            npcData.isServed = true;

            // 총 재화 증가
            totalReward += reward;

            Debug.Log($"[ScoreManager] NPC {currentNPCIndex + 1} 보상 지급: {reward}원 (품질: {recipe.quality:F1}, 총합: {totalReward}원)");

            // 이벤트 발생
            OnNPCRewarded?.Invoke(currentNPCIndex + 1, reward);
            OnTotalRewardChanged?.Invoke(totalReward);

            // 다음 NPC로
            currentNPCIndex++;

            // 모든 NPC 완료 확인
            if (currentNPCIndex >= maxNPCCount)
            {
                OnAllNPCsServed();
            }
        }

        // ==========================================
        // Stage Completion
        // ==========================================

        /// <summary>
        /// 모든 NPC에게 음식을 제공했을 때
        /// </summary>
        private void OnAllNPCsServed()
        {
            Debug.Log($"[ScoreManager] 모든 NPC 완료! 총 재화: {totalReward}원 / 목표: {targetTotalReward}원");

            // 목표 달성 확인
            bool success = totalReward >= targetTotalReward;

            if (success)
            {
                UnlockDialogue();
            }
            else
            {
                Debug.Log($"[ScoreManager] 목표 미달성! 부족한 재화: {targetTotalReward - totalReward}원");
            }

            // 스테이지 완료 이벤트
            OnStageCompleted?.Invoke(success);

            // 게임 이벤트 발생
            GameEvents.TriggerAllCustomersServed();
        }

        /// <summary>
        /// 대화 잠금 해제
        /// </summary>
        private void UnlockDialogue()
        {
            if (isDialogueUnlocked)
            {
                Debug.LogWarning("[ScoreManager] 이미 대화가 잠금 해제되었습니다!");
                return;
            }

            isDialogueUnlocked = true;

            Debug.Log("[ScoreManager] 대화 잠금 해제! 플레이어와 대화할 수 있습니다.");

            // 이벤트 발생
            OnDialogueUnlocked?.Invoke();

            // 알림 표시
            GameEvents.TriggerShowNotification("목표 달성! 대화가 가능합니다.");
        }

        // ==========================================
        // Helper Methods
        // ==========================================

        /// <summary>
        /// RecipeConfigSO 찾기
        /// </summary>
        private RecipeConfigSO FindRecipeConfig()
        {
            CookingManager cookingManager = CookingManager.Instance;
            if (cookingManager != null)
            {
                // CookingManager에서 config 가져오기 (reflection 사용)
                var field = typeof(CookingManager).GetField("recipeConfig",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(cookingManager) as RecipeConfigSO;
                }
            }

            // 대체: Resources에서 찾기
            RecipeConfigSO[] configs = Resources.FindObjectsOfTypeAll<RecipeConfigSO>();
            if (configs.Length > 0)
            {
                return configs[0];
            }

            return null;
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 스테이지 재시작
        /// </summary>
        public void RestartStage()
        {
            totalReward = 0;
            currentNPCIndex = 0;
            isDialogueUnlocked = false;

            InitializeNPCRewards();

            OnTotalRewardChanged?.Invoke(totalReward);

            Debug.Log("[ScoreManager] 스테이지 재시작");
        }

        /// <summary>
        /// 현재 총 재화 가져오기
        /// </summary>
        public int GetTotalReward()
        {
            return totalReward;
        }

        /// <summary>
        /// 목표 재화 가져오기
        /// </summary>
        public int GetTargetReward()
        {
            return targetTotalReward;
        }

        /// <summary>
        /// 대화 잠금 해제 여부
        /// </summary>
        public bool IsDialogueUnlocked()
        {
            return isDialogueUnlocked;
        }

        /// <summary>
        /// NPC 보상 데이터 가져오기
        /// </summary>
        public List<NPCRewardData> GetNPCRewards()
        {
            return npcRewards;
        }

        /// <summary>
        /// 진행률 가져오기 (0-1)
        /// </summary>
        public float GetProgress()
        {
            return (float)totalReward / targetTotalReward;
        }

        // ==========================================
        // Public Test Methods (버튼에서 호출 가능)
        // ==========================================

        /// <summary>
        /// 테스트용: 완벽한 요리로 NPC 서빙 (최고 재화 획득)
        /// UI 버튼에서 호출 가능
        /// </summary>
        public void TestServePerfectFood()
        {
            if (currentNPCIndex >= maxNPCCount)
            {
                Debug.Log("[TestButton] 모든 NPC 완료!");
                return;
            }

            // 완벽한 레시피 생성
            HotdogRecipe perfectRecipe = new HotdogRecipe
            {
                hasStick = true,
                fillingType = Cooking.FillingType.Sausage,
                batterAmount = 100f,
                fryingTime = 8f, // Golden 구간
                fryingColor = Cooking.FryingColor.Golden,
                hasSugar = false,
                quality = 100f,
                matchesOrder = true
            };

            // RecipeConfig 찾기
            RecipeConfigSO config = FindRecipeConfig();
            if (config == null)
            {
                Debug.LogError("[TestButton] RecipeConfigSO를 찾을 수 없습니다!");
                return;
            }

            // 최고 보상 계산 (100 * 1.0 * 1.5 = 150원)
            int reward = perfectRecipe.CalculateReward(config);

            // NPC 보상 데이터 저장
            NPCRewardData npcData = npcRewards[currentNPCIndex];
            npcData.reward = reward;
            npcData.quality = 100f;
            npcData.orderMatched = true;
            npcData.isServed = true;

            // 총 재화 증가
            totalReward += reward;

            Debug.Log($"[TestButton] NPC {currentNPCIndex + 1} 완벽 서빙! 보상: {reward}원 (총: {totalReward}원)");

            // 이벤트 발생
            OnNPCRewarded?.Invoke(currentNPCIndex + 1, reward);
            OnTotalRewardChanged?.Invoke(totalReward);

            // 다음 NPC로
            currentNPCIndex++;

            // 레시피 완료 이벤트 발생 (다음 NPC 스폰 트리거)
            GameEvents.TriggerRecipeCompleted(perfectRecipe);

            // 모든 NPC 완료 확인
            if (currentNPCIndex >= maxNPCCount)
            {
                OnAllNPCsServed();
            }
        }

        /// <summary>
        /// 테스트용: 현재 NPC의 주문에 완벽히 맞는 음식 제공 (최고 재화 획득)
        /// UI 버튼에서 호출 가능
        /// </summary>
        public void TestServePerfectFoodForCurrentOrder()
        {
            if (currentNPCIndex >= maxNPCCount)
            {
                Debug.Log("[TestButton] 모든 NPC 완료!");
                return;
            }

            // 현재 NPC 가져오기
            NPC.NPCSpawnManager spawnManager = FindFirstObjectByType<NPC.NPCSpawnManager>();
            if (spawnManager == null)
            {
                Debug.LogError("[TestButton] NPCSpawnManager를 찾을 수 없습니다!");
                return;
            }

            GameObject currentNPC = spawnManager.GetCurrentNPC();
            if (currentNPC == null)
            {
                Debug.LogError("[TestButton] 현재 NPC가 없습니다!");
                return;
            }

            // NPC의 주문 가져오기
            NPC.NPCOrderController orderController = currentNPC.GetComponent<NPC.NPCOrderController>();
            if (orderController == null)
            {
                Debug.LogError("[TestButton] NPCOrderController를 찾을 수 없습니다!");
                return;
            }

            CustomerOrder currentOrder = orderController.GetCustomerOrder();
            if (currentOrder == null)
            {
                Debug.LogError("[TestButton] 현재 주문이 없습니다!");
                return;
            }

            // 주문에 맞는 완벽한 레시피 생성
            HotdogRecipe perfectRecipe = CreatePerfectRecipe(currentOrder);

            // RecipeConfig 찾기
            RecipeConfigSO config = FindRecipeConfig();
            if (config == null)
            {
                Debug.LogError("[TestButton] RecipeConfigSO를 찾을 수 없습니다!");
                return;
            }

            // 최고 보상 계산
            int reward = perfectRecipe.CalculateReward(config);

            // NPC 보상 데이터 저장
            NPCRewardData npcData = npcRewards[currentNPCIndex];
            npcData.reward = reward;
            npcData.quality = 100f;
            npcData.orderMatched = true;
            npcData.isServed = true;

            // 총 재화 증가
            totalReward += reward;

            Debug.Log($"[TestButton] NPC {currentNPCIndex + 1} 완벽 서빙! 주문: {currentOrder.GetDescription()}, 보상: {reward}원 (총: {totalReward}원)");

            // 이벤트 발생
            OnNPCRewarded?.Invoke(currentNPCIndex + 1, reward);
            OnTotalRewardChanged?.Invoke(totalReward);

            // NPC 퇴장 처리 (currentNPCIndex 증가 전에 실행)
            if (currentNPC != null)
            {
                NPC.NPCMovement movement = currentNPC.GetComponent<NPC.NPCMovement>();
                if (movement != null)
                {
                    movement.OnOrderComplete(); // NPC 퇴장 시작 (퇴장 완료 시 자동으로 다음 NPC 스폰됨)
                    Debug.Log("[TestButton] NPC 퇴장 명령 전송");
                }
                else
                {
                    Debug.LogWarning("[TestButton] NPCMovement를 찾을 수 없습니다!");
                }
            }

            // 다음 NPC로 (인덱스만 증가, 스폰은 NPC 퇴장 완료 후 자동으로 처리됨)
            currentNPCIndex++;

            // 모든 NPC 완료 확인
            if (currentNPCIndex >= maxNPCCount)
            {
                OnAllNPCsServed();
            }

            // 주의: TestButton은 정상 플레이를 우회하므로 GameEvents.TriggerRecipeCompleted를 호출하지 않음
            // 정상 플레이에서는 CookingManager가 이벤트를 발생시켜 ScoreManager.OnRecipeCompleted가 호출됨
            // TestButton에서는 이미 보상을 직접 처리했으므로 이벤트 발생 시 중복 처리가 발생함
        }

        /// <summary>
        /// 주문에 완벽히 맞는 레시피 생성
        /// </summary>
        private HotdogRecipe CreatePerfectRecipe(CustomerOrder order)
        {
            HotdogRecipe recipe = new HotdogRecipe
            {
                hasStick = true,
                fillingType = order.filling,
                batterAmount = 100f, // 완벽한 반죽 양
                fryingTime = 8f, // Golden 구간 (7-9초)
                fryingColor = Cooking.FryingColor.Golden,
                hasSugar = order.requiresSugar,
                quality = 100f,
                matchesOrder = true
            };

            // 소스 추가 (주문에 맞게)
            recipe.sauces = new System.Collections.Generic.List<Cooking.SauceType>();
            recipe.sauceAmounts = new System.Collections.Generic.Dictionary<Cooking.SauceType, float>();

            foreach (var sauceReq in order.sauces)
            {
                recipe.sauces.Add(sauceReq.type);

                // 소스 양을 정확히 맞춤
                float amount = GetPerfectSauceAmount(sauceReq.amount);
                recipe.sauceAmounts[sauceReq.type] = amount;
            }

            return recipe;
        }

        /// <summary>
        /// SauceAmount에 맞는 완벽한 소스 양 반환
        /// </summary>
        private float GetPerfectSauceAmount(Cooking.SauceAmount amount)
        {
            switch (amount)
            {
                case Cooking.SauceAmount.Low:
                    return 20f; // Low: 0-33%, 중간값 사용
                case Cooking.SauceAmount.Medium:
                    return 50f; // Medium: 34-66%, 중간값 사용
                case Cooking.SauceAmount.High:
                    return 85f; // High: 67-100%, 중간값 사용
                default:
                    return 50f;
            }
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Add Random Reward")]
        private void TestAddReward()
        {
            if (currentNPCIndex >= maxNPCCount)
            {
                Debug.Log("모든 NPC 완료!");
                return;
            }

            // 테스트용 랜덤 보상
            int testReward = Random.Range(50, 150);

            NPCRewardData npcData = npcRewards[currentNPCIndex];
            npcData.reward = testReward;
            npcData.quality = Random.Range(60f, 100f);
            npcData.orderMatched = Random.value > 0.5f;
            npcData.isServed = true;

            totalReward += testReward;
            currentNPCIndex++;

            Debug.Log($"[Test] NPC {currentNPCIndex} 보상: {testReward}원, 총합: {totalReward}원");

            if (currentNPCIndex >= maxNPCCount)
            {
                OnAllNPCsServed();
            }
        }

        [ContextMenu("Test: Restart Stage")]
        private void TestRestartStage()
        {
            RestartStage();
        }

        [ContextMenu("Log Current State")]
        private void LogCurrentState()
        {
            Debug.Log("=== ScoreManager State ===");
            Debug.Log($"Total Reward: {totalReward} / {targetTotalReward}");
            Debug.Log($"Progress: {GetProgress() * 100f:F1}%");
            Debug.Log($"Current NPC: {currentNPCIndex + 1} / {maxNPCCount}");
            Debug.Log($"Dialogue Unlocked: {isDialogueUnlocked}");

            Debug.Log("NPC Rewards:");
            foreach (var data in npcRewards)
            {
                if (data.isServed)
                {
                    Debug.Log($"  NPC {data.npcIndex}: {data.reward}원 (품질: {data.quality:F1}, 일치: {data.orderMatched})");
                }
            }
        }
#endif
    }

    // ==========================================
    // Data Classes
    // ==========================================

    /// <summary>
    /// NPC별 보상 데이터
    /// </summary>
    [System.Serializable]
    public class NPCRewardData
    {
        public int npcIndex;        // NPC 번호 (1-5)
        public int reward;          // 획득한 재화
        public float quality;       // 품질 점수
        public bool orderMatched;   // 주문 일치 여부
        public bool isServed;       // 제공 완료 여부
    }
}
