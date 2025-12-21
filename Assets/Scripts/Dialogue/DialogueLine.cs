using UnityEngine;

namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 대화 한 줄의 데이터
    /// 발화자와 내용을 포함
    /// </summary>
    [System.Serializable]
    public class DialogueLine
    {
        [Header("발화자")]
        [Tooltip("누가 말하는가")]
        public SpeakerType speaker = SpeakerType.NPC;

        [Header("대화 내용")]
        [TextArea(2, 5)]
        [Tooltip("대화 텍스트")]
        public string text = "";

        [Header("연출 옵션 (TODO)")]
        [Tooltip("대화 표시 시간 (초), 0이면 기본값 사용")]
        public float displayDuration = 0f;

        // TODO: 추가 연출 옵션
        // - 캐릭터 표정/감정
        // - 효과음
        // - 카메라 연출
        // - 애니메이션 트리거

        /// <summary>
        /// 빈 대화 라인인지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(text);
        }

        /// <summary>
        /// 생성자 (편의용)
        /// </summary>
        public DialogueLine()
        {
        }

        /// <summary>
        /// 생성자 (발화자, 텍스트 지정)
        /// </summary>
        public DialogueLine(SpeakerType speaker, string text)
        {
            this.speaker = speaker;
            this.text = text;
        }

        /// <summary>
        /// 디버그용 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"[{speaker}] {text}";
        }
    }

    /// <summary>
    /// 대화 그룹 (특정 타입의 대화들을 묶음)
    /// </summary>
    [System.Serializable]
    public class DialogueGroup
    {
        [Header("대화 타입")]
        [Tooltip("이 대화 그룹의 타입 (Intro, Order, ServedSuccess 등)")]
        public DialogueType type;

        [Header("대화 라인들")]
        [Tooltip("이 타입에 속하는 대화 라인 목록")]
        public DialogueLine[] lines = new DialogueLine[0];

        /// <summary>
        /// 빈 그룹인지 확인
        /// </summary>
        public bool IsEmpty()
        {
            return lines == null || lines.Length == 0;
        }

        /// <summary>
        /// 유효한 대화 라인 개수
        /// </summary>
        public int GetValidLineCount()
        {
            if (lines == null) return 0;

            int count = 0;
            foreach (var line in lines)
            {
                if (line != null && !line.IsEmpty())
                    count++;
            }
            return count;
        }
    }
}
