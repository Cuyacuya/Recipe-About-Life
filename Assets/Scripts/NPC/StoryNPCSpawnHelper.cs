using UnityEngine;
using System.Collections.Generic;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// 스토리 NPC 스폰 헬퍼
    ///
    /// 역할:
    /// 1. NPCSpawnManager가 랜덤 NPC를 선택한 후
    /// 2. 마지막 NPC(5번째)를 현재 스테이지의 스토리 NPC로 교체
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

        [Header("교체 설정")]
        [SerializeField]
        [Tooltip("마지막 NPC를 스토리 NPC로 교체할지 여부")]
        private bool replaceLastNPCWithStory = true;

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
        /// 마지막 NPC를 스토리 NPC로 교체
        /// </summary>
        /// <param name="selectedNPCs">선택된 NPC 프리팹 리스트</param>
        private void OnNPCsSelected(List<GameObject> selectedNPCs)
        {
            if (!replaceLastNPCWithStory)
            {
                Debug.Log("[StoryNPCSpawnHelper] 스토리 NPC 교체가 비활성화되어 있습니다.");
                return;
            }

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

            // 현재 스테이지의 스토리 NPC 가져오기
            GameObject storyNPCPrefab = storyNPCConfig.GetStoryNPCForStage(currentStageID);
            if (storyNPCPrefab == null)
            {
                Debug.LogWarning($"[StoryNPCSpawnHelper] Stage {currentStageID}에 스토리 NPC가 설정되지 않았습니다!");
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
        /// 스토리 NPC 교체 활성화/비활성화
        /// </summary>
        /// <param name="enabled">활성화 여부</param>
        public void SetStoryNPCReplacementEnabled(bool enabled)
        {
            replaceLastNPCWithStory = enabled;
            Debug.Log($"[StoryNPCSpawnHelper] 스토리 NPC 교체: {(enabled ? "활성화" : "비활성화")}");
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
            Debug.Log($"Replace Last NPC: {replaceLastNPCWithStory}");
            Debug.Log($"StoryNPCConfig: {(storyNPCConfig != null ? storyNPCConfig.name : "NULL")}");

            if (storyNPCConfig != null && storyNPCConfig.HasStoryNPCForStage(currentStageID))
            {
                GameObject storyNPC = storyNPCConfig.GetStoryNPCForStage(currentStageID);
                Debug.Log($"Story NPC for Stage {currentStageID}: {(storyNPC != null ? storyNPC.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning($"No Story NPC configured for Stage {currentStageID}");
            }
        }
#endif
    }
}
