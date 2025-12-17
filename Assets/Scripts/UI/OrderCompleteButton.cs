using UnityEngine;
using UnityEngine.UI;
using RecipeAboutLife.NPC;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// 임시 주문 완료 버튼
    /// 테스트용 - 현재 NPC의 주문을 완료 처리
    /// </summary>
    public class OrderCompleteButton : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("완료 버튼")]
        private Button completeButton;

        [Header("참조")]
        [SerializeField]
        [Tooltip("NPC 스폰 매니저")]
        private NPCSpawnManager spawnManager;

        private void Awake()
        {
            // 버튼 자동 찾기
            if (completeButton == null)
            {
                completeButton = GetComponent<Button>();
                if (completeButton == null)
                {
                    Debug.LogError("[OrderCompleteButton] Button 컴포넌트를 찾을 수 없습니다!");
                }
            }

            // SpawnManager 자동 찾기
            if (spawnManager == null)
            {
                spawnManager = FindObjectOfType<NPCSpawnManager>();
                if (spawnManager == null)
                {
                    Debug.LogError("[OrderCompleteButton] NPCSpawnManager를 찾을 수 없습니다!");
                }
            }
        }

        private void Start()
        {
            // 버튼 클릭 이벤트 등록
            if (completeButton != null)
            {
                completeButton.onClick.AddListener(OnCompleteButtonClicked);
            }
        }

        /// <summary>
        /// 완료 버튼 클릭 시
        /// </summary>
        private void OnCompleteButtonClicked()
        {
            if (spawnManager == null)
            {
                Debug.LogError("[OrderCompleteButton] SpawnManager가 없습니다!");
                return;
            }

            // 현재 NPC 가져오기
            GameObject currentNPC = spawnManager.GetCurrentNPC();
            if (currentNPC == null)
            {
                Debug.LogWarning("[OrderCompleteButton] 현재 NPC가 없습니다!");
                return;
            }

            // NPC에게 주문 완료 알림
            NPCMovement npcMovement = currentNPC.GetComponent<NPCMovement>();
            if (npcMovement != null)
            {
                Debug.Log("[OrderCompleteButton] 주문 완료 처리!");
                npcMovement.OnOrderComplete();
            }
            else
            {
                Debug.LogError("[OrderCompleteButton] NPCMovement 컴포넌트를 찾을 수 없습니다!");
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 제거
            if (completeButton != null)
            {
                completeButton.onClick.RemoveListener(OnCompleteButtonClicked);
            }
        }
    }
}
