using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RecipeAboutLife.Cooking;    // ⭐ 추가!

/// <summary>
/// 속재료 끼우기 팝업
/// 2단계에서 사용
/// </summary>
public class IngredientPopup : PopupBase
{
    [Header("Ingredient Popup Settings")]
    [Tooltip("재료 2개가 끼워진 후 대기 시간")]
    public float autoCloseDelay = 0.7f;
    
    [Header("Ingredient References")]
    [Tooltip("소시지 아이콘")]
    public GameObject sausageIcon;
    
    [Tooltip("치즈 아이콘")]
    public GameObject cheeseIcon;
    
    [Tooltip("재료 슬롯 1 (상단)")]
    public DropZone ingredientSlot1;
    
    [Tooltip("재료 슬롯 2 (하단)")]
    public DropZone ingredientSlot2;
    
    [Tooltip("도마 이미지")]
    public GameObject cuttingBoardImage;
    
    [Tooltip("꼬치 이미지")]
    public GameObject stickImage;
    
    // 이벤트
    public event Action<FillingType, FillingType> OnIngredientsCompleted;
    public event Action<FillingType> OnIngredientAdded;
    
    // 상태
    private int ingredientCount = 0;
    private FillingType[] selectedIngredients = new FillingType[2];
    private Coroutine autoCloseCoroutine;
    
    // 드래그 가능한 재료들
    private List<DraggableObject> ingredientObjects = new List<DraggableObject>();
    
    protected override void Awake()
    {
        base.Awake();
        
        // 팝업 ID 설정
        popupId = "IngredientPopup";
        
        // 슬롯 이벤트 구독
        if (ingredientSlot1 != null)
        {
            ingredientSlot1.OnObjectReceived += OnSlot1Received;
        }
        
        if (ingredientSlot2 != null)
        {
            ingredientSlot2.OnObjectReceived += OnSlot2Received;
        }
    }
    
    protected override void OnPopupOpening()
    {
        base.OnPopupOpening();
        
        // 상태 초기화
        ingredientCount = 0;
        selectedIngredients[0] = FillingType.Sausage;  // None 대신 Sausage 사용
        selectedIngredients[1] = FillingType.Sausage;
        
        // 슬롯 활성화
        if (ingredientSlot1 != null)
        {
            ingredientSlot1.SetDroppable(true);
        }
        
        if (ingredientSlot2 != null)
        {
            ingredientSlot2.SetDroppable(true);
        }
        
        // 드래그 가능한 재료 생성
        CreateIngredientObjects();
        
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
        
        // 드래그 가능한 재료 정리
        ClearIngredientObjects();
        
        Debug.Log("[IngredientPopup] Popup closing");
    }
    
    /// <summary>
    /// 드래그 가능한 재료 생성
    /// </summary>
    private void CreateIngredientObjects()
    {
        // 소시지
        if (sausageIcon != null)
        {
            DraggableObject sausageDrag = sausageIcon.GetComponent<DraggableObject>();
            if (sausageDrag == null)
            {
                sausageDrag = sausageIcon.AddComponent<DraggableObject>();
            }
            
            sausageDrag.SetDraggable(true);
            sausageDrag.allowedDropZoneTags = new string[] { "IngredientSlot" };
            sausageDrag.OnDropped += OnIngredientDropped;
            sausageDrag.OnDragCancelled += OnIngredientDragCancelled;
            
            ingredientObjects.Add(sausageDrag);
        }
        
        // 치즈
        if (cheeseIcon != null)
        {
            DraggableObject cheeseDrag = cheeseIcon.GetComponent<DraggableObject>();
            if (cheeseDrag == null)
            {
                cheeseDrag = cheeseIcon.AddComponent<DraggableObject>();
            }
            
            cheeseDrag.SetDraggable(true);
            cheeseDrag.allowedDropZoneTags = new string[] { "IngredientSlot" };
            cheeseDrag.OnDropped += OnIngredientDropped;
            cheeseDrag.OnDragCancelled += OnIngredientDragCancelled;
            
            ingredientObjects.Add(cheeseDrag);
        }
    }
    
    /// <summary>
    /// 드래그 가능한 재료 정리
    /// </summary>
    private void ClearIngredientObjects()
    {
        foreach (var obj in ingredientObjects)
        {
            if (obj != null)
            {
                obj.OnDropped -= OnIngredientDropped;
                obj.OnDragCancelled -= OnIngredientDragCancelled;
            }
        }
        
        ingredientObjects.Clear();
    }
    
    /// <summary>
    /// 재료가 드롭됨
    /// </summary>
    private void OnIngredientDropped(DraggableObject obj, DropZone zone)
    {
        // 슬롯이 아닌 곳에 드롭하면 재료 삭제
        if (zone == null || (!zone.Equals(ingredientSlot1) && !zone.Equals(ingredientSlot2)))
        {
            Debug.Log($"[IngredientPopup] Ingredient dropped on wrong zone, destroying: {obj.gameObject.name}");
            obj.DestroyObject();
        }
    }
    
    /// <summary>
    /// 재료 드래그 취소됨 (원상복귀)
    /// </summary>
    private void OnIngredientDragCancelled(DraggableObject obj)
    {
        Debug.Log($"[IngredientPopup] Ingredient drag cancelled: {obj.gameObject.name}");
    }
    
    /// <summary>
    /// 슬롯 1에 재료가 들어옴
    /// </summary>
    private void OnSlot1Received(DraggableObject obj)
    {
        AddIngredient(obj, 0);
    }
    
    /// <summary>
    /// 슬롯 2에 재료가 들어옴
    /// </summary>
    private void OnSlot2Received(DraggableObject obj)
    {
        AddIngredient(obj, 1);
    }
    
    /// <summary>
    /// 재료 추가
    /// </summary>
    private void AddIngredient(DraggableObject obj, int slotIndex)
    {
        if (obj == null) return;
        
        // 재료 타입 판별
        FillingType fillingType = FillingType.Sausage;  // 기본값
        bool isValidIngredient = false;
        
        if (obj.gameObject == sausageIcon || obj.CompareTag("Sausage"))
        {
            fillingType = FillingType.Sausage;
            isValidIngredient = true;
        }
        else if (obj.gameObject == cheeseIcon || obj.CompareTag("Cheese"))
        {
            fillingType = FillingType.Cheese;
            isValidIngredient = true;
        }
        
        if (!isValidIngredient)
        {
            Debug.LogWarning($"[IngredientPopup] Unknown ingredient type: {obj.gameObject.name}");
            return;
        }
        
        // 재료 저장
        selectedIngredients[slotIndex] = fillingType;
        ingredientCount++;
        
        Debug.Log($"[IngredientPopup] Ingredient added to slot {slotIndex + 1}: {fillingType}, Total: {ingredientCount}");
        
        // 이벤트 발생
        OnIngredientAdded?.Invoke(fillingType);
        
        // 재료가 2개 채워지면 자동 닫기
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
        
        // 더 이상 드래그 불가능
        foreach (var obj in ingredientObjects)
        {
            if (obj != null)
            {
                obj.SetDraggable(false);
            }
        }
        
        // 슬롯 비활성화
        if (ingredientSlot1 != null)
        {
            ingredientSlot1.SetDroppable(false);
        }
        
        if (ingredientSlot2 != null)
        {
            ingredientSlot2.SetDroppable(false);
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