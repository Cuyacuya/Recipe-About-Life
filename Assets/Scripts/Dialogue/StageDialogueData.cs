using UnityEngine;
using System.Collections.Generic;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 스테이지 종료 대화 데이터 (ScriptableObject)
    /// 스테이지 클리어 후 "마지막 손님"과의 특별 대화
    /// </summary>
    [CreateAssetMenu(fileName = "Stage_Dialogue", menuName = "RecipeAboutLife/Dialogue/Stage Dialogue", order = 2)]
    public class StageDialogueData : ScriptableObject
    {
        // ==========================================
        // 스테이지 정보
        // ==========================================

        [Header("스테이지 정보")]
        [Tooltip("스테이지 번호 (1, 2, 3...)")]
        public int stageID = 1;

        [Tooltip("스테이지 이름 (에디터 참고용)")]
        public string stageName = "Stage 1";

        [TextArea(2, 3)]
        [Tooltip("스테이지 설명")]
        public string description = "";

        // ==========================================
        // 스테이지 종료 대화
        // ==========================================

        [Header("성공 대화")]
        [Tooltip("목표 달성 시 표시되는 대화 (마지막 손님과의 대화)")]
        public List<DialogueLine> finalSuccessDialogue = new List<DialogueLine>();

        [Header("실패 대화")]
        [Tooltip("목표 미달성 시 표시되는 대화")]
        public List<DialogueLine> finalFailDialogue = new List<DialogueLine>();

        // ==========================================
        // AfterStory (편지 + 독백) - Stage 3 전용
        // ==========================================

        [Header("AfterStory 연출 (Stage 3 전용)")]
        [Tooltip("AfterStory 연출 사용 여부")]
        public bool hasAfterStory = false;

        [Tooltip("편지 이미지 (검은 화면에서 표시)")]
        public Sprite afterStoryImage;

        [Tooltip("Player 독백 대화 (편지 이후)")]
        public List<DialogueLine> afterStoryDialogue = new List<DialogueLine>();

        // ==========================================
        // 초기화 (에디터에서 생성 시 자동 설정)
        // ==========================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 기본 대화가 없으면 예시 생성
            if (finalSuccessDialogue == null || finalSuccessDialogue.Count == 0)
            {
                InitializeDefaultSuccessDialogue();
            }

            if (finalFailDialogue == null || finalFailDialogue.Count == 0)
            {
                InitializeDefaultFailDialogue();
            }
        }

        /// <summary>
        /// 기본 성공 대화 초기화
        /// </summary>
        private void InitializeDefaultSuccessDialogue()
        {
            finalSuccessDialogue = new List<DialogueLine>
            {
                new DialogueLine(SpeakerType.NPC, "와, 정말 맛있었어요!"),
                new DialogueLine(SpeakerType.NPC, "목표 금액도 달성하셨네요!"),
                new DialogueLine(SpeakerType.Player, "감사합니다! 열심히 했어요."),
                new DialogueLine(SpeakerType.NPC, "다음에 또 올게요!"),
                new DialogueLine(SpeakerType.System, "[스테이지 클리어!]")
            };

            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 기본 실패 대화 초기화
        /// </summary>
        private void InitializeDefaultFailDialogue()
        {
            finalFailDialogue = new List<DialogueLine>
            {
                new DialogueLine(SpeakerType.NPC, "음식은 맛있었는데..."),
                new DialogueLine(SpeakerType.NPC, "목표 금액에는 조금 못 미쳤네요."),
                new DialogueLine(SpeakerType.Player, "다음엔 더 잘할게요!"),
                new DialogueLine(SpeakerType.NPC, "화이팅하세요!"),
                new DialogueLine(SpeakerType.System, "[스테이지 실패... 다시 도전하세요!]")
            };

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 성공/실패에 따른 대화 가져오기
        /// </summary>
        public List<DialogueLine> GetFinalDialogue(bool isSuccess)
        {
            if (isSuccess)
            {
                return finalSuccessDialogue;
            }
            else
            {
                return finalFailDialogue;
            }
        }

        /// <summary>
        /// 성공 대화가 설정되어 있는지 확인
        /// </summary>
        public bool HasSuccessDialogue()
        {
            return finalSuccessDialogue != null && finalSuccessDialogue.Count > 0;
        }

        /// <summary>
        /// 실패 대화가 설정되어 있는지 확인
        /// </summary>
        public bool HasFailDialogue()
        {
            return finalFailDialogue != null && finalFailDialogue.Count > 0;
        }

        /// <summary>
        /// AfterStory가 설정되어 있는지 확인
        /// </summary>
        public bool HasAfterStory()
        {
            return hasAfterStory && afterStoryImage != null;
        }

        /// <summary>
        /// AfterStory 독백이 있는지 확인
        /// </summary>
        public bool HasAfterStoryDialogue()
        {
            return hasAfterStory && afterStoryDialogue != null && afterStoryDialogue.Count > 0;
        }

        /// <summary>
        /// AfterStory 이미지 가져오기
        /// </summary>
        public Sprite GetAfterStoryImage()
        {
            return afterStoryImage;
        }

        /// <summary>
        /// AfterStory 독백 대화 가져오기
        /// </summary>
        public List<DialogueLine> GetAfterStoryDialogue()
        {
            return afterStoryDialogue;
        }

        /// <summary>
        /// 스테이지 대화 데이터 유효성 검증
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            // 스테이지 ID 확인
            if (stageID <= 0)
            {
                errorMessage = "스테이지 ID는 1 이상이어야 합니다.";
                return false;
            }

            // 성공 대화 확인
            if (!HasSuccessDialogue())
            {
                errorMessage = "성공 대화가 설정되지 않았습니다.";
                return false;
            }

            // 실패 대화 확인
            if (!HasFailDialogue())
            {
                errorMessage = "실패 대화가 설정되지 않았습니다.";
                return false;
            }

            // 대화 라인 유효성 확인
            foreach (var line in finalSuccessDialogue)
            {
                if (line == null || line.IsEmpty())
                {
                    errorMessage = "성공 대화에 빈 라인이 포함되어 있습니다.";
                    return false;
                }
            }

            foreach (var line in finalFailDialogue)
            {
                if (line == null || line.IsEmpty())
                {
                    errorMessage = "실패 대화에 빈 라인이 포함되어 있습니다.";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }

        // ==========================================
        // Debug
        // ==========================================

        /// <summary>
        /// 스테이지 대화 정보 로그 출력
        /// </summary>
        public void LogInfo()
        {
            Debug.Log($"=== StageDialogueData: {stageName} (ID: {stageID}) ===");
            Debug.Log($"Description: {description}");
            Debug.Log($"Success Dialogue Lines: {finalSuccessDialogue?.Count ?? 0}");
            Debug.Log($"Fail Dialogue Lines: {finalFailDialogue?.Count ?? 0}");

            if (finalSuccessDialogue != null && finalSuccessDialogue.Count > 0)
            {
                Debug.Log("Success Dialogue:");
                foreach (var line in finalSuccessDialogue)
                {
                    Debug.Log($"  {line}");
                }
            }

            if (finalFailDialogue != null && finalFailDialogue.Count > 0)
            {
                Debug.Log("Fail Dialogue:");
                foreach (var line in finalFailDialogue)
                {
                    Debug.Log($"  {line}");
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Stage Dialogue")]
        private void ValidateStageDialogue()
        {
            if (Validate(out string error))
            {
                Debug.Log($"[StageDialogueData] {stageName} 검증 성공!");
            }
            else
            {
                Debug.LogError($"[StageDialogueData] {stageName} 검증 실패: {error}");
            }
        }

        [ContextMenu("Log Stage Dialogue Info")]
        private void LogStageDialogueInfo()
        {
            LogInfo();
        }
#endif
    }
}
