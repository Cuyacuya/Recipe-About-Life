using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// 스토리 NPC 설정 (ScriptableObject)
    /// 각 스테이지의 마지막 NPC로 등장하는 스토리 NPC 정보 관리
    /// </summary>
    [CreateAssetMenu(fileName = "StoryNPCConfig", menuName = "RecipeAboutLife/NPC/Story NPC Config", order = 1)]
    public class StoryNPCConfig : ScriptableObject
    {
        // ==========================================
        // 스토리 NPC 목록
        // ==========================================

        [Header("스토리 NPC 설정")]
        [Tooltip("스테이지별 스토리 NPC 매핑")]
        public List<StageStoryNPC> stageStoryNPCs = new List<StageStoryNPC>();

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 특정 스테이지의 스토리 NPC DialogueSet 가져오기
        /// </summary>
        /// <param name="stageID">스테이지 번호 (1, 2, 3...)</param>
        /// <param name="afterStoryOnly">AfterStory 전용 NPC를 가져올지 여부 (기본: false)</param>
        /// <returns>DialogueSet (없으면 null)</returns>
        public Dialogue.NPCDialogueSet GetDialogueSetForStage(int stageID, bool afterStoryOnly = false)
        {
            var stageNPC = stageStoryNPCs.FirstOrDefault(s => s.stageID == stageID && s.isAfterStoryOnly == afterStoryOnly);

            if (stageNPC == null)
            {
                string npcType = afterStoryOnly ? "AfterStory 전용" : "일반 스토리";
                Debug.LogWarning($"[StoryNPCConfig] Stage {stageID}에 {npcType} NPC가 설정되지 않았습니다!");
                return null;
            }

            if (stageNPC.dialogueSet == null)
            {
                Debug.LogError($"[StoryNPCConfig] Stage {stageID}의 DialogueSet이 null입니다!");
                return null;
            }

            return stageNPC.dialogueSet;
        }

        /// <summary>
        /// 특정 스테이지의 스토리 NPC 스프라이트 가져오기
        /// </summary>
        /// <param name="stageID">스테이지 번호 (1, 2, 3...)</param>
        /// <param name="afterStoryOnly">AfterStory 전용 NPC를 가져올지 여부 (기본: false)</param>
        /// <returns>NPC Sprite (없으면 null)</returns>
        public Sprite GetSpriteForStage(int stageID, bool afterStoryOnly = false)
        {
            var stageNPC = stageStoryNPCs.FirstOrDefault(s => s.stageID == stageID && s.isAfterStoryOnly == afterStoryOnly);

            if (stageNPC == null)
            {
                string npcType = afterStoryOnly ? "AfterStory 전용" : "일반 스토리";
                Debug.LogWarning($"[StoryNPCConfig] Stage {stageID}에 {npcType} NPC가 설정되지 않았습니다!");
                return null;
            }

            if (stageNPC.npcSprite == null)
            {
                Debug.LogError($"[StoryNPCConfig] Stage {stageID}의 NPC Sprite가 null입니다!");
                return null;
            }

            return stageNPC.npcSprite;
        }

        /// <summary>
        /// 특정 스테이지의 스토리 NPC 프리팹 가져오기 (레거시, 사용 안 함)
        /// </summary>
        /// <param name="stageID">스테이지 번호 (1, 2, 3...)</param>
        /// <returns>스토리 NPC 프리팹 (없으면 null)</returns>
        public GameObject GetStoryNPCForStage(int stageID)
        {
            var stageNPC = stageStoryNPCs.FirstOrDefault(s => s.stageID == stageID);

            if (stageNPC == null)
            {
                Debug.LogWarning($"[StoryNPCConfig] Stage {stageID}에 스토리 NPC가 설정되지 않았습니다!");
                return null;
            }

            if (stageNPC.storyNPCPrefab == null)
            {
                Debug.LogError($"[StoryNPCConfig] Stage {stageID}의 스토리 NPC 프리팹이 null입니다!");
                return null;
            }

            return stageNPC.storyNPCPrefab;
        }

        /// <summary>
        /// 특정 NPC 프리팹이 스토리 NPC인지 확인
        /// </summary>
        /// <param name="npcPrefab">확인할 NPC 프리팹</param>
        /// <returns>스토리 NPC이면 true</returns>
        public bool IsStoryNPC(GameObject npcPrefab)
        {
            if (npcPrefab == null)
                return false;

            return stageStoryNPCs.Any(s => s.storyNPCPrefab == npcPrefab);
        }

        /// <summary>
        /// 특정 NPC 이름이 스토리 NPC인지 확인
        /// </summary>
        /// <param name="npcName">확인할 NPC 이름</param>
        /// <returns>스토리 NPC이면 true</returns>
        public bool IsStoryNPCByName(string npcName)
        {
            if (string.IsNullOrEmpty(npcName))
                return false;

            return stageStoryNPCs.Any(s =>
                s.storyNPCPrefab != null &&
                s.storyNPCPrefab.name == npcName
            );
        }

        /// <summary>
        /// 특정 스테이지에 스토리 NPC가 설정되어 있는지 확인
        /// </summary>
        /// <param name="stageID">스테이지 번호</param>
        /// <param name="afterStoryOnly">AfterStory 전용 NPC를 확인할지 여부 (기본: false)</param>
        /// <returns>설정되어 있으면 true</returns>
        public bool HasStoryNPCForStage(int stageID, bool afterStoryOnly = false)
        {
            var stageNPC = stageStoryNPCs.FirstOrDefault(s => s.stageID == stageID && s.isAfterStoryOnly == afterStoryOnly);
            return stageNPC != null && stageNPC.dialogueSet != null && stageNPC.npcSprite != null;
        }

        /// <summary>
        /// NPC 프리팹이 속한 스테이지 ID 가져오기
        /// </summary>
        /// <param name="npcPrefab">확인할 NPC 프리팹</param>
        /// <returns>스테이지 ID (없으면 -1)</returns>
        public int GetStageIDForNPC(GameObject npcPrefab)
        {
            if (npcPrefab == null)
                return -1;

            var stageNPC = stageStoryNPCs.FirstOrDefault(s => s.storyNPCPrefab == npcPrefab);
            return stageNPC != null ? stageNPC.stageID : -1;
        }

        /// <summary>
        /// 모든 스토리 NPC 프리팹 목록 가져오기
        /// </summary>
        /// <returns>스토리 NPC 프리팹 리스트</returns>
        public List<GameObject> GetAllStoryNPCPrefabs()
        {
            return stageStoryNPCs
                .Where(s => s.storyNPCPrefab != null)
                .Select(s => s.storyNPCPrefab)
                .ToList();
        }

        // ==========================================
        // Validation
        // ==========================================

        /// <summary>
        /// 설정 유효성 검증
        /// </summary>
        /// <param name="errorMessage">오류 메시지</param>
        /// <returns>유효하면 true</returns>
        public bool Validate(out string errorMessage)
        {
            // 스토리 NPC 목록 확인
            if (stageStoryNPCs == null || stageStoryNPCs.Count == 0)
            {
                errorMessage = "스토리 NPC가 설정되지 않았습니다.";
                return false;
            }

            // 중복 스테이지 ID 확인 (isAfterStoryOnly가 다르면 허용)
            var duplicateStages = stageStoryNPCs
                .GroupBy(s => new { s.stageID, s.isAfterStoryOnly })
                .Where(g => g.Count() > 1)
                .Select(g => $"Stage {g.Key.stageID} (AfterStoryOnly: {g.Key.isAfterStoryOnly})")
                .ToList();

            if (duplicateStages.Count > 0)
            {
                errorMessage = $"중복된 스테이지 설정: {string.Join(", ", duplicateStages)}";
                return false;
            }

            // 각 스토리 NPC 확인
            foreach (var stageNPC in stageStoryNPCs)
            {
                if (stageNPC.stageID <= 0)
                {
                    errorMessage = "스테이지 ID는 1 이상이어야 합니다.";
                    return false;
                }

                // DialogueSet 확인 (필수)
                if (stageNPC.dialogueSet == null)
                {
                    string npcType = stageNPC.isAfterStoryOnly ? "AfterStory 전용" : "일반 스토리";
                    errorMessage = $"Stage {stageNPC.stageID} ({npcType})의 DialogueSet이 설정되지 않았습니다.";
                    return false;
                }

                // NPC Sprite 확인 (필수)
                if (stageNPC.npcSprite == null)
                {
                    string npcType = stageNPC.isAfterStoryOnly ? "AfterStory 전용" : "일반 스토리";
                    errorMessage = $"Stage {stageNPC.stageID} ({npcType})의 NPC Sprite가 설정되지 않았습니다.";
                    return false;
                }

                // 프리팹 확인 (일반 스토리 NPC만 필수, AfterStory 전용은 선택)
                if (!stageNPC.isAfterStoryOnly && stageNPC.storyNPCPrefab == null)
                {
                    errorMessage = $"Stage {stageNPC.stageID} (일반 스토리)의 스토리 NPC 프리팹이 설정되지 않았습니다.";
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
        /// 설정 정보 로그 출력
        /// </summary>
        public void LogInfo()
        {
            Debug.Log("=== StoryNPCConfig ===");
            Debug.Log($"Total Story NPCs: {stageStoryNPCs.Count}");

            foreach (var stageNPC in stageStoryNPCs)
            {
                string npcName = stageNPC.dialogueSet != null ? stageNPC.dialogueSet.npcID : "NULL";
                string npcType = stageNPC.isAfterStoryOnly ? "AfterStory 전용" : "일반 스토리";
                Debug.Log($"  Stage {stageNPC.stageID} ({npcType}): {npcName}");
                if (!string.IsNullOrEmpty(stageNPC.description))
                {
                    Debug.Log($"    Description: {stageNPC.description}");
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Validate Config")]
        private void ValidateConfig()
        {
            if (Validate(out string error))
            {
                Debug.Log("[StoryNPCConfig] 검증 성공!");
            }
            else
            {
                Debug.LogError($"[StoryNPCConfig] 검증 실패: {error}");
            }
        }

        [ContextMenu("Log Config Info")]
        private void LogConfigInfo()
        {
            LogInfo();
        }
#endif
    }

    // ==========================================
    // Data Classes
    // ==========================================

    /// <summary>
    /// 스테이지별 스토리 NPC 매핑
    /// </summary>
    [System.Serializable]
    public class StageStoryNPC
    {
        [Header("스테이지 정보")]
        [Tooltip("스테이지 번호 (1, 2, 3...)")]
        public int stageID = 1;

        [Header("NPC 타입")]
        [Tooltip("AfterStory만 진행하는 특별 NPC인지 여부 (true면 5번째 손님으로 등장하지 않고 결산 후에만 등장)")]
        public bool isAfterStoryOnly = false;

        [Header("스토리 NPC 데이터")]
        [Tooltip("NPC 대화 세트 (DialogueSet)")]
        public Dialogue.NPCDialogueSet dialogueSet;

        [Tooltip("NPC 스프라이트 (정면 이미지)")]
        public Sprite npcSprite;

        [Header("레거시 (사용 안 함)")]
        [Tooltip("이 스테이지 마지막에 등장하는 스토리 NPC 프리팹 (더 이상 사용하지 않음)")]
        public GameObject storyNPCPrefab;

        [Header("설명 (선택)")]
        [TextArea(2, 3)]
        [Tooltip("스토리 NPC 설명 (에디터 참고용)")]
        public string description = "";
    }
}
