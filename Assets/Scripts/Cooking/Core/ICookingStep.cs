using UnityEngine;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 요리 단계 인터페이스
    /// 각 요리 단계(StickPickup, Ingredient, Batter, Frying, Topping, Completion)가 구현
    /// </summary>
    public interface ICookingStep
    {
        /// <summary>
        /// 단계 진입 시 호출
        /// </summary>
        /// <param name="recipe">현재 레시피</param>
        void Enter(HotdogRecipe recipe);
        
        /// <summary>
        /// 매 프레임 호출 (Update)
        /// 단계별로 필요한 경우에만 구현
        /// </summary>
        void Update();
        
        /// <summary>
        /// 단계 처리 (데이터 전달 및 완료 체크)
        /// </summary>
        /// <param name="data">처리할 데이터 (단계마다 다름)</param>
        /// <param name="quality">품질 점수 (ref로 수정 가능)</param>
        /// <returns>단계 완료 여부</returns>
        bool Process(object data, ref float quality);
        
        /// <summary>
        /// 단계 종료 시 호출
        /// </summary>
        void Exit();
    }
}