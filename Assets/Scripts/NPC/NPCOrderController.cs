using UnityEngine;
using RecipeAboutLife.Orders;
using RecipeAboutLife.Cooking;
using RecipeAboutLife.Events;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// NPC 주문 컨트롤러
    /// NPC가 정지했을 때 DialogueSet에서 주문을 가져와 말풍선 UI에 표시하고
    /// 요리 시스템에 주문 전달
    /// </summary>
    public class NPCOrderController : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField]
        [Tooltip("NPC 대화 세트 (주문 정보 포함)")]
        private Dialogue.NPCDialogueSet dialogueSet;

        [SerializeField]
        [Tooltip("말풍선 UI (DialogueBubbleUI 사용)")]
        private UI.DialogueBubbleUI dialogueBubbleUI;

        [Header("상태")]
        private OrderData currentOrder;
        private CustomerOrder currentCustomerOrder;
        private bool hasOrder = false;
        private bool isWaitingForFood = false;

        private void Awake()
        {
            // DialogueBubbleUI 자동 찾기 (비활성화된 것도 찾기)
            if (dialogueBubbleUI == null)
            {
                dialogueBubbleUI = GetComponentInChildren<UI.DialogueBubbleUI>(true);
                if (dialogueBubbleUI == null)
                {
                    Debug.LogWarning("[NPCOrderController] DialogueBubbleUI를 찾을 수 없습니다!");
                }
            }

            // DialogueSet 자동 찾기 (NPCDialogueController에서 가져오기)
            if (dialogueSet == null)
            {
                var dialogueController = GetComponent<Dialogue.NPCDialogueController>();
                if (dialogueController != null)
                {
                    dialogueSet = dialogueController.GetDialogueSet();
                }

                if (dialogueSet == null)
                {
                    Debug.LogError("[NPCOrderController] DialogueSet을 찾을 수 없습니다!");
                }
            }
        }

        /// <summary>
        /// NPC가 정지했을 때 호출 - 주문 요청
        /// </summary>
        public void OnNPCStopped()
        {
            if (hasOrder)
            {
                Debug.LogWarning("[NPCOrderController] 이미 주문을 받았습니다.");
                return;
            }

            RequestOrder();
        }

        /// <summary>
        /// 주문 요청 - DialogueSet에서 주문 가져오기
        /// </summary>
        private void RequestOrder()
        {
            if (dialogueSet == null)
            {
                Debug.LogError("[NPCOrderController] DialogueSet이 없어서 주문을 받을 수 없습니다!");
                return;
            }

            // DialogueSet에서 주문 가져오기
            currentOrder = dialogueSet.npcOrder;

            if (currentOrder != null)
            {
                hasOrder = true;
                DisplayOrder();

                // 주문을 요리 시스템으로 전달
                SendOrderToCookingSystem();

                Debug.Log($"[NPCOrderController] DialogueSet에서 주문 받음: {currentOrder.OrderName}");
            }
            else
            {
                Debug.LogError($"[NPCOrderController] DialogueSet({dialogueSet.npcID})에 주문 정보가 없습니다!");
            }
        }

        /// <summary>
        /// 주문을 요리 시스템으로 전달
        /// </summary>
        private void SendOrderToCookingSystem()
        {
            // OrderData를 CustomerOrder로 변환
            currentCustomerOrder = OrderDataConverter.ToCustomerOrder(currentOrder);

            if (currentCustomerOrder == null)
            {
                Debug.LogError("[NPCOrderController] 주문 변환 실패!");
                return;
            }

            // CookingManager에 주문 전달
            if (CookingManager.Instance != null)
            {
                CookingManager.Instance.StartCooking(currentCustomerOrder);
                isWaitingForFood = true;

                Debug.Log($"[NPCOrderController] 요리 시스템에 주문 전달: {currentOrder.OrderName}");

                // 고객 도착 이벤트 발생
                GameEvents.TriggerCustomerArrived(currentCustomerOrder);
            }
            else
            {
                Debug.LogError("[NPCOrderController] CookingManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 말풍선 UI에 주문 표시
        /// </summary>
        private void DisplayOrder()
        {
            if (dialogueBubbleUI == null)
            {
                Debug.LogWarning("[NPCOrderController] DialogueBubbleUI가 없어서 주문을 표시할 수 없습니다!");
                return;
            }

            // Canvas가 꺼져있으면 켜기
            if (!dialogueBubbleUI.gameObject.activeSelf)
            {
                dialogueBubbleUI.gameObject.SetActive(true);
                Debug.Log("[NPCOrderController] DialogueCanvas를 활성화했습니다.");
            }

            // DialogueBubbleUI의 ShowOrder 메서드 사용
            dialogueBubbleUI.ShowOrder(currentOrder);
        }

        /// <summary>
        /// 현재 주문 가져오기
        /// </summary>
        public OrderData GetCurrentOrder()
        {
            return currentOrder;
        }

        /// <summary>
        /// 주문 초기화
        /// </summary>
        public void ClearOrder()
        {
            currentOrder = null;
            currentCustomerOrder = null;
            hasOrder = false;
            isWaitingForFood = false;

            if (dialogueBubbleUI != null)
            {
                dialogueBubbleUI.Hide();
            }
        }

        /// <summary>
        /// 음식 제공 대기 중인지 확인
        /// </summary>
        public bool IsWaitingForFood()
        {
            return isWaitingForFood;
        }

        /// <summary>
        /// CustomerOrder 가져오기
        /// </summary>
        public Cooking.CustomerOrder GetCustomerOrder()
        {
            return currentCustomerOrder;
        }

    }
}
