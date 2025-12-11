using UnityEngine;
using UnityEngine.UI;
using System;
using RecipeAboutLife.Cooking;    // ⭐ 추가!

/// <summary>
/// 토핑/소스 팝업
/// 5단계에서 사용
/// </summary>
public class ToppingPopup : PopupBase
{
    [Header("Topping Popup Settings")]
    [Tooltip("완료 버튼")]
    public Button completeButton;
    
    [Header("Sugar System")]
    [Tooltip("설탕 트레이")]
    public GameObject sugarTray;
    
    [Tooltip("설탕 트레이 DropZone")]
    public DropZone sugarDropZone;
    
    [Tooltip("핫도그 (설탕 입히기 대상)")]
    public GameObject hotdogObject;
    
    [Tooltip("설탕 입힌 효과 이미지")]
    public GameObject sugarCoatedEffect;
    
    [Header("Sauce System")]
    [Tooltip("케첩 병")]
    public GameObject ketchupBottle;
    
    [Tooltip("케첩 병 외곽선")]
    public GameObject ketchupOutline;
    
    [Tooltip("머스타드 병")]
    public GameObject mustardBottle;
    
    [Tooltip("머스타드 병 외곽선")]
    public GameObject mustardOutline;
    
    [Header("Sauce Gauges")]
    [Tooltip("케첩 게이지 배경")]
    public Image ketchupGaugeBackground;
    
    [Tooltip("케첩 게이지 채움")]
    public Image ketchupGaugeFill;
    
    [Tooltip("머스타드 게이지 배경")]
    public Image mustardGaugeBackground;
    
    [Tooltip("머스타드 게이지 채움")]
    public Image mustardGaugeFill;
    
    [Header("Audio")]
    [Tooltip("설탕 입히기 사운드")]
    public AudioClip sugarApplySound;
    
    [Tooltip("설탕 거부 사운드")]
    public AudioClip denySound;
    
    [Tooltip("소스 병 선택 사운드")]
    public AudioClip bottleSelectSound;
    
    [Tooltip("완료 사운드")]
    public AudioClip completeSound;
    
    // 이벤트
    public event Action OnSugarApplied;
    public event Action<SauceType> OnSauceSelected;
    public event Action<SauceType, float> OnSauceAmountChanged;
    public event Action<bool, float, float> OnToppingCompleted; // hasSugar, ketchupAmount, mustardAmount
    
    // 상태
    private bool hasSugar = false;
    private bool sauceApplied = false; // 소스가 하나라도 뿌려졌는지
    private SauceType? selectedSauce = null;
    
    // 소스 양 (0-100%)
    private float ketchupAmount = 0f;
    private float mustardAmount = 0f;
    
    // 드래그 가능한 핫도그
    private DraggableObject hotdogDraggable;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 팝업 ID 설정
        popupId = "ToppingPopup";
        
        // 완료 버튼 이벤트
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(OnCompleteButtonClicked);
        }
        
        // 소스 병 클릭 이벤트 설정
        SetupBottleClickEvents();
        
        // 설탕 드롭존 이벤트
        if (sugarDropZone != null)
        {
            sugarDropZone.OnObjectReceived += OnSugarDropZoneReceived;
        }
    }
    
    protected override void OnPopupOpening()
    {
        base.OnPopupOpening();
        
        // 상태 초기화
        hasSugar = false;
        sauceApplied = false;
        selectedSauce = null;
        ketchupAmount = 0f;
        mustardAmount = 0f;
        
        // UI 초기화
        UpdateSugarUI();
        UpdateSauceSelection();
        UpdateSauceGauges();
        
        // 핫도그 드래그 가능하게
        SetupHotdogDraggable();
        
        // 설탕 트레이 활성화
        if (sugarDropZone != null)
        {
            sugarDropZone.SetDroppable(true);
        }
        
        Debug.Log("[ToppingPopup] Popup opening - Ready for toppings");
    }
    
    protected override void OnPopupClosing()
    {
        base.OnPopupClosing();
        
        // 핫도그 드래그 정리
        if (hotdogDraggable != null)
        {
            hotdogDraggable.OnDropped -= OnHotdogDropped;
            hotdogDraggable.OnDragCancelled -= OnHotdogDragCancelled;
        }
        
        Debug.Log("[ToppingPopup] Popup closing");
    }
    
    /// <summary>
    /// 소스 병 클릭 이벤트 설정
    /// </summary>
    private void SetupBottleClickEvents()
    {
        // 케첩 병
        if (ketchupBottle != null)
        {
            Button ketchupButton = ketchupBottle.GetComponent<Button>();
            if (ketchupButton == null)
            {
                ketchupButton = ketchupBottle.AddComponent<Button>();
            }
            ketchupButton.onClick.AddListener(() => SelectSauceBottle(SauceType.Ketchup));
        }
        
        // 머스타드 병
        if (mustardBottle != null)
        {
            Button mustardButton = mustardBottle.GetComponent<Button>();
            if (mustardButton == null)
            {
                mustardButton = mustardBottle.AddComponent<Button>();
            }
            mustardButton.onClick.AddListener(() => SelectSauceBottle(SauceType.Mustard));
        }
    }
    
    /// <summary>
    /// 핫도그 드래그 설정
    /// </summary>
    private void SetupHotdogDraggable()
    {
        if (hotdogObject == null) return;
        
        hotdogDraggable = hotdogObject.GetComponent<DraggableObject>();
        if (hotdogDraggable == null)
        {
            hotdogDraggable = hotdogObject.AddComponent<DraggableObject>();
        }
        
        hotdogDraggable.SetDraggable(true);
        hotdogDraggable.allowedDropZoneTags = new string[] { "SugarTray" };
        hotdogDraggable.OnDropped += OnHotdogDropped;
        hotdogDraggable.OnDragCancelled += OnHotdogDragCancelled;
    }
    
    /// <summary>
    /// 핫도그가 드롭됨
    /// </summary>
    private void OnHotdogDropped(DraggableObject obj, DropZone zone)
    {
        // 설탕 트레이가 아니면 원상복귀
        if (zone == null || zone != sugarDropZone)
        {
            Debug.Log("[ToppingPopup] Hotdog dropped on wrong zone, returning");
            obj.ReturnToOriginalPosition();
        }
    }
    
    /// <summary>
    /// 핫도그 드래그 취소
    /// </summary>
    private void OnHotdogDragCancelled(DraggableObject obj)
    {
        Debug.Log("[ToppingPopup] Hotdog drag cancelled");
    }
    
    /// <summary>
    /// 설탕 드롭존에 핫도그가 들어옴
    /// </summary>
    private void OnSugarDropZoneReceived(DraggableObject obj)
    {
        // 이미 소스가 뿌려졌으면 설탕 불가능
        if (sauceApplied)
        {
            Debug.Log("[ToppingPopup] Cannot apply sugar after sauce!");
            
            // 거부 애니메이션 (위아래 흔들림)
            StartCoroutine(ShakeAnimation(sugarTray));
            
            // 거부 사운드
            PlayDenySound();
            
            // 핫도그 원상복귀
            obj.ReturnToOriginalPosition();
            
            return;
        }
        
        // 설탕 입히기 성공
        ApplySugar();
        
        // 핫도그 원상복귀
        obj.ReturnToOriginalPosition();
    }
    
    /// <summary>
    /// 설탕 입히기
    /// </summary>
    private void ApplySugar()
    {
        if (hasSugar) return;
        
        hasSugar = true;
        
        // UI 업데이트
        UpdateSugarUI();
        
        // 사운드 재생
        PlaySugarSound();
        
        // 이벤트 발생
        OnSugarApplied?.Invoke();
        
        Debug.Log("[ToppingPopup] Sugar applied!");
    }
    
    /// <summary>
    /// 소스 병 선택
    /// </summary>
    private void SelectSauceBottle(SauceType sauceType)
    {
        // 이미 선택된 병이면 선택 해제
        if (selectedSauce == sauceType)
        {
            selectedSauce = null;
        }
        else
        {
            selectedSauce = sauceType;
            sauceApplied = true; // 소스 선택 시점에 sauceApplied = true
        }
        
        // UI 업데이트
        UpdateSauceSelection();
        
        // 사운드 재생
        PlayBottleSelectSound();
        
        // 이벤트 발생
        OnSauceSelected?.Invoke(sauceType);
        
        Debug.Log($"[ToppingPopup] Sauce bottle selected: {sauceType}");
    }
    
    /// <summary>
    /// 소스 양 변경 (SpriteTrailSauceSystem에서 호출)
    /// </summary>
    public void SetSauceAmount(SauceType sauceType, float amount)
    {
        amount = Mathf.Clamp01(amount);
        
        if (sauceType == SauceType.Ketchup)
        {
            ketchupAmount = amount;
        }
        else if (sauceType == SauceType.Mustard)
        {
            mustardAmount = amount;
        }
        
        // UI 업데이트
        UpdateSauceGauges();
        
        // 이벤트 발생
        OnSauceAmountChanged?.Invoke(sauceType, amount);
    }
    
    /// <summary>
    /// 완료 버튼 클릭
    /// </summary>
    private void OnCompleteButtonClicked()
    {
        Debug.Log($"[ToppingPopup] Complete! Sugar: {hasSugar}, Ketchup: {ketchupAmount:F2}, Mustard: {mustardAmount:F2}");
        
        // 사운드 재생
        PlayCompleteSound();
        
        // 이벤트 발생
        OnToppingCompleted?.Invoke(hasSugar, ketchupAmount, mustardAmount);
        
        // 팝업 닫기
        Close();
    }
    
    /// <summary>
    /// 설탕 UI 업데이트
    /// </summary>
    private void UpdateSugarUI()
    {
        if (sugarCoatedEffect != null)
        {
            sugarCoatedEffect.SetActive(hasSugar);
        }
    }
    
    /// <summary>
    /// 소스 선택 UI 업데이트
    /// </summary>
    private void UpdateSauceSelection()
    {
        // 외곽선 표시/숨김
        if (ketchupOutline != null)
        {
            ketchupOutline.SetActive(selectedSauce == SauceType.Ketchup);
        }
        
        if (mustardOutline != null)
        {
            mustardOutline.SetActive(selectedSauce == SauceType.Mustard);
        }
        
        // 게이지 표시/숨김
        if (ketchupGaugeBackground != null)
        {
            ketchupGaugeBackground.gameObject.SetActive(selectedSauce == SauceType.Ketchup);
        }
        
        if (mustardGaugeBackground != null)
        {
            mustardGaugeBackground.gameObject.SetActive(selectedSauce == SauceType.Mustard);
        }
    }
    
    /// <summary>
    /// 소스 게이지 UI 업데이트
    /// </summary>
    private void UpdateSauceGauges()
    {
        // 케첩 게이지
        if (ketchupGaugeFill != null)
        {
            ketchupGaugeFill.fillAmount = ketchupAmount;
        }
        
        // 머스타드 게이지
        if (mustardGaugeFill != null)
        {
            mustardGaugeFill.fillAmount = mustardAmount;
        }
    }
    
    /// <summary>
    /// 흔들림 애니메이션
    /// </summary>
    private System.Collections.IEnumerator ShakeAnimation(GameObject target)
    {
        if (target == null) yield break;
        
        Vector3 originalPosition = target.transform.localPosition;
        float duration = 0.3f;
        float magnitude = 10f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = originalPosition.x + Mathf.Sin(elapsed * 50f) * magnitude * (1f - elapsed / duration);
            target.transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        target.transform.localPosition = originalPosition;
    }
    
    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySugarSound()
    {
        if (sugarApplySound != null)
        {
            // AudioSource에서 재생 (PopupBase의 PlaySound 메서드 활용 가능)
            AudioSource.PlayClipAtPoint(sugarApplySound, Camera.main.transform.position);
        }
    }
    
    private void PlayDenySound()
    {
        if (denySound != null)
        {
            AudioSource.PlayClipAtPoint(denySound, Camera.main.transform.position);
        }
    }
    
    private void PlayBottleSelectSound()
    {
        if (bottleSelectSound != null)
        {
            AudioSource.PlayClipAtPoint(bottleSelectSound, Camera.main.transform.position);
        }
    }
    
    private void PlayCompleteSound()
    {
        if (completeSound != null)
        {
            AudioSource.PlayClipAtPoint(completeSound, Camera.main.transform.position);
        }
    }
    
    /// <summary>
    /// 현재 선택된 소스
    /// </summary>
    public SauceType? GetSelectedSauce()
    {
        return selectedSauce;
    }
    
    /// <summary>
    /// 설탕 여부
    /// </summary>
    public bool HasSugar()
    {
        return hasSugar;
    }
    
    /// <summary>
    /// 소스 양 가져오기
    /// </summary>
    public float GetSauceAmount(SauceType sauceType)
    {
        return sauceType == SauceType.Ketchup ? ketchupAmount : mustardAmount;
    }
}