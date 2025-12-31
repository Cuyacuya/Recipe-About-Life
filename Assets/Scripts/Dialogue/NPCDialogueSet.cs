using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// NPC별 대화 세트 (ScriptableObject)
    /// 한 NPC가 사용하는 모든 대화를 타입별로 관리
    /// </summary>
    [CreateAssetMenu(fileName = "NPC_Dialogue_Set", menuName = "RecipeAboutLife/Dialogue/NPC Dialogue Set", order = 1)]
    public class NPCDialogueSet : ScriptableObject
    {
        // ==========================================
        // NPC 정보
        // ==========================================

        [Header("NPC 정보")]
        [Tooltip("NPC 식별자 (예: NPC_Businessman, NPC_Student)")]
        public string npcID = "";

        [Tooltip("NPC 표시 이름 (UI에 표시될 이름)")]
        public string npcDisplayName = "손님";

        [TextArea(2, 3)]
        [Tooltip("NPC 설명 (에디터 참고용)")]
        public string description = "";

        // ==========================================
        // 주문 정보
        // ==========================================

        [Header("주문 정보")]
        [Tooltip("이 NPC의 고정 주문 (Order 대화 타입에서 자동으로 표시됨)")]
        public Orders.OrderData npcOrder;

        // ==========================================
        // 대화 그룹들
        // ==========================================

        [Header("대화 그룹들")]
        [Tooltip("NPC가 사용하는 모든 대화 그룹")]
        public List<DialogueGroup> dialogueGroups = new List<DialogueGroup>();

        // ==========================================
        // 초기화 (에디터에서 생성 시 자동 설정)
        // ==========================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 기본 대화 그룹이 없으면 자동 생성
            if (dialogueGroups == null || dialogueGroups.Count == 0)
            {
                InitializeDefaultGroups();
            }
        }

        /// <summary>
        /// 기본 대화 그룹 초기화 (에디터용)
        /// </summary>
        private void InitializeDefaultGroups()
        {
            dialogueGroups = new List<DialogueGroup>
            {
                new DialogueGroup
                {
                    type = DialogueType.Intro,
                    lines = new DialogueLine[]
                    {
                        new DialogueLine(SpeakerType.NPC, "안녕하세요! 주문할게요.")
                    }
                },
                new DialogueGroup
                {
                    type = DialogueType.Order,
                    lines = new DialogueLine[]
                    {
                        new DialogueLine(SpeakerType.NPC, "이걸로 주세요!")
                    }
                },
                new DialogueGroup
                {
                    type = DialogueType.ServedSuccess,
                    lines = new DialogueLine[]
                    {
                        new DialogueLine(SpeakerType.NPC, "맛있겠네요! 감사합니다!")
                    }
                },
                new DialogueGroup
                {
                    type = DialogueType.ServedFail,
                    lines = new DialogueLine[]
                    {
                        new DialogueLine(SpeakerType.NPC, "음... 제가 주문한 게 아닌데요?")
                    }
                },
                new DialogueGroup
                {
                    type = DialogueType.Exit,
                    lines = new DialogueLine[]
                    {
                        new DialogueLine(SpeakerType.NPC, "잘 먹겠습니다!")
                    }
                }
            };

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 특정 타입의 대화 그룹 가져오기
        /// </summary>
        public DialogueGroup GetDialogueGroup(DialogueType type)
        {
            if (dialogueGroups == null || dialogueGroups.Count == 0)
            {
                Debug.LogWarning($"[NPCDialogueSet] {npcID}에 대화 그룹이 없습니다!");
                return null;
            }

            var group = dialogueGroups.FirstOrDefault(g => g.type == type);

            if (group == null)
            {
                Debug.LogWarning($"[NPCDialogueSet] {npcID}에 {type} 타입 대화가 없습니다!");
                return null;
            }

            return group;
        }

        /// <summary>
        /// 특정 타입의 대화 라인들 가져오기
        /// </summary>
        public DialogueLine[] GetDialogueLines(DialogueType type)
        {
            var group = GetDialogueGroup(type);
            return group?.lines;
        }

        /// <summary>
        /// 특정 타입의 대화가 존재하는지 확인
        /// </summary>
        public bool HasDialogue(DialogueType type)
        {
            var group = GetDialogueGroup(type);
            return group != null && !group.IsEmpty();
        }

        /// <summary>
        /// 모든 대화 타입이 설정되어 있는지 확인
        /// </summary>
        public bool IsComplete()
        {
            var allTypes = System.Enum.GetValues(typeof(DialogueType)) as DialogueType[];
            foreach (var type in allTypes)
            {
                if (!HasDialogue(type))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 대화 세트 유효성 검증
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            // NPC ID 확인
            if (string.IsNullOrWhiteSpace(npcID))
            {
                errorMessage = "NPC ID가 설정되지 않았습니다.";
                return false;
            }

            // 대화 그룹 확인
            if (dialogueGroups == null || dialogueGroups.Count == 0)
            {
                errorMessage = "대화 그룹이 없습니다.";
                return false;
            }

            // 각 그룹의 유효성 확인
            foreach (var group in dialogueGroups)
            {
                if (group == null)
                {
                    errorMessage = "null 대화 그룹이 포함되어 있습니다.";
                    return false;
                }

                if (group.IsEmpty())
                {
                    errorMessage = $"{group.type} 대화 그룹이 비어있습니다.";
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
        /// 대화 세트 정보 로그 출력
        /// </summary>
        public void LogInfo()
        {
            Debug.Log($"=== NPCDialogueSet: {npcID} ===");
            Debug.Log($"Display Name: {npcDisplayName}");
            Debug.Log($"Description: {description}");
            Debug.Log($"Total Groups: {dialogueGroups?.Count ?? 0}");

            if (dialogueGroups != null)
            {
                foreach (var group in dialogueGroups)
                {
                    Debug.Log($"  - {group.type}: {group.GetValidLineCount()} lines");
                }
            }

            Debug.Log($"Is Complete: {IsComplete()}");
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Dialogue Set")]
        private void ValidateDialogueSet()
        {
            if (Validate(out string error))
            {
                Debug.Log($"[NPCDialogueSet] {npcID} 검증 성공!");
            }
            else
            {
                Debug.LogError($"[NPCDialogueSet] {npcID} 검증 실패: {error}");
            }
        }

        [ContextMenu("Log Dialogue Info")]
        private void LogDialogueInfo()
        {
            LogInfo();
        }
#endif
    }
}
