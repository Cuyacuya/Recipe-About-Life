using UnityEngine;
using TMPro;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// 대화 말풍선 UI
    /// NPC나 플레이어 옆에 붙어서 대화 내용을 표시
    /// OrderBubbleUI와 유사하지만 대화 전용
    /// </summary>
    public class DialogueBubbleUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField]
        [Tooltip("대화 텍스트를 표시할 TextMeshProUGUI")]
        private TextMeshProUGUI dialogueText;

        [Header("설정")]
        [SerializeField]
        [Tooltip("캐릭터로부터의 오프셋")]
        private Vector3 offset = new Vector3(0f, 1.5f, 0f);

        [SerializeField]
        [Tooltip("자동 숨김 여부 (false면 수동으로 Hide() 호출 필요)")]
        private bool autoHide = false;

        [SerializeField]
        [Tooltip("자동 숨김 시간 (초)")]
        private float autoHideDelay = 3f;

        private void Awake()
        {
            if (dialogueText == null)
            {
                dialogueText = GetComponentInChildren<TextMeshProUGUI>();
                if (dialogueText == null)
                {
                    Debug.LogError("[DialogueBubbleUI] TextMeshProUGUI를 찾을 수 없습니다!");
                }
            }

            // 초기에는 Canvas를 켜둠 (NPCOrderController에서 제어)
            // Hide()를 호출하지 않음
        }

        /// <summary>
        /// 대화 설정 및 표시
        /// </summary>
        public void ShowDialogue(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                Debug.LogWarning("[DialogueBubbleUI] 메시지가 비어있습니다!");
                Hide();
                return;
            }

            if (dialogueText == null)
            {
                Debug.LogError("[DialogueBubbleUI] DialogueText가 없어서 대화를 표시할 수 없습니다!");
                return;
            }

            // 대화 텍스트 설정
            dialogueText.text = message;

            // 표시
            gameObject.SetActive(true);

            Debug.Log($"[DialogueBubbleUI] 대화 표시: {message}");

            // 자동 숨김
            if (autoHide)
            {
                CancelInvoke(nameof(Hide));
                Invoke(nameof(Hide), autoHideDelay);
            }
        }

        /// <summary>
        /// 여러 줄 대화 표시
        /// </summary>
        public void ShowDialogue(string[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                Hide();
                return;
            }

            string combinedMessage = string.Join("\n", messages);
            ShowDialogue(combinedMessage);
        }

        /// <summary>
        /// 말풍선 숨기기
        /// </summary>
        public void Hide()
        {
            CancelInvoke(nameof(Hide));
            gameObject.SetActive(false);
            Debug.Log("[DialogueBubbleUI] 대화 UI 숨김");
        }

        /// <summary>
        /// 말풍선 표시 (텍스트 변경 없이)
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 현재 표시 여부
        /// </summary>
        public bool IsShowing()
        {
            return gameObject.activeSelf;
        }

        /// <summary>
        /// 텍스트 변경 (이미 표시 중일 때)
        /// </summary>
        public void UpdateText(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }
        }

        /// <summary>
        /// 주문 데이터 표시 (OrderBubbleUI 대체 기능)
        /// DialogueBubbleUI 하나로 주문과 대화 모두 표시 가능
        /// </summary>
        public void ShowOrder(Orders.OrderData order)
        {
            if (order == null)
            {
                Debug.LogWarning("[DialogueBubbleUI] Order가 null입니다!");
                Hide();
                return;
            }

            if (dialogueText == null)
            {
                Debug.LogError("[DialogueBubbleUI] DialogueText가 없어서 주문을 표시할 수 없습니다!");
                return;
            }

            // 주문 텍스트 설정
            dialogueText.text = order.GetOrderDescription();

            // 표시
            gameObject.SetActive(true);

            Debug.Log($"[DialogueBubbleUI] 주문 표시: {order.OrderName}");

            // 자동 숨김
            if (autoHide)
            {
                CancelInvoke(nameof(Hide));
                Invoke(nameof(Hide), autoHideDelay);
            }
        }
    }
}
