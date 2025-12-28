using UnityEngine;
using System.Collections;

namespace RecipeAboutLife.Camera
{
    /// <summary>
    /// 카메라 줌 컨트롤러
    /// 스토리 NPC에게 카메라를 확대하여 연출
    /// </summary>
    public class CameraZoomController : MonoBehaviour
    {
        // ==========================================
        // Singleton Pattern
        // ==========================================

        private static CameraZoomController _instance;
        public static CameraZoomController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CameraZoomController>();
                }
                return _instance;
            }
        }

        // ==========================================
        // Configuration
        // ==========================================

        [Header("줌 설정")]
        [SerializeField]
        [Tooltip("줌 인 시 카메라 크기 (Orthographic Size)")]
        private float zoomInSize = 2f;

        [SerializeField]
        [Tooltip("줌 인 시간 (초)")]
        private float zoomInDuration = 1f;

        [SerializeField]
        [Tooltip("줌 아웃 시간 (초)")]
        private float zoomOutDuration = 1f;

        [SerializeField]
        [Tooltip("줌 인 시 Y 오프셋 (NPC 위치에서 얼마나 위로)")]
        private float targetYOffset = 0.5f;

        // ==========================================
        // State
        // ==========================================

        private UnityEngine.Camera targetCamera;
        private Vector3 originalPosition;
        private float originalSize;
        private bool isZooming = false;
        private Coroutine zoomCoroutine = null;

        // ==========================================
        // Events
        // ==========================================

        /// <summary>
        /// 줌 인 완료 시 발생
        /// </summary>
        public System.Action OnZoomInComplete;

        /// <summary>
        /// 줌 아웃 완료 시 발생
        /// </summary>
        public System.Action OnZoomOutComplete;

        // ==========================================
        // Lifecycle
        // ==========================================

        private void Awake()
        {
            // Singleton 설정
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // 메인 카메라 찾기
            targetCamera = UnityEngine.Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("[CameraZoomController] 메인 카메라를 찾을 수 없습니다!");
                return;
            }

            // 원래 카메라 설정 저장
            originalPosition = targetCamera.transform.position;
            originalSize = targetCamera.orthographicSize;

            Debug.Log($"[CameraZoomController] 초기화 완료 - 원래 위치: {originalPosition}, 크기: {originalSize}");
        }

        // ==========================================
        // Public API
        // ==========================================

        /// <summary>
        /// 특정 NPC에게 줌 인
        /// </summary>
        /// <param name="targetNPC">줌 인할 NPC GameObject</param>
        public void ZoomToNPC(GameObject targetNPC)
        {
            if (targetNPC == null)
            {
                Debug.LogError("[CameraZoomController] 대상 NPC가 null입니다!");
                return;
            }

            if (isZooming)
            {
                Debug.LogWarning("[CameraZoomController] 이미 줌 중입니다!");
                return;
            }

            // NPC 위치 가져오기
            Vector3 npcPosition = targetNPC.transform.position;
            Vector3 targetPosition = new Vector3(
                npcPosition.x,
                npcPosition.y + targetYOffset,
                originalPosition.z  // Z 위치는 유지
            );

            Debug.Log($"[CameraZoomController] NPC({targetNPC.name})에게 줌 인 시작 - 목표 위치: {targetPosition}");

            // 줌 인 시작
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            zoomCoroutine = StartCoroutine(ZoomInCoroutine(targetPosition));
        }

        /// <summary>
        /// 원래 카메라 위치로 복귀
        /// </summary>
        public void ZoomOut()
        {
            if (!isZooming)
            {
                Debug.LogWarning("[CameraZoomController] 줌 상태가 아닙니다!");
                return;
            }

            Debug.Log("[CameraZoomController] 줌 아웃 시작");

            // 줌 아웃 시작
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
            }
            zoomCoroutine = StartCoroutine(ZoomOutCoroutine());
        }

        /// <summary>
        /// 현재 줌 상태 확인
        /// </summary>
        public bool IsZooming()
        {
            return isZooming;
        }

        /// <summary>
        /// 원래 위치 강제 복귀 (즉시)
        /// </summary>
        public void ResetToOriginal()
        {
            if (zoomCoroutine != null)
            {
                StopCoroutine(zoomCoroutine);
                zoomCoroutine = null;
            }

            if (targetCamera != null)
            {
                targetCamera.transform.position = originalPosition;
                targetCamera.orthographicSize = originalSize;
            }

            isZooming = false;

            Debug.Log("[CameraZoomController] 카메라 즉시 복귀");
        }

        // ==========================================
        // Zoom Coroutines
        // ==========================================

        /// <summary>
        /// 줌 인 코루틴
        /// </summary>
        private IEnumerator ZoomInCoroutine(Vector3 targetPosition)
        {
            isZooming = true;

            Vector3 startPosition = targetCamera.transform.position;
            float startSize = targetCamera.orthographicSize;

            float elapsedTime = 0f;

            while (elapsedTime < zoomInDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / zoomInDuration);

                // Ease-in-out 적용
                t = Mathf.SmoothStep(0f, 1f, t);

                // 위치 보간
                targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                // 크기 보간
                targetCamera.orthographicSize = Mathf.Lerp(startSize, zoomInSize, t);

                yield return null;
            }

            // 최종 위치/크기
            targetCamera.transform.position = targetPosition;
            targetCamera.orthographicSize = zoomInSize;

            Debug.Log($"[CameraZoomController] 줌 인 완료 - 위치: {targetPosition}, 크기: {zoomInSize}");

            // 이벤트 발생
            OnZoomInComplete?.Invoke();

            zoomCoroutine = null;
        }

        /// <summary>
        /// 줌 아웃 코루틴
        /// </summary>
        private IEnumerator ZoomOutCoroutine()
        {
            Vector3 startPosition = targetCamera.transform.position;
            float startSize = targetCamera.orthographicSize;

            float elapsedTime = 0f;

            while (elapsedTime < zoomOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / zoomOutDuration);

                // Ease-in-out 적용
                t = Mathf.SmoothStep(0f, 1f, t);

                // 위치 보간
                targetCamera.transform.position = Vector3.Lerp(startPosition, originalPosition, t);

                // 크기 보간
                targetCamera.orthographicSize = Mathf.Lerp(startSize, originalSize, t);

                yield return null;
            }

            // 최종 위치/크기
            targetCamera.transform.position = originalPosition;
            targetCamera.orthographicSize = originalSize;

            isZooming = false;

            Debug.Log($"[CameraZoomController] 줌 아웃 완료 - 원래 위치: {originalPosition}, 크기: {originalSize}");

            // 이벤트 발생
            OnZoomOutComplete?.Invoke();

            zoomCoroutine = null;
        }

        // ==========================================
        // Debug
        // ==========================================

#if UNITY_EDITOR
        [ContextMenu("Test: Zoom In")]
        private void TestZoomIn()
        {
            // 테스트용: 현재 NPC 찾아서 줌 인
            NPC.NPCSpawnManager spawnManager = FindFirstObjectByType<NPC.NPCSpawnManager>();
            if (spawnManager != null)
            {
                GameObject currentNPC = spawnManager.GetCurrentNPC();
                if (currentNPC != null)
                {
                    ZoomToNPC(currentNPC);
                }
                else
                {
                    Debug.LogWarning("[Test] 현재 NPC가 없습니다!");
                }
            }
        }

        [ContextMenu("Test: Zoom Out")]
        private void TestZoomOut()
        {
            ZoomOut();
        }

        [ContextMenu("Test: Reset")]
        private void TestReset()
        {
            ResetToOriginal();
        }
#endif
    }
}
