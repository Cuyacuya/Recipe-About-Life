using UnityEngine;
using System;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 재료 소스 (드래그 시작점)
    /// 소시지통, 치즈통에 부착
    /// </summary>
    public class IngredientSource : MonoBehaviour
    {
        public event Action OnDragStarted;

        private void OnMouseDown()
        {
            OnDragStarted?.Invoke();
            Debug.Log($"[IngredientSource] {gameObject.name} 클릭됨");
        }
    }
}
