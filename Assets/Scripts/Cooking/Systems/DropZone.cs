using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 드롭 가능한 영역 컴포넌트
/// 도마, 반죽통, 기름통, 식힘망 등에 부착
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DropZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("드롭 가능 여부")]
    public bool isDroppable = true;
    
    [Tooltip("드롭 시 중앙으로 스냅")]
    public bool snapToCenter = false;
    
    [Tooltip("허용된 오브젝트 태그들 (비어있으면 모든 오브젝트 허용)")]
    public string[] allowedObjectTags;
    
    [Header("Visual Feedback")]
    [Tooltip("하이라이트 표시 여부")]
    public bool showHighlight = true;
    
    [Tooltip("하이라이트 스프라이트 (선택사항)")]
    public SpriteRenderer highlightSprite;
    
    [Tooltip("하이라이트 이미지 (UI용)")]
    public Image highlightImage;
    
    [Tooltip("하이라이트 색상")]
    public Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    
    [Tooltip("정상 색상")]
    public Color normalColor = new Color(1f, 1f, 1f, 0f);
    
    // 이벤트
    public event Action<DraggableObject> OnObjectEntered;
    public event Action<DraggableObject> OnObjectExited;
    public event Action<DraggableObject> OnObjectReceived;
    
    // 컴포넌트
    private Collider2D dropZoneCollider;
    private SpriteRenderer spriteRenderer;
    
    // 현재 상태
    private bool isHighlighted = false;
    private DraggableObject currentDragObject = null;
    
    private void Awake()
    {
        // Collider 가져오기
        dropZoneCollider = GetComponent<Collider2D>();
        if (dropZoneCollider != null)
        {
            dropZoneCollider.isTrigger = true;
        }
        
        // SpriteRenderer 가져오기
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 하이라이트 초기화
        if (highlightSprite != null)
        {
            highlightSprite.color = normalColor;
        }
        if (highlightImage != null)
        {
            highlightImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// 드래그 오브젝트가 진입 (마우스 오버)
    /// </summary>
    public void OnDragEnter(DraggableObject dragObject)
    {
        if (!isDroppable) return;
        
        // 허용된 오브젝트인지 체크
        if (!IsValidObject(dragObject))
        {
            return;
        }
        
        currentDragObject = dragObject;
        
        // 하이라이트 표시
        if (showHighlight)
        {
            ShowHighlight(true);
        }
        
        // 이벤트 발생
        OnObjectEntered?.Invoke(dragObject);
        
        Debug.Log($"[DropZone] Object entered: {dragObject.gameObject.name} -> {gameObject.name}");
    }
    
    /// <summary>
    /// 드래그 오브젝트가 퇴장 (마우스 아웃)
    /// </summary>
    public void OnDragExit(DraggableObject dragObject)
    {
        if (currentDragObject != dragObject) return;
        
        currentDragObject = null;
        
        // 하이라이트 숨김
        if (showHighlight)
        {
            ShowHighlight(false);
        }
        
        // 이벤트 발생
        OnObjectExited?.Invoke(dragObject);
        
        Debug.Log($"[DropZone] Object exited: {dragObject.gameObject.name} <- {gameObject.name}");
    }
    
    /// <summary>
    /// 오브젝트가 드롭됨
    /// </summary>
    public void OnObjectDropped(DraggableObject dragObject)
    {
        if (!isDroppable) return;
        
        // 허용된 오브젝트인지 체크
        if (!IsValidObject(dragObject))
        {
            Debug.LogWarning($"[DropZone] Invalid object dropped: {dragObject.gameObject.name}");
            return;
        }
        
        // 하이라이트 숨김
        if (showHighlight)
        {
            ShowHighlight(false);
        }
        
        currentDragObject = null;
        
        // 이벤트 발생
        OnObjectReceived?.Invoke(dragObject);
        
        Debug.Log($"[DropZone] Object received: {dragObject.gameObject.name} at {gameObject.name}");
    }
    
    /// <summary>
    /// 오브젝트 유효성 검사
    /// </summary>
    private bool IsValidObject(DraggableObject dragObject)
    {
        if (dragObject == null) return false;
        
        // 태그 체크 (allowedObjectTags가 비어있으면 모든 오브젝트 허용)
        if (allowedObjectTags != null && allowedObjectTags.Length > 0)
        {
            bool tagMatched = false;
            foreach (var tag in allowedObjectTags)
            {
                if (dragObject.CompareTag(tag))
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
    /// 하이라이트 표시/숨김
    /// </summary>
    private void ShowHighlight(bool show)
    {
        isHighlighted = show;
        Color targetColor = show ? highlightColor : normalColor;
        
        // SpriteRenderer 하이라이트
        if (highlightSprite != null)
        {
            highlightSprite.color = targetColor;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
        
        // UI Image 하이라이트
        if (highlightImage != null)
        {
            highlightImage.color = targetColor;
        }
    }
    
    /// <summary>
    /// 드롭 가능 여부 설정
    /// </summary>
    public void SetDroppable(bool droppable)
    {
        isDroppable = droppable;
        
        // 드롭 불가능하면 하이라이트 해제
        if (!droppable && isHighlighted)
        {
            ShowHighlight(false);
        }
    }
    
    /// <summary>
    /// 위치에 오브젝트가 있는지 체크
    /// </summary>
    public bool ContainsPoint(Vector2 point)
    {
        if (dropZoneCollider == null) return false;
        
        return dropZoneCollider.OverlapPoint(point);
    }
    
    /// <summary>
    /// 중앙 위치 반환
    /// </summary>
    public Vector3 GetCenterPosition()
    {
        if (dropZoneCollider != null)
        {
            return dropZoneCollider.bounds.center;
        }
        return transform.position;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 에디터에서 드롭존 영역 표시
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = isDroppable ? Color.green : Color.red;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
#endif
}