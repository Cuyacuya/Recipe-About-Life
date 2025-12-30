using UnityEngine;
using System.Collections.Generic;

namespace RecipeAboutLife.Lobby
{
    /// <summary>
    /// 개별 스테이지 정보
    /// </summary>
    [System.Serializable]
    public class StageInfo
    {
        public int stageIndex;
        public string stageName;
        public bool isUnlocked;
    }

    /// <summary>
    /// 스테이지 해금 데이터 (ScriptableObject)
    /// Project 창 > 우클릭 > Create > RecipeAboutLife > Stage Unlock Data
    /// </summary>
    [CreateAssetMenu(fileName = "StageUnlockData", menuName = "RecipeAboutLife/Stage Unlock Data")]
    public class StageUnlockData : ScriptableObject
    {
        [Header("스테이지 목록")]
        [SerializeField] private List<StageInfo> stages = new List<StageInfo>();

        public int StageCount => stages.Count;
        public IReadOnlyList<StageInfo> Stages => stages;

        public StageInfo GetStage(int stageIndex)
        {
            return stages.Find(s => s.stageIndex == stageIndex);
        }

        public bool IsStageUnlocked(int stageIndex)
        {
            var stage = GetStage(stageIndex);
            return stage != null && stage.isUnlocked;
        }

        public void UnlockStage(int stageIndex)
        {
            var stage = GetStage(stageIndex);
            if (stage != null)
            {
                stage.isUnlocked = true;
                Debug.Log($"[StageUnlockData] Stage {stageIndex} 해금!");
            }
        }

        public int GetLastUnlockedStageIndex()
        {
            int lastUnlocked = 0;
            foreach (var stage in stages)
            {
                if (stage.isUnlocked && stage.stageIndex > lastUnlocked)
                {
                    lastUnlocked = stage.stageIndex;
                }
            }
            return lastUnlocked;
        }

        public void ResetAllStages(bool unlockFirst = true)
        {
            foreach (var stage in stages)
            {
                stage.isUnlocked = false;
            }
            if (unlockFirst && stages.Count > 0)
            {
                stages[0].isUnlocked = true;
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Initialize Default Stages (3개)")]
        private void InitializeDefaultStages()
        {
            stages.Clear();
            stages.Add(new StageInfo { stageIndex = 1, stageName = "Stage 1", isUnlocked = true });
            stages.Add(new StageInfo { stageIndex = 2, stageName = "Stage 2", isUnlocked = false });
            stages.Add(new StageInfo { stageIndex = 3, stageName = "Stage 3", isUnlocked = false });
            UnityEditor.EditorUtility.SetDirty(this);
        }

        [ContextMenu("Unlock All Stages")]
        private void UnlockAllStages()
        {
            foreach (var stage in stages)
                stage.isUnlocked = true;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
