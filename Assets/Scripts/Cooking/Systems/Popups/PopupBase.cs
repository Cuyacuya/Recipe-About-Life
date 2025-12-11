using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

/// <summary>
/// 모든 팝업의 기본 클래스
/// IngredientPopup, ToppingPopup 등이 이 클래스를 상속받음
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class PopupBase : MonoBehaviour
{
    [Header("Popup Settings")]
    [Tooltip("팝업 ID (고유 식별자)")]
    public string popupId = "Popup";
    
    [Tooltip("팝업이 열릴 때 메인 화면 비활성화")]
    public bool blockBackground = true;
    
    [Tooltip("ESC 키로 닫기 가능 여부")]
    public bool closeOnEscape = false;
    
    [Header("Animation Settings")]
    [Tooltip("애니메이션 사용 여부")]
    public bool useAnimation = true;
    
    [Tooltip("애니메이션 타입")]
    public PopupAnimationType animationType = PopupAnimationType.Fade;
    
    [Tooltip("애니메이션 지속 시간")]
    public float animationDuration = 0.3f;
    
    [Tooltip("열기 애니메이션 Ease 타입")]
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("닫기 애니메이션 Ease 타입")]
    public AnimationCurve closeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Audio")]
    [Tooltip("열기 사운드")]
    public AudioClip openSound;
    
    [Tooltip("닫기 사운드")]
    public AudioClip closeSound;
    
    // 이벤트
    public event Action<PopupBase> OnPopupOpened;
    public event Action<PopupBase> OnPopupClosed;
    public event Action<PopupBase> OnPopupFullyOpened;
    public event Action<PopupBase> OnPopupFullyClosed;
    
    // 컴포넌트
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;
    protected Canvas canvas;
    
    // 상태
    private bool isOpen = false;
    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    
    // 초기 상태
    private Vector3 initialScale;
    private float initialAlpha;
    
    public bool IsOpen => isOpen;
    public bool IsAnimating => isAnimating;
    
    protected virtual void Awake()
    {
        // 컴포넌트 가져오기
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        // 초기 상태 저장
        initialScale = rectTransform.localScale;
        initialAlpha = canvasGroup.alpha;
        
        // 초기에는 비활성화
        gameObject.SetActive(false);
    }
    
    protected virtual void Update()
    {
        // ESC 키로 닫기
        if (isOpen && !isAnimating && closeOnEscape)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    }
    
    /// <summary>
    /// 팝업 열기
    /// </summary>
    public virtual void Open()
    {
        if (isOpen || isAnimating) return;
        
        // 활성화
        gameObject.SetActive(true);
        isOpen = true;
        
        // PopupManager에 등록
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.RegisterPopup(this);
        }
        
        // 초기화
        OnPopupOpening();
        
        // 애니메이션
        if (useAnimation)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(OpenAnimation());
        }
        else
        {
            // 애니메이션 없이 즉시 열기
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            OnPopupFullyOpened?.Invoke(this);
        }
        
        // 사운드 재생
        PlaySound(openSound);
        
        // 이벤트 발생
        OnPopupOpened?.Invoke(this);
        
        Debug.Log($"[PopupBase] Opened: {popupId}");
    }
    
    /// <summary>
    /// 팝업 닫기
    /// </summary>
    public virtual void Close()
    {
        if (!isOpen || isAnimating) return;
        
        isOpen = false;
        
        // 정리
        OnPopupClosing();
        
        // 애니메이션
        if (useAnimation)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(CloseAnimation());
        }
        else
        {
            // 애니메이션 없이 즉시 닫기
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnPopupFullyClosed?.Invoke(this);
        }
        
        // 사운드 재생
        PlaySound(closeSound);
        
        // 이벤트 발생
        OnPopupClosed?.Invoke(this);
        
        // PopupManager에서 제거
        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.UnregisterPopup(this);
        }
        
        Debug.Log($"[PopupBase] Closed: {popupId}");
    }
    
    /// <summary>
    /// 열기 애니메이션
    /// </summary>
    private IEnumerator OpenAnimation()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        
        // 초기 상태 설정
        switch (animationType)
        {
            case PopupAnimationType.Fade:
                canvasGroup.alpha = 0f;
                break;
            case PopupAnimationType.Scale:
                rectTransform.localScale = Vector3.zero;
                canvasGroup.alpha = 1f;
                break;
            case PopupAnimationType.FadeAndScale:
                canvasGroup.alpha = 0f;
                rectTransform.localScale = Vector3.zero;
                break;
        }
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;
        
        // 애니메이션 실행
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = openCurve.Evaluate(t);
            
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    canvasGroup.alpha = curveValue;
                    break;
                case PopupAnimationType.Scale:
                    rectTransform.localScale = initialScale * curveValue;
                    break;
                case PopupAnimationType.FadeAndScale:
                    canvasGroup.alpha = curveValue;
                    rectTransform.localScale = initialScale * curveValue;
                    break;
            }
            
            yield return null;
        }
        
        // 최종 상태
        canvasGroup.alpha = 1f;
        rectTransform.localScale = initialScale;
        canvasGroup.interactable = true;
        
        isAnimating = false;
        OnPopupFullyOpened?.Invoke(this);
    }
    
    /// <summary>
    /// 닫기 애니메이션
    /// </summary>
    private IEnumerator CloseAnimation()
    {
        isAnimating = true;
        canvasGroup.interactable = false;
        
        float elapsed = 0f;
        
        // 애니메이션 실행
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float curveValue = closeCurve.Evaluate(t);
            
            switch (animationType)
            {
                case PopupAnimationType.Fade:
                    canvasGroup.alpha = curveValue;
                    break;
                case PopupAnimationType.Scale:
                    rectTransform.localScale = initialScale * curveValue;
                    break;
                case PopupAnimationType.FadeAndScale:
                    canvasGroup.alpha = curveValue;
                    rectTransform.localScale = initialScale * curveValue;
                    break;
            }
            
            yield return null;
        }
        
        // 최종 상태
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        
        isAnimating = false;
        OnPopupFullyClosed?.Invoke(this);
    }
    
    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        // AudioSource 찾기 또는 생성
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null && canvas != null)
        {
            audioSource = canvas.GetComponent<AudioSource>();
        }
        
        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    /// <summary>
    /// 팝업이 열리기 시작할 때 (오버라이드 가능)
    /// </summary>
    protected virtual void OnPopupOpening()
    {
        // 자식 클래스에서 구현
    }
    
    /// <summary>
    /// 팝업이 닫히기 시작할 때 (오버라이드 가능)
    /// </summary>
    protected virtual void OnPopupClosing()
    {
        // 자식 클래스에서 구현
    }
    
    /// <summary>
    /// 즉시 열기 (애니메이션 없이)
    /// </summary>
    public void OpenImmediate()
    {
        bool originalUseAnimation = useAnimation;
        useAnimation = false;
        Open();
        useAnimation = originalUseAnimation;
    }
    
    /// <summary>
    /// 즉시 닫기 (애니메이션 없이)
    /// </summary>
    public void CloseImmediate()
    {
        bool originalUseAnimation = useAnimation;
        useAnimation = false;
        Close();
        useAnimation = originalUseAnimation;
    }
}

/// <summary>
/// 팝업 애니메이션 타입
/// </summary>
public enum PopupAnimationType
{
    None,           // 애니메이션 없음
    Fade,           // 페이드 인/아웃
    Scale,          // 스케일
    FadeAndScale    // 페이드 + 스케일
}