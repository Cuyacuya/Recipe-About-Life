using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 드래그 앤 드롭 시스템 전역 관리자
/// Singleton 패턴
/// </summary>
public class DragDropManager : MonoBehaviour
{
    // Singleton
    private static DragDropManager _instance;
    public static DragDropManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DragDropManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DragDropManager");
                    _instance = go.AddComponent<DragDropManager>();
                }
            }
            return _instance;
        }
    }
    
    [Header("Settings")]
    [Tooltip("드래그 가능 여부 (전역)")]
    public bool isDragEnabled = true;
    
    [Tooltip("드래그 중 다른 입력 차단")]
    public bool blockOtherInputsWhileDragging = true;
    
    [Header("Audio")]
    [Tooltip("드래그 시작 사운드")]
    public AudioClip dragStartSound;
    
    [Tooltip("드롭 성공 사운드")]
    public AudioClip dropSuccessSound;
    
    [Tooltip("드롭 실패 사운드")]
    public AudioClip dropFailSound;
    
    // 이벤트
    public event Action<DraggableObject> OnGlobalDragStart;
    public event Action<DraggableObject, DropZone> OnGlobalDragEnd;
    public event Action<DraggableObject, DropZone> OnGlobalDropSuccess;
    public event Action<DraggableObject> OnGlobalDropFail;
    
    // 현재 상태
    private DraggableObject currentDraggingObject = null;
    private List<DraggableObject> allDraggableObjects = new List<DraggableObject>();
    private List<DropZone> allDropZones = new List<DropZone>();
    
    // 드래그 상태
    public bool IsDragging => currentDraggingObject != null;
    public DraggableObject CurrentDraggingObject => currentDraggingObject;
    
    private void Awake()
    {
        // Singleton 체크
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 씬 전환 시 파괴되지 않도록 (선택사항)
        // DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 씬에 있는 모든 DraggableObject와 DropZone 찾기
        RefreshDraggableObjects();
        RefreshDropZones();
    }
    
    /// <summary>
    /// 드래그 가능한 오브젝트 목록 갱신
    /// </summary>
    public void RefreshDraggableObjects()
    {
        allDraggableObjects.Clear();
        allDraggableObjects.AddRange(FindObjectsByType<DraggableObject>(FindObjectsSortMode.None));
        Debug.Log($"[DragDropManager] Found {allDraggableObjects.Count} draggable objects");
    }
    
    /// <summary>
    /// 드롭존 목록 갱신
    /// </summary>
    public void RefreshDropZones()
    {
        allDropZones.Clear();
        allDropZones.AddRange(FindObjectsByType<DropZone>(FindObjectsSortMode.None));
        Debug.Log($"[DragDropManager] Found {allDropZones.Count} drop zones");
    }
    
    /// <summary>
    /// 드래그 시작 알림
    /// </summary>
    public void OnDragStart(DraggableObject dragObject)
    {
        if (!isDragEnabled) return;
        
        currentDraggingObject = dragObject;
        
        // 사운드 재생
        PlaySound(dragStartSound);
        
        // 이벤트 발생
        OnGlobalDragStart?.Invoke(dragObject);
        
        Debug.Log($"[DragDropManager] Drag started: {dragObject.gameObject.name}");
    }
    
    /// <summary>
    /// 드래그 종료 알림
    /// </summary>
    public void OnDragEnd(DraggableObject dragObject, DropZone dropZone)
    {
        if (currentDraggingObject != dragObject) return;
        
        bool success = dropZone != null;
        
        // 사운드 재생
        if (success)
        {
            PlaySound(dropSuccessSound);
            OnGlobalDropSuccess?.Invoke(dragObject, dropZone);
        }
        else
        {
            PlaySound(dropFailSound);
            OnGlobalDropFail?.Invoke(dragObject);
        }
        
        // 이벤트 발생
        OnGlobalDragEnd?.Invoke(dragObject, dropZone);
        
        currentDraggingObject = null;
        
        Debug.Log($"[DragDropManager] Drag ended: {dragObject.gameObject.name}, Success: {success}");
    }
    
    /// <summary>
    /// 사운드 재생
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        
        // AudioSource가 있으면 재생
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.PlayOneShot(clip);
    }
    
    /// <summary>
    /// 특정 위치에서 가장 가까운 드롭존 찾기
    /// </summary>
    public DropZone GetNearestDropZone(Vector2 position, float maxDistance = float.MaxValue)
    {
        DropZone nearest = null;
        float minDistance = maxDistance;
        
        foreach (var dropZone in allDropZones)
        {
            if (!dropZone.isDroppable) continue;
            
            float distance = Vector2.Distance(position, dropZone.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = dropZone;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// 특정 태그의 드롭존 찾기
    /// </summary>
    public DropZone GetDropZoneByTag(string tag)
    {
        foreach (var dropZone in allDropZones)
        {
            if (dropZone.CompareTag(tag))
            {
                return dropZone;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 모든 드래그 가능한 오브젝트 활성화/비활성화
    /// </summary>
    public void SetAllDraggable(bool draggable)
    {
        foreach (var obj in allDraggableObjects)
        {
            if (obj != null)
            {
                obj.SetDraggable(draggable);
            }
        }
    }
    
    /// <summary>
    /// 모든 드롭존 활성화/비활성화
    /// </summary>
    public void SetAllDropZonesDroppable(bool droppable)
    {
        foreach (var zone in allDropZones)
        {
            if (zone != null)
            {
                zone.SetDroppable(droppable);
            }
        }
    }
    
    /// <summary>
    /// 특정 태그의 모든 오브젝트 드래그 가능 설정
    /// </summary>
    public void SetDraggableByTag(string tag, bool draggable)
    {
        foreach (var obj in allDraggableObjects)
        {
            if (obj != null && obj.CompareTag(tag))
            {
                obj.SetDraggable(draggable);
            }
        }
    }
    
    /// <summary>
    /// 특정 태그의 모든 드롭존 활성화 설정
    /// </summary>
    public void SetDropZonesByTag(string tag, bool droppable)
    {
        foreach (var zone in allDropZones)
        {
            if (zone != null && zone.CompareTag(tag))
            {
                zone.SetDroppable(droppable);
            }
        }
    }
    
    /// <summary>
    /// 드래그 가능한 오브젝트 등록
    /// </summary>
    public void RegisterDraggable(DraggableObject obj)
    {
        if (!allDraggableObjects.Contains(obj))
        {
            allDraggableObjects.Add(obj);
        }
    }
    
    /// <summary>
    /// 드래그 가능한 오브젝트 등록 해제
    /// </summary>
    public void UnregisterDraggable(DraggableObject obj)
    {
        allDraggableObjects.Remove(obj);
    }
    
    /// <summary>
    /// 드롭존 등록
    /// </summary>
    public void RegisterDropZone(DropZone zone)
    {
        if (!allDropZones.Contains(zone))
        {
            allDropZones.Add(zone);
        }
    }
    
    /// <summary>
    /// 드롭존 등록 해제
    /// </summary>
    public void UnregisterDropZone(DropZone zone)
    {
        allDropZones.Remove(zone);
    }
    
    /// <summary>
    /// 현재 드래그 취소
    /// </summary>
    public void CancelCurrentDrag()
    {
        if (currentDraggingObject != null)
        {
            currentDraggingObject.ReturnToOriginalPosition();
            currentDraggingObject = null;
        }
    }
    
#if UNITY_EDITOR
    [ContextMenu("Refresh All")]
    private void RefreshAll()
    {
        RefreshDraggableObjects();
        RefreshDropZones();
    }
    
    [ContextMenu("Log Status")]
    private void LogStatus()
    {
        Debug.Log($"=== DragDropManager Status ===");
        Debug.Log($"Draggable Objects: {allDraggableObjects.Count}");
        Debug.Log($"Drop Zones: {allDropZones.Count}");
        Debug.Log($"Is Dragging: {IsDragging}");
        Debug.Log($"Current Object: {(currentDraggingObject != null ? currentDraggingObject.name : "None")}");
    }
#endif
}