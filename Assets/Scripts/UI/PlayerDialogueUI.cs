using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// 플레이어 대화 UI (화면 하단)
    /// Player가 말하는 대사를 화면 하단에 표시
    /// Singleton 패턴으로 전역 접근 가능
    /// </summary>
    public class PlayerDialogueUI : MonoBehaviour
    {
        // ==========================================
        // Singleton
        // ==========================================

        private static PlayerDialogueUI _instance;
        public static PlayerDialogueUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<PlayerDialogueUI>();
                }
                return _instance;
            }
        }

        // ==========================================
        // UI References
        // ==========================================

        [Header("UI 참조")]
        [SerializeField]
        [Tooltip("대화 패널 (전체 UI 컨테이너)")]
        private GameObject dialoguePanel;

        [SerializeField]
        [Tooltip("대화 텍스트")]
        private TextMeshProUGUI dialogueText;

        [SerializeField]
        [Tooltip("스킵 안내 이미지 (터치 유도)")]
        private UnityEngine.UI.Image skipImage;

        [Header("애니메이션 설정")]
        [SerializeField]
        [Tooltip("스킵 이미지 이동 거리")]
        private float skipImageMoveDistance = 20f;

        [SerializeField]
        [Tooltip("스킵 이미지 이동 속도")]
        private float skipImageMoveSpeed = 1f;

        // ==========================================
        // State
        // ==========================================

        private Coroutine skipImageAnimationCoroutine;
        private Vector2 skipImageOriginalPosition;

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

            // 스킵 이미지 원래 위치 저장
            if (skipImage != null)
            {
                skipImageOriginalPosition = skipImage.rectTransform.anchoredPosition;
            }

            // 시작 시 숨김
            Hide();
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 플레이어 대화 표시
        /// </summary>
        /// <param name="text">대화 내용 (빈 문자열 가능)</param>
        public void Show(string text)
        {
            // 패널 활성화
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // 텍스트 설정 (빈 문자열도 허용)
            if (dialogueText != null)
            {
                dialogueText.text = text ?? "";
            }
            else
            {
                Debug.LogError("[PlayerDialogueUI] DialogueText가 설정되지 않았습니다!");
            }

            // 스킵 이미지 애니메이션 시작
            StartSkipImageAnimation();

            if (!string.IsNullOrWhiteSpace(text))
            {
                Debug.Log($"[PlayerDialogueUI] Player: {text}");
            }
            else
            {
                Debug.Log("[PlayerDialogueUI] Player 대화창 빈칸으로 표시");
            }
        }

        /// <summary>
        /// 대화 숨기기
        /// </summary>
        public void Hide()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // 스킵 이미지 애니메이션 중지
            StopSkipImageAnimation();
        }

        /// <summary>
        /// 현재 표시 중인지 확인
        /// </summary>
        public bool IsVisible()
        {
            return dialoguePanel != null && dialoguePanel.activeSelf;
        }

        // ==========================================
        // Skip Image Animation
        // ==========================================

        /// <summary>
        /// 스킵 이미지 애니메이션 시작
        /// </summary>
        private void StartSkipImageAnimation()
        {
            if (skipImage == null)
                return;

            // 기존 애니메이션이 있으면 중지
            StopSkipImageAnimation();

            // 새 애니메이션 시작
            skipImageAnimationCoroutine = StartCoroutine(SkipImageBounceAnimation());
        }

        /// <summary>
        /// 스킵 이미지 애니메이션 중지
        /// </summary>
        private void StopSkipImageAnimation()
        {
            if (skipImageAnimationCoroutine != null)
            {
                StopCoroutine(skipImageAnimationCoroutine);
                skipImageAnimationCoroutine = null;
            }

            // 원래 위치로 복원
            if (skipImage != null)
            {
                skipImage.rectTransform.anchoredPosition = skipImageOriginalPosition;
            }
        }

        /// <summary>
        /// 스킵 이미지 위아래 바운스 애니메이션
        /// </summary>
        private System.Collections.IEnumerator SkipImageBounceAnimation()
        {
            if (skipImage == null)
                yield break;

            RectTransform rectTransform = skipImage.rectTransform;
            // 저장된 원래 위치 사용 (매번 현재 위치를 사용하면 누적됨)
            Vector2 originalPosition = skipImageOriginalPosition;

            while (true)
            {
                float time = 0f;

                // 위로 이동
                while (time < 1f)
                {
                    time += Time.deltaTime * skipImageMoveSpeed;
                    float offset = Mathf.Sin(time * Mathf.PI) * skipImageMoveDistance;
                    rectTransform.anchoredPosition = originalPosition + new Vector2(0, offset);
                    yield return null;
                }

                // 아래로 이동
                time = 0f;
                while (time < 1f)
                {
                    time += Time.deltaTime * skipImageMoveSpeed;
                    float offset = Mathf.Sin((1f - time) * Mathf.PI) * skipImageMoveDistance;
                    rectTransform.anchoredPosition = originalPosition + new Vector2(0, offset);
                    yield return null;
                }
            }
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Show Dialogue")]
        private void TestShow()
        {
            Show("테스트 대사입니다.");
        }

        [ContextMenu("Test: Hide Dialogue")]
        private void TestHide()
        {
            Hide();
        }
#endif
    }
}
