using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 게임 UI 관리자
/// Day 표시, 돈 표시, 일시정지 버튼 등 관리
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("=== Day UI ===")]
    public Image dayImage;
    public Sprite day1Sprite;
    public Sprite day2Sprite;
    public Sprite day3Sprite;

    [Header("=== Money UI ===")]
    public Image moneyBackground;
    public TMP_Text moneyText;
    public string moneyFormat = "{0}";  // 표시 형식 (예: "{0}원")

    [Header("=== Pause Button ===")]
    public Button pauseButton;

    [Header("=== Pause Popup ===")]
    public GameObject pausePopup;
    public Button resumeButton;      // 계속 진행
    public Button settingsButton;    // 설정
    public Button quitButton;        // 종료

    private GameManager gameManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[GameUIManager] GameManager를 찾을 수 없습니다!");
            return;
        }

        // 이벤트 구독
        gameManager.OnDayChanged += UpdateDayUI;
        gameManager.OnMoneyChanged += UpdateMoneyUI;
        gameManager.OnPauseChanged += UpdatePauseUI;

        // 버튼 이벤트 연결
        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        // 초기 UI 설정
        InitializeUI();
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnDayChanged -= UpdateDayUI;
            gameManager.OnMoneyChanged -= UpdateMoneyUI;
            gameManager.OnPauseChanged -= UpdatePauseUI;
        }

        // 버튼 이벤트 해제
        if (pauseButton != null)
            pauseButton.onClick.RemoveListener(OnPauseButtonClicked);

        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(OnResumeButtonClicked);

        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);

        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }

    /// <summary>
    /// 초기 UI 설정
    /// </summary>
    private void InitializeUI()
    {
        // 현재 상태로 UI 초기화
        UpdateDayUI(gameManager.CurrentDay);
        UpdateMoneyUI(gameManager.CurrentMoney);
        
        // 일시정지 팝업 숨김
        if (pausePopup != null)
            pausePopup.SetActive(false);
    }

    #region UI Update Methods

    /// <summary>
    /// Day UI 업데이트
    /// </summary>
    private void UpdateDayUI(int day)
    {
        if (dayImage == null) return;

        Sprite sprite = day switch
        {
            1 => day1Sprite,
            2 => day2Sprite,
            3 => day3Sprite,
            _ => day1Sprite
        };

        if (sprite != null)
        {
            dayImage.sprite = sprite;
            Debug.Log($"[GameUIManager] Day UI 업데이트: Day {day}");
        }
        else
        {
            Debug.LogWarning($"[GameUIManager] Day {day} 스프라이트가 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// Money UI 업데이트
    /// </summary>
    private void UpdateMoneyUI(int money)
    {
        if (moneyText == null) return;

        moneyText.text = string.Format(moneyFormat, money);
        Debug.Log($"[GameUIManager] Money UI 업데이트: {money}");
    }

    /// <summary>
    /// Pause UI 업데이트
    /// </summary>
    private void UpdatePauseUI(bool isPaused)
    {
        // 일시정지 팝업 표시/숨김
        if (pausePopup != null)
        {
            pausePopup.SetActive(isPaused);
            Debug.Log($"[GameUIManager] Pause Popup: {(isPaused ? "표시" : "숨김")}");
        }
    }

    #endregion

    #region Button Callbacks

    /// <summary>
    /// 일시정지 버튼 클릭
    /// </summary>
    private void OnPauseButtonClicked()
    {
        Debug.Log("[GameUIManager] 일시정지 버튼 클릭");
        gameManager?.SetPause(true);
    }

    /// <summary>
    /// 계속 진행 버튼 클릭
    /// </summary>
    private void OnResumeButtonClicked()
    {
        Debug.Log("[GameUIManager] 계속 진행 버튼 클릭");
        gameManager?.ResumeGame();
    }

    /// <summary>
    /// 설정 버튼 클릭
    /// </summary>
    private void OnSettingsButtonClicked()
    {
        Debug.Log("[GameUIManager] 설정 버튼 클릭");
        gameManager?.OpenSetting();
    }

    /// <summary>
    /// 종료 버튼 클릭
    /// </summary>
    private void OnQuitButtonClicked()
    {
        Debug.Log("[GameUIManager] 종료 버튼 클릭");
        gameManager?.QuitGame();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 돈 표시 형식 변경
    /// </summary>
    public void SetMoneyFormat(string format)
    {
        moneyFormat = format;
        UpdateMoneyUI(gameManager.CurrentMoney);
    }

    /// <summary>
    /// Day 이미지 수동 설정 (Inspector 미할당 시 사용)
    /// </summary>
    public void SetDaySprites(Sprite d1, Sprite d2, Sprite d3)
    {
        day1Sprite = d1;
        day2Sprite = d2;
        day3Sprite = d3;
        UpdateDayUI(gameManager.CurrentDay);
    }

    #endregion
}
