using UnityEngine;
using UnityEngine.UI;
using RecipeAboutLife.Managers;
using RecipeAboutLife.NPC;
using RecipeAboutLife.Cooking;

namespace RecipeAboutLife.Testing
{
    /// <summary>
    /// 빠른 테스트용 버튼
    /// 클릭 시 현재 손님에게 최고 보상을 주고 즉시 퇴장시킴
    /// </summary>
    public class QuickTestButton : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField]
        [Tooltip("테스트 버튼 (없으면 자동으로 이 오브젝트의 Button 사용)")]
        private Button testButton;

        [Header("설정")]
        [SerializeField]
        [Tooltip("한 번 클릭으로 모든 NPC 처리 (5명 한번에)")]
        private bool processAllAtOnce = false;

        private void Awake()
        {
            // 버튼이 없으면 자동으로 찾기
            if (testButton == null)
            {
                testButton = GetComponent<Button>();
            }

            // 버튼 이벤트 등록
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClicked);
            }
        }

        /// <summary>
        /// 테스트 버튼 클릭 - 현재 NPC에게 최고 보상 후 즉시 퇴장
        /// </summary>
        public void OnTestButtonClicked()
        {
            if (processAllAtOnce)
            {
                // 모든 NPC 한번에 처리
                ProcessAllNPCs();
            }
            else
            {
                // 현재 NPC만 처리
                ProcessCurrentNPC();
            }
        }

        /// <summary>
        /// 현재 NPC 처리 (최고 보상 + 즉시 퇴장)
        /// </summary>
        private void ProcessCurrentNPC()
        {
            // ScoreManager 확인
            if (ScoreManager.Instance == null)
            {
                UnityEngine.Debug.LogError("[QuickTest] ScoreManager를 찾을 수 없습니다!");
                return;
            }

            // NPCSpawnManager 확인
            NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
            if (spawnManager == null)
            {
                UnityEngine.Debug.LogError("[QuickTest] NPCSpawnManager를 찾을 수 없습니다!");
                return;
            }

            // 현재 NPC 확인
            GameObject currentNPC = spawnManager.GetCurrentNPC();
            if (currentNPC == null)
            {
                UnityEngine.Debug.LogWarning("[QuickTest] 현재 NPC가 없습니다!");
                return;
            }

            // 최고 보상 지급 (ScoreManager의 테스트 메서드 사용)
            ScoreManager.Instance.TestServePerfectFoodForCurrentOrder();

            UnityEngine.Debug.Log("[QuickTest] ✅ 현재 NPC 처리 완료! (최고 보상 + 퇴장)");
        }

        /// <summary>
        /// 모든 NPC 한번에 처리 (5명 전부)
        /// </summary>
        private void ProcessAllNPCs()
        {
            StartCoroutine(ProcessAllNPCsCoroutine());
        }

        private System.Collections.IEnumerator ProcessAllNPCsCoroutine()
        {
            UnityEngine.Debug.Log("[QuickTest] === 모든 NPC 빠른 처리 시작 ===");

            for (int i = 0; i < 5; i++)
            {
                // ScoreManager 확인
                if (ScoreManager.Instance == null)
                {
                    UnityEngine.Debug.LogError("[QuickTest] ScoreManager를 찾을 수 없습니다!");
                    yield break;
                }

                // NPCSpawnManager 확인
                NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
                if (spawnManager == null)
                {
                    UnityEngine.Debug.LogError("[QuickTest] NPCSpawnManager를 찾을 수 없습니다!");
                    yield break;
                }

                // 현재 NPC 확인
                GameObject currentNPC = spawnManager.GetCurrentNPC();
                if (currentNPC == null)
                {
                    UnityEngine.Debug.Log($"[QuickTest] NPC {i + 1}: 현재 NPC 없음, 대기 중...");
                    yield return new WaitForSeconds(0.5f);
                    continue;
                }

                // 최고 보상 지급
                ScoreManager.Instance.TestServePerfectFoodForCurrentOrder();
                UnityEngine.Debug.Log($"[QuickTest] NPC {i + 1}/5 처리 완료!");

                // 다음 NPC 스폰 대기
                yield return new WaitForSeconds(1.0f);
            }

            UnityEngine.Debug.Log("[QuickTest] === 모든 NPC 처리 완료! ===");
            UnityEngine.Debug.Log($"[QuickTest] 총 재화: {ScoreManager.Instance.GetTotalReward()}원");
        }

        /// <summary>
        /// 외부에서 호출 가능한 단일 NPC 처리
        /// </summary>
        public static void QuickProcessCurrentNPC()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.TestServePerfectFoodForCurrentOrder();
                UnityEngine.Debug.Log("[QuickTest] Static: 현재 NPC 처리 완료!");
            }
        }
    }
}
