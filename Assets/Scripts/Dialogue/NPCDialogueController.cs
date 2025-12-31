using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// NPC 대화 실행 컨트롤러
    /// 개별 NPC에게 부착되어 해당 NPC의 대화를 관리
    /// 기존 DialogueManager와는 별개로 동작 (NPC 전용 대화)
    /// </summary>
    public class NPCDialogueController : MonoBehaviour
    {
        // ==========================================
        // Configuration
        // ==========================================

        [Header("대화 데이터")]
        [SerializeField]
        [Tooltip("이 NPC가 사용할 대화 세트 (ScriptableObject)")]
        private NPCDialogueSet dialogueSet;

        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("대화를 표시할 말풍선 UI")]
        private UI.DialogueBubbleUI dialogueBubble;

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
        private bool waitingForInput = false;
        private bool inputReceived = false;
        private DialogueType currentDialogueType; // 현재 진행 중인 대화 타입

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// 대화 시작 시
        /// </summary>
        public System.Action<DialogueType> OnDialogueStarted;

        /// <summary>
        /// 대화 종료 시
        /// </summary>
        public System.Action<DialogueType> OnDialogueEnded;

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // DialogueBubbleUI 자동 찾기 (비활성화된 것도 찾기)
            if (dialogueBubble == null)
            {
                dialogueBubble = GetComponentInChildren<UI.DialogueBubbleUI>(true);
                if (dialogueBubble == null)
                {
                    Debug.LogWarning($"[NPCDialogueController] {gameObject.name}에 DialogueBubbleUI가 없습니다!");
                }
            }
        }

        private void Update()
        {
            // 대화 진행 중이고 입력 대기 중일 때만 체크
            if (!waitingForInput || inputReceived)
                return;

            // 모바일 터치 입력 감지
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                inputReceived = true;
            }
            // PC 마우스 클릭 입력 감지 (테스트용)
            else if (Input.GetMouseButtonDown(0))
            {
                inputReceived = true;
            }
        }

        // ==========================================
        // Public API - 대화 시작
        // ==========================================

        /// <summary>
        /// 특정 타입의 대화 시작
        /// </summary>
        /// <param name="type">대화 타입 (Intro, Order, ServedSuccess 등)</param>
        /// <returns>대화 시작 성공 여부</returns>
        public bool StartDialogue(DialogueType type)
        {
            // 대화 세트 검증
            if (dialogueSet == null)
            {
                Debug.LogError($"[NPCDialogueController] {gameObject.name}에 DialogueSet이 설정되지 않았습니다!");
                return false;
            }

            // 대화 라인 가져오기
            DialogueLine[] lines = dialogueSet.GetDialogueLines(type);
            if (lines == null || lines.Length == 0)
            {
                Debug.LogWarning($"[NPCDialogueController] {dialogueSet.npcID}에 {type} 대화가 없습니다!");
                return false;
            }

            // 이미 대화 중이면 중단
            if (isDialogueActive)
            {
                Debug.LogWarning($"[NPCDialogueController] {gameObject.name}이 이미 대화 중입니다. 기존 대화를 중단합니다.");
                StopDialogue();
            }

            // 대화 시작
            currentDialogueCoroutine = StartCoroutine(PlayDialogueSequence(type, lines));
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
            waitingForInput = false;
            inputReceived = false;

            // 모든 UI 숨기기 (어떤 대화가 진행 중이었는지 모르므로)
            if (UI.NPCDialogueUI.Instance != null)
            {
                UI.NPCDialogueUI.Instance.Hide();
            }
            if (UI.PlayerDialogueUI.Instance != null)
            {
                UI.PlayerDialogueUI.Instance.Hide();
            }
            if (dialogueBubble != null)
            {
                dialogueBubble.Hide();
            }

            Debug.Log($"[NPCDialogueController] {gameObject.name} 대화 중단");
        }

        // ==========================================
        // Dialogue Playback
        // ==========================================

        /// <summary>
        /// 대화 시퀀스 재생
        /// </summary>
        private IEnumerator PlayDialogueSequence(DialogueType type, DialogueLine[] lines)
        {
            isDialogueActive = true;
            currentDialogueType = type; // 현재 대화 타입 저장

            Debug.Log($"[NPCDialogueController] {dialogueSet.npcID} {type} 대화 시작 ({lines.Length} 라인)");

            // 이벤트 발생
            OnDialogueStarted?.Invoke(type);

            // Order 타입일 때 주문 UI 표시하지 않음
            // 주문 내용은 대화문에 이미 포함되어 있으므로 별도 UI 불필요
            // 주문 정보는 NPCOrderController에서 내부적으로 요리 시스템에 전달됨

            // 각 대화 라인 재생
            for (int i = 0; i < lines.Length; i++)
            {
                DialogueLine line = lines[i];

                // 빈 라인은 건너뛰기
                if (line == null || line.IsEmpty())
                {
                    Debug.LogWarning($"[NPCDialogueController] 빈 대화 라인을 건너뜁니다. (Index: {i})");
                    continue;
                }

                // 대화 표시
                DisplayDialogueLine(line);

                // 터치 입력 대기 (최대 표시 시간까지)
                float displayTime = line.displayDuration > 0 ? line.displayDuration : lineDisplayTime;
                yield return WaitForInputOrTime(displayTime);

                // 마지막 라인이 아니면 짧은 간격
                if (i < lines.Length - 1)
                {
                    yield return new WaitForSeconds(linePauseDuration);
                }
            }

            // 대화 종료
            isDialogueActive = false;

            // StoryAfterSummary였다면 Overlay Canvas 숨기기
            if (currentDialogueType == DialogueType.StoryAfterSummary)
            {
                if (UI.NPCDialogueUI.Instance != null)
                {
                    UI.NPCDialogueUI.Instance.Hide();
                }
                if (UI.PlayerDialogueUI.Instance != null)
                {
                    UI.PlayerDialogueUI.Instance.Hide();
                }
            }
            else
            {
                // 일반 대화는 World Space 말풍선만 숨기기
                if (dialogueBubble != null)
                {
                    dialogueBubble.Hide();
                }
            }

            Debug.Log($"[NPCDialogueController] {dialogueSet.npcID} {type} 대화 종료");

            // 이벤트 발생
            OnDialogueEnded?.Invoke(type);

            currentDialogueCoroutine = null;
        }

        /// <summary>
        /// 개별 대화 라인 표시
        /// </summary>
        private void DisplayDialogueLine(DialogueLine line)
        {
            // StoryAfterSummary 대화일 때만 Overlay Canvas 사용
            if (currentDialogueType == DialogueType.StoryAfterSummary)
            {
                // Speaker에 따라 다른 UI 사용 (Overlay Canvas)
                switch (line.speaker)
                {
                    case SpeakerType.NPC:
                        DisplayNPCDialogueOverlay(line.text);
                        break;

                    case SpeakerType.Player:
                        DisplayPlayerDialogue(line.text);
                        break;

                    case SpeakerType.System:
                        DisplayNPCDialogueOverlay(line.text);
                        break;
                }
            }
            else
            {
                // 일반 대화는 기존 World Space 말풍선 사용
                DisplayNPCDialogueWorldSpace(line.text);
            }

            Debug.Log($"[NPCDialogueController] [{line.speaker}] {line.text}");
        }

        /// <summary>
        /// NPC 대화 표시 - World Space (기존 방식)
        /// 일반 대화 (Intro, Order, Exit 등)에 사용
        /// </summary>
        private void DisplayNPCDialogueWorldSpace(string text)
        {
            if (dialogueBubble == null)
            {
                Debug.LogError($"[NPCDialogueController] DialogueBubbleUI가 없어서 대화를 표시할 수 없습니다!");
                return;
            }

            // Canvas가 꺼져있으면 켜기
            if (!dialogueBubble.gameObject.activeSelf)
            {
                dialogueBubble.gameObject.SetActive(true);
            }

            // 말풍선에 텍스트 표시
            dialogueBubble.ShowDialogue(text);
        }

        /// <summary>
        /// NPC 대화 표시 - Overlay Canvas
        /// StoryAfterSummary 대화에만 사용
        /// </summary>
        private void DisplayNPCDialogueOverlay(string text)
        {
            // Player 대화 UI 숨기기
            if (UI.PlayerDialogueUI.Instance != null)
            {
                UI.PlayerDialogueUI.Instance.Hide();
            }

            // NPC 대화 UI 표시 (Overlay)
            if (UI.NPCDialogueUI.Instance != null)
            {
                // NPC 이름 가져오기 (dialogueSet에서)
                string npcName = dialogueSet != null ? dialogueSet.npcDisplayName : null;

                // NPC 스프라이트 가져오기 (SpriteRenderer에서)
                Sprite npcSprite = null;
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    npcSprite = spriteRenderer.sprite;
                }

                UI.NPCDialogueUI.Instance.Show(text, npcName, npcSprite);
            }
            else
            {
                Debug.LogWarning("[NPCDialogueController] NPCDialogueUI를 찾을 수 없습니다! 폴백으로 World Space 말풍선 사용.");
                // 폴백: World Space 말풍선 사용
                DisplayNPCDialogueWorldSpace(text);
            }
        }

        /// <summary>
        /// Player 대화 표시 (화면 하단)
        /// StoryAfterSummary 대화에만 사용
        /// </summary>
        private void DisplayPlayerDialogue(string text)
        {
            // NPC 대화 UI 숨기기 (Overlay)
            if (UI.NPCDialogueUI.Instance != null)
            {
                UI.NPCDialogueUI.Instance.Hide();
            }

            // Player 대화 UI 표시
            if (UI.PlayerDialogueUI.Instance != null)
            {
                UI.PlayerDialogueUI.Instance.Show(text);
            }
            else
            {
                Debug.LogWarning("[NPCDialogueController] PlayerDialogueUI를 찾을 수 없습니다! 씬에 PlayerDialogueUI를 추가하세요.");
            }
        }

        /// <summary>
        /// 터치 입력 또는 시간 대기
        /// 터치하면 즉시 진행, 안하면 maxTime까지 대기
        /// </summary>
        private IEnumerator WaitForInputOrTime(float maxTime)
        {
            waitingForInput = true;
            inputReceived = false;

            float elapsedTime = 0f;

            while (elapsedTime < maxTime && !inputReceived)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            waitingForInput = false;
            inputReceived = false;
        }

        // ==========================================
        // Public API - 상태 확인
        // ==========================================

        /// <summary>
        /// 대화 진행 중인지 확인
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        /// <summary>
        /// 대화 세트 설정
        /// </summary>
        public void SetDialogueSet(NPCDialogueSet newDialogueSet)
        {
            dialogueSet = newDialogueSet;
        }

        /// <summary>
        /// 현재 대화 세트 가져오기
        /// </summary>
        public NPCDialogueSet GetDialogueSet()
        {
            return dialogueSet;
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Play Intro")]
        private void TestPlayIntro()
        {
            StartDialogue(DialogueType.Intro);
        }

        [ContextMenu("Test: Play Order")]
        private void TestPlayOrder()
        {
            StartDialogue(DialogueType.Order);
        }

        [ContextMenu("Test: Play Served Success")]
        private void TestPlayServedSuccess()
        {
            StartDialogue(DialogueType.ServedSuccess);
        }

        [ContextMenu("Test: Play Served Fail")]
        private void TestPlayServedFail()
        {
            StartDialogue(DialogueType.ServedFail);
        }

        [ContextMenu("Test: Play Exit")]
        private void TestPlayExit()
        {
            StartDialogue(DialogueType.Exit);
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
     * === 대화 시작 트리거 예시 ===
     *
     * 1. NPC 등장 직후 (Intro)
     *    - NPCMovement.OnArrived() 또는 NPCMovement.Start()에서 호출
     *    - 예시:
     *      NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
     *      if (dialogueController != null)
     *      {
     *          dialogueController.StartDialogue(DialogueType.Intro);
     *      }
     *
     * 2. 주문 시작 시 (Order)
     *    - NPCOrderController.RequestOrder() 이후 호출
     *    - 예시:
     *      NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
     *      if (dialogueController != null)
     *      {
     *          dialogueController.StartDialogue(DialogueType.Order);
     *      }
     *
     * 3. 음식 서빙 완료 후 (ServedSuccess / ServedFail)
     *    - ScoreManager.OnRecipeCompleted()에서 주문 일치 여부 확인 후 호출
     *    - 예시:
     *      NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
     *      GameObject currentNPC = spawnManager.GetCurrentNPC();
     *      NPCDialogueController dialogueController = currentNPC.GetComponent<NPCDialogueController>();
     *
     *      if (dialogueController != null)
     *      {
     *          DialogueType type = recipe.matchesOrder ? DialogueType.ServedSuccess : DialogueType.ServedFail;
     *          dialogueController.StartDialogue(type);
     *      }
     *
     * 4. NPC 퇴장 시 (Exit)
     *    - NPCMovement.OnOrderComplete() 호출 전 실행
     *    - 예시:
     *      NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
     *      if (dialogueController != null)
     *      {
     *          dialogueController.StartDialogue(DialogueType.Exit);
     *
     *          // 대화 종료 후 퇴장 시작 (이벤트 사용)
     *          dialogueController.OnDialogueEnded += (type) =>
     *          {
     *              if (type == DialogueType.Exit)
     *              {
     *                  // 퇴장 시작
     *                  StartExit();
     *              }
     *          };
     *      }
     *
     * 5. 스테이지 종료 후 (마지막 손님 대화)
     *    - ScoreManager.OnStageCompleted(success) 이벤트에서 호출
     *    - 새로운 StageDialogueController 필요 (별도 구현)
     *    - 예시:
     *      StageDialogueController stageDialogueController = FindFirstObjectByType<StageDialogueController>();
     *      if (stageDialogueController != null)
     *      {
     *          stageDialogueController.StartStageFinalDialogue(currentStageID, success);
     *      }
     *
     * === NPC 프리팹 구성 예시 ===
     *
     * NPC GameObject
     * ├─ NPCMovement (기존)
     * ├─ NPCOrderController (기존)
     * ├─ NPCDialogueController (신규 추가)
     * │  └─ DialogueSet 참조 (Inspector에서 설정)
     * └─ DialogueBubbleUI (기존, 주문 + 대화 겸용 가능)
     *
     * === 주의사항 ===
     * - 기존 주문 시스템(OrderManager, NPCOrderController)은 수정하지 않음
     * - 대화는 게임플레이에 영향을 주지 않는 "연출"로만 사용
     * - 대화 중에도 게임 진행이 멈추지 않도록 설계됨
     * - 필요시 대화 중 입력 차단 등은 별도 구현 필요
     */
}
