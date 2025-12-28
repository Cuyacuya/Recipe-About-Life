using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// Phase 4: 튀기기 핸들러
    /// 튀김기에서 시간 경과에 따라 색상 변화
    /// </summary>
    public class FryingHandler : MonoBehaviour
    {
        [Header("References")]
        public SimpleDropZone fryingZone;    // 튀김기 영역
        public SimpleDropZone coolingRack;   // 식힘망 영역

        [Header("Frying Time Settings")]
        public float timeRaw = 3f;           // Raw: 0-3초
        public float timeYellow = 7f;        // Yellow: 3-7초
        public float timeGolden = 9f;        // Golden: 7-9초 ✅
        public float timeBrown = 11f;        // Brown: 9-11초
        // 11초+ = Burnt

        private SimpleCookingManager manager;
        private SimpleDraggable draggable;
        private SpriteRenderer spriteRenderer;

        private bool isActive = false;
        private bool isFrying = false;
        private float fryingTime = 0f;
        private FryingState currentState = FryingState.Raw;
        private GameObject bubbleEffect;

        private void Start()
        {
            manager = SimpleCookingManager.Instance;
            draggable = GetComponent<SimpleDraggable>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (manager != null)
            {
                manager.OnPhaseChanged += OnPhaseChanged;
                
                // 참조 자동 설정
                if (fryingZone == null && manager.fryingStation != null) 
                    fryingZone = manager.fryingStation.GetComponent<SimpleDropZone>();
                if (coolingRack == null && manager.coolingRack != null) 
                    coolingRack = manager.coolingRack.GetComponent<SimpleDropZone>();
                
                Debug.Log($"[FryingHandler] fryingZone: {(fryingZone != null ? fryingZone.name : "null")}");
                Debug.Log($"[FryingHandler] coolingRack: {(coolingRack != null ? coolingRack.name : "null")}");
                
                // 동적으로 추가된 경우: 이미 Frying Phase면 바로 활성화
                if (manager.CurrentPhase == CookingPhase.Frying)
                {
                    OnPhaseChanged(CookingPhase.Frying);
                }
            }

            if (draggable != null)
            {
                draggable.OnDragStarted += OnDragStarted;
                draggable.OnDragEnded += OnDragEnded;
            }
            
            Debug.Log("[FryingHandler] Start 완료");
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnPhaseChanged -= OnPhaseChanged;
            }

            if (draggable != null)
            {
                draggable.OnDragStarted -= OnDragStarted;
                draggable.OnDragEnded -= OnDragEnded;
            }

            // 이펙트 정리
            if (bubbleEffect != null)
            {
                Destroy(bubbleEffect);
            }
        }

        private void OnPhaseChanged(CookingPhase phase)
        {
            isActive = (phase == CookingPhase.Frying);
            
            if (isActive && draggable != null)
            {
                draggable.isDraggable = true;
                Debug.Log("[FryingHandler] 튀김 단계 활성화");
            }
        }

        private void Update()
        {
            if (!isActive || !isFrying) return;

            // 튀김 시간 증가
            fryingTime += Time.deltaTime;

            // 상태 업데이트
            FryingState newState = GetFryingState(fryingTime);
            if (newState != currentState)
            {
                currentState = newState;
                UpdateFryingVisual();
                Debug.Log($"[FryingHandler] 튀김 상태: {currentState} ({fryingTime:F1}초)");
            }
        }

        private void OnDragStarted(SimpleDraggable d)
        {
            if (!isActive) return;

            // 튀김 중지
            isFrying = false;
            StopBubbleEffect();

            Debug.Log("[FryingHandler] 핫도그 집어올림");
        }

        private void OnDragEnded(SimpleDraggable d, SimpleDropZone dropZone)
        {
            if (!isActive) return;

            Debug.Log($"[FryingHandler] 드롭 감지 - dropZone: {(dropZone != null ? dropZone.name + " (" + dropZone.zoneType + ")" : "null")}");

            // 튀김기에 드롭 (ZoneType으로 비교)
            if (dropZone != null && dropZone.zoneType == SimpleDropZone.ZoneType.FryingStation)
            {
                Debug.Log("[FryingHandler] ✅ 튀김기에 드롭!");

                // 정해진 위치에 세로로 배치 (오프셋 적용)
                Vector3 fryingPos = dropZone.GetSnapPosition();
                if (manager != null)
                {
                    fryingPos += manager.fryingHotdogOffset;
                }
                d.MoveTo(fryingPos);
                d.SetRotation(90);

                // 튀김 시작
                StartFrying();
            }
            // 식힘망에 드롭 (ZoneType으로 비교)
            else if (dropZone != null && dropZone.zoneType == SimpleDropZone.ZoneType.CoolingRack)
            {
                Debug.Log($"[FryingHandler] ✅ 식힘망에 드롭! 튀김 상태: {currentState}");

                // 가로로 배치
                d.MoveTo(dropZone.GetSnapPosition());
                d.SetRotation(0);

                // 데이터 저장
                if (manager != null)
                {
                    manager.CurrentHotdogData.fryingState = currentState;
                    manager.CurrentHotdogData.fryingTime = fryingTime;
                }

                // 튀김 종료
                StopFrying();

                // 다음 단계로
                manager.NextPhase();
                manager.ShowToppingPopup();
            }
            else
            {
                Debug.Log("[FryingHandler] 드롭 실패 - 유효하지 않은 영역");
                
                // 이미 튀김 중이었다면 튀김기로 복귀
                if (fryingTime > 0 && fryingZone != null)
                {
                    Vector3 fryingPos = fryingZone.GetSnapPosition();
                    if (manager != null)
                    {
                        fryingPos += manager.fryingHotdogOffset;
                    }
                    d.MoveTo(fryingPos);
                    d.SetRotation(90);
                    StartFrying();
                }
            }
        }

        /// <summary>
        /// 시간에 따른 튀김 상태 계산
        /// </summary>
        private FryingState GetFryingState(float time)
        {
            if (time < timeRaw) return FryingState.Raw;
            if (time < timeYellow) return FryingState.Yellow;
            if (time < timeGolden) return FryingState.Golden;
            if (time < timeBrown) return FryingState.Brown;
            return FryingState.Burnt;
        }

        /// <summary>
        /// 튀김 시작
        /// </summary>
        private void StartFrying()
        {
            isFrying = true;
            StartBubbleEffect();
            Debug.Log($"[FryingHandler] 튀김 시작! (현재 {fryingTime:F1}초)");
        }

        /// <summary>
        /// 튀김 종료
        /// </summary>
        private void StopFrying()
        {
            isFrying = false;
            StopBubbleEffect();
            Debug.Log($"[FryingHandler] 튀김 종료! 최종: {currentState}");
        }

        /// <summary>
        /// 튀김 시각 효과 업데이트
        /// </summary>
        private void UpdateFryingVisual()
        {
            if (manager == null || spriteRenderer == null) return;

            Sprite newSprite = currentState switch
            {
                FryingState.Raw => manager.fryingRaw,
                FryingState.Yellow => manager.fryingYellow,
                FryingState.Golden => manager.fryingGolden,
                FryingState.Brown => manager.fryingBrown,
                FryingState.Burnt => manager.fryingBurnt,
                _ => null
            };

            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
            }
        }

        /// <summary>
        /// 보글보글 이펙트 시작
        /// </summary>
        private void StartBubbleEffect()
        {
            if (manager == null || manager.fryingBubblePrefab == null) return;
            if (bubbleEffect != null) return;

            // 이펙트 위치 계산 (핫도그 위치 + 오프셋)
            Vector3 effectPos = transform.position + manager.fryingEffectOffset;

            bubbleEffect = Instantiate(manager.fryingBubblePrefab, effectPos, Quaternion.identity);
            Debug.Log($"[FryingHandler] 보글보글 이펙트 시작 - 위치: {effectPos}");
        }

        /// <summary>
        /// 보글보글 이펙트 종료
        /// </summary>
        private void StopBubbleEffect()
        {
            if (bubbleEffect != null)
            {
                Destroy(bubbleEffect);
                bubbleEffect = null;
            }
        }
    }
}