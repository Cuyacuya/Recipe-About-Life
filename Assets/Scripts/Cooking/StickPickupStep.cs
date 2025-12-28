using UnityEngine;
using System;
using RecipeAboutLife.Events;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 1단계: 꼬치 들기 (StickPickup)
    /// 
    /// 동작:
    /// 1. StickStation(꼬치통) 활성화
    /// 2. 꼬치통 클릭 → 꼬치 Prefab 생성
    /// 3. 꼬치를 CuttingBoard(도마)로 드래그
    /// 4. 도마에 드롭 → 단계 완료
    /// 
    /// Phase 2.2 구현
    /// </summary>
    public class StickPickupStep : ICookingStep
    {
        // ==========================================
        // Configuration
        // ==========================================
        
        private RecipeConfigSO config;
        
        // ==========================================
        // References
        // ==========================================
        
        private HotdogRecipe currentRecipe;
        private CookingStation stickStation;
        private CookingStation cuttingBoard;
        
        // ==========================================
        // State
        // ==========================================
        
        private GameObject stickObject;          // 생성된 꼬치
        private DraggableObject stickDraggable;   // 꼬치의 DraggableObject
        private int stickBaseSortingOrder = 0;
        private bool isStickCreated = false;
        private bool isStickOnBoard = false;
        
        // ==========================================
        // Prefab
        // ==========================================
        
        private GameObject stickPrefab;
        
        // ==========================================
        // Constructor
        // ==========================================
        
        public StickPickupStep(RecipeConfigSO recipeConfig)
        {
            config = recipeConfig;
        }
        
        // ==========================================
        // ICookingStep Implementation
        // ==========================================
        
        /// <summary>
        /// 단계 진입
        /// </summary>
        public void Enter(HotdogRecipe recipe)
        {
            currentRecipe = recipe;
            
            Debug.Log("[StickPickupStep] === ENTER ===");
            Debug.Log("[StickPickupStep] Ready to pick up stick from station");
            
            // 꼬치 Prefab 로드
            LoadStickPrefab();
            
            // Station 찾기 및 활성화
            FindAndActivateStations();
            
            // 이벤트 구독
            SubscribeEvents();
            
            // 상태 초기화
            isStickCreated = false;
            isStickOnBoard = false;
        }
        
        /// <summary>
        /// 매 프레임 업데이트 (사용 안 함)
        /// </summary>
        public void Update()
        {
            // 이 단계에서는 Update 불필요
        }
        
        /// <summary>
        /// 단계 처리
        /// </summary>
        public bool Process(object data, ref float quality)
        {
            // data는 드롭된 DropZone 객체
            DropZone dropZone = data as DropZone;
            
            if (dropZone == null)
            {
                Debug.LogWarning("[StickPickupStep] Process called with invalid data");
                return false;
            }
            
            // 도마(CuttingBoard)인지 확인
            CookingStation station = dropZone.GetComponent<CookingStation>();
            if (station != null && station.stationType == StationType.CuttingBoard)
            {
                Debug.Log("[StickPickupStep] ✅ Stick dropped on cutting board - STEP COMPLETE!");
                
                // 레시피 업데이트
                currentRecipe.hasStick = true;
                isStickOnBoard = true;
                
                // 효과음
                GameEvents.TriggerSFXRequested(config.sfxStepComplete);
                
                // 단계 완료!
                return true;
            }
            
            Debug.Log($"[StickPickupStep] ❌ Dropped on wrong station: {station?.stationType}");
            return false;
        }
        
        /// <summary>
        /// 단계 종료
        /// </summary>
        public void Exit()
        {
            Debug.Log("[StickPickupStep] === EXIT ===");
            
            // 이벤트 구독 해제
            UnsubscribeEvents();
            
            // Station 비활성화
            DeactivateStations();
            
            // 꼬치 오브젝트 정리
            if (stickObject != null && isStickOnBoard)
            {
                // 도마에 있는 꼬치는 그대로 둠 (다음 단계에서 사용)
                // DraggableObject 컴포넌트만 비활성화
                if (stickDraggable != null)
                {
                    stickDraggable.SetDraggable(false);
                }
                
                Debug.Log("[StickPickupStep] Stick left on cutting board for next step");
            }
        }
        
        // ==========================================
        // Station Management
        // ==========================================
        
        /// <summary>
        /// Station 찾기 및 활성화
        /// </summary>
        private void FindAndActivateStations()
        {
            // CookingManager를 통해 Station 찾기
            if (CookingManager.Instance != null)
            {
                stickStation = CookingManager.Instance.GetStation(StationType.StickStation);
                cuttingBoard = CookingManager.Instance.GetStation(StationType.CuttingBoard);
                
                // StickStation 활성화
                if (stickStation != null)
                {
                    stickStation.SetActive(true);
                    Debug.Log("[StickPickupStep] ✅ StickStation activated");
                }
                else
                {
                    Debug.LogError("[StickPickupStep] ❌ StickStation not found!");
                }
                
                // CuttingBoard 활성화 (드롭 대상)
                if (cuttingBoard != null)
                {
                    cuttingBoard.SetActive(true);
                    Debug.Log("[StickPickupStep] ✅ CuttingBoard activated");
                }
                else
                {
                    Debug.LogError("[StickPickupStep] ❌ CuttingBoard not found!");
                }
            }
            else
            {
                Debug.LogError("[StickPickupStep] ❌ CookingManager.Instance is null!");
            }
        }
        
        /// <summary>
        /// Station 비활성화
        /// </summary>
        private void DeactivateStations()
        {
            if (stickStation != null)
            {
                stickStation.SetActive(false);
                Debug.Log("[StickPickupStep] StickStation deactivated");
            }
            
            // CuttingBoard는 비활성화 안 함 (다음 단계에서도 사용)
        }
        
        // ==========================================
        // Event Handling
        // ==========================================
        
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeEvents()
        {
            if (stickStation != null)
            {
                stickStation.OnStationClicked += OnStickStationClicked;
                Debug.Log("[StickPickupStep] Subscribed to StickStation click event");
            }
        }
        
        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeEvents()
        {
            if (stickStation != null)
            {
                stickStation.OnStationClicked -= OnStickStationClicked;
            }
            
            // 꼬치 드래그 이벤트 구독 해제
            if (stickDraggable != null)
            {
                stickDraggable.OnDropped -= OnStickDropped;
                stickDraggable.OnDragCancelled -= OnStickDragCancelled;
            }
        }
        
        /// <summary>
        /// 꼬치통 클릭 시
        /// </summary>
        private void OnStickStationClicked(CookingStation station)
        {
            // 이미 꼬치가 생성되었으면 무시
            if (isStickCreated)
            {
                Debug.Log("[StickPickupStep] Stick already created - ignoring click");
                return;
            }
            
            Debug.Log("[StickPickupStep] 🖱️ StickStation CLICKED - Creating stick...");
            
            // 꼬치 생성
            CreateStick();
        }
        
        /// <summary>
        /// 꼬치가 드롭되었을 때
        /// </summary>
        private void OnStickDropped(DraggableObject obj, DropZone zone)
        {
            if (zone == null)
            {
                Debug.Log("[StickPickupStep] ❌ Stick dropped on INVALID zone - returning to origin");
                return;
            }
            
            Debug.Log($"[StickPickupStep] 📍 Stick dropped on: {zone.gameObject.name}");
            
            // ⭐ 도마에 드롭되었을 때 회전 0도로 변경
            CookingStation station = zone.GetComponent<CookingStation>();
            if (station != null && station.stationType == StationType.CuttingBoard)
            {
                // Z축 회전을 0도로
                if (stickObject != null)
                {
                    stickObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    Debug.Log("[StickPickupStep] 🔄 Stick rotation reset to 0 degrees");
                }
            }
            
            // CookingManager에 Process 요청
            if (CookingManager.Instance != null)
            {
                CookingManager.Instance.ProcessCurrentStep(zone);
            }
        }
        
        /// <summary>
        /// 꼬치 드래그 취소 (원상복귀)
        /// </summary>
        private void OnStickDragCancelled(DraggableObject obj)
        {
            Debug.Log("[StickPickupStep] 🔄 Stick drag cancelled - returning to station");
        }
        
        // ==========================================
        // Stick Creation
        // ==========================================
        
        /// <summary>
        /// 꼬치 Prefab 로드
        /// </summary>
        private void LoadStickPrefab()
        {
            // Resources 폴더에서 Prefab 로드
            stickPrefab = Resources.Load<GameObject>("Prefabs/Stick");  
            
            if (stickPrefab == null)
            {
                Debug.LogError("[StickPickupStep] ❌ Stick Prefab not found at Resources/Prefabs/Stick!");
                Debug.LogWarning("[StickPickupStep] Creating fallback stick prefab...");
                
                // Fallback: 간단한 꼬치 생성
                CreateFallbackStickPrefab();
            }
            else
            {
                Debug.Log("[StickPickupStep] ✅ Stick Prefab loaded successfully");
            }
        }
        
        /// <summary>
        /// Prefab이 없을 때 임시 꼬치 생성
        /// </summary>
        private void CreateFallbackStickPrefab()
        {
            stickPrefab = new GameObject("Stick_Fallback");
            
            // SpriteRenderer 추가 (임시로 흰색 사각형)
            SpriteRenderer sr = stickPrefab.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSimpleSprite();
            sr.sortingOrder = 10;
            sr.color = new Color(0.8f, 0.6f, 0.4f); // 나무색
            
            // BoxCollider2D 추가
            BoxCollider2D collider = stickPrefab.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.2f, 1.5f);
            
            Debug.Log("[StickPickupStep] ✅ Fallback stick prefab created");
        }
        
        /// <summary>
        /// 간단한 Sprite 생성 (Fallback용)
        /// </summary>
        private Sprite CreateSimpleSprite()
        {
            // 16x64 텍스처 (긴 막대 모양)
            Texture2D texture = new Texture2D(16, 64);
            
            // 나무색으로 채우기
            Color woodColor = new Color(0.8f, 0.6f, 0.4f);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    texture.SetPixel(x, y, woodColor);
                }
            }
            
            texture.Apply();
            
            return Sprite.Create(
                texture,
                new Rect(0, 0, 16, 64),
                new Vector2(0.5f, 0.5f),
                100f
            );
        }
        
        /// <summary>
        /// 꼬치 생성
        /// </summary>
        private void CreateStick()
        {
            if (stickPrefab == null)
            {
                Debug.LogError("[StickPickupStep] ❌ Cannot create stick - prefab is null");
                return;
            }
            
            if (stickStation == null)
            {
                Debug.LogError("[StickPickupStep] ❌ Cannot create stick - station is null");
                return;
            }
            
            // ItemPlacement 위치에 생성
            Vector3 spawnPosition = stickStation.GetItemPlacementPosition();
            
            // ⭐ Z축 90도 회전으로 생성
            Quaternion rotation = Quaternion.Euler(0f, 0f, 90f);
            stickObject = GameObject.Instantiate(stickPrefab, spawnPosition, rotation);
            stickObject.name = "Stick";
            
            // ⭐ SortingOrder 설정
            SpriteRenderer stickRenderer = stickObject.GetComponent<SpriteRenderer>();
            if (stickRenderer != null)
            {
                int stationSortingOrder = 4;
                SpriteRenderer stationRenderer = stickStation.GetComponentInChildren<SpriteRenderer>();
                if (stationRenderer != null)
                {
                    stationSortingOrder = stationRenderer.sortingOrder;
                }

                stickBaseSortingOrder = stationSortingOrder - 1;
                stickRenderer.sortingOrder = stickBaseSortingOrder;
                Debug.Log($"[StickPickupStep] Stick SortingOrder set to {stickBaseSortingOrder} (station {stationSortingOrder})");
            }
            
            Debug.Log($"[StickPickupStep] 🎯 Stick created at {spawnPosition} with rotation Z=90");
            
            // DraggableObject 컴포넌트 추가/가져오기
            stickDraggable = stickObject.GetComponent<DraggableObject>();
            if (stickDraggable == null)
            {
                stickDraggable = stickObject.AddComponent<DraggableObject>();
                Debug.Log("[StickPickupStep] Added DraggableObject component to stick");
            }
            
            // DraggableObject 설정
            SetupStickDraggable();
            
            // ⭐ 즉시 드래그 시작 (터치 위치로 이동)
            StartDragImmediately();
            
            // 생성 완료
            isStickCreated = true;
            
            // 효과음
            GameEvents.TriggerSFXRequested("StickPickup");
            
            Debug.Log("[StickPickupStep] ✅ Stick creation complete - drag started!");
        }
        
        /// <summary>
        /// 꼬치 DraggableObject 설정
        /// </summary>
        private void SetupStickDraggable()
        {
            if (stickDraggable == null) return;
            
            // 드래그 가능 설정
            stickDraggable.isDraggable = true;
            stickDraggable.draggingSortingOrder = 100;
            stickDraggable.returnSpeed = 10f;
            stickDraggable.dragScale = 1.1f;
            
            // 허용된 드롭존: CuttingBoard만
            stickDraggable.allowedDropZoneTags = new string[] { "CuttingBoard" };
            
            // 이벤트 구독
            stickDraggable.OnDropped += OnStickDropped;
            stickDraggable.OnDragCancelled += OnStickDragCancelled;
            
            Debug.Log("[StickPickupStep] ✅ Stick draggable setup complete");
            Debug.Log("[StickPickupStep]    - Allowed drop zones: CuttingBoard");
            Debug.Log("[StickPickupStep]    - Drag scale: 1.1x");
        }
        
        /// <summary>
        /// 즉시 드래그 시작 (터치 위치 추적)
        /// </summary>
        private void StartDragImmediately()
        {
            if (stickDraggable == null || stickObject == null) return;
            
            // 마우스/터치 위치로 즉시 이동
            Vector3 worldPosition = UnityEngine.Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPosition.z = stickObject.transform.position.z;
            
            // 꼬치를 마우스 위치로 이동
            stickObject.transform.position = worldPosition;
            
            // 드래그 상태 강제 시작
            stickDraggable.SimulateBeginDrag();
            
            Debug.Log("[StickPickupStep] 🚀 Stick drag started immediately at mouse position");
        }
    }
}