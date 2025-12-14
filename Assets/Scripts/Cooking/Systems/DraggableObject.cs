using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 드래그 가능한 오브젝트 컴포넌트
/// 꼬치, 핫도그, 재료 등 드래그할 수 있는 모든 오브젝트에 부착
/// ⭐ Phase 2.2: 즉시 드래그 시작 기능 추가
/// </summary>
public class DraggableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
    [Header("Settings")]
    [Tooltip("드래그 가능 여부")]
    public bool isDraggable = true;
    
    [Tooltip("드래그 중 레이어 순서 (높을수록 앞에 표시)")]
    public int draggingSortingOrder = 100;
    
    [Tooltip("원상복귀 애니메이션 속도")]
    public float returnSpeed = 10f;
    
    [Tooltip("드래그 중 스케일")]
    public float dragScale = 1.1f;
    
    [Header("Drop Validation")]
    [Tooltip("허용된 드롭존 태그들 (비어있으면 모든 드롭존 허용)")]
    public string[] allowedDropZoneTags;
    
    // 이벤트
    public event Action<DraggableObject> OnDragStarted;
    public event Action<DraggableObject, Vector2> OnDragging;
    public event Action<DraggableObject, DropZone> OnDropped;
    public event Action<DraggableObject> OnDragCancelled;
    
    // 컴포넌트
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private SpriteRenderer spriteRenderer;
    
    // 드래그 상태
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSortingOrder;
    private Transform originalParent;
    private bool isDragging = false;
    private bool isReturning = false;
    
    // 드롭 검증
    private DropZone currentDropZone = null;
    
    private void Awake()
    {
        // 컴포넌트 가져오기
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // CanvasGroup이 없으면 추가 (UI용)
        if (rectTransform != null && canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Canvas 찾기 (UI용)
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
    }
    
    /// <summary>
    /// 터치/클릭 시작
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        // 원래 상태 저장
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalParent = transform.parent;
        
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
    }
    
    /// <summary>
    /// 드래그 시작
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isDraggable) return;
        
        isDragging = true;
        isReturning = false;
        
        // 드래그 중 설정
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = draggingSortingOrder;
        }
        
        // 스케일 변경
        transform.localScale = originalScale * dragScale;
        
        // 이벤트 발생
        OnDragStarted?.Invoke(this);
        
        // DragDropManager에 알림
        if (DragDropManager.Instance != null)
        {
            DragDropManager.Instance.OnDragStart(this);
        }
        
        Debug.Log($"[DraggableObject] Drag started: {gameObject.name}");
    }
    
    /// <summary>
    /// 드래그 중
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable || !isDragging) return;
        
        // 위치 업데이트
        Vector2 position;
        if (rectTransform != null && canvas != null)
        {
            // UI 좌표계
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out position
            );
            transform.position = canvas.transform.TransformPoint(position);
        }
        else
        {
            // 월드 좌표계
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPosition.z = transform.position.z;
            transform.position = worldPosition;
            
            // Vector2로 변환 (이벤트용)
            position = worldPosition;
        }
        
        // 이벤트 발생
        OnDragging?.Invoke(this, position);
        
        // 드롭존 체크
        CheckDropZone(position);
    }
    
    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable || !isDragging) return;
        
        isDragging = false;
        
        // 드래그 중 설정 복원
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = originalSortingOrder;
        }
        
        // 스케일 복원
        transform.localScale = originalScale;
        
        // 드롭존 체크
        if (currentDropZone != null && IsValidDropZone(currentDropZone))
        {
            // 드롭 성공
            OnDropSuccess(currentDropZone);
        }
        else
        {
            // 드롭 실패 - 원상복귀
            ReturnToOriginalPosition();
        }
        
        // DragDropManager에 알림
        if (DragDropManager.Instance != null)
        {
            DragDropManager.Instance.OnDragEnd(this, currentDropZone);
        }
    }
    
    /// <summary>
    /// 드롭존 체크
    /// </summary>
    private void CheckDropZone(Vector2 position)
    {
        // Raycast로 드롭존 찾기
        DropZone foundDropZone = null;
        
        if (rectTransform != null)
        {
            // UI Raycast
            var results = new System.Collections.Generic.List<RaycastResult>();
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = position;
            EventSystem.current.RaycastAll(eventData, results);
            
            foreach (var result in results)
            {
                var dropZone = result.gameObject.GetComponent<DropZone>();
                if (dropZone != null)
                {
                    foundDropZone = dropZone;
                    break;
                }
            }
        }
        else
        {
             // 2D Physics Overlap (point-based)
            Collider2D hitCollider = Physics2D.OverlapPoint(position);
            if (hitCollider != null)
            {
               foundDropZone = hitCollider.GetComponent<DropZone>();
            }
        }
        
        // 드롭존 변경 감지
        if (foundDropZone != currentDropZone)
        {
            // 이전 드롭존 하이라이트 해제
            if (currentDropZone != null)
            {
                currentDropZone.OnDragExit(this);
            }
            
            // 새 드롭존 하이라이트
            currentDropZone = foundDropZone;
            if (currentDropZone != null)
            {
                currentDropZone.OnDragEnter(this);
            }
        }
    }
    
    /// <summary>
    /// 드롭존 유효성 검사
    /// </summary>
    private bool IsValidDropZone(DropZone dropZone)
    {
        if (dropZone == null || !dropZone.isDroppable) return false;
        
        // 태그 체크 (allowedDropZoneTags가 비어있으면 모든 드롭존 허용)
        if (allowedDropZoneTags != null && allowedDropZoneTags.Length > 0)
        {
            bool tagMatched = false;
            foreach (var tag in allowedDropZoneTags)
            {
                if (dropZone.CompareTag(tag))
                {
                    tagMatched = true;
                    break;
                }
            }
            return tagMatched;
        }
        
        return true;
    }
    
    /// <summary>
    /// 드롭 성공 처리
    /// </summary>
    private void OnDropSuccess(DropZone dropZone)
    {
        Debug.Log($"[DraggableObject] Dropped on: {dropZone.gameObject.name}");
        
        // 드롭존의 위치로 이동 (선택사항)
        if (dropZone.snapToCenter)
        {
            transform.position = dropZone.transform.position;
        }
        
        // 이벤트 발생
        OnDropped?.Invoke(this, dropZone);
        dropZone.OnObjectDropped(this);
    }
    
    /// <summary>
    /// 원상복귀
    /// </summary>
    public void ReturnToOriginalPosition()
    {
        Debug.Log($"[DraggableObject] Returning to original position: {gameObject.name}");
        
        isReturning = true;
        
        // 이벤트 발생
        OnDragCancelled?.Invoke(this);
        
        // 드롭존 하이라이트 해제
        if (currentDropZone != null)
        {
            currentDropZone.OnDragExit(this);
            currentDropZone = null;
        }
    }
    
    private void Update()
    {
        // 원상복귀 애니메이션
        if (isReturning)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                originalPosition,
                Time.deltaTime * returnSpeed
            );
            
            // 목표 위치에 거의 도달하면 완료
            if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
            {
                transform.position = originalPosition;
                isReturning = false;
            }
        }
        
        // ⭐ 드래그 중이면 마우스 위치를 계속 따라가기
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = transform.position.z;
            transform.position = worldPosition;
            
            // 드롭존 체크
            CheckDropZone(worldPosition);
        }
    }
    
    /// <summary>
    /// 드래그 가능 여부 설정
    /// </summary>
    public void SetDraggable(bool draggable)
    {
        isDraggable = draggable;
    }
    
    /// <summary>
    /// 오브젝트 삭제 (실패 시)
    /// </summary>
    public void DestroyObject()
    {
        Debug.Log($"[DraggableObject] Destroying: {gameObject.name}");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 프로그래밍 방식으로 드래그 시작 (외부에서 호출)
    /// ⭐ 꼬치통 클릭 시 즉시 드래그 시작용
    /// </summary>
    public void SimulateBeginDrag()
    {
        if (!isDraggable) return;
        
        // 원래 상태 저장
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalParent = transform.parent;
        
        if (spriteRenderer != null)
        {
            originalSortingOrder = spriteRenderer.sortingOrder;
        }
        
        isDragging = true;
        isReturning = false;
        
        // 드래그 중 설정
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = draggingSortingOrder;
        }
        
        // 스케일 변경
        transform.localScale = originalScale * dragScale;
        
        // 이벤트 발생
        OnDragStarted?.Invoke(this);
        
        // DragDropManager에 알림
        if (DragDropManager.Instance != null)
        {
            DragDropManager.Instance.OnDragStart(this);
        }
        
        Debug.Log($"[DraggableObject] Drag simulated: {gameObject.name}");
    }
}