using UnityEngine;
using System;
using System.Collections.Generic;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 간단한 드래그 가능 오브젝트
    /// </summary>
    public class SimpleDraggable : MonoBehaviour
    {
        [Header("Settings")]
        public bool isDraggable = true;
        public int draggingSortingOrder = 100;

        // 이벤트
        public event Action<SimpleDraggable> OnDragStarted;
        public event Action<SimpleDraggable> OnDragging;
        public event Action<SimpleDraggable, SimpleDropZone> OnDragEnded;

        // 상태
        private bool isDragging = false;
        private Vector3 originalPosition;
        private int originalSortingOrder;
        private SpriteRenderer spriteRenderer;
        private UnityEngine.Camera mainCamera;

        // 자식 SpriteRenderer들의 원래 Sorting Order 저장
        private Dictionary<SpriteRenderer, int> childOriginalSortingOrders = new Dictionary<SpriteRenderer, int>();

        public bool IsDragging => isDragging;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = UnityEngine.Camera.main;
        }

        private void OnMouseDown()
        {
            Debug.Log($"[SimpleDraggable] OnMouseDown 호출됨: {gameObject.name}, isDraggable: {isDraggable}");
            if (!isDraggable) return;
            StartDrag();
        }

        private void OnMouseDrag()
        {
            if (!isDragging) return;
            UpdateDrag();
        }

        private void OnMouseUp()
        {
            if (!isDragging) return;
            EndDrag();
        }

        /// <summary>
        /// 드래그 시작
        /// </summary>
        public void StartDrag()
        {
            if (!isDraggable) return;

            isDragging = true;
            originalPosition = transform.position;

            // 본체 Sorting Order 올리기
            if (spriteRenderer != null)
            {
                originalSortingOrder = spriteRenderer.sortingOrder;
                spriteRenderer.sortingOrder = draggingSortingOrder;
            }

            // 자식들 Sorting Order도 올리기
            childOriginalSortingOrders.Clear();
            SpriteRenderer[] childRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in childRenderers)
            {
                if (sr != spriteRenderer) // 본체 제외
                {
                    childOriginalSortingOrders[sr] = sr.sortingOrder;
                    sr.sortingOrder = draggingSortingOrder + 1; // 본체보다 위에
                }
            }

            OnDragStarted?.Invoke(this);
            Debug.Log($"[SimpleDraggable] Drag started: {gameObject.name}");
        }

        /// <summary>
        /// 드래그 중 (외부에서 호출 가능)
        /// </summary>
        public void UpdateDrag()
        {
            if (!isDragging) return;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;
            transform.position = mousePos;

            OnDragging?.Invoke(this);
        }

        /// <summary>
        /// 드래그 종료
        /// </summary>
        public void EndDrag()
        {
            if (!isDragging) return;

            isDragging = false;

            // 본체 Sorting Order 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = originalSortingOrder;
            }

            // 자식들 Sorting Order 복원
            foreach (var kvp in childOriginalSortingOrders)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.sortingOrder = kvp.Value;
                }
            }
            childOriginalSortingOrders.Clear();

            // 드롭존 찾기
            SimpleDropZone dropZone = FindDropZone();
            OnDragEnded?.Invoke(this, dropZone);

            Debug.Log($"[SimpleDraggable] Drag ended: {gameObject.name}, DropZone: {(dropZone != null ? dropZone.name : "None")}");
        }

        /// <summary>
        /// 현재 위치의 드롭존 찾기
        /// </summary>
        private SimpleDropZone FindDropZone()
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
            
            foreach (var hit in hits)
            {
                SimpleDropZone zone = hit.GetComponent<SimpleDropZone>();
                if (zone != null && zone.isDroppable)
                {
                    return zone;
                }
            }
            return null;
        }

        /// <summary>
        /// 원래 위치로 복귀
        /// </summary>
        public void ReturnToOrigin()
        {
            transform.position = originalPosition;
            Debug.Log($"[SimpleDraggable] Returned to origin: {gameObject.name}");
        }

        /// <summary>
        /// 특정 위치로 이동
        /// </summary>
        public void MoveTo(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// 회전 설정
        /// </summary>
        public void SetRotation(float zAngle)
        {
            transform.rotation = Quaternion.Euler(0, 0, zAngle);
        }

        /// <summary>
        /// 스프라이트 변경
        /// </summary>
        public void SetSprite(Sprite newSprite)
        {
            if (spriteRenderer != null && newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
            }
        }
    }
}