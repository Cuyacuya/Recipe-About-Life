using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 스테이지 종료 대화 컨트롤러
    /// ScoreManager의 OnStageCompleted 이벤트를 받아 마지막 손님 전용 대화 출력
    /// 기존 DialogueManager와는 별개로 동작 (스테이지 종료 전용)
    /// </summary>
    public class StageDialogueController : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================

        private static StageDialogueController _instance;
        public static StageDialogueController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<StageDialogueController>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration
        // ==========================================

        [Header("스테이지 대화 데이터")]
        [SerializeField]
        [Tooltip("스테이지별 대화 데이터 목록")]
        private List<StageDialogueData> stageDialogues = new List<StageDialogueData>();

        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("대화 패널 (Screen Space Canvas)")]
        private GameObject dialoguePanel;

        [SerializeField]
        [Tooltip("대화 텍스트 (TextMeshProUGUI)")]
        private TextMeshProUGUI dialogueText;

        [SerializeField]
        [Tooltip("발화자 이름 표시 텍스트 (선택 사항)")]
        private TextMeshProUGUI speakerNameText;

        [Header("표시 설정")]
        [SerializeField]
        [Tooltip("각 대화 라인 표시 시간 (초)")]
        private float lineDisplayTime = 3f;

        [SerializeField]
        [Tooltip("대화 라인 간 간격 (초)")]
        private float linePauseDuration = 0.5f;

        // ==========================================
        // State
        // ==========================================

        private bool isDialogueActive = false;
        private Coroutine currentDialogueCoroutine = null;

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// 스테이지 대화 시작 시
        /// </summary>
        public System.Action<int, bool> OnStageDialogueStarted; // (stageID, isSuccess)

        /// <summary>
        /// 스테이지 대화 종료 시
        /// </summary>
        public System.Action<int, bool> OnStageDialogueEnded; // (stageID, isSuccess)

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
            HideDialoguePanel();
        }

        private void OnEnable()
        {
            // ScoreManager의 스테이지 완료 이벤트 구독
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.OnStageCompleted += OnStageCompleted;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.OnStageCompleted -= OnStageCompleted;
            }
        }

        // ==========================================
        // Event Handlers
        // ==========================================

        /// <summary>
        /// 스테이지 완료 이벤트 처리
        /// </summary>
        private void OnStageCompleted(bool isSuccess)
        {
            // TODO: 실제로는 현재 스테이지 ID를 가져와야 함
            // 지금은 하드코딩된 Stage 1로 테스트
            int currentStageID = 1;

            Debug.Log($"[StageDialogueController] 스테이지 완료! (성공: {isSuccess})");

            // 스테이지 종료 대화 시작
            StartStageFinalDialogue(currentStageID, isSuccess);
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 스테이지 종료 대화 시작
        /// </summary>
        /// <param name="stageID">스테이지 번호</param>
        /// <param name="isSuccess">성공 여부</param>
        /// <returns>대화 시작 성공 여부</returns>
        public bool StartStageFinalDialogue(int stageID, bool isSuccess)
        {
            // 스테이지 대화 데이터 찾기
            StageDialogueData stageData = GetStageDialogueData(stageID);
            if (stageData == null)
            {
                Debug.LogWarning($"[StageDialogueController] 스테이지 {stageID} 대화 데이터를 찾을 수 없습니다!");
                return false;
            }

            // 성공/실패 대화 가져오기
            List<DialogueLine> lines = stageData.GetFinalDialogue(isSuccess);
            if (lines == null || lines.Count == 0)
            {
                Debug.LogWarning($"[StageDialogueController] 스테이지 {stageID} {(isSuccess ? "성공" : "실패")} 대화가 없습니다!");
                return false;
            }

            // 이미 대화 중이면 중단
            if (isDialogueActive)
            {
                Debug.LogWarning("[StageDialogueController] 이미 대화 중입니다. 기존 대화를 중단합니다.");
                StopDialogue();
            }

            // 대화 시작
            currentDialogueCoroutine = StartCoroutine(PlayStageDialogueSequence(stageID, isSuccess, lines));
            return true;
        }

        /// <summary>
        /// 대화 중단
        /// </summary>
        public void StopDialogue()
        {
            if (currentDialogueCoroutine != null)
            {
                StopCoroutine(currentDialogueCoroutine);
                currentDialogueCoroutine = null;
            }

            isDialogueActive = false;
            HideDialoguePanel();

            Debug.Log("[StageDialogueController] 대화 중단");
        }

        // ==========================================
        // Dialogue Playback
        // ==========================================

        /// <summary>
        /// 스테이지 대화 시퀀스 재생
        /// </summary>
        private IEnumerator PlayStageDialogueSequence(int stageID, bool isSuccess, List<DialogueLine> lines)
        {
            isDialogueActive = true;

            Debug.Log($"[StageDialogueController] 스테이지 {stageID} {(isSuccess ? "성공" : "실패")} 대화 시작 ({lines.Count} 라인)");

            // 이벤트 발생
            OnStageDialogueStarted?.Invoke(stageID, isSuccess);

            // 대화 패널 표시
            ShowDialoguePanel();

            // 각 대화 라인 재생
            for (int i = 0; i < lines.Count; i++)
            {
                DialogueLine line = lines[i];

                // 빈 라인은 건너뛰기
                if (line == null || line.IsEmpty())
                {
                    Debug.LogWarning($"[StageDialogueController] 빈 대화 라인을 건너뜁니다. (Index: {i})");
                    continue;
                }

                // 대화 표시
                DisplayDialogueLine(line);

                // 표시 시간만큼 대기
                float displayTime = line.displayDuration > 0 ? line.displayDuration : lineDisplayTime;
                yield return new WaitForSeconds(displayTime);

                // 마지막 라인이 아니면 짧은 간격
                if (i < lines.Count - 1)
                {
                    yield return new WaitForSeconds(linePauseDuration);
                }
            }

            // TODO: 다음 대화로 넘기는 버튼/터치 로직 구현
            // 현재는 자동 진행만 지원

            // 대화 종료
            isDialogueActive = false;
            HideDialoguePanel();

            Debug.Log($"[StageDialogueController] 스테이지 {stageID} 대화 종료");

            // 이벤트 발생
            OnStageDialogueEnded?.Invoke(stageID, isSuccess);

            currentDialogueCoroutine = null;
        }

        /// <summary>
        /// 개별 대화 라인 표시
        /// </summary>
        private void DisplayDialogueLine(DialogueLine line)
        {
            if (dialogueText == null)
            {
                Debug.LogError("[StageDialogueController] DialogueText가 없어서 대화를 표시할 수 없습니다!");
                return;
            }

            // 대화 텍스트 설정
            dialogueText.text = line.text;

            // 발화자 이름 표시 (선택 사항)
            if (speakerNameText != null)
            {
                speakerNameText.text = GetSpeakerName(line.speaker);
            }

            Debug.Log($"[StageDialogueController] [{line.speaker}] {line.text}");

            // TODO: 발화자에 따른 추가 연출
            // - SpeakerType.NPC: 마지막 손님 캐릭터 표시
            // - SpeakerType.Player: 플레이어 캐릭터 표시
            // - SpeakerType.System: 시스템 메시지 스타일
            // - 효과음, 애니메이션 등
        }

        /// <summary>
        /// 발화자 이름 가져오기
        /// </summary>
        private string GetSpeakerName(SpeakerType speaker)
        {
            switch (speaker)
            {
                case SpeakerType.NPC:
                    return "손님";
                case SpeakerType.Player:
                    return "플레이어";
                case SpeakerType.System:
                    return "";
                default:
                    return "";
            }
        }

        // ==========================================
        // UI Control
        // ==========================================

        /// <summary>
        /// 대화 패널 표시
        /// </summary>
        private void ShowDialoguePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }
        }

        /// <summary>
        /// 대화 패널 숨김
        /// </summary>
        private void HideDialoguePanel()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (dialogueText != null)
            {
                dialogueText.text = "";
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = "";
            }
        }

        // ==========================================
        // Helper Methods
        // ==========================================

        /// <summary>
        /// 스테이지 ID로 대화 데이터 찾기
        /// </summary>
        private StageDialogueData GetStageDialogueData(int stageID)
        {
            if (stageDialogues == null || stageDialogues.Count == 0)
            {
                Debug.LogWarning("[StageDialogueController] 스테이지 대화 데이터가 설정되지 않았습니다!");
                return null;
            }

            foreach (var data in stageDialogues)
            {
                if (data != null && data.stageID == stageID)
                {
                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// 대화 진행 중인지 확인
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Play Success Dialogue")]
        private void TestPlaySuccessDialogue()
        {
            StartStageFinalDialogue(1, true);
        }

        [ContextMenu("Test: Play Fail Dialogue")]
        private void TestPlayFailDialogue()
        {
            StartStageFinalDialogue(1, false);
        }

        [ContextMenu("Test: Stop Dialogue")]
        private void TestStopDialogue()
        {
            StopDialogue();
        }
#endif
    }

    // ==========================================
    // 기존 시스템과의 연결 예시 (주석)
    // ==========================================

    /*
     * === 스테이지 종료 대화 트리거 예시 ===
     *
     * 1. ScoreManager.OnAllNPCsServed()에서 호출
     *    - 기존 코드:
     *      private void OnAllNPCsServed()
     *      {
     *          bool success = totalReward >= targetTotalReward;
     *          OnStageCompleted?.Invoke(success);
     *      }
     *
     *    - StageDialogueController가 자동으로 OnStageCompleted 이벤트를 구독하여 처리
     *    - 추가 코드 필요 없음!
     *
     * 2. 수동으로 호출하는 경우 (테스트 또는 특수 상황)
     *    - 예시:
     *      StageDialogueController controller = StageDialogueController.Instance;
     *      if (controller != null)
     *      {
     *          controller.StartStageFinalDialogue(currentStageID, isSuccess);
     *      }
     *
     * === UI 설정 예시 ===
     *
     * Canvas (Screen Space)
     * └─ DialoguePanel (GameObject)
     *    ├─ Background (Image)
     *    ├─ SpeakerNameText (TextMeshProUGUI)
     *    └─ DialogueText (TextMeshProUGUI)
     *
     * StageDialogueController에 다음을 연결:
     * - dialoguePanel: DialoguePanel GameObject
     * - dialogueText: DialogueText (TextMeshProUGUI)
     * - speakerNameText: SpeakerNameText (TextMeshProUGUI, 선택 사항)
     *
     * === TODO: 향후 구현 ===
     * 1. 다음 대화로 넘기는 버튼 (현재는 자동 진행만 지원)
     * 2. 대화 중 게임 입력 차단
     * 3. 마지막 손님 캐릭터 표시
     * 4. 효과음, 배경음악 연출
     * 5. 대화 타이핑 효과 (한 글자씩 표시)
     * 6. 스킵 기능
     */
}
