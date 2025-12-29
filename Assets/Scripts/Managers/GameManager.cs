using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 게임 전체 관리자
/// Day, 돈, 일시정지 등 게임 상태 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("=== Day Settings ===")]
    [SerializeField] private int currentDay = 1;
    public int maxDay = 3;

    [Header("=== Money Settings ===")]
    [SerializeField] private int currentMoney = 0;

    [Header("=== Pause Settings ===")]
    [SerializeField] private bool isPaused = false;

    // 이벤트
    public event Action<int> OnDayChanged;
    public event Action<int> OnMoneyChanged;
    public event Action<bool> OnPauseChanged;

    // 프로퍼티
    public int CurrentDay => currentDay;
    public int CurrentMoney => currentMoney;
    public bool IsPaused => isPaused;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Day Management

    /// <summary>
    /// Day 설정 (외부 호출용)
    /// </summary>
    public void SetDay(int day)
    {
        if (day < 1 || day > maxDay)
        {
            Debug.LogWarning($"[GameManager] 유효하지 않은 Day: {day} (1~{maxDay})");
            return;
        }

        currentDay = day;
        OnDayChanged?.Invoke(currentDay);
        Debug.Log($"[GameManager] Day 변경: {currentDay}");
    }

    /// <summary>
    /// 다음 Day로 진행
    /// </summary>
    public void NextDay()
    {
        if (currentDay < maxDay)
        {
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
            Debug.Log($"[GameManager] 다음 Day: {currentDay}");
        }
        else
        {
            Debug.Log("[GameManager] 마지막 Day입니다!");
            // 게임 클리어 로직 추가 가능
        }
    }

    #endregion

    #region Money Management

    /// <summary>
    /// 돈 추가
    /// </summary>
    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[GameManager] AddMoney에 음수 값 전달됨");
            return;
        }

        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"[GameManager] 돈 추가: +{amount} (현재: {currentMoney})");
    }

    /// <summary>
    /// 돈 사용
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[GameManager] SpendMoney에 음수 값 전달됨");
            return false;
        }

        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(currentMoney);
            Debug.Log($"[GameManager] 돈 사용: -{amount} (현재: {currentMoney})");
            return true;
        }
        else
        {
            Debug.Log($"[GameManager] 돈 부족! 필요: {amount}, 현재: {currentMoney}");
            return false;
        }
    }

    /// <summary>
    /// 돈 설정 (직접 설정)
    /// </summary>
    public void SetMoney(int amount)
    {
        currentMoney = Mathf.Max(0, amount);
        OnMoneyChanged?.Invoke(currentMoney);
        Debug.Log($"[GameManager] 돈 설정: {currentMoney}");
    }

    #endregion

    #region Pause Management

    /// <summary>
    /// 일시정지 토글
    /// </summary>
    public void TogglePause()
    {
        SetPause(!isPaused);
    }

    /// <summary>
    /// 일시정지 설정
    /// </summary>
    public void SetPause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = isPaused ? 0f : 1f;
        OnPauseChanged?.Invoke(isPaused);
        Debug.Log($"[GameManager] 일시정지: {isPaused}");
    }

    /// <summary>
    /// 게임 재개 (계속 진행)
    /// </summary>
    public void ResumeGame()
    {
        SetPause(false);
    }

    #endregion

    #region Scene Management

    /// <summary>
    /// 로비 씬으로 이동
    /// </summary>
    public void LoadLobbyScene()
    {
        // 일시정지 해제 후 씬 이동
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("LobbyScene");
    }

    /// <summary>
    /// 설정 열기
    /// </summary>
    public void OpenSetting()
    {
        // 설정 팝업 열기 로직 (추후 구현)
        Debug.Log("[GameManager] 설정 열기");
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] 게임 종료");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("Add 100 Money")]
    private void DebugAddMoney() => AddMoney(100);

    [ContextMenu("Next Day")]
    private void DebugNextDay() => NextDay();

    [ContextMenu("Toggle Pause")]
    private void DebugTogglePause() => TogglePause();
#endif
}