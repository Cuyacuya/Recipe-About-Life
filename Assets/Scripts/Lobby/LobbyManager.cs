using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace RecipeAboutLife.Lobby
{
    /// <summary>
    /// 로비 매니저 - 핀 클릭 → 트럭 이동 → 씬 전환
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        [Header("스테이지 데이터")]
        [SerializeField] private StageUnlockData stageUnlockData;

        [Header("핀")]
        [SerializeField] private List<StagePin> stagePins = new List<StagePin>();

        [Header("트럭")]
        [SerializeField] private TruckController truck;
        [SerializeField] private float truckYOffset = -0.5f;

        [Header("맵 배경")]
        [SerializeField] private GameObject mapBackgroundObject;
        [SerializeField] private List<Sprite> mapSprites = new List<Sprite>();

        private SpriteRenderer mapBackgroundRenderer;

        [Header("씬 전환")]
        [SerializeField] private string gamePlaySceneName = "GamePlayScene";
        [SerializeField] private float fadeDelay = 0.3f;

        private int selectedStage = -1;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            // 맵 배경 SpriteRenderer 캐싱
            if (mapBackgroundObject != null)
                mapBackgroundRenderer = mapBackgroundObject.GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            InitializePins();

            // 트럭 도착 이벤트 연결
            if (truck != null)
            {
                truck.OnArrived += OnTruckArrived;
            }

            // 맵 배경 업데이트
            UpdateMapBackground();

            // 페이드 아웃
            StartCoroutine(FadeOut());
        }

        private void OnDestroy()
        {
            foreach (var pin in stagePins)
            {
                if (pin != null)
                    pin.OnClicked -= OnPinClicked;
            }

            if (truck != null)
                truck.OnArrived -= OnTruckArrived;
        }

        private void InitializePins()
        {
            for (int i = 0; i < stagePins.Count; i++)
            {
                var pin = stagePins[i];
                if (pin == null) continue;

                // 클릭 이벤트 연결
                pin.OnClicked += OnPinClicked;

                // StageUnlockData에서 해금 상태 가져오기
                if (stageUnlockData != null)
                {
                    var stageInfo = stageUnlockData.GetStage(pin.StageIndex);
                    if (stageInfo != null)
                    {
                        pin.SetUnlocked(stageInfo.isUnlocked);
                    }
                }
            }
        }

        private void OnPinClicked(StagePin pin)
        {
            if (truck != null && truck.IsMoving) return;
            if (!pin.IsUnlocked)
            {
                Debug.Log($"[Lobby] Stage {pin.StageIndex} 잠김");
                return;
            }

            selectedStage = pin.StageIndex;
            Debug.Log($"[Lobby] Stage {selectedStage} 선택");

            // 트럭 이동
            if (truck != null)
            {
                Vector3 targetPos = pin.transform.position;
                targetPos.y += truckYOffset;
                truck.MoveToPosition(targetPos);
            }
            else
            {
                LoadGamePlayScene();
            }
        }

        private void OnTruckArrived()
        {
            LoadGamePlayScene();
        }

        private void LoadGamePlayScene()
        {
            PlayerPrefs.SetInt("SelectedStageIndex", selectedStage);
            PlayerPrefs.Save();

            StartCoroutine(FadeAndLoadScene());
        }

        private IEnumerator FadeAndLoadScene()
        {
            var fadeUI = UI.FadeUI.Instance;
            if (fadeUI != null)
            {
                bool done = false;
                fadeUI.FadeIn(0.5f, () => done = true);
                while (!done) yield return null;
            }

            yield return new WaitForSeconds(fadeDelay);

            Debug.Log($"[Lobby] {gamePlaySceneName} 로드");
            SceneManager.LoadScene(gamePlaySceneName);
        }

        private IEnumerator FadeOut()
        {
            var fadeUI = UI.FadeUI.Instance;
            if (fadeUI != null)
            {
                fadeUI.SetBlack();
                yield return new WaitForSeconds(0.2f);

                bool done = false;
                fadeUI.FadeOut(0.5f, () => done = true);
                while (!done) yield return null;
            }
        }

        private void UpdateMapBackground()
        {
            if (mapBackgroundRenderer == null || mapSprites.Count == 0) return;

            int lastUnlocked = stageUnlockData != null
                ? stageUnlockData.GetLastUnlockedStageIndex()
                : 1;

            int index = Mathf.Clamp(lastUnlocked - 1, 0, mapSprites.Count - 1);
            if (mapSprites[index] != null)
            {
                mapBackgroundRenderer.sprite = mapSprites[index];
            }
        }

        /// <summary>
        /// 스테이지 해금 (외부에서 호출)
        /// </summary>
        public void UnlockStage(int stageIndex)
        {
            // 데이터 업데이트
            if (stageUnlockData != null)
            {
                stageUnlockData.UnlockStage(stageIndex);
            }

            // 핀 비주얼 업데이트
            var pin = stagePins.Find(p => p != null && p.StageIndex == stageIndex);
            if (pin != null)
            {
                pin.SetUnlocked(true);
            }

            // 맵 배경 업데이트
            UpdateMapBackground();
        }

        /// <summary>
        /// GamePlay 완료 후 다음 스테이지 해금
        /// </summary>
        public void OnGamePlayCompleted(int completedStageIndex, bool success)
        {
            if (success && stageUnlockData != null)
            {
                int nextStage = completedStageIndex + 1;
                if (stageUnlockData.GetStage(nextStage) != null)
                {
                    stageUnlockData.UnlockStage(nextStage);
                    Debug.Log($"[Lobby] Stage {nextStage} 해금 조건 달성!");
                }
            }
        }

        public static int GetSelectedStageIndex()
        {
            return PlayerPrefs.GetInt("SelectedStageIndex", 1);
        }
    }
}
