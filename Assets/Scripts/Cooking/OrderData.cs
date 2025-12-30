using System;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 주문 데이터 구조
    /// 팀원의 NPC 시스템에서 이 구조로 주문 데이터를 전달받음
    /// </summary>
    [Serializable]
    public class OrderData
    {
        /// <summary>
        /// 첫번째 재료 (Sausage / Cheese)
        /// </summary>
        public string filling1;

        /// <summary>
        /// 두번째 재료 (Sausage / Cheese)
        /// </summary>
        public string filling2;

        /// <summary>
        /// 설탕 원함 여부
        /// </summary>
        public bool wantsSugar;

        /// <summary>
        /// 케첩 원함 여부
        /// </summary>
        public bool wantsKetchup;

        /// <summary>
        /// 머스타드 원함 여부
        /// </summary>
        public bool wantsMustard;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public OrderData()
        {
            filling1 = "";
            filling2 = "";
            wantsSugar = false;
            wantsKetchup = false;
            wantsMustard = false;
        }

        /// <summary>
        /// 매개변수 생성자
        /// </summary>
        public OrderData(string fill1, string fill2, bool sugar, bool ketchup, bool mustard)
        {
            filling1 = fill1;
            filling2 = fill2;
            wantsSugar = sugar;
            wantsKetchup = ketchup;
            wantsMustard = mustard;
        }

        /// <summary>
        /// 주문 내용을 문자열로 반환
        /// </summary>
        public override string ToString()
        {
            string toppings = "";
            if (wantsSugar) toppings += "설탕 ";
            if (wantsKetchup) toppings += "케첩 ";
            if (wantsMustard) toppings += "머스타드 ";
            if (string.IsNullOrEmpty(toppings)) toppings = "없음";

            return $"재료: {filling1}, {filling2} / 토핑: {toppings.Trim()}";
        }

        /// <summary>
        /// 테스트용 랜덤 주문 생성
        /// </summary>
        public static OrderData GenerateRandom()
        {
            string[] fillings = { "Sausage", "Cheese" };
            
            return new OrderData
            {
                filling1 = fillings[UnityEngine.Random.Range(0, fillings.Length)],
                filling2 = fillings[UnityEngine.Random.Range(0, fillings.Length)],
                wantsSugar = UnityEngine.Random.value > 0.5f,
                wantsKetchup = UnityEngine.Random.value > 0.5f,
                wantsMustard = UnityEngine.Random.value > 0.5f
            };
        }
    }
}
