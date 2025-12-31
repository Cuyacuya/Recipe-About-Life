using System.Collections.Generic;
using UnityEngine;
using RecipeAboutLife.Events;
using RecipeAboutLife.Managers;
using RecipeAboutLife.Cooking;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// NPC 스폰 관리자
    /// 스테이지마다 10명 중 랜덤 5명을 순차적으로 스폰
    /// ScoreManager와 연동하여 재화 관리
    /// </summary>
    public class NPCSpawnManager : MonoBehaviour
    {
        [Header("NPC 프리팹 풀")]
        [SerializeField]
        [Tooltip("사용 가능한 NPC 프리팹 목록 (10개)")]
        private List<GameObject> npcPrefabs = new List<GameObject>();

        [Header("스폰 설정")]
        [SerializeField]
        [Tooltip("스테이지당 스폰할 NPC 수")]
        private int npcsPerStage = 5;

        [SerializeField]
        [Tooltip("NPC 스폰 부모 트랜스폼")]
        private Transform npcParent;

        [SerializeField]
        [Tooltip("스테이지 시작 시 자동 스폰 여부")]
        private bool autoStartOnAwake = true;

        [Header("현재 상태")]
        private List<GameObject> selectedNPCPrefabs = new List<GameObject>();
        private int currentNPCIndex = 0;
        private GameObject currentNPCInstance = null;
        private bool isStageActive = false;

        // ==========================================
        // Events (확장 포인트)
        // ==========================================

        /// <summary>
        /// NPC 선택 완료 후 호출되는 이벤트
        /// 스토리 NPC 교체 등 확장 기능을 위한 Hook
        /// </summary>
        public System.Action<List<GameObject>> OnNPCsSelected;

        private void Awake()
        {
            // NPC 부모가 없으면 새로 생성
            if (npcParent == null)
            {
                GameObject npcContainer = new GameObject("--- NPCs ---");
                npcParent = npcContainer.transform;
                Debug.Log("[NPCSpawnManager] NPC 컨테이너 자동 생성");
            }

            // 프리팹 개수 검증
            if (npcPrefabs.Count < npcsPerStage)
            {
                Debug.LogWarning($"[NPCSpawnManager] NPC 프리팹이 {npcPrefabs.Count}개만 있습니다. 최소 {npcsPerStage}개 필요합니다.");
            }
        }

        private void Start()
        {
            if (autoStartOnAwake)
            {
                StartStage();
            }
        }

        private void OnEnable()
        {
            // 레시피 완료 이벤트 구독
            GameEvents.OnRecipeCompleted += OnRecipeCompleted;
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            GameEvents.OnRecipeCompleted -= OnRecipeCompleted;
        }

        /// <summary>
        /// 스테이지 시작 - 랜덤 NPC 선택 및 첫 NPC 스폰
        /// </summary>
        public void StartStage()
        {
            if (isStageActive)
            {
                Debug.LogWarning("[NPCSpawnManager] 이미 스테이지가 진행 중입니다!");
                return;
            }

            if (npcPrefabs.Count == 0)
            {
                Debug.LogError("[NPCSpawnManager] NPC 프리팹이 없습니다!");
                return;
            }

            // 랜덤 NPC 선택
            SelectRandomNPCs();

            // 첫 NPC 스폰
            currentNPCIndex = 0;
            isStageActive = true;
            SpawnNextNPC();

            Debug.Log($"[NPCSpawnManager] 스테이지 시작! {selectedNPCPrefabs.Count}명의 NPC 선택됨");
        }

        /// <summary>
        /// 랜덤으로 NPC 선택
        /// </summary>
        private void SelectRandomNPCs()
        {
            selectedNPCPrefabs.Clear();

            // 프리팹 리스트 복사
            List<GameObject> availablePrefabs = new List<GameObject>(npcPrefabs);

            // 랜덤하게 선택
            int selectCount = Mathf.Min(npcsPerStage, availablePrefabs.Count);
            for (int i = 0; i < selectCount; i++)
            {
                int randomIndex = Random.Range(0, availablePrefabs.Count);
                selectedNPCPrefabs.Add(availablePrefabs[randomIndex]);
                availablePrefabs.RemoveAt(randomIndex);
            }

            Debug.Log($"[NPCSpawnManager] {selectedNPCPrefabs.Count}명의 NPC를 랜덤 선택했습니다.");

            // 확장 포인트: NPC 선택 완료 이벤트 발생
            // (스토리 NPC 교체 등을 위한 Hook)
            OnNPCsSelected?.Invoke(selectedNPCPrefabs);
        }

        /// <summary>
        /// 다음 NPC 스폰
        /// </summary>
        private void SpawnNextNPC()
        {
            if (currentNPCIndex >= selectedNPCPrefabs.Count)
            {
                // 모든 NPC 스폰 완료
                OnStageComplete();
                return;
            }

            // NPC 생성
            GameObject npcPrefab = selectedNPCPrefabs[currentNPCIndex];
            currentNPCInstance = Instantiate(npcPrefab, npcParent);
            currentNPCInstance.name = $"NPC_{currentNPCIndex + 1}_{npcPrefab.name}";

            Debug.Log($"[NPCSpawnManager] NPC {currentNPCIndex + 1}/{selectedNPCPrefabs.Count} 스폰: {currentNPCInstance.name}");

            currentNPCIndex++;
        }

        /// <summary>
        /// 레시피 완료 이벤트 처리
        /// CookingManager에서 요리가 완료되면 자동으로 호출됨
        /// </summary>
        private void OnRecipeCompleted(HotdogRecipe recipe)
        {
            Debug.Log($"[NPCSpawnManager] 레시피 완료! 품질: {recipe.quality:F1}, 보상: {recipe.CalculateReward(null)}");

            // ScoreManager가 보상을 처리하므로 여기서는 NPC 퇴장만 처리
            // 현재 NPC에게 퇴장 명령
            if (currentNPCInstance != null)
            {
                NPCMovement movement = currentNPCInstance.GetComponent<NPCMovement>();
                if (movement != null)
                {
                    movement.OnOrderComplete(); // 퇴장 시작 (퇴장 완료 시 OnNPCOrderComplete 자동 호출됨)
                    Debug.Log("[NPCSpawnManager] NPC 퇴장 시작");
                }
                else
                {
                    // Movement가 없으면 바로 다음 NPC 스폰
                    Debug.LogWarning("[NPCSpawnManager] NPCMovement를 찾을 수 없어 바로 처리합니다.");
                    OnNPCOrderComplete();
                }
            }
            else
            {
                Debug.LogWarning("[NPCSpawnManager] 현재 NPC가 없습니다!");
            }
        }

        /// <summary>
        /// 현재 NPC가 주문을 완료했을 때 호출 (NPCMovement.OnExitComplete에서 호출됨)
        /// </summary>
        public void OnNPCOrderComplete()
        {
            Debug.Log("[NPCSpawnManager] NPC 퇴장 완료! 다음 NPC 스폰 준비");

            // 현재 NPC 제거 (퇴장이 완료되었으므로)
            if (currentNPCInstance != null)
            {
                Destroy(currentNPCInstance);
                currentNPCInstance = null;
            }

            // 다음 NPC 스폰 (약간의 딜레이 후)
            Invoke(nameof(SpawnNextNPC), 0.5f);
        }

        /// <summary>
        /// 스테이지 완료
        /// </summary>
        private void OnStageComplete()
        {
            isStageActive = false;
            Debug.Log("[NPCSpawnManager] 스테이지 완료! 모든 NPC가 주문을 마쳤습니다.");

            // ScoreManager에서 총 점수 및 대화 잠금 해제 처리
            if (ScoreManager.Instance != null)
            {
                int totalReward = ScoreManager.Instance.GetTotalReward();
                int targetReward = ScoreManager.Instance.GetTargetReward();
                bool isUnlocked = ScoreManager.Instance.IsDialogueUnlocked();

                Debug.Log($"[NPCSpawnManager] 최종 재화: {totalReward}/{targetReward}, 대화 잠금 해제: {isUnlocked}");
            }
            else
            {
                Debug.LogWarning("[NPCSpawnManager] ScoreManager를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 스테이지 재시작
        /// </summary>
        public void RestartStage()
        {
            // 현재 NPC 정리
            if (currentNPCInstance != null)
            {
                Destroy(currentNPCInstance);
                currentNPCInstance = null;
            }

            // 모든 자식 NPC 제거
            foreach (Transform child in npcParent)
            {
                Destroy(child.gameObject);
            }

            // 상태 초기화
            selectedNPCPrefabs.Clear();
            currentNPCIndex = 0;
            isStageActive = false;

            // ScoreManager 초기화
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.RestartStage();
            }

            // 새 스테이지 시작
            StartStage();
        }

        /// <summary>
        /// 현재 NPC 인스턴스 가져오기
        /// </summary>
        public GameObject GetCurrentNPC()
        {
            return currentNPCInstance;
        }

        /// <summary>
        /// 스테이지 진행 상태 확인
        /// </summary>
        public bool IsStageActive()
        {
            return isStageActive;
        }
    }
}
