using UnityEngine;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// 프레임 패널 UI
    /// StoryAfterSummary 대화 시 화면 확대 연출을 위한 프레임
    /// Singleton 패턴으로 전역 접근 가능
    /// </summary>
    public class FramePanelUI : MonoBehaviour
    {
        // ==========================================
        // Singleton
        // ==========================================

        private static FramePanelUI _instance;
        public static FramePanelUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<FramePanelUI>();
                }
                return _instance;
            }
        }

        // ==========================================
        // UI References
        // ==========================================

        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("프레임 패널 (전체 UI 컨테이너)")]
        private GameObject framePanel;

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // Singleton 설정
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // framePanel이 설정되지 않았으면 자기 자신을 사용
            if (framePanel == null)
            {
                framePanel = gameObject;
            }

            // 시작 시 숨김
            Hide();
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 프레임 패널 표시 (확대 연출)
        /// </summary>
        public void Show()
        {
            if (framePanel != null)
            {
                framePanel.SetActive(true);
                Debug.Log("[FramePanelUI] 프레임 패널 활성화 (확대 연출)");
            }
            else
            {
                Debug.LogError("[FramePanelUI] FramePanel이 설정되지 않았습니다!");
            }
        }

        /// <summary>
        /// 프레임 패널 숨기기
        /// </summary>
        public void Hide()
        {
            if (framePanel != null)
            {
                framePanel.SetActive(false);
                Debug.Log("[FramePanelUI] 프레임 패널 비활성화");
            }
        }

        /// <summary>
        /// 현재 표시 중인지 확인
        /// </summary>
        public bool IsVisible()
        {
            return framePanel != null && framePanel.activeSelf;
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Show Frame Panel")]
        private void TestShow()
        {
            Show();
        }

        [ContextMenu("Test: Hide Frame Panel")]
        private void TestHide()
        {
            Hide();
        }
#endif
    }
}
