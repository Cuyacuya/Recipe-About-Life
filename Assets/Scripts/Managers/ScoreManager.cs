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
