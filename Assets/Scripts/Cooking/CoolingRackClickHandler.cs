using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 식힘망 핫도그 클릭 핸들러
    /// 클릭 시 토핑 팝업 열기
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CoolingRackClickHandler : MonoBehaviour
    {
        private SimpleCookingManager manager;
        private bool hasOpened = false;

        private void Start()
        {
            manager = SimpleCookingManager.Instance;
            
            // Collider2D 확인
            Collider2D col = GetComponent<Collider2D>();
            if (col == null)
            {
                BoxCollider2D boxCol = gameObject.AddComponent<BoxCollider2D>();
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                {
                    boxCol.size = sr.sprite.bounds.size;
                }
            }
            
            Debug.Log("[CoolingRackClickHandler] 준비 완료 - 클릭하면 토핑 팝업 열림");
        }

        private void OnMouseDown()
        {
            if (hasOpened) return;
            if (manager == null) return;
            if (manager.CurrentPhase != CookingPhase.Topping) return;

            hasOpened = true;
            
            Debug.Log("[CoolingRackClickHandler] ✅ 핫도그 클릭! 토핑 팝업 열기");
            manager.ShowToppingPopup();
        }

        /// <summary>
        /// 토핑 완료 후 다시 클릭 가능하게 리셋 (필요시)
        /// </summary>
        public void ResetClick()
        {
            hasOpened = false;
        }
    }
}
