using UnityEngine;
using RecipeAboutLife.Managers;
using RecipeAboutLife.NPC;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 스테이지 스토리 대화 컨트롤러
    ///
    /// 역할:
    /// 1. 모든 NPC 주문 완료 후 재화 조건 확인
    /// 2. 조건 달성 시 마지막 NPC(스토리 NPC)의 StoryAfterSummary 대화 트리거
    /// 3. 결산 UI가 추가되면 해당 위치에서 호출 (현재는 UI 없이 동작)
    ///
    /// ※ 기존 시스템 수정 없이 이벤트 구독으로만 동작
    /// </summary>
    public class StageStoryController : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================

        private static StageStoryController _instance;
        public static StageStoryController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<StageStoryController>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration
        // ==========================================

        [Header("스토리 NPC 설정")]
        [SerializeField]
        [Tooltip("스토리 NPC 정보 (ScriptableObject)")]
        private StoryNPCConfig storyNPCConfig;

        [Header("현재 스테이지")]
        [SerializeField]
        [Tooltip("현재 스테이지 번호 (1, 2, 3...)")]
        private int currentStageID = 1;

        [Header("대화 설정")]
        [SerializeField]
        [Tooltip("StoryAfterSummary 대화 시작 전 대기 시간 (초)")]
        private float delayBeforeStoryDialogue = 1f;

        // ==========================================
        // State
        // ==========================================

        [Header("현재 상태 (Debug)")]
        [SerializeField]
        private bool isStoryDialogueTriggered = false;

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// StoryAfterSummary 대화가 시작되었을 때
        /// </summary>
        public System.Action OnStoryDialogueStarted;

        /// <summary>
        /// StoryAfterSummary 대화가 종료되었을 때
        /// </summary>
        public System.Action OnStoryDialogueEnded;

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
        }

        private void OnEnable()
        {
            // ScoreManager의 스테이지 완료 이벤트 구독
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnStageCompleted += OnStageCompleted;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnStageCompleted -= OnStageCompleted;
            }
        }

        // ==========================================
        // Stage Completion Handler
        // ==========================================

        /// <summary>
        /// 스테이지 완료 이벤트 처리
        /// ScoreManager에서 모든 NPC 완료 시 호출됨
        /// </summary>
        /// <param name="success">재화 목표 달성 여부</param>
        private void OnStageCompleted(bool success)
        {
            Debug.Log($"[StageStoryController] 스테이지 완료! 성공: {success}, Stage ID: {currentStageID}");

            // TODO: 결산 UI 연결 예정 위치
            // ============================================
            // 나중에 결산 UI를 추가할 때:
            // 1. 여기서 결산 UI를 표시
            // 2. 결산 UI에 "확인" 버튼 추가
            // 3. 확인 버튼 클릭 시 → CheckAndTriggerStoryDialogue(success) 호출
            //
            // 현재는 결산 UI 없이 즉시 스토리 대화 진행
            // ============================================

            // 재화 조건 확인 및 스토리 대화 트리거
            CheckAndTriggerStoryDialogue(success);
        }

        /// <summary>
        /// 재화 조건 확인 및 스토리 대화 트리거
        /// </summary>
        /// <param name="success">재화 목표 달성 여부</param>
        private void CheckAndTriggerStoryDialogue(bool success)
        {
            // 이미 트리거됨
            if (isStoryDialogueTriggered)
            {
                Debug.LogWarning("[StageStoryController] StoryAfterSummary 대화가 이미 트리거되었습니다!");
                return;
            }

            // StoryNPCConfig 확인
            if (storyNPCConfig == null)
            {
                Debug.LogError("[StageStoryController] StoryNPCConfig가 설정되지 않았습니다!");
                return;
            }

            // 현재 스테이지에 스토리 NPC가 있는지 확인
            if (!storyNPCConfig.HasStoryNPCForStage(currentStageID))
            {
                Debug.LogWarning($"[StageStoryController] Stage {currentStageID}에 스토리 NPC가 설정되지 않았습니다!");
                return;
            }

            // 재화 조건 달성 확인
            if (!success)
            {
                Debug.Log("[StageStoryController] 재화 목표 미달성. StoryAfterSummary 대화를 실행하지 않습니다.");
                return;
            }

            // 현재 NPC 가져오기
            NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
            if (spawnManager == null)
            {
                Debug.LogError("[StageStoryController] NPCSpawnManager를 찾을 수 없습니다!");
                return;
            }

            GameObject currentNPC = spawnManager.GetCurrentNPC();
            if (currentNPC == null)
            {
                Debug.LogWarning("[StageStoryController] 현재 NPC가 없습니다!");
                return;
            }

            // 현재 NPC가 스토리 NPC인지 확인
            bool isStoryNPC = storyNPCConfig.IsStoryNPCByName(currentNPC.name);
            if (!isStoryNPC)
            {
                Debug.LogWarning($"[StageStoryController] 현재 NPC({currentNPC.name})는 스토리 NPC가 아닙니다!");
                return;
            }

            Debug.Log($"[StageStoryController] ✅ 조건 만족! StoryAfterSummary 대화 트리거");
            Debug.Log($"  - Stage: {currentStageID}");
            Debug.Log($"  - NPC: {currentNPC.name}");
            Debug.Log($"  - 재화 달성: {success}");

            // 스토리 대화 트리거 (딜레이 후)
            isStoryDialogueTriggered = true;
            Invoke(nameof(TriggerStoryAfterSummary), delayBeforeStoryDialogue);
        }

        /// <summary>
        /// StoryAfterSummary 대화 시작
        /// </summary>
        private void TriggerStoryAfterSummary()
        {
            // 현재 NPC 가져오기
            NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
            if (spawnManager == null)
            {
                Debug.LogError("[StageStoryController] NPCSpawnManager를 찾을 수 없습니다!");
                return;
            }

            GameObject currentNPC = spawnManager.GetCurrentNPC();
            if (currentNPC == null)
            {
                Debug.LogWarning("[StageStoryController] 현재 NPC가 없습니다!");
                return;
            }

            // NPCDialogueController 가져오기
            NPCDialogueController dialogueController = currentNPC.GetComponent<NPCDialogueController>();
            if (dialogueController == null)
            {
                Debug.LogError($"[StageStoryController] {currentNPC.name}에 NPCDialogueController가 없습니다!");
                return;
            }

            // StoryAfterSummary 대화 시작
            bool hasDialogue = dialogueController.StartDialogue(DialogueType.StoryAfterSummary);

            if (hasDialogue)
            {
                Debug.Log($"[StageStoryController] StoryAfterSummary 대화 시작! NPC: {currentNPC.name}");

                // 대화 종료 이벤트 구독
                dialogueController.OnDialogueEnded += OnStoryAfterSummaryEnded;

                // 이벤트 발생
                OnStoryDialogueStarted?.Invoke();
            }
            else
            {
                Debug.LogWarning($"[StageStoryController] {currentNPC.name}에 StoryAfterSummary 대화가 없습니다!");
                isStoryDialogueTriggered = false;
            }
        }

        /// <summary>
        /// StoryAfterSummary 대화 종료 시 호출
        /// </summary>
        /// <param name="type">대화 타입</param>
        private void OnStoryAfterSummaryEnded(DialogueType type)
        {
            // StoryAfterSummary 대화가 끝났을 때만 처리
            if (type != DialogueType.StoryAfterSummary)
                return;

            Debug.Log("[StageStoryController] StoryAfterSummary 대화 종료!");

            // 현재 NPC의 DialogueController에서 이벤트 구독 해제
            NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
            if (spawnManager != null)
            {
                GameObject currentNPC = spawnManager.GetCurrentNPC();
                if (currentNPC != null)
                {
                    NPCDialogueController dialogueController = currentNPC.GetComponent<NPCDialogueController>();
                    if (dialogueController != null)
                    {
                        dialogueController.OnDialogueEnded -= OnStoryAfterSummaryEnded;
                    }
                }
            }

            // 이벤트 발생
            OnStoryDialogueEnded?.Invoke();

            // TODO: 스테이지 종료 처리
            // ============================================
            // 나중에 추가할 기능:
            // - 스테이지 클리어 UI 표시
            // - 다음 스테이지로 이동
            // - 보상 지급
            // - 저장 기능
            // ============================================

            Debug.Log("[StageStoryController] 스테이지 스토리 완료!");
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 현재 스테이지 ID 설정
        /// </summary>
        /// <param name="stageID">스테이지 번호</param>
        public void SetCurrentStageID(int stageID)
        {
            currentStageID = stageID;
            Debug.Log($"[StageStoryController] 현재 스테이지: {currentStageID}");
        }

        /// <summary>
        /// 현재 스테이지 ID 가져오기
        /// </summary>
        /// <returns>스테이지 번호</returns>
        public int GetCurrentStageID()
        {
            return currentStageID;
        }

        /// <summary>
        /// 스토리 대화 트리거 여부 확인
        /// </summary>
        /// <returns>트리거되었으면 true</returns>
        public bool IsStoryDialogueTriggered()
        {
            return isStoryDialogueTriggered;
        }

        /// <summary>
        /// 스테이지 초기화 (재시작 시 호출)
        /// </summary>
        public void ResetStage()
        {
            CancelInvoke(nameof(TriggerStoryAfterSummary));
            isStoryDialogueTriggered = false;

            Debug.Log("[StageStoryController] 스테이지 초기화");
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Trigger Story Dialogue")]
        private void TestTriggerStoryDialogue()
        {
            CheckAndTriggerStoryDialogue(true);
        }

        [ContextMenu("Log Current State")]
        private void LogCurrentState()
        {
            Debug.Log("=== StageStoryController State ===");
            Debug.Log($"Current Stage ID: {currentStageID}");
            Debug.Log($"Story Dialogue Triggered: {isStoryDialogueTriggered}");

            if (storyNPCConfig != null)
            {
                bool hasStoryNPC = storyNPCConfig.HasStoryNPCForStage(currentStageID);
                Debug.Log($"Has Story NPC for Stage {currentStageID}: {hasStoryNPC}");
            }
            else
            {
                Debug.LogWarning("StoryNPCConfig is not assigned!");
            }

            if (ScoreManager.Instance != null)
            {
                int currentReward = ScoreManager.Instance.GetTotalReward();
                int targetReward = ScoreManager.Instance.GetTargetReward();
                bool isUnlocked = ScoreManager.Instance.IsDialogueUnlocked();
                Debug.Log($"Reward: {currentReward} / {targetReward}");
                Debug.Log($"Dialogue Unlocked: {isUnlocked}");
            }
        }
#endif
    }
}
