using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// Phase 3: 반죽 입히기 핸들러
    /// 도마의 핫도그에 부착 - 반죽통에서 드래그 유지 시 반죽 증가
    /// </summary>
    public class BatterHandler : MonoBehaviour
    {
        [Header("References")]
        public SimpleDropZone batterZone;    // 반죽통 영역

        [Header("Settings")]
        public float firstStageDelay = 0.7f; // 1단계 진입까지 시간 (초)
        public float timePerStage = 1.5f;    // 단계당 시간 (초)
        public int maxStage = 3;             // 최대 단계

        private SimpleCookingManager manager;
        private SimpleDraggable draggable;
        private SpriteRenderer spriteRenderer;

        private bool isActive = false;
        private bool isInBatterZone = false;
        private float batterTime = 0f;
        private int currentStage = 0;

        private void Start()
        {
            manager = SimpleCookingManager.Instance;
            draggable = GetComponent<SimpleDraggable>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (manager != null)
            {
                manager.OnPhaseChanged += OnPhaseChanged;
                
                // batterZone 자동 설정 (할당 안 된 경우)
                if (batterZone == null && manager.batterZone != null)
                {
                    batterZone = manager.batterZone.GetComponent<SimpleDropZone>();
                    Debug.Log($"[BatterHandler] batterZone 자동 설정: {(batterZone != null ? batterZone.name : "null")}");
                }
                
                // 동적으로 추가된 경우: 이미 Batter Phase면 바로 활성화
                if (manager.CurrentPhase == CookingPhase.Batter)
                {
                    OnPhaseChanged(CookingPhase.Batter);
                }
            }

            if (draggable != null)
            {
                draggable.OnDragStarted += OnDragStarted;
                draggable.OnDragging += OnDragging;
                draggable.OnDragEnded += OnDragEnded;
            }
            
            Debug.Log("[BatterHandler] Start 완료");
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
                draggable.OnDragging -= OnDragging;
                draggable.OnDragEnded -= OnDragEnded;
            }
        }

        private void OnPhaseChanged(CookingPhase phase)
        {
            isActive = (phase == CookingPhase.Batter);
            
            if (isActive && draggable != null)
            {
                draggable.isDraggable = true;
                Debug.Log("[BatterHandler] 반죽 단계 활성화 - 핫도그 드래그 가능");
            }
        }

        private void OnDragStarted(SimpleDraggable d)
        {
            if (!isActive) return;
            Debug.Log("[BatterHandler] 핫도그 드래그 시작");
        }

        private void OnDragging(SimpleDraggable d)
        {
            if (!isActive || batterZone == null) return;

            // 반죽통 영역 안에 있는지 확인
            bool wasInZone = isInBatterZone;
            isInBatterZone = batterZone.ContainsPoint(transform.position);

            if (isInBatterZone)
            {
                if (!wasInZone)
                {
                    Debug.Log("[BatterHandler] 반죽통 진입!");
                    // 반죽 소리 시작
                    AudioManager.Instance?.PlayBatteringLoop();
                }

                // 시간 누적
                batterTime += Time.deltaTime;

                // 단계 계산 (firstStageDelay 이후부터 1단계 시작)
                int newStage = 0;
                if (batterTime >= firstStageDelay)
                {
                    newStage = Mathf.Min(
                        Mathf.FloorToInt((batterTime - firstStageDelay) / timePerStage) + 1, 
                        maxStage
                    );
                }
                
                if (newStage != currentStage)
                {
                    currentStage = newStage;
                    UpdateBatterVisual();
                    Debug.Log($"[BatterHandler] 반죽 {currentStage}단계! ({batterTime:F1}초)");
                }
            }
            else
            {
                // 반죽통에서 나감 - 소리 정지
                if (wasInZone)
                {
                    AudioManager.Instance?.StopBatteringLoop();
                }
            }
        }

        private void OnDragEnded(SimpleDraggable d, SimpleDropZone dropZone)
        {
            if (!isActive) return;

            // 반죽 소리 정지
            AudioManager.Instance?.StopBatteringLoop();

            Debug.Log($"[BatterHandler] 드래그 종료 - 반죽 {currentStage}단계");

            // 반죽이 1단계 이상이면 다음 단계로
            if (currentStage >= 1)
            {
                // 데이터 저장
                if (manager != null)
                {
                    manager.CurrentHotdogData.batterStage = currentStage;
                }

                // 반죽통 안에 세로로 배치
                if (batterZone != null)
                {
                    d.MoveTo(batterZone.GetSnapPosition());
                    d.SetRotation(90); // 세로로
                }

                d.isDraggable = true; // 다음 단계를 위해 드래그 가능 유지

                // 반죽 3단계 도달 시 다음 단계로
                if (currentStage >= maxStage)
                {
                    Debug.Log("[BatterHandler] ✅ 반죽 최대! 다음 단계로");
                    
                    // BatterHandler 비활성화, FryingHandler로 전환
                    isActive = false;
                    
                    // FryingHandler 활성화를 위해 컴포넌트 추가/활성화
                    FryingHandler fryingHandler = gameObject.GetComponent<FryingHandler>();
                    if (fryingHandler == null)
                    {
                        fryingHandler = gameObject.AddComponent<FryingHandler>();
                    }
                    
                    manager.NextPhase();
                }
            }
            else
            {
                // 반죽 안 됨 - 도마로 복귀
                Debug.Log("[BatterHandler] 반죽 안 됨 - 복귀");
                d.ReturnToOrigin();
            }

            // 초기화
            batterTime = 0f;
            isInBatterZone = false;
        }

        /// <summary>
        /// 반죽 시각 효과 업데이트
        /// </summary>
        private void UpdateBatterVisual()
        {
            if (manager == null || spriteRenderer == null) return;

            // 반죽 1단계 시작 시 자식 재료들 삭제
            if (currentStage == 1)
            {
                RemoveIngredientChildren();
            }

            Sprite newSprite = currentStage switch
            {
                1 => manager.batterStage1,
                2 => manager.batterStage2,
                3 => manager.batterStage3,
                _ => null
            };

            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
                
                // 반죽 스케일 적용
                transform.localScale = manager.batterScale;
                
                Debug.Log($"[BatterHandler] 스프라이트 변경: 반죽 {currentStage}단계, 스케일: {manager.batterScale}");
            }
        }

        /// <summary>
        /// 자식 재료 오브젝트들 삭제
        /// </summary>
        private void RemoveIngredientChildren()
        {
            // "Ingredient_" 로 시작하는 자식들 찾아서 삭제
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Ingredient_"))
                {
                    Debug.Log($"[BatterHandler] 재료 삭제: {child.name}");
                    Destroy(child.gameObject);
                }
            }
        }
    }
}