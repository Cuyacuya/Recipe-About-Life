using System;
using System.Collections.Generic;

namespace RecipeAboutLife.Cooking
{
    /// <summary>
    /// 하루 결산 데이터
    /// 팀원의 결산 화면에서 사용할 데이터
    /// </summary>
    [Serializable]
    public class DayResultData
    {
        /// <summary>
        /// 현재 Day (1, 2, 3)
        /// </summary>
        public int day;

        /// <summary>
        /// 목표 금액
        /// </summary>
        public int goalAmount;

        /// <summary>
        /// 번 금액
        /// </summary>
        public int earnedAmount;

        /// <summary>
        /// 서빙한 손님 수
        /// </summary>
        public int customersServed;

        /// <summary>
        /// 전체 손님 수
        /// </summary>
        public int totalCustomers;

        /// <summary>
        /// 목표 달성 여부
        /// </summary>
        public bool isGoalAchieved;

        /// <summary>
        /// 손님별 수입 기록
        /// </summary>
        public List<int> earningsPerCustomer;

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public DayResultData()
        {
            day = 1;
            goalAmount = 0;
            earnedAmount = 0;
            customersServed = 0;
            totalCustomers = 5;
            isGoalAchieved = false;
            earningsPerCustomer = new List<int>();
        }

        /// <summary>
        /// 평균 수입 계산
        /// </summary>
        public float GetAverageEarnings()
        {
            if (customersServed <= 0) return 0f;
            return (float)earnedAmount / customersServed;
        }

        /// <summary>
        /// 목표 달성률 계산 (0.0 ~ 1.0+)
        /// </summary>
        public float GetGoalProgress()
        {
            if (goalAmount <= 0) return 0f;
            return (float)earnedAmount / goalAmount;
        }

        /// <summary>
        /// 목표 달성률 퍼센트 (0 ~ 100+)
        /// </summary>
        public int GetGoalProgressPercent()
        {
            return (int)(GetGoalProgress() * 100);
        }

        /// <summary>
        /// 최고 수입 손님
        /// </summary>
        public int GetBestEarning()
        {
            if (earningsPerCustomer == null || earningsPerCustomer.Count == 0) return 0;
            
            int max = 0;
            foreach (var e in earningsPerCustomer)
            {
                if (e > max) max = e;
            }
            return max;
        }

        /// <summary>
        /// 최저 수입 손님
        /// </summary>
        public int GetWorstEarning()
        {
            if (earningsPerCustomer == null || earningsPerCustomer.Count == 0) return 0;
            
            int min = int.MaxValue;
            foreach (var e in earningsPerCustomer)
            {
                if (e < min) min = e;
            }
            return min == int.MaxValue ? 0 : min;
        }

        /// <summary>
        /// 만점(2000원) 달성 횟수
        /// </summary>
        public int GetPerfectCount()
        {
            if (earningsPerCustomer == null) return 0;
            
            int count = 0;
            foreach (var e in earningsPerCustomer)
            {
                if (e >= ScoreCalculator.MAX_SCORE) count++;
            }
            return count;
        }

        /// <summary>
        /// 결과 요약 문자열
        /// </summary>
        public override string ToString()
        {
            return $"Day {day}: {earnedAmount}/{goalAmount}원 ({GetGoalProgressPercent()}%) - " +
                   $"손님 {customersServed}/{totalCustomers}명 - " +
                   $"{(isGoalAchieved ? "성공!" : "실패")}";
        }

        /// <summary>
        /// 데이터 초기화
        /// </summary>
        public void Reset()
        {
            earnedAmount = 0;
            customersServed = 0;
            isGoalAchieved = false;
            earningsPerCustomer.Clear();
        }
    }
}
