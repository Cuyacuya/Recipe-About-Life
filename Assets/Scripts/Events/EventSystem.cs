using System;
using System.Collections.Generic;
using UnityEngine;

namespace RecipeAboutLife.Events
{
    /// <summary>
    /// 전역 이벤트 시스템 (문자열 기반)
    /// 다양한 데이터 타입을 전달할 수 있는 범용 이벤트 시스템
    /// </summary>
    public static class EventManager
    {
        private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] += listener;
            }
            else
            {
                eventDictionary[eventName] = listener;
            }
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;
            }
        }

        /// <summary>
        /// 이벤트 발생
        /// </summary>
        public static void Trigger(string eventName, object data = null)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName]?.Invoke(data);
            }
        }

        /// <summary>
        /// 모든 이벤트 구독 해제
        /// </summary>
        public static void Clear()
        {
            eventDictionary.Clear();
        }
    }

    /// <summary>
    /// 타입 안전 이벤트 클래스
    /// 주요 게임 이벤트를 타입 안전하게 관리
    /// </summary>
    public static class GameEvents
    {
        // ===== 요리 이벤트 =====
        
        /// <summary>
        /// 요리 단계가 변경될 때 발생
        /// </summary>
        public static event Action<Cooking.CookingStepType> OnCookingStepChanged;
        
        /// <summary>
        /// 레시피가 완성되었을 때 발생 (다른 시스템이 구독)
        /// </summary>
        public static event Action<Cooking.HotdogRecipe> OnRecipeCompleted;
        
        /// <summary>
        /// 레시피가 폐기되었을 때 발생
        /// </summary>
        public static event Action OnRecipeDiscarded;
        
        /// <summary>
        /// 품질이 계산되었을 때 발생 (UI 업데이트용)
        /// </summary>
        public static event Action<float> OnQualityCalculated;

        // ===== 손님 이벤트 =====
        
        /// <summary>
        /// 손님이 도착했을 때 발생 (CustomerManager가 발생)
        /// </summary>
        public static event Action<object> OnCustomerArrived;
        
        /// <summary>
        /// 모든 손님 서빙 완료 (CustomerManager가 발생)
        /// </summary>
        public static event Action OnAllCustomersServed;

        // ===== 멘탈 이벤트 =====
        
        /// <summary>
        /// 실수가 발생했을 때 (MentalManager가 구독)
        /// </summary>
        public static event Action OnMistakeMade;
        
        /// <summary>
        /// 멘탈이 변경되었을 때 (MentalManager가 발생, UI가 구독)
        /// </summary>
        public static event Action<int> OnMentalChanged;
        
        /// <summary>
        /// 채도가 변경되었을 때 (MentalManager가 발생, 비주얼이 구독)
        /// </summary>
        public static event Action<float> OnSaturationChanged;

        // ===== UI 이벤트 =====
        
        /// <summary>
        /// 알림 표시 (UI가 구독)
        /// </summary>
        public static event Action<string> OnShowNotification;

        // ===== 스테이지 이벤트 =====
        
        /// <summary>
        /// 스테이지가 시작되었을 때
        /// </summary>
        public static event Action OnStageStarted;
        
        /// <summary>
        /// 스테이지가 완료되었을 때 (StageManager가 발생)
        /// </summary>
        public static event Action<object> OnStageCompleted;

        // ===== 오디오 이벤트 =====
        
        /// <summary>
        /// 효과음 재생 요청 (AudioManager가 구독)
        /// </summary>
        public static event Action<string> OnSFXRequested;

        // ==========================================
        // 이벤트 트리거 메서드들
        // ==========================================

        public static void TriggerCookingStepChanged(Cooking.CookingStepType step)
        {
            OnCookingStepChanged?.Invoke(step);
        }

        public static void TriggerRecipeCompleted(Cooking.HotdogRecipe recipe)
        {
            OnRecipeCompleted?.Invoke(recipe);
        }

        public static void TriggerRecipeDiscarded()
        {
            OnRecipeDiscarded?.Invoke();
        }

        public static void TriggerQualityCalculated(float quality)
        {
            OnQualityCalculated?.Invoke(quality);
        }

        public static void TriggerCustomerArrived(object customerData)
        {
            OnCustomerArrived?.Invoke(customerData);
        }

        public static void TriggerAllCustomersServed()
        {
            OnAllCustomersServed?.Invoke();
        }

        public static void TriggerMistakeMade()
        {
            OnMistakeMade?.Invoke();
        }

        public static void TriggerMentalChanged(int level)
        {
            OnMentalChanged?.Invoke(level);
        }

        public static void TriggerSaturationChanged(float saturation)
        {
            OnSaturationChanged?.Invoke(saturation);
        }

        public static void TriggerShowNotification(string message)
        {
            OnShowNotification?.Invoke(message);
        }

        public static void TriggerStageStarted()
        {
            OnStageStarted?.Invoke();
        }

        public static void TriggerStageCompleted(object result)
        {
            OnStageCompleted?.Invoke(result);
        }

        public static void TriggerSFXRequested(string sfxName)
        {
            OnSFXRequested?.Invoke(sfxName);
        }

        /// <summary>
        /// 모든 이벤트 리스너 제거 (씬 전환 시 호출)
        /// </summary>
        public static void ClearAllEvents()
        {
            OnCookingStepChanged = null;
            OnRecipeCompleted = null;
            OnRecipeDiscarded = null;
            OnQualityCalculated = null;
            OnCustomerArrived = null;
            OnAllCustomersServed = null;
            OnMistakeMade = null;
            OnMentalChanged = null;
            OnSaturationChanged = null;
            OnShowNotification = null;
            OnStageStarted = null;
            OnStageCompleted = null;
            OnSFXRequested = null;
        }
    }
}