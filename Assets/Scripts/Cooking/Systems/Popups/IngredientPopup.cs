using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using RecipeAboutLife.Cooking;

/// <summary>
/// 속재료 끼우기 팝업 (Button 클릭 방식)
/// 
/// 동작:
/// - 배경 이미지에 도마 + 소시지통 + 치즈통 포함
/// - 소시지/치즈 통 영역 클릭 → 재료 생성 + 즉시 드래그
/// - 꼬치에 드롭 → 꼬치의 자식으로 배치
/// - 2개 채우면 0.7초 후 자동 닫힘
/// 
/// StickPickupStep과 동일한 패턴
/// </summary>
public class IngredientPopup : PopupBase
{
    [Header("Ingredient Popup Settings")]
    [Tooltip("재료 2개가 끼워진 후 대기 시간")]
    public float autoCloseDelay = 0.7f;
    
    [Header("Ingredient Click Areas")]
    [Tooltip("소시지 클릭 영역 (Button)")]
    public GameObject sausageIcon;
    
    [Tooltip("치즈 클릭 영역 (Button)")]
    public GameObject cheeseIcon;
    
    [Header("Drop Zone")]
    [Tooltip("꼬치 드롭존 (1개, 넓은 영역)")]
    public DropZone stickDropZone;
    
    [Tooltip("꼬치 이미지 (재료의 부모가 됨)")]
    public Transform stickImage;
    
    [Header("Ingredient Placement")]
    [Tooltip("첫 번째 재료 X 위치 (꼬치 기준)")]
    public float firstIngredientPosX = -100f;
    
    [Tooltip("두 번째 재료 X 위치 (꼬치 기준)")]
    public float secondIngredientPosX = 100f;
    
    [Tooltip("재료 Y 위치 (꼬치 기준)")]
    public float ingredientPosY = 0f;
    
    [Tooltip("재료 크기")]
    public float ingredientScale = 1.0f;
    
    // Prefabs
    private GameObject sausagePrefab;
    private GameObject cheesePrefab;
    
    // 이벤트
    public event Action<FillingType, FillingType> OnIngredientsCompleted;
    public event Action<FillingType> OnIngredientAdded;
    
    // 상태
    private int ingredientCount = 0;
    private FillingType[] selectedIngredients = new FillingType[2];
    private List<GameObject> placedIngredients = new List<GameObject>();
    private Coroutine autoCloseCoroutine;
    
    // 현재 드래그 중인 재료
    private GameObject currentDraggingIngredient = null;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 팝업 ID 설정
        popupId = "IngredientPopup";
        
        // 드롭존 이벤트 구독
        if (stickDropZone != null)
        {
            stickDropZone.OnObjectReceived += OnStickDropZoneReceived;
        }
        
        // Prefab 로드
        LoadPrefabs();
    }
    
    protected override void OnPopupOpening()
    {
        base.OnPopupOpening();
        
        // 상태 초기화
        ingredientCount = 0;
        selectedIngredients[0] = FillingType.Sausage;
        selectedIngredients[1] = FillingType.Sausage;
        currentDraggingIngredient = null;
        
        // 이전에 배치된 재료들 삭제
        ClearPlacedIngredients();
        
        // 드롭존 활성화
        if (stickDropZone != null)
        {
            stickDropZone.SetDroppable(true);
        }
        
        // 버튼 클릭 이벤트 설정
        SetupClickHandlers();
        
        Debug.Log("[IngredientPopup] Popup opening - Ready for ingredients");
    }
    
    protected override void OnPopupClosing()
    {
        base.OnPopupClosing();
        
        // 자동 닫기 코루틴 중지
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
        
        // 버튼 클릭 이벤트 해제
        ClearClickHandlers();
        
        // 드래그 중인 재료 정리
        if (currentDraggingIngredient != null)
        {
            Destroy(currentDraggingIngredient);
            currentDraggingIngredient = null;
        }
        
        Debug.Log("[IngredientPopup] Popup closing");
    }
    
    /// <summary>
    /// Prefab 로드
    /// </summary>
    private void LoadPrefabs()
    {
        // Resources 폴더에서 로드
        sausagePrefab = Resources.Load<GameObject>("Prefabs/Ingredients/Sausage");
        cheesePrefab = Resources.Load<GameObject>("Prefabs/Ingredients/Cheese");
        
        if (sausagePrefab == null)
        {
            Debug.LogWarning("[IngredientPopup] Sausage prefab not found! Creating fallback...");
            sausagePrefab = CreateFallbackIngredientPrefab("Sausage");
        }
        
        if (cheesePrefab == null)
        {
            Debug.LogWarning("[IngredientPopup] Cheese prefab not found! Creating fallback...");
            cheesePrefab = CreateFallbackIngredientPrefab("Cheese");
        }
    }
    
    /// <summary>
    /// Fallback Prefab 생성
    /// </summary>
    private GameObject CreateFallbackIngredientPrefab(string ingredientName)
    {
        GameObject prefab = new GameObject(ingredientName);
        
        // RectTransform 추가
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(100, 150);
        
        // Image 추가
        UnityEngine.UI.Image image = prefab.AddComponent<UnityEngine.UI.Image>();
        
        // 색상 (디버그용)
        if (ingredientName == "Sausage")
        {
            image.color = new Color(0.8f, 0.5f, 0.3f); // 갈색
        }
        else
        {
            image.color = new Color(1f, 0.9f, 0.5f); // 노란색
        }
        
        return prefab;
    }
    
    /// <summary>
    /// 버튼 클릭 이벤트 설정
    /// </summary>
    private void SetupClickHandlers()
    {
        // 소시지 버튼
        if (sausageIcon != null)
        {
            Button sausageButton = sausageIcon.GetComponent<Button>();
            if (sausageButton == null)
            {
                sausageButton = sausageIcon.AddComponent<Button>();
            }
            sausageButton.onClick.AddListener(OnSausageIconClicked);
        }
        
        // 치즈 버튼
        if (cheeseIcon != null)
        {
            Button cheeseButton = cheeseIcon.GetComponent<Button>();
            if (cheeseButton == null)
            {
                cheeseButton = cheeseIcon.AddComponent<Button>();
            }
            cheeseButton.onClick.AddListener(OnCheeseIconClicked);
        }
    }
    
    /// <summary>
    /// 버튼 클릭 이벤트 해제
    /// </summary>
    private void ClearClickHandlers()
    {
        if (sausageIcon != null)
        {
            Button sausageButton = sausageIcon.GetComponent<Button>();
            if (sausageButton != null)
            {
                sausageButton.onClick.RemoveListener(OnSausageIconClicked);
            }
        }
        
        if (cheeseIcon != null)
        {
            Button cheeseButton = cheeseIcon.GetComponent<Button>();
            if (cheeseButton != null)
            {
                cheeseButton.onClick.RemoveListener(OnCheeseIconClicked);
            }
        }
    }
    
    /// <summary>
    /// 배치된 재료들 삭제
    /// </summary>
    private void ClearPlacedIngredients()
    {
        foreach (var ingredient in placedIngredients)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }
        
        placedIngredients.Clear();
    }
    
    /// <summary>
    /// 소시지 아이콘 클릭
    /// </summary>
    private void OnSausageIconClicked()
    {
        // 이미 2개 채워졌으면 무시
        if (ingredientCount >= 2)
        {
            Debug.Log("[IngredientPopup] Already full!");
            return;
        }
        
        // 이미 드래그 중이면 무시
        if (currentDraggingIngredient != null)
        {
            Debug.Log("[IngredientPopup] Already dragging!");
            return;
        }
        
        Debug.Log("[IngredientPopup] 🖱️ Sausage icon CLICKED - Creating ingredient...");
        
        // 소시지 생성 및 드래그 시작
        CreateIngredient(FillingType.Sausage, sausagePrefab);
    }
    
    /// <summary>
    /// 치즈 아이콘 클릭
    /// </summary>
    private void OnCheeseIconClicked()
    {
        // 이미 2개 채워졌으면 무시
        if (ingredientCount >= 2)
        {
            Debug.Log("[IngredientPopup] Already full!");
            return;
        }
        
        // 이미 드래그 중이면 무시
        if (currentDraggingIngredient != null)
        {
            Debug.Log("[IngredientPopup] Already dragging!");
            return;
        }
        
        Debug.Log("[IngredientPopup] 🖱️ Cheese icon CLICKED - Creating ingredient...");
        
        // 치즈 생성 및 드래그 시작
        CreateIngredient(FillingType.Cheese, cheesePrefab);
    }
    
    /// <summary>
    /// 재료 생성 및 드래그 시작
    /// </summary>
    private void CreateIngredient(FillingType fillingType, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"[IngredientPopup] Prefab is null for {fillingType}!");
            return;
        }
        
        // 마우스 위치에 생성
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPosition.z = 0;
        
        // Prefab 인스턴스 생성 (Popup의 자식으로)
        currentDraggingIngredient = Instantiate(prefab, transform);
        currentDraggingIngredient.name = $"{fillingType}_Dragging";
        currentDraggingIngredient.transform.position = worldPosition;
        
        // DraggableObject 추가
        DraggableObject draggable = currentDraggingIngredient.GetComponent<DraggableObject>();
        if (draggable == null)
        {
            draggable = currentDraggingIngredient.AddComponent<DraggableObject>();
        }
        
        // DraggableObject 설정
        draggable.isDraggable = true;
        draggable.draggingSortingOrder = 100;
        draggable.returnSpeed = 10f;
        draggable.dragScale = 1.2f;
        draggable.allowedDropZoneTags = new string[] { "StickDropZone" };
        
        // 이벤트 구독
        draggable.OnDropped += (obj, zone) => OnIngredientDropped(obj, zone, fillingType);
        draggable.OnDragCancelled += OnIngredientDragCancelled;
        
        // 즉시 드래그 시작
        draggable.SimulateBeginDrag();
        
        Debug.Log($"[IngredientPopup] ✨ {fillingType} created and drag started!");
    }
    
    /// <summary>
    /// 재료가 드롭됨
    /// </summary>
    private void OnIngredientDropped(DraggableObject obj, DropZone zone, FillingType fillingType)
    {
        // 드롭존이 아닌 곳에 드롭하면 삭제
        if (zone == null || zone != stickDropZone)
        {
            Debug.Log($"[IngredientPopup] Ingredient dropped on wrong zone, destroying");
            Destroy(obj.gameObject);
            currentDraggingIngredient = null;
            return;
        }
        
        // 꼬치에 정상 드롭됨
        Debug.Log($"[IngredientPopup] Ingredient dropped on stick!");
        
        // 드래그 중인 재료 참조 제거
        currentDraggingIngredient = null;
        
        // OnStickDropZoneReceived는 자동 호출되지 않으므로 직접 처리
        PlaceIngredientOnStick(obj, fillingType);
    }
    
    /// <summary>
    /// 재료 드래그 취소됨
    /// </summary>
    private void OnIngredientDragCancelled(DraggableObject obj)
    {
        Debug.Log($"[IngredientPopup] Ingredient drag cancelled, destroying");
        Destroy(obj.gameObject);
        currentDraggingIngredient = null;
    }
    
    /// <summary>
    /// 꼬치 드롭존에 재료가 들어옴 (DropZone 이벤트용)
    /// </summary>
    private void OnStickDropZoneReceived(DraggableObject obj)
    {
        // Button 방식에서는 이 이벤트 사용 안 함
        // OnIngredientDropped에서 직접 처리
    }
    
    /// <summary>
    /// 재료를 꼬치 위에 배치
    /// </summary>
    private void PlaceIngredientOnStick(DraggableObject draggedObject, FillingType fillingType)
    {
        if (stickImage == null)
        {
            Debug.LogError("[IngredientPopup] StickImage is null!");
            Destroy(draggedObject.gameObject);
            return;
        }
        
        // 재료 저장
        selectedIngredients[ingredientCount] = fillingType;
        
        // 위치 계산 (첫 번째는 왼쪽, 두 번째는 오른쪽)
        float posX = ingredientCount == 0 ? firstIngredientPosX : secondIngredientPosX;
        
        // 드래그된 오브젝트를 꼬치의 자식으로 이동
        draggedObject.transform.SetParent(stickImage, false);
        
        // RectTransform 설정
        RectTransform rectTransform = draggedObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(posX, ingredientPosY);
            rectTransform.localScale = Vector3.one * ingredientScale;
        }
        
        // 렌더링 순서 조정 (Sibling Index 사용)
        // UI에서는 Hierarchy 순서가 렌더링 순서를 결정
        // 나중에 있는 자식이 위에 렌더링됨
        draggedObject.transform.SetAsLastSibling();
        
        Debug.Log($"[IngredientPopup] Ingredient rendering order set (SetAsLastSibling)");
        
        
        // DraggableObject 비활성화 (더 이상 드래그 불가)
        DraggableObject draggable = draggedObject.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            draggable.SetDraggable(false);
        }
        
        // 배치된 재료 목록에 추가
        placedIngredients.Add(draggedObject.gameObject);
        
        // 재료 개수 증가
        ingredientCount++;
        
        Debug.Log($"[IngredientPopup] Ingredient placed on stick: {fillingType} at position ({posX}, {ingredientPosY}), Total: {ingredientCount}/2");
        
        // 이벤트 발생
        OnIngredientAdded?.Invoke(fillingType);
        
        // 2개가 채워지면 자동 완료
        if (ingredientCount >= 2)
        {
            OnIngredientsComplete();
        }
    }
    
    /// <summary>
    /// 재료 2개 완성
    /// </summary>
    private void OnIngredientsComplete()
    {
        Debug.Log($"[IngredientPopup] Ingredients complete: {selectedIngredients[0]}, {selectedIngredients[1]}");
        
        // 버튼 비활성화
        if (sausageIcon != null)
        {
            Button sausageButton = sausageIcon.GetComponent<Button>();
            if (sausageButton != null) sausageButton.interactable = false;
        }
        
        if (cheeseIcon != null)
        {
            Button cheeseButton = cheeseIcon.GetComponent<Button>();
            if (cheeseButton != null) cheeseButton.interactable = false;
        }
        
        // 드롭존 비활성화
        if (stickDropZone != null)
        {
            stickDropZone.SetDroppable(false);
        }
        
        // 이벤트 발생
        OnIngredientsCompleted?.Invoke(selectedIngredients[0], selectedIngredients[1]);
        
        // 자동 닫기
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
        autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
    }
    
    /// <summary>
    /// 지연 후 자동 닫기
    /// </summary>
    private IEnumerator AutoCloseAfterDelay()
    {
        Debug.Log($"[IngredientPopup] Auto-closing in {autoCloseDelay} seconds...");
        
        yield return new WaitForSeconds(autoCloseDelay);
        
        Close();
    }
    
    /// <summary>
    /// 선택된 재료 가져오기
    /// </summary>
    public FillingType[] GetSelectedIngredients()
    {
        return selectedIngredients;
    }
    
    /// <summary>
    /// 재료 개수 가져오기
    /// </summary>
    public int GetIngredientCount()
    {
        return ingredientCount;
    }
    
    /// <summary>
    /// 완성 여부
    /// </summary>
    public bool IsComplete()
    {
        return ingredientCount >= 2;
    }
}