using UnityEngine;
using System;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 요리 스테이션 (꼬치통, 도마, 식힘망 등)
    /// 각 요리 단계에 필요한 상호작용 영역
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class CookingStation : MonoBehaviour
    {
        [Header("Station Info")]
        [Tooltip("스테이션 타입")]
        public StationType stationType;
        
        [Tooltip("이 스테이션을 사용하는 요리 단계")]
        public CookingStepType requiredStepType;
        
        [Header("Item Placement")]
        [Tooltip("아이템 배치 위치 (자식 Transform)")]
        public Transform itemPlacementPosition;
        
        [Header("Interaction")]
        [Tooltip("클릭/터치 가능 여부")]
        public bool isInteractable = true;
        
        [Tooltip("하이라이트 표시 여부")]
        public bool showHighlight = true;
        
        [Tooltip("하이라이트 색상")]
        public Color highlightColor = new Color(1f, 1f, 0f, 0.3f);
        
        // 컴포넌트
        private SpriteRenderer spriteRenderer;
        private Collider2D col2D;
        private Color originalColor;
        
        // 상태
        private bool isActive = false;
        private bool isHighlighted = false;
        
        // 이벤트 (C# 이벤트만 사용)
        public event Action<CookingStation> OnStationClicked;
        public event Action<CookingStation> OnStationHoverEnter;
        public event Action<CookingStation> OnStationHoverExit;
        
        private void Awake()
        {
            // 컴포넌트 가져오기
            spriteRenderer = GetComponent<SpriteRenderer>();
            col2D = GetComponent<Collider2D>();
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            
            // Collider를 Trigger로 설정
            if (col2D != null)
            {
                col2D.isTrigger = true;
            }
            
            // 아이템 배치 위치가 없으면 자동 생성
            if (itemPlacementPosition == null)
            {
                GameObject placementObj = new GameObject("ItemPlacement");
                placementObj.transform.SetParent(transform);
                placementObj.transform.localPosition = Vector3.zero;
                itemPlacementPosition = placementObj.transform;
            }
        }
        
        /// <summary>
        /// 스테이션 활성화
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
            
            // 비활성화 시 하이라이트 제거
            if (!active && isHighlighted)
            {
                SetHighlight(false);
            }
            
            Debug.Log($"[CookingStation] {stationType} active: {active}");
        }
        
        /// <summary>
        /// 하이라이트 표시/숨김
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            if (!showHighlight || spriteRenderer == null) return;
            
            isHighlighted = highlight;
            
            if (highlight)
            {
                spriteRenderer.color = highlightColor;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
        
        /// <summary>
        /// 마우스/터치 클릭 감지
        /// </summary>
        private void OnMouseDown()
        {
            if (!isActive || !isInteractable) return;
            
            Debug.Log($"[CookingStation] {stationType} clicked!");
            
            // C# 이벤트 발생
            OnStationClicked?.Invoke(this);
        }
        
        /// <summary>
        /// 마우스 호버 시작
        /// </summary>
        private void OnMouseEnter()
        {
            if (!isActive || !isInteractable) return;
            
            SetHighlight(true);
            OnStationHoverEnter?.Invoke(this);
        }
        
        /// <summary>
        /// 마우스 호버 종료
        /// </summary>
        private void OnMouseExit()
        {
            if (!isActive || !isInteractable) return;
            
            SetHighlight(false);
            OnStationHoverExit?.Invoke(this);
        }
        
        /// <summary>
        /// 아이템 배치 위치 가져오기
        /// </summary>
        public Vector3 GetItemPlacementPosition()
        {
            return itemPlacementPosition != null ? itemPlacementPosition.position : transform.position;
        }
        
        /// <summary>
        /// 스테이션 타입 확인
        /// </summary>
        public bool IsStationType(StationType type)
        {
            return stationType == type;
        }
        
        /// <summary>
        /// 요리 단계 확인
        /// </summary>
        public bool IsForStep(CookingStepType stepType)
        {
            return requiredStepType == stepType;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 아이템 배치 위치 표시
            if (itemPlacementPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(itemPlacementPosition.position, 0.2f);
                Gizmos.DrawLine(transform.position, itemPlacementPosition.position);
            }
            
            // Collider 영역 표시
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.color = isActive ? Color.yellow : Color.gray;
                
                if (col is BoxCollider2D box)
                {
                    Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
                }
                else if (col is CircleCollider2D circle)
                {
                    Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // 선택 시 상세 정보 표시
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1f,
                $"{stationType}\n{requiredStepType}"
            );
        }
#endif
    }

    // ============================================
    // Enums
    // ============================================

    /// <summary>
    /// 스테이션 타입
    /// </summary>
    public enum StationType
    {
        StickStation,      // 꼬치통
        CuttingBoard,      // 도마
        CoolingRack,       // 식힘망
        BatterStation,     // 반죽 스테이션
        FryingStation,     // 튀김기
        ToppingStation     // 토핑 스테이션
    }

} // namespace RecipeAboutLife.Cooking