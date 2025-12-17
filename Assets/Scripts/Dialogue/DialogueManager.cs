using UnityEngine;
using RecipeAboutLife.Managers;
using RecipeAboutLife.Events;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 대화 시스템 관리자
    /// 재화 목표 달성 시 플레이어와 대화 가능
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================

        private static DialogueManager _instance;
        public static DialogueManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<DialogueManager>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration
        // ==========================================

        [Header("대화 설정")]
        [SerializeField]
        [Tooltip("대화 UI 패널")]
        private GameObject dialoguePanel;

        [SerializeField]
        [Tooltip("잠금 상태 메시지 UI")]
        private GameObject lockedMessagePanel;

        [Header("플레이어 캐릭터")]
        [SerializeField]
        [Tooltip("대화할 플레이어 게임오브젝트")]
        private GameObject playerCharacter;

        // ==========================================
        // State
        // ==========================================

        [Header("현재 상태 (Debug)")]
        [SerializeField]
        private bool isDialogueUnlocked = false;

        [SerializeField]
        private bool isDialogueActive = false;

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// 대화가 시작되었을 때
        /// </summary>
        public System.Action OnDialogueStarted;

        /// <summary>
        /// 대화가 종료되었을 때
        /// </summary>
        public System.Action OnDialogueEnded;

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

            // UI 초기화
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (lockedMessagePanel != null)
                lockedMessagePanel.SetActive(false);
        }

        private void OnEnable()
        {
            // ScoreManager의 대화 잠금 해제 이벤트 구독
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnDialogueUnlocked += OnDialogueUnlocked;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnDialogueUnlocked -= OnDialogueUnlocked;
            }
        }

        // ==========================================
        // Dialogue Unlock
        // ==========================================

        /// <summary>
        /// 대화 잠금 해제 이벤트 처리
        /// </summary>
        private void OnDialogueUnlocked()
        {
            isDialogueUnlocked = true;

            Debug.Log("[DialogueManager] 대화가 잠금 해제되었습니다!");

            // 알림 표시 (예: 화면에 "대화 가능!" 메시지)
            ShowUnlockedNotification();
        }

        /// <summary>
        /// 잠금 해제 알림 표시
        /// </summary>
        private void ShowUnlockedNotification()
        {
            // TODO: 실제 알림 UI 구현
            Debug.Log("[DialogueManager] 알림: 플레이어와 대화할 수 있습니다!");

            // 게임 이벤트 발생
            GameEvents.TriggerShowNotification("목표 달성! 플레이어와 대화하세요.");
        }

        // ==========================================
        // Dialogue Control
        // ==========================================

        /// <summary>
        /// 대화 시작 시도
        /// </summary>
        /// <returns>대화 시작 성공 여부</returns>
        public bool TryStartDialogue()
        {
            if (!isDialogueUnlocked)
            {
                ShowLockedMessage();
                return false;
            }

            if (isDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] 이미 대화 중입니다!");
                return false;
            }

            StartDialogue();
            return true;
        }

        /// <summary>
        /// 대화 시작
        /// </summary>
        private void StartDialogue()
        {
            isDialogueActive = true;

            // UI 표시
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            Debug.Log("[DialogueManager] 대화 시작!");

            // 이벤트 발생
            OnDialogueStarted?.Invoke();

            // TODO: 실제 대화 스크립트 재생
            // 예: DialogueScript.Play();
        }

        /// <summary>
        /// 대화 종료
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive)
            {
                Debug.LogWarning("[DialogueManager] 대화가 활성화되지 않았습니다!");
                return;
            }

            isDialogueActive = false;

            // UI 숨김
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            Debug.Log("[DialogueManager] 대화 종료!");

            // 이벤트 발생
            OnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// 잠금 메시지 표시
        /// </summary>
        private void ShowLockedMessage()
        {
            Debug.Log("[DialogueManager] 대화가 잠겨있습니다! 목표 재화를 달성하세요.");

            // 잠금 메시지 UI 표시
            if (lockedMessagePanel != null)
            {
                lockedMessagePanel.SetActive(true);
                Invoke(nameof(HideLockedMessage), 2f); // 2초 후 자동 숨김
            }

            // ScoreManager에서 진행 상황 가져오기
            if (ScoreManager.Instance != null)
            {
                int currentReward = ScoreManager.Instance.GetTotalReward();
                int targetReward = ScoreManager.Instance.GetTargetReward();
                int remaining = targetReward - currentReward;

                string message = $"목표 미달성! 필요 재화: {remaining}원 더 필요합니다.";
                GameEvents.TriggerShowNotification(message);
            }
        }

        /// <summary>
        /// 잠금 메시지 숨김
        /// </summary>
        private void HideLockedMessage()
        {
            if (lockedMessagePanel != null)
            {
                lockedMessagePanel.SetActive(false);
            }
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 대화 잠금 해제 여부 확인
        /// </summary>
        public bool IsDialogueUnlocked()
        {
            return isDialogueUnlocked;
        }

        /// <summary>
        /// 대화 활성화 여부 확인
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        /// <summary>
        /// 강제로 대화 잠금 해제 (테스트용)
        /// </summary>
        public void ForceUnlock()
        {
            isDialogueUnlocked = true;
            Debug.Log("[DialogueManager] 강제 잠금 해제 (테스트)");
        }

        /// <summary>
        /// 대화 시스템 초기화
        /// </summary>
        public void ResetDialogueSystem()
        {
            isDialogueUnlocked = false;
            isDialogueActive = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (lockedMessagePanel != null)
                lockedMessagePanel.SetActive(false);

            Debug.Log("[DialogueManager] 대화 시스템 초기화");
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Try Start Dialogue")]
        private void TestTryStartDialogue()
        {
            bool success = TryStartDialogue();
            Debug.Log($"[Test] 대화 시작 시도: {(success ? "성공" : "실패")}");
        }

        [ContextMenu("Test: Force Unlock")]
        private void TestForceUnlock()
        {
            ForceUnlock();
        }

        [ContextMenu("Test: End Dialogue")]
        private void TestEndDialogue()
        {
            EndDialogue();
        }

        [ContextMenu("Log Current State")]
        private void LogCurrentState()
        {
            Debug.Log("=== DialogueManager State ===");
            Debug.Log($"Dialogue Unlocked: {isDialogueUnlocked}");
            Debug.Log($"Dialogue Active: {isDialogueActive}");

            if (ScoreManager.Instance != null)
            {
                int currentReward = ScoreManager.Instance.GetTotalReward();
                int targetReward = ScoreManager.Instance.GetTargetReward();
                Debug.Log($"Current Reward: {currentReward} / {targetReward}");
            }
        }
#endif
    }
}
