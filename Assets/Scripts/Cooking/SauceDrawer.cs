using UnityEngine;
using System.Collections.Generic;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 소스 그리기 (스프라이트 트레일 방식)
    /// 핫도그 위에서 드래그하며 소스 그리기
    /// </summary>
    public class SauceDrawer : MonoBehaviour
    {
        [Header("=== Sauce Sprites ===")]
        public Sprite ketchupDotSprite;      // 케첩 점 스프라이트
        public Sprite mustardDotSprite;      // 머스타드 점 스프라이트

        [Header("=== Settings ===")]
        public float dotSpacing = 0.1f;      // 점 사이 간격
        public float dotScale = 0.3f;        // 점 크기
        public int ketchupSortingOrder = 53; // 케첩 레이어
        public int mustardSortingOrder = 54; // 머스타드 레이어

        [Header("=== Draw Area ===")]
        public Collider2D drawArea;          // 소스 그릴 수 있는 영역 (핫도그 위)

        // 그려진 소스 점들
        private List<GameObject> ketchupDots = new List<GameObject>();
        private List<GameObject> mustardDots = new List<GameObject>();

        // 현재 그리기 상태
        private bool isDrawing = false;
        private bool isActuallyDrawing = false;  // 실제로 드래그하며 그리는 중인지
        private ToppingType currentDrawingType = ToppingType.None;
        private Vector3 lastDotPosition;

        // 이벤트
        public System.Action<float> OnSauceUsed;  // 소스 사용량 콜백

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        /// <summary>
        /// 소스 그리기 시작
        /// </summary>
        public void StartDrawing(ToppingType type)
        {
            if (type == ToppingType.None) return;

            currentDrawingType = type;
            isDrawing = true;
            isActuallyDrawing = false;
            lastDotPosition = Vector3.zero;

            Debug.Log($"[SauceDrawer] {type} 그리기 시작");
        }

        /// <summary>
        /// 소스 그리기 중지
        /// </summary>
        public void StopDrawing()
        {
            // 소리 정지
            if (isActuallyDrawing)
            {
                AudioManager.Instance?.StopSauceLoop();
                isActuallyDrawing = false;
            }

            isDrawing = false;
            currentDrawingType = ToppingType.None;
            Debug.Log("[SauceDrawer] 그리기 중지");
        }

        private void Update()
        {
            if (!isDrawing) return;

            // 마우스/터치 입력 처리
            if (Input.GetMouseButton(0))
            {
                if (mainCamera == null)
                {
                    mainCamera = Camera.main;
                    if (mainCamera == null) return;
                }
                
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0;

                // 그리기 영역 안인지 확인
                bool inArea = IsInDrawArea(mousePos);
                
                if (inArea)
                {
                    // 실제로 그리기 시작 - 소리 재생
                    if (!isActuallyDrawing)
                    {
                        isActuallyDrawing = true;
                        AudioManager.Instance?.PlaySauceLoop();
                        Debug.Log("[SauceDrawer] 소스 뿌리기 시작 - 소리 재생");
                    }
                    
                    TryDrawDot(mousePos);
                }
                else
                {
                    // 영역 밖으로 나감 - 소리 정지
                    if (isActuallyDrawing)
                    {
                        isActuallyDrawing = false;
                        AudioManager.Instance?.StopSauceLoop();
                        Debug.Log("[SauceDrawer] 영역 밖 - 소리 정지");
                    }
                }
            }
            else
            {
                // 마우스 버튼 뗌 - 소리 정지 (그리기 모드는 유지)
                if (isActuallyDrawing)
                {
                    isActuallyDrawing = false;
                    AudioManager.Instance?.StopSauceLoop();
                    Debug.Log("[SauceDrawer] 드래그 종료 - 소리 정지");
                }
            }
        }

        /// <summary>
        /// 그리기 영역 안인지 확인
        /// </summary>
        private bool IsInDrawArea(Vector2 point)
        {
            // drawArea가 null이면 항상 그리기 허용
            if (drawArea == null) 
            {
                return true;
            }
            return drawArea.OverlapPoint(point);
        }

        /// <summary>
        /// 점 그리기 시도
        /// </summary>
        private void TryDrawDot(Vector3 position)
        {
            // 첫 점이거나 간격이 충분하면 그리기
            if (lastDotPosition == Vector3.zero || 
                Vector3.Distance(position, lastDotPosition) >= dotSpacing)
            {
                Debug.Log($"[SauceDrawer] 점 그리기: {position}, 타입: {currentDrawingType}");
                CreateDot(position);
                lastDotPosition = position;

                // 소스 사용량 콜백
                OnSauceUsed?.Invoke(dotSpacing);
            }
        }

        /// <summary>
        /// 소스 점 생성
        /// </summary>
        private void CreateDot(Vector3 position)
        {
            Sprite dotSprite = currentDrawingType == ToppingType.Ketchup 
                ? ketchupDotSprite 
                : mustardDotSprite;

            if (dotSprite == null)
            {
                Debug.LogError($"[SauceDrawer] ❌ {currentDrawingType} 스프라이트가 Inspector에서 할당되지 않았습니다!");
                return;
            }

            // 점 오브젝트 생성
            GameObject dot = new GameObject($"SauceDot_{currentDrawingType}");
            dot.transform.SetParent(transform);
            dot.transform.position = position;
            dot.transform.localScale = Vector3.one * dotScale;

            // SpriteRenderer 추가
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = dotSprite;
            sr.sortingOrder = currentDrawingType == ToppingType.Ketchup 
                ? ketchupSortingOrder 
                : mustardSortingOrder;

            // 리스트에 추가
            if (currentDrawingType == ToppingType.Ketchup)
                ketchupDots.Add(dot);
            else
                mustardDots.Add(dot);
        }

        /// <summary>
        /// 모든 소스 점 삭제
        /// </summary>
        public void ClearAllDots()
        {
            foreach (var dot in ketchupDots)
            {
                if (dot != null) Destroy(dot);
            }
            ketchupDots.Clear();

            foreach (var dot in mustardDots)
            {
                if (dot != null) Destroy(dot);
            }
            mustardDots.Clear();

            Debug.Log("[SauceDrawer] 모든 소스 점 삭제");
        }

        /// <summary>
        /// 특정 타입 소스 점 삭제
        /// </summary>
        public void ClearDots(ToppingType type)
        {
            List<GameObject> dots = type == ToppingType.Ketchup ? ketchupDots : mustardDots;
            
            foreach (var dot in dots)
            {
                if (dot != null) Destroy(dot);
            }
            dots.Clear();
        }

        /// <summary>
        /// 그려진 소스 점들 반환 (메인 화면 반영용)
        /// </summary>
        public List<SauceDotData> GetAllSauceDots()
        {
            List<SauceDotData> result = new List<SauceDotData>();

            foreach (var dot in ketchupDots)
            {
                if (dot != null)
                {
                    result.Add(new SauceDotData
                    {
                        type = ToppingType.Ketchup,
                        localPosition = dot.transform.localPosition,
                        scale = dot.transform.localScale.x
                    });
                }
            }

            foreach (var dot in mustardDots)
            {
                if (dot != null)
                {
                    result.Add(new SauceDotData
                    {
                        type = ToppingType.Mustard,
                        localPosition = dot.transform.localPosition,
                        scale = dot.transform.localScale.x
                    });
                }
            }

            return result;
        }

        /// <summary>
        /// 소스 점이 있는지 확인
        /// </summary>
        public bool HasAnySauce()
        {
            return ketchupDots.Count > 0 || mustardDots.Count > 0;
        }

        public bool HasKetchup() => ketchupDots.Count > 0;
        public bool HasMustard() => mustardDots.Count > 0;
    }

    /// <summary>
    /// 소스 점 데이터 (메인 화면 반영용)
    /// </summary>
    [System.Serializable]
    public class SauceDotData
    {
        public ToppingType type;
        public Vector3 localPosition;
        public float scale;
    }
}