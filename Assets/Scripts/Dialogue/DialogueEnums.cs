namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 대화 타입 (NPC 라이프사이클별 대화 분류)
    /// </summary>
    public enum DialogueType
    {
        /// <summary>
        /// NPC 첫 등장 시 대사
        /// </summary>
        Intro,

        /// <summary>
        /// 주문 설명/멘트
        /// </summary>
        Order,

        /// <summary>
        /// 주문 성공 (음식이 주문과 일치)
        /// </summary>
        ServedSuccess,

        /// <summary>
        /// 주문 실패 (음식이 주문과 불일치 또는 품질 낮음)
        /// </summary>
        ServedFail,

        /// <summary>
        /// 퇴장 시 대사
        /// </summary>
        Exit
    }

    /// <summary>
    /// 대화 발화자 타입
    /// </summary>
    public enum SpeakerType
    {
        /// <summary>
        /// NPC가 말하는 대사
        /// </summary>
        NPC,

        /// <summary>
        /// 플레이어가 말하는 대사
        /// </summary>
        Player,

        /// <summary>
        /// 시스템 내레이션/설명
        /// </summary>
        System
    }
}
