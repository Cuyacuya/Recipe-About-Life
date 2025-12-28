using UnityEngine;
using System.Collections.Generic;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// 스토리 NPC 스폰 헬퍼
    ///
    /// 역할:
    /// 1. NPCSpawnManager가 랜덤 NPC를 선택한 후
    /// 2. 마지막 NPC(5번째)를 현재 스테이지의 일반 스토리 NPC로 교체
    /// 3. AfterStory 전용 NPC는 교체하지 않음 (StageStoryController가 처리)
    ///
    /// ※ NPCSpawnManager를 수정하지 않고 이벤트 구독으로만 동작
    /// </summary>
    [RequireComponent(typeof(NPCSpawnManager))]
    public class StoryNPCSpawnHelper : MonoBehaviour
    {
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

        // ==========================================
        // References
        // ==========================================

        private NPCSpawnManager spawnManager;

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // NPCSpawnManager 참조 가져오기
            spawnManager = GetComponent<NPCSpawnManager>();
            if (spawnManager == null)
            {
                Debug.LogError("[StoryNPCSpawnHelper] NPCSpawnManager를 찾을 수 없습니다!");
                return;
            }

            // StoryNPCConfig 확인
            if (storyNPCConfig == null)
            {
                Debug.LogWarning("[StoryNPCSpawnHelper] StoryNPCConfig가 설정되지 않았습니다!");
            }
        }

        private void OnEnable()
        {
            // NPCSpawnManager의 NPC 선택 완료 이벤트 구독
            if (spawnManager != null)
            {
                spawnManager.OnNPCsSelected += OnNPCsSelected;
            }
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            if (spawnManager != null)
            {
                spawnManager.OnNPCsSelected -= OnNPCsSelected;
            }
        }

        // ==========================================
        // NPC Selection Handler
        // ==========================================

        /// <summary>
        /// NPCSpawnManager가 NPC를 선택한 후 호출되는 이벤트
        /// 마지막 NPC를 일반 스토리 NPC로 교체 (AfterStory 전용 제외)
        /// </summary>
        /// <param name="selectedNPCs">선택된 NPC 프리팹 리스트</param>
        private void OnNPCsSelected(List<GameObject> selectedNPCs)
        {
            if (selectedNPCs == null || selectedNPCs.Count == 0)
            {
                Debug.LogWarning("[StoryNPCSpawnHelper] 선택된 NPC가 없습니다!");
                return;
            }

            if (storyNPCConfig == null)
            {
                Debug.LogWarning("[StoryNPCSpawnHelper] StoryNPCConfig가 없어서 스토리 NPC로 교체할 수 없습니다!");
                return;
            }

            // 일반 스토리 NPC 확인 (isAfterStoryOnly = false)
            if (!storyNPCConfig.HasStoryNPCForStage(currentStageID, afterStoryOnly: false))
            {
                Debug.LogWarning($"[StoryNPCSpawnHelper] Stage {currentStageID}에 일반 스토리 NPC가 없습니다!");
                return;
            }

            // 현재 스테이지의 일반 스토리 NPC 프리팹 가져오기 (5번째 손님으로 스폰될 NPC)
            GameObject storyNPCPrefab = storyNPCConfig.GetStoryNPCForStage(currentStageID);
            if (storyNPCPrefab == null)
            {
                Debug.LogWarning($"[StoryNPCSpawnHelper] Stage {currentStageID}의 스토리 NPC 프리팹이 없습니다!");
                return;
            }

            // 마지막 NPC를 스토리 NPC로 교체
            int lastIndex = selectedNPCs.Count - 1;
            GameObject originalNPC = selectedNPCs[lastIndex];
            selectedNPCs[lastIndex] = storyNPCPrefab;

            Debug.Log($"[StoryNPCSpawnHelper] ✅ 마지막 NPC 교체 완료!");
            Debug.Log($"  - Stage: {currentStageID}");
            Debug.Log($"  - Original NPC: {originalNPC.name}");
            Debug.Log($"  - Story NPC: {storyNPCPrefab.name}");
            Debug.Log($"  - Position: {lastIndex + 1}/{selectedNPCs.Count}");
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
            Debug.Log($"[StoryNPCSpawnHelper] 현재 스테이지: {currentStageID}");
        }

        /// <summary>
        /// StoryNPCConfig 설정
        /// </summary>
        /// <param name="config">스토리 NPC 설정</param>
        public void SetStoryNPCConfig(StoryNPCConfig config)
        {
            storyNPCConfig = config;
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Log Current State")]
        private void LogCurrentState()
        {
            Debug.Log("=== StoryNPCSpawnHelper State ===");
            Debug.Log($"Current Stage ID: {currentStageID}");
            Debug.Log($"StoryNPCConfig: {(storyNPCConfig != null ? storyNPCConfig.name : "NULL")}");

            if (storyNPCConfig != null)
            {
                bool hasRegularStoryNPC = storyNPCConfig.HasStoryNPCForStage(currentStageID, afterStoryOnly: false);
                bool hasAfterStoryOnlyNPC = storyNPCConfig.HasStoryNPCForStage(currentStageID, afterStoryOnly: true);

                Debug.Log($"Has Regular Story NPC for Stage {currentStageID}: {hasRegularStoryNPC}");
                Debug.Log($"Has AfterStory Only NPC for Stage {currentStageID}: {hasAfterStoryOnlyNPC}");

                if (hasRegularStoryNPC)
                {
                    GameObject storyNPC = storyNPCConfig.GetStoryNPCForStage(currentStageID);
                    Debug.Log($"Regular Story NPC: {(storyNPC != null ? storyNPC.name : "NULL")}");
                }
            }
            else
            {
                Debug.LogWarning("StoryNPCConfig is not assigned!");
            }
        }
#endif
    }
}
