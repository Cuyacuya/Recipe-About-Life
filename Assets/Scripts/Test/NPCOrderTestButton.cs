using UnityEngine;
using UnityEngine.UI;

namespace RecipeAboutLife.Test
{
    /// <summary>
    /// NPC 주문 테스트 버튼
    /// 에디터에서 NPC 주문 시스템을 테스트하기 위한 버튼
    /// </summary>
    public class NPCOrderTestButton : MonoBehaviour
    {
        [Header("버튼 참조")]
        [SerializeField]
        [Tooltip("테스트 버튼")]
        private Button testButton;

        [Header("테스트 설정")]
        [SerializeField]
        [Tooltip("테스트할 NPC 프리팹")]
        private GameObject npcPrefab;

        [SerializeField]
        [Tooltip("NPC 생성 위치")]
        private Transform spawnPosition;

        [SerializeField]
        [Tooltip("버튼 텍스트 (선택)")]
        private TMPro.TextMeshProUGUI buttonText;

        // ==========================================
        // State
        // ==========================================

        private GameObject currentNPC;

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // 버튼 자동 찾기
            if (testButton == null)
            {
                testButton = GetComponent<Button>();
            }

            // 버튼 클릭 이벤트 등록
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClicked);
            }

            // 버튼 텍스트 자동 찾기
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
            }

            // 기본 텍스트 설정
            if (buttonText != null)
            {
                buttonText.text = "NPC 주문 테스트";
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (testButton != null)
            {
                testButton.onClick.RemoveListener(OnTestButtonClicked);
            }
        }

        // ==========================================
        // Button Handler
        // ==========================================

        /// <summary>
        /// 테스트 버튼 클릭 시
        /// </summary>
        private void OnTestButtonClicked()
        {
            if (npcPrefab == null)
            {
                Debug.LogError("[NPCOrderTestButton] NPC 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 이미 NPC가 있으면 삭제
            if (currentNPC != null)
            {
                Destroy(currentNPC);
                currentNPC = null;
                Debug.Log("[NPCOrderTestButton] 기존 NPC 삭제");
            }

            // NPC 생성
            Vector3 position = spawnPosition != null ? spawnPosition.position : new Vector3(-5f, 0f, 0f);
            currentNPC = Instantiate(npcPrefab, position, Quaternion.identity);
            currentNPC.name = npcPrefab.name; // (Clone) 제거

            Debug.Log($"[NPCOrderTestButton] NPC 생성: {currentNPC.name} at {position}");

            // NPCOrderController 가져와서 주문 시작
            NPC.NPCOrderController orderController = currentNPC.GetComponent<NPC.NPCOrderController>();
            if (orderController != null)
            {
                orderController.OnNPCStopped();
                Debug.Log("[NPCOrderTestButton] NPC 주문 요청");
            }
            else
            {
                Debug.LogWarning("[NPCOrderTestButton] NPCOrderController를 찾을 수 없습니다!");
            }

            // NPCDialogueController 가져와서 Intro 대화 시작
            Dialogue.NPCDialogueController dialogueController = currentNPC.GetComponent<Dialogue.NPCDialogueController>();
            if (dialogueController != null)
            {
                dialogueController.StartDialogue(Dialogue.DialogueType.Intro);
                Debug.Log("[NPCOrderTestButton] Intro 대화 시작");
            }
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// NPC 프리팹 설정
        /// </summary>
        public void SetNPCPrefab(GameObject prefab)
        {
            npcPrefab = prefab;
        }

        /// <summary>
        /// 현재 생성된 NPC 가져오기
        /// </summary>
        public GameObject GetCurrentNPC()
        {
            return currentNPC;
        }

        /// <summary>
        /// 현재 NPC 삭제
        /// </summary>
        public void ClearCurrentNPC()
        {
            if (currentNPC != null)
            {
                Destroy(currentNPC);
                currentNPC = null;
                Debug.Log("[NPCOrderTestButton] NPC 삭제");
            }
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Spawn NPC")]
        private void TestSpawnNPC()
        {
            OnTestButtonClicked();
        }

        [ContextMenu("Test: Clear NPC")]
        private void TestClearNPC()
        {
            ClearCurrentNPC();
        }
#endif
    }
}
