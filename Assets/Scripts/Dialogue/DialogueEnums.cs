namespace RecipeAboutLife.Dialogue
{
    /// <summary>
    /// 대화 타입 (NPC 라이프사이클별 대화 분류)
    /// </summary>
    public enum DialogueType
    {
        // ==========================================
        // 일반 NPC 대화 (랜덤 NPC용)
        // ==========================================

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
        Exit,

        // ==========================================
        // 스토리 NPC 전용 대화
        // ==========================================

        /// <summary>
        /// 스토리 NPC 등장 시 대사 (일반 Intro보다 길고 스토리 포함)
        /// </summary>
        StoryIntro,

        /// <summary>
        /// 스토리 NPC 주문 시 대사 (스토리 연출 포함)
        /// </summary>
        StoryOrder,

        /// <summary>
        /// 스토리 NPC 주문 성공 시 대사
        /// </summary>
        StoryServedSuccess,

        /// <summary>
        /// 스토리 NPC 주문 실패 시 대사
        /// </summary>
        StoryServedFail,

        /// <summary>
        /// ★ 재화 조건 달성 후 스토리 대화
        /// 스테이지 마지막 NPC(스토리 NPC)가 모든 주문 완료 후
        /// totalReward >= targetReward 조건을 만족하면 실행되는 대화
        /// Player ↔ NPC 대화 포함
        /// </summary>
        StoryAfterSummary
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
