using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 팝업 시스템 전역 관리자
/// Singleton 패턴
/// </summary>
public class PopupManager : MonoBehaviour
{
    // Singleton
    private static PopupManager _instance;
    public static PopupManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PopupManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PopupManager");
                    _instance = go.AddComponent<PopupManager>();
                }
            }
            return _instance;
        }
    }
    
    [Header("Settings")]
    [Tooltip("팝업이 열릴 때 메인 화면 비활성화")]
    public bool blockMainScreenWhenPopupOpen = true;
    
    [Tooltip("배경 어둡게 하기")]
    public GameObject backgroundDimmer;
    
    [Tooltip("배경 어두움 정도 (0-1)")]
    [Range(0f, 1f)]
    public float dimmerAlpha = 0.7f;
    
    [Header("References")]
    [Tooltip("메인 화면 (비활성화할 대상)")]
    public GameObject mainScreen;
    
    [Tooltip("팝업 컨테이너 (모든 팝업의 부모)")]
    public Transform popupContainer;
    
    // 이벤트
    public event Action<PopupBase> OnPopupOpened;
    public event Action<PopupBase> OnPopupClosed;
    
    // 팝업 관리
    private Stack<PopupBase> popupStack = new Stack<PopupBase>();
    private Dictionary<string, PopupBase> popupRegistry = new Dictionary<string, PopupBase>();
    
    // 상태
    public bool HasOpenPopup => popupStack.Count > 0;
    public PopupBase CurrentPopup => HasOpenPopup ? popupStack.Peek() : null;
    public int PopupCount => popupStack.Count;
    
    private void Awake()
    {
        // Singleton 체크
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        // 초기화
        InitializeDimmer();
        
        // 모든 팝업 찾기
        RegisterAllPopups();
    }
    
    private void Start()
    {
        // 초기 상태 설정
        UpdateMainScreenState();
        UpdateDimmer();
    }
    
    /// <summary>
    /// 배경 어둡게 하기 초기화
    /// </summary>
    private void InitializeDimmer()
    {
        if (backgroundDimmer != null)
        {
            // 초기에는 비활성화
            backgroundDimmer.SetActive(false);
            
            // 알파값 설정
            CanvasGroup dimmerGroup = backgroundDimmer.GetComponent<CanvasGroup>();
            if (dimmerGroup != null)
            {
                dimmerGroup.alpha = dimmerAlpha;
            }
        }
    }
    
    /// <summary>
    /// 씬에 있는 모든 팝업 등록
    /// </summary>
    private void RegisterAllPopups()
    {
        PopupBase[] allPopups = FindObjectsByType<PopupBase>(FindObjectsSortMode.None);
        foreach (var popup in allPopups)
        {
            if (!popupRegistry.ContainsKey(popup.popupId))
            {
                popupRegistry.Add(popup.popupId, popup);
                Debug.Log($"[PopupManager] Registered popup: {popup.popupId}");
            }
        }
        
        Debug.Log($"[PopupManager] Total registered popups: {popupRegistry.Count}");
    }
    
    /// <summary>
    /// 팝업 등록 (동적 생성 시)
    /// </summary>
    public void RegisterPopup(PopupBase popup)
    {
        if (popup == null) return;
        
        // 스택에 추가
        if (!popupStack.Contains(popup))
        {
            popupStack.Push(popup);
        }
        
        // 레지스트리에 등록
        if (!popupRegistry.ContainsKey(popup.popupId))
        {
            popupRegistry.Add(popup.popupId, popup);
        }
        
        // 상태 업데이트
        UpdateMainScreenState();
        UpdateDimmer();
        
        // 이벤트 발생
        OnPopupOpened?.Invoke(popup);
        
        Debug.Log($"[PopupManager] Popup opened: {popup.popupId}, Stack count: {popupStack.Count}");
    }
    
    /// <summary>
    /// 팝업 등록 해제
    /// </summary>
    public void UnregisterPopup(PopupBase popup)
    {
        if (popup == null) return;
        
        // 스택에서 제거
        if (popupStack.Contains(popup))
        {
            // 스택의 맨 위가 아니면 경고
            if (popupStack.Peek() != popup)
            {
                Debug.LogWarning($"[PopupManager] Closing popup {popup.popupId} that is not on top of stack!");
            }
            
            // 스택을 임시 리스트로 변환하여 제거
            var tempList = new List<PopupBase>(popupStack);
            tempList.Remove(popup);
            popupStack.Clear();
            foreach (var p in tempList)
            {
                popupStack.Push(p);
            }
        }
        
        // 상태 업데이트
        UpdateMainScreenState();
        UpdateDimmer();
        
        // 이벤트 발생
        OnPopupClosed?.Invoke(popup);
        
        Debug.Log($"[PopupManager] Popup closed: {popup.popupId}, Stack count: {popupStack.Count}");
    }
    
    /// <summary>
    /// ID로 팝업 열기
    /// </summary>
    public PopupBase OpenPopup(string popupId)
    {
        if (!popupRegistry.ContainsKey(popupId))
        {
            Debug.LogError($"[PopupManager] Popup not found: {popupId}");
            return null;
        }
        
        PopupBase popup = popupRegistry[popupId];
        popup.Open();
        return popup;
    }
    
    /// <summary>
    /// 현재 팝업 닫기
    /// </summary>
    public void CloseCurrentPopup()
    {
        if (HasOpenPopup)
        {
            CurrentPopup.Close();
        }
    }
    
    /// <summary>
    /// 모든 팝업 닫기
    /// </summary>
    public void CloseAllPopups()
    {
        while (HasOpenPopup)
        {
            CurrentPopup.Close();
        }
    }
    
    /// <summary>
    /// 특정 팝업 찾기
    /// </summary>
    public PopupBase GetPopup(string popupId)
    {
        if (popupRegistry.ContainsKey(popupId))
        {
            return popupRegistry[popupId];
        }
        return null;
    }
    
    /// <summary>
    /// 특정 타입의 팝업 찾기
    /// </summary>
    public T GetPopup<T>() where T : PopupBase
    {
        foreach (var popup in popupRegistry.Values)
        {
            if (popup is T)
            {
                return popup as T;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 메인 화면 상태 업데이트
    /// </summary>
    private void UpdateMainScreenState()
    {
        if (mainScreen == null || !blockMainScreenWhenPopupOpen) return;
        
        // 팝업이 열려있으면 메인 화면 비활성화
        bool shouldBlock = HasOpenPopup;
        
        CanvasGroup mainScreenGroup = mainScreen.GetComponent<CanvasGroup>();
        if (mainScreenGroup != null)
        {
            mainScreenGroup.interactable = !shouldBlock;
            mainScreenGroup.blocksRaycasts = !shouldBlock;
        }
        else
        {
            mainScreen.SetActive(!shouldBlock);
        }
    }
    
    /// <summary>
    /// 배경 어두움 업데이트
    /// </summary>
    private void UpdateDimmer()
    {
        if (backgroundDimmer == null) return;
        
        backgroundDimmer.SetActive(HasOpenPopup);
    }
    
    /// <summary>
    /// 팝업 생성 (Prefab에서)
    /// </summary>
    public T CreatePopup<T>(GameObject prefab) where T : PopupBase
    {
        if (prefab == null)
        {
            Debug.LogError("[PopupManager] Popup prefab is null!");
            return null;
        }
        
        // 컨테이너 확인
        Transform parent = popupContainer != null ? popupContainer : transform;
        
        // 인스턴스 생성
        GameObject instance = Instantiate(prefab, parent);
        T popup = instance.GetComponent<T>();
        
        if (popup != null)
        {
            // 등록
            if (!popupRegistry.ContainsKey(popup.popupId))
            {
                popupRegistry.Add(popup.popupId, popup);
            }
            
            Debug.Log($"[PopupManager] Created popup: {popup.popupId}");
        }
        
        return popup;
    }
    
    /// <summary>
    /// 팝업 제거
    /// </summary>
    public void DestroyPopup(string popupId)
    {
        if (!popupRegistry.ContainsKey(popupId)) return;
        
        PopupBase popup = popupRegistry[popupId];
        
        // 열려있으면 닫기
        if (popup.IsOpen)
        {
            popup.Close();
        }
        
        // 레지스트리에서 제거
        popupRegistry.Remove(popupId);
        
        // 오브젝트 파괴
        Destroy(popup.gameObject);
        
        Debug.Log($"[PopupManager] Destroyed popup: {popupId}");
    }
    
#if UNITY_EDITOR
    [ContextMenu("Refresh Popups")]
    private void RefreshPopups()
    {
        popupRegistry.Clear();
        RegisterAllPopups();
    }
    
    [ContextMenu("Close All Popups")]
    private void CloseAllPopupsMenu()
    {
        CloseAllPopups();
    }
    
    [ContextMenu("Log Status")]
    private void LogStatus()
    {
        Debug.Log($"=== PopupManager Status ===");
        Debug.Log($"Registered Popups: {popupRegistry.Count}");
        Debug.Log($"Open Popups: {popupStack.Count}");
        Debug.Log($"Current Popup: {(HasOpenPopup ? CurrentPopup.popupId : "None")}");
        
        Debug.Log("--- Registered Popups ---");
        foreach (var kvp in popupRegistry)
        {
            Debug.Log($"  - {kvp.Key}: {kvp.Value.gameObject.name} (Active: {kvp.Value.gameObject.activeSelf})");
        }
    }
#endif
}