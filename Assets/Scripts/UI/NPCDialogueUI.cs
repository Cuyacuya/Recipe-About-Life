using UnityEngine;
using TMPro;

namespace RecipeAboutLife.UI
{
    /// <summary>
    /// NPC 대화 UI (DialogueCanvas 내부)
    /// NPC가 말하는 대사를 표시 (기존 말풍선 대신 사용)
    /// </summary>
    public class NPCDialogueUI : MonoBehaviour
    {
        // ==========================================
        // Singleton
        // ==========================================

        private static NPCDialogueUI _instance;
        public static NPCDialogueUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<NPCDialogueUI>();
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
        [Tooltip("NPC 이름 텍스트 (선택)")]
        private TextMeshProUGUI nameText;

        [SerializeField]
        [Tooltip("NPC 이미지 (캐릭터 스프라이트)")]
        private UnityEngine.UI.Image npcImage;

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
        /// NPC 대화 표시
        /// </summary>
        /// <param name="text">대화 내용 (빈 문자열 가능)</param>
        /// <param name="npcName">NPC 이름 (선택)</param>
        /// <param name="npcSprite">NPC 스프라이트 (선택)</param>
        public void Show(string text, string npcName = null, Sprite npcSprite = null)
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
                Debug.LogError("[NPCDialogueUI] DialogueText가 설정되지 않았습니다!");
            }

            // 이름 설정
            if (nameText != null && !string.IsNullOrEmpty(npcName))
            {
                nameText.text = npcName;
                nameText.gameObject.SetActive(true);
            }
            else if (nameText != null)
            {
                nameText.gameObject.SetActive(false);
            }

            // NPC 이미지 설정
            if (npcImage != null && npcSprite != null)
            {
                npcImage.sprite = npcSprite;
                npcImage.gameObject.SetActive(true);
            }
            else if (npcImage != null)
            {
                npcImage.gameObject.SetActive(false);
            }

            // 스킵 이미지 애니메이션 시작
            StartSkipImageAnimation();

            if (!string.IsNullOrWhiteSpace(text))
            {
                Debug.Log($"[NPCDialogueUI] NPC: {text}");
            }
            else
            {
                Debug.Log("[NPCDialogueUI] NPC 대화창 빈칸으로 표시");
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
            Show("테스트 대사입니다.", "손님");
        }

        [ContextMenu("Test: Hide Dialogue")]
        private void TestHide()
        {
            Hide();
        }
#endif
    }
}
