using UnityEngine;
using UnityEngine.UI;
using RecipeAboutLife.Managers;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// 완벽한 주문 완료 테스트 버튼
    /// 현재 NPC의 주문에 완벽히 맞는 음식을 제공하여 최고 재화를 받음
    /// </summary>
    public class PerfectOrderTestButton : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("테스트 버튼")]
        private Button testButton;

        private void Awake()
        {
            // 버튼 자동 찾기
            if (testButton == null)
            {
                testButton = GetComponent<Button>();
                if (testButton == null)
                {
                    Debug.LogError("[PerfectOrderTestButton] Button 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }

        private void Start()
        {
            // 버튼 클릭 이벤트 등록
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClicked);
            }
        }

        /// <summary>
        /// 테스트 버튼 클릭 시
        /// </summary>
        private void OnTestButtonClicked()
        {
            if (ScoreManager.Instance == null)
            {
                Debug.LogError("[PerfectOrderTestButton] ScoreManager를 찾을 수 없습니다!");
                return;
            }

            // 현재 NPC의 주문에 완벽히 맞는 음식 제공
            ScoreManager.Instance.TestServePerfectFoodForCurrentOrder();

            Debug.Log("[PerfectOrderTestButton] 완벽한 주문 완료!");
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 제거
            if (testButton != null)
            {
                testButton.onClick.RemoveListener(OnTestButtonClicked);
            }
        }
    }
}
