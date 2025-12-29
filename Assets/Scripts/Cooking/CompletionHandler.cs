using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// Phase 6: 완료 핸들러
    /// 완성된 핫도그를 창문에 드롭하여 제공
    /// </summary>
    public class CompletionHandler : MonoBehaviour
    {
        private SimpleCookingManager manager;
        private SimpleDraggable currentDraggable;
        private bool isListening = false;

        private void Start()
        {
            manager = SimpleCookingManager.Instance;
            
            if (manager == null)
            {
                Debug.LogError("[CompletionHandler] SimpleCookingManager를 찾을 수 없습니다!");
                return;
            }

            // Phase 변경 이벤트 구독
            manager.OnPhaseChanged += OnPhaseChanged;
            Debug.Log("[CompletionHandler] 초기화 완료");
        }

        private void OnDestroy()
        {
            if (manager != null)
            {
                manager.OnPhaseChanged -= OnPhaseChanged;
            }

            // 드래그 이벤트 정리
            CleanupDragEvents();
        }

        /// <summary>
        /// Phase 변경 처리
        /// </summary>
        private void OnPhaseChanged(CookingPhase phase)
        {
            if (phase == CookingPhase.Completed)
            {
                Debug.Log("[CompletionHandler] Phase 6 (Completed) 진입 - 드래그 활성화");
                EnableHotdogDrag();
            }
            else
            {
                // 다른 Phase로 변경되면 이벤트 정리
                CleanupDragEvents();
            }
        }

        /// <summary>
        /// 핫도그 드래그 활성화
        /// </summary>
        private void EnableHotdogDrag()
        {
            GameObject hotdog = manager.CurrentHotdog;
            
            if (hotdog == null)
            {
                Debug.LogError("[CompletionHandler] CurrentHotdog가 null입니다!");
                return;
            }

            // SimpleDraggable 컴포넌트 확인
            currentDraggable = hotdog.GetComponent<SimpleDraggable>();
            
            if (currentDraggable == null)
            {
                Debug.LogError("[CompletionHandler] 핫도그에 SimpleDraggable 컴포넌트가 없습니다!");
                return;
            }

            // 드래그 활성화
            currentDraggable.isDraggable = true;
            
            // 기존 이벤트 제거 후 새로 등록 (중복 방지)
            currentDraggable.OnDragEnded -= OnHotdogDropped;
            currentDraggable.OnDragEnded += OnHotdogDropped;
            
            isListening = true;

            Debug.Log("[CompletionHandler] ✅ 핫도그 드래그 활성화됨 - 창문에 드롭하여 제공하세요!");
        }

        /// <summary>
        /// 핫도그 드롭 처리
        /// </summary>
        private void OnHotdogDropped(SimpleDraggable draggable, SimpleDropZone dropZone)
        {
            if (dropZone == null)
            {
                Debug.Log("[CompletionHandler] 드롭존 없음 - 원위치로 복귀");
                draggable.ReturnToOrigin();
                return;
            }

            Debug.Log($"[CompletionHandler] 드롭 감지: {dropZone.zoneType}");

            // 창문에 드롭했는지 확인
            if (dropZone.zoneType == SimpleDropZone.ZoneType.ServingWindow)
            {
                Debug.Log("[CompletionHandler] ✅ 창문에 드롭! 요리 제공 처리");
                
                // 이벤트 정리 (핫도그가 곧 제거됨)
                CleanupDragEvents();
                
                // 요리 제공
                manager.ServeHotdog();
            }
            else
            {
                Debug.Log("[CompletionHandler] 창문이 아닌 곳에 드롭 - 원위치로 복귀");
                draggable.ReturnToOrigin();
            }
        }

        /// <summary>
        /// 드래그 이벤트 정리
        /// </summary>
        private void CleanupDragEvents()
        {
            if (currentDraggable != null && isListening)
            {
                currentDraggable.OnDragEnded -= OnHotdogDropped;
                isListening = false;
            }
            currentDraggable = null;
        }
    }
}
