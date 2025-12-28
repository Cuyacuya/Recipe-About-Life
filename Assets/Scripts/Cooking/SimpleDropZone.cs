using UnityEngine;
using System;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 간단한 드롭존
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SimpleDropZone : MonoBehaviour
    {
        [Header("Settings")]
        public bool isDroppable = true;
        public ZoneType zoneType = ZoneType.None;

        [Header("Snap Settings")]
        public bool snapToCenter = true;
        public Transform snapPosition;  // null이면 Collider center 사용

        // 이벤트
        public event Action<SimpleDraggable> OnObjectDropped;

        private Collider2D zoneCollider;

        public enum ZoneType
        {
            None,
            CuttingBoard,   // 도마
            BatterZone,     // 반죽통
            FryingStation,  // 튀김기
            CoolingRack,    // 식힘망
            StickDropZone,  // 팝업 내 꼬치 드롭존
            SugarTray       // 설탕 트레이 (토핑 팝업)
        }

        private void Awake()
        {
            zoneCollider = GetComponent<Collider2D>();
            if (zoneCollider != null)
            {
                zoneCollider.isTrigger = true;
            }
        }

        /// <summary>
        /// 드롭 처리
        /// </summary>
        public void HandleDrop(SimpleDraggable draggable)
        {
            if (!isDroppable || draggable == null) return;

            Debug.Log($"[SimpleDropZone] Object dropped: {draggable.name} on {gameObject.name} ({zoneType})");
            OnObjectDropped?.Invoke(draggable);
        }

        /// <summary>
        /// 스냅 위치 반환
        /// </summary>
        public Vector3 GetSnapPosition()
        {
            if (snapPosition != null)
            {
                return snapPosition.position;
            }
            
            if (zoneCollider != null)
            {
                return zoneCollider.bounds.center;
            }

            return transform.position;
        }

        /// <summary>
        /// 포인트가 영역 내에 있는지 확인
        /// </summary>
        public bool ContainsPoint(Vector2 point)
        {
            if (zoneCollider == null) return false;
            return zoneCollider.OverlapPoint(point);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Gizmos.color = isDroppable ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);

                // 스냅 위치 표시
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(GetSnapPosition(), 0.2f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"{zoneType}"
            );
        }
#endif
    }
}