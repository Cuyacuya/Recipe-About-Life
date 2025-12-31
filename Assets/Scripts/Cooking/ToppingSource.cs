using UnityEngine;
using System;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 토핑 타입
    /// </summary>
    public enum ToppingType
    {
        None,
        Ketchup,
        Mustard
    }

    /// <summary>
    /// 소스통 클릭 감지
    /// 케첩통, 머스타드통에 부착
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ToppingSource : MonoBehaviour
    {
        [Header("Settings")]
        public ToppingType sourceType = ToppingType.None;

        // 이벤트
        public event Action<ToppingType> OnSourceClicked;

        private void OnMouseDown()
        {
            Debug.Log($"[ToppingSource] {sourceType} 클릭됨");
            OnSourceClicked?.Invoke(sourceType);
        }
    }
}
