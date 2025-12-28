# NPC 대화 시스템 통합 가이드

## 📋 개요

NPC 대화 시스템과 요리 시스템을 통합하여 재화 기반 대화 잠금 해제 시스템을 구현했습니다.

### ✅ 구현된 기능
1. **ScoreManager**: 5명의 NPC에게 음식을 제공하고 재화 획득
2. **NPCOrderController**: 주문을 요리 시스템으로 전달
3. **NPCSpawnManager**: NPC 스폰 및 진행 관리
4. **DialogueManager**: 목표 재화 달성 시 대화 잠금 해제
5. **OrderDataConverter**: 주문 데이터 변환 (OrderData → CustomerOrder)

---

## 🎯 시스템 흐름

```
1. NPCSpawnManager: NPC 스폰 (5명 중 1명)
   ↓
2. NPCOrderController: NPC가 주문 요청
   ↓
3. OrderDataConverter: OrderData → CustomerOrder 변환
   ↓
4. CookingManager: 요리 시작
   ↓
5. 플레이어: 6단계 요리 진행
   ↓
6. CookingManager: 레시피 완료 이벤트 발생
   ↓
7. ScoreManager: 품질 점수 계산 → 재화 지급
   ↓
8. NPCSpawnManager: 다음 NPC 스폰 (2/5, 3/5, ...)
   ↓
9. (5번 반복 후)
   ↓
10. ScoreManager: 목표 재화 달성 확인
    ↓
11. DialogueManager: 대화 잠금 해제! (달성 시)
```

---

## 🛠️ Unity 씬 설정

### 1단계: 기본 매니저 생성

#### ScoreManager 생성
1. 빈 게임오브젝트 생성: `ScoreManager`
2. `ScoreManager` 스크립트 추가
3. Inspector 설정:
   - **Target Total Reward**: `400` (목표 재화)
   - **Max NPC Count**: `5`

#### NPCSpawnManager 설정
1. 기존 `NPCSpawnManager` 오브젝트 찾기
2. Inspector 확인:
   - NPC Prefabs 목록에 10개의 NPC 프리팹 등록
   - **Npcs Per Stage**: `5`

#### DialogueManager 생성
1. 빈 게임오브젝트 생성: `DialogueManager`
2. `DialogueManager` 스크립트 추가
3. Inspector 설정:
   - **Dialogue Panel**: 대화 UI 패널 할당
   - **Locked Message Panel**: 잠금 메시지 UI 할당
   - **Player Character**: 플레이어 오브젝트 할당 (선택)

#### CookingManager 설정
1. 기존 `CookingManager` 오브젝트 찾기
2. Inspector 확인:
   - **Recipe Config**: RecipeConfigSO 할당

---

### 2단계: UI 설정 (선택사항)

#### 재화 표시 UI
```csharp
// ScoreManager 이벤트 구독 예시
ScoreManager.Instance.OnTotalRewardChanged += (totalReward) => {
    scoreText.text = $"재화: {totalReward}원";
};

ScoreManager.Instance.OnNPCRewarded += (npcIndex, reward) => {
    Debug.Log($"NPC {npcIndex}: +{reward}원");
};
```

#### 대화 잠금 해제 알림
```csharp
// DialogueManager 이벤트 구독 예시
DialogueManager.Instance.OnDialogueStarted += () => {
    Debug.Log("대화 시작!");
};
```

---

## 🔧 주요 클래스 설명

### ScoreManager
**위치**: `Assets/Scripts/Managers/ScoreManager.cs`

**주요 메서드**:
- `GetTotalReward()`: 현재 총 재화
- `GetTargetReward()`: 목표 재화
- `IsDialogueUnlocked()`: 대화 잠금 해제 여부
- `RestartStage()`: 스테이지 재시작

**이벤트**:
- `OnTotalRewardChanged`: 재화 변경 시
- `OnNPCRewarded`: NPC 보상 지급 시
- `OnDialogueUnlocked`: 대화 잠금 해제 시
- `OnStageCompleted`: 스테이지 완료 시

---

### DialogueManager
**위치**: `Assets/Scripts/Dialogue/DialogueManager.cs`

**주요 메서드**:
- `TryStartDialogue()`: 대화 시작 시도 (잠금 확인)
- `EndDialogue()`: 대화 종료
- `IsDialogueUnlocked()`: 잠금 해제 여부
- `ForceUnlock()`: 강제 잠금 해제 (테스트용)

**이벤트**:
- `OnDialogueStarted`: 대화 시작 시
- `OnDialogueEnded`: 대화 종료 시

---

### OrderDataConverter
**위치**: `Assets/Scripts/Orders/OrderDataConverter.cs`

**주요 메서드**:
- `ToCustomerOrder(OrderData)`: OrderData를 CustomerOrder로 변환

**변환 규칙**:
- 속재료: 반쪽 2개 → 완성품 1개
  - 소시지 + 소시지 = Sausage
  - 치즈 + 치즈 = Cheese
  - 소시지 + 치즈 = Mixed
- 소스: Orders.SauceType → Cooking.SauceType
- 설탕: 그대로 전달

---

## 🎮 테스트 방법

### 방법 1: 실제 플레이 테스트
1. Unity 에디터에서 플레이 시작
2. NPC 1번이 등장하고 주문을 받음
3. 6단계 요리 진행:
   - 꼬치 들기 → 속재료 → 반죽 → 튀기기 → 소스 → 완성
4. 요리 완료 시 재화 획득
5. NPC 2번 등장 (자동)
6. 5번 반복
7. 총 재화가 400원 이상이면 대화 잠금 해제
8. `DialogueManager.TryStartDialogue()` 호출 → 성공

### 방법 2: Inspector 테스트 (빠른 테스트)

#### ScoreManager 테스트
1. 플레이 모드 진입
2. Hierarchy에서 `ScoreManager` 선택
3. Inspector 우클릭 → `Test: Add Random Reward` 클릭
4. 5번 반복
5. Console에서 결과 확인

#### DialogueManager 테스트
1. 플레이 모드 진입
2. Hierarchy에서 `DialogueManager` 선택
3. Inspector 우클릭 → `Test: Force Unlock` 클릭
4. Inspector 우클릭 → `Test: Try Start Dialogue` 클릭
5. Console에서 대화 시작 확인

---

## 📊 보상 계산 로직

### 품질 점수 (0-100)
- 기본: 100점
- 감점 요소:
  - 속재료 불일치: -30
  - 소스 불일치: -30
  - 설탕 불일치: -10
  - 반죽 부족: -10
  - 튀김 상태:
    - Raw: -40
    - Yellow: -15
    - Golden: 0 (최적!)
    - Brown: -15
    - Burnt: -40

### 재화 계산
```
기본 보상 = 100원
품질 배율 = 품질 점수 / 100

주문 일치 시:
  품질 배율 *= 1.5 (50% 보너스)

최종 재화 = 기본 보상 * 품질 배율
최소 재화 = 10원
```

### 예시
- 품질 100점, 주문 일치: 100 * 1.0 * 1.5 = **150원**
- 품질 80점, 주문 일치: 100 * 0.8 * 1.5 = **120원**
- 품질 80점, 주문 불일치: 100 * 0.8 = **80원**
- 품질 60점, 주문 일치: 100 * 0.6 * 1.5 = **90원**

### 목표 달성
- 목표 재화: **400원**
- 5명의 NPC
- 평균 80원 이상 필요 (품질 80점 + 주문 일치)

---

## 🐛 디버깅 팁

### Console 로그 확인
```
[ScoreManager] NPC 1 보상 지급: 120원 (품질: 85.0, 총합: 120원)
[NPCSpawnManager] 레시피 완료! 품질: 85.0, 보상: 120
[CookingManager] Recipe completed! Quality: 85.0, Matches: True, Reward: 120
```

### Context Menu 사용
- ScoreManager:
  - `Log Current State`: 현재 상태 출력
  - `Test: Add Random Reward`: 랜덤 보상 추가
  - `Test: Restart Stage`: 스테이지 재시작

- DialogueManager:
  - `Log Current State`: 현재 상태 출력
  - `Test: Try Start Dialogue`: 대화 시작 테스트
  - `Test: Force Unlock`: 강제 잠금 해제

---

## ⚠️ 주의사항

### 1. RecipeConfigSO 필수
- CookingManager에 RecipeConfigSO가 할당되어 있어야 함
- 없으면 보상 계산 실패

### 2. 이벤트 구독 순서
- ScoreManager는 CookingManager보다 먼저 활성화되어야 함
- DialogueManager는 ScoreManager보다 먼저 활성화되어야 함
- Script Execution Order 확인:
  1. CookingManager
  2. ScoreManager
  3. DialogueManager

### 3. Singleton 주의
- 씬에 각 Manager가 1개씩만 존재해야 함
- 중복 시 자동으로 삭제됨

---

## 🔄 씬 전환 시 초기화

스테이지를 재시작하거나 씬을 전환할 때:

```csharp
// 스테이지 재시작
ScoreManager.Instance.RestartStage();
DialogueManager.Instance.ResetDialogueSystem();
NPCSpawnManager.Instance.RestartStage();

// 또는 한 번에
NPCSpawnManager.Instance.RestartStage(); // 내부에서 ScoreManager 초기화
DialogueManager.Instance.ResetDialogueSystem();
```

---

## 📝 확장 가이드

### 1. 대화 컨텐츠 추가
`DialogueManager.StartDialogue()`에서:
```csharp
// 실제 대화 스크립트 재생
DialogueScript dialogueScript = GetDialogueScript();
dialogueScript.Play();
```

### 2. UI 추가
ScoreManager 이벤트를 구독하여 UI 업데이트:
```csharp
public class ScoreUI : MonoBehaviour
{
    void OnEnable()
    {
        ScoreManager.Instance.OnTotalRewardChanged += UpdateUI;
    }

    void UpdateUI(int totalReward)
    {
        scoreText.text = $"{totalReward}원";
        progressBar.value = ScoreManager.Instance.GetProgress();
    }
}
```

### 3. 난이도 조정
ScoreManager Inspector에서:
- **Target Total Reward** 변경 (쉬움: 300, 어려움: 500)
- **Max NPC Count** 변경 (더 많은 NPC)

---

## ✅ 체크리스트

통합 완료 전 확인:

- [ ] ScoreManager 오브젝트 생성 및 설정
- [ ] DialogueManager 오브젝트 생성 및 설정
- [ ] CookingManager에 RecipeConfigSO 할당
- [ ] NPCSpawnManager에 NPC 프리팹 등록
- [ ] OrderManager에 OrderDatabase 할당
- [ ] 5명의 NPC 테스트 완료
- [ ] 재화 계산 정상 작동 확인
- [ ] 대화 잠금 해제 확인
- [ ] Console 에러 없음

---

## 🆘 문제 해결

### "CookingManager를 찾을 수 없습니다!"
- 씬에 CookingManager가 있는지 확인
- Singleton Instance가 제대로 생성되었는지 확인

### "RecipeConfigSO를 찾을 수 없습니다!"
- CookingManager Inspector에서 RecipeConfig 할당

### "재화가 지급되지 않음"
- ScoreManager가 활성화되어 있는지 확인
- GameEvents.OnRecipeCompleted 이벤트 구독 확인
- Console에서 `[ScoreManager] NPC X 보상 지급` 로그 확인

### "대화 잠금이 해제되지 않음"
- 총 재화가 목표치 이상인지 확인
- ScoreManager Inspector에서 Total Reward 확인
- DialogueManager가 활성화되어 있는지 확인

---

## 📞 지원

추가 질문이나 문제가 있으면:
1. Console 로그 확인
2. Context Menu로 상태 확인
3. 각 Manager의 `Log Current State` 실행

---

**작성일**: 2025-12-17
**버전**: 1.0
**작성자**: Claude Code Assistant
