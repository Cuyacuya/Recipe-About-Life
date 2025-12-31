# 스토리 NPC 시스템 구현 가이드

## 📋 개요

기존 대화 시스템을 유지하면서 **스토리 NPC**와 **재화 조건 기반 스토리 대화** 시스템을 확장했습니다.

### 주요 기능
- ✅ 10명의 NPC 중 3명의 스토리 NPC 지정
- ✅ 각 스테이지 마지막 NPC를 스토리 NPC로 자동 교체
- ✅ 재화 조건 달성 시 StoryAfterSummary 대화 자동 트리거
- ✅ 기존 시스템 (NPCSpawnManager, ScoreManager, DialogueManager) 유지
- ✅ 결산 UI 연결을 위한 TODO 위치 명확히 표시

---

## 🆕 추가된 파일

### 1. **DialogueEnums.cs** (수정)
- 기존 DialogueType에 스토리 전용 타입 추가:
  - `StoryIntro` - 스토리 NPC 등장 시
  - `StoryOrder` - 스토리 NPC 주문 시
  - `StoryServedSuccess` - 스토리 NPC 주문 성공
  - `StoryServedFail` - 스토리 NPC 주문 실패
  - `StoryAfterSummary` ⭐ - **재화 조건 달성 후 스토리 대화**

### 2. **StoryNPCConfig.cs** (신규 - ScriptableObject)
- 위치: `Assets/Scripts/NPC/StoryNPCConfig.cs`
- 역할: 스테이지별 스토리 NPC 매핑 관리
- 생성: `Create > RecipeAboutLife > NPC > Story NPC Config`

### 3. **StageStoryController.cs** (신규 - MonoBehaviour)
- 위치: `Assets/Scripts/Dialogue/StageStoryController.cs`
- 역할: 재화 조건 확인 및 StoryAfterSummary 대화 트리거
- 씬에 GameObject로 배치 필요

### 4. **StoryNPCSpawnHelper.cs** (신규 - MonoBehaviour)
- 위치: `Assets/Scripts/NPC/StoryNPCSpawnHelper.cs`
- 역할: 마지막 NPC를 스토리 NPC로 자동 교체
- NPCSpawnManager와 같은 GameObject에 부착

### 5. **NPCSpawnManager.cs** (최소 확장)
- 추가 내용: `OnNPCsSelected` 이벤트 (확장 hook)
- 기존 로직 변경 없음

---

## ⚙️ 설정 방법

### **Step 1: StoryNPCConfig 생성**

1. **ScriptableObject 생성**
   ```
   Project 창 우클릭
   → Create > RecipeAboutLife > NPC > Story NPC Config
   → 파일명: "StoryNPCConfig"
   ```

2. **스토리 NPC 설정**

   Inspector에서:
   ```
   Stage Story NPCs
   ├─ Size: 3
   │
   ├─ Element 0 (Stage 1)
   │  ├─ Stage ID: 1
   │  ├─ Story NPC Prefab: [스토리 NPC A 프리팹]
   │  └─ Description: "첫 번째 스토리 NPC"
   │
   ├─ Element 1 (Stage 2)
   │  ├─ Stage ID: 2
   │  ├─ Story NPC Prefab: [스토리 NPC B 프리팹]
   │  └─ Description: "두 번째 스토리 NPC"
   │
   └─ Element 2 (Stage 3)
      ├─ Stage ID: 3
      ├─ Story NPC Prefab: [스토리 NPC C 프리팹]
      └─ Description: "메인 스토리 NPC"
   ```

3. **우클릭 Context Menu로 검증**
   ```
   StoryNPCConfig 선택
   → 우클릭 → "Validate Config"
   → Console에서 검증 결과 확인
   ```

---

### **Step 2: 스토리 NPC 프리팹 준비**

각 스토리 NPC 프리팹에 다음이 있는지 확인:

```
Story NPC GameObject
├─ NPCMovement (기존)
├─ NPCOrderController (기존)
├─ NPCDialogueController ⭐ (필수)
│  └─ Dialogue Set 참조 (NPCDialogueSet)
└─ DialogueBubbleUI (기존)
```

**NPCDialogueSet 설정**

Inspector에서:
```
NPC Dialogue Set
├─ NPC ID: "StoryNPC_A"
├─ NPC Order: [고정 주문 할당]
│
└─ Dialogue Groups
   ├─ StoryIntro (스토리 등장 대사)
   ├─ StoryOrder (스토리 주문 대사)
   ├─ StoryServedSuccess
   ├─ StoryServedFail
   └─ StoryAfterSummary ⭐ (재화 달성 후 대사)
```

**StoryAfterSummary 예시:**
```
Lines
├─ [NPC] "축하해요! 목표 금액을 달성하셨네요!"
├─ [Player] "감사합니다! 정말 힘들었어요."
├─ [NPC] "당신의 노력이 빛을 발했어요."
├─ [Player] "앞으로도 열심히 하겠습니다!"
└─ [NPC] "응원할게요! 내일도 기대되네요."
```

---

### **Step 3: 씬 설정**

#### 3-1. StageStoryController 추가

1. **GameObject 생성**
   ```
   Hierarchy 우클릭
   → Create Empty
   → 이름: "StageStoryController"
   ```

2. **컴포넌트 추가**
   ```
   Add Component
   → "StageStoryController" 검색
   → 추가
   ```

3. **설정**
   ```
   StageStoryController (Script)
   ├─ Story NPC Config: [Step 1에서 만든 StoryNPCConfig]
   ├─ Current Stage ID: 1
   └─ Delay Before Story Dialogue: 1
   ```

#### 3-2. StoryNPCSpawnHelper 추가

1. **NPCSpawnManager 찾기**
   ```
   Hierarchy에서 NPCSpawnManager가 있는 GameObject 선택
   ```

2. **컴포넌트 추가**
   ```
   Add Component
   → "StoryNPCSpawnHelper" 검색
   → 추가
   ```

3. **설정**
   ```
   StoryNPCSpawnHelper (Script)
   ├─ Story NPC Config: [Step 1에서 만든 StoryNPCConfig]
   ├─ Current Stage ID: 1
   └─ Replace Last NPC With Story: ✅ (체크)
   ```

---

## 🎮 작동 흐름

### **정상 플레이 시나리오**

```
게임 시작
   ↓
NPCSpawnManager.StartStage()
   ↓
랜덤 NPC 4명 선택
   ↓
[Hook] OnNPCsSelected 이벤트 발생
   ↓
StoryNPCSpawnHelper가 마지막 NPC를 스토리 NPC로 교체
   ↓
NPC 1~4 등장 (랜덤 NPC)
   - Intro → Order → ServedSuccess/Fail → Exit
   ↓
NPC 5 등장 (스토리 NPC) ⭐
   - StoryIntro → StoryOrder → StoryServedSuccess/Fail → (대기)
   ↓
모든 NPC 완료
   ↓
ScoreManager.OnStageCompleted(success) 발생
   ↓
StageStoryController가 조건 확인:
   - totalReward >= targetReward?
   - 현재 NPC가 스토리 NPC?
   - 스테이지 마지막 NPC?
   ↓
조건 만족 시:
   ↓
StoryAfterSummary 대화 시작 ⭐
   - Player ↔ NPC 대화
   - 스토리 진행
   ↓
대화 종료
   ↓
스테이지 완료
```

---

## 🔍 디버그 로그 확인

### **1. 스토리 NPC 교체 로그**

```
[StoryNPCSpawnHelper] ✅ 마지막 NPC 교체 완료!
  - Stage: 1
  - Original NPC: NPC_RandomCustomer
  - Story NPC: StoryNPC_A
  - Position: 5/5
```

### **2. 재화 조건 확인 로그**

```
[StageStoryController] 스테이지 완료! 성공: True, Stage ID: 1
[StageStoryController] ✅ 조건 만족! StoryAfterSummary 대화 트리거
  - Stage: 1
  - NPC: StoryNPC_A
  - 재화 달성: True
```

### **3. StoryAfterSummary 대화 로그**

```
[StageStoryController] StoryAfterSummary 대화 시작! NPC: StoryNPC_A
[NPCDialogueController] [NPC] 축하해요! 목표 금액을 달성하셨네요!
[NPCDialogueController] [Player] 감사합니다! 정말 힘들었어요.
...
[StageStoryController] StoryAfterSummary 대화 종료!
```

### **4. 조건 미달성 로그**

```
[StageStoryController] 재화 목표 미달성. StoryAfterSummary 대화를 실행하지 않습니다.
```

---

## ❗ 중요 사항

### **기존 시스템 유지**

- ✅ `NPCSpawnManager` - 최소 확장만 추가 (OnNPCsSelected 이벤트)
- ✅ `ScoreManager` - 수정 없음
- ✅ `DialogueManager` - 수정 없음
- ✅ `OrderManager` - 수정 없음

### **StoryAfterSummary 실행 조건**

다음 **3가지 조건을 모두 만족**해야 실행:
1. ✅ 모든 NPC 주문 완료
2. ✅ `totalReward >= targetReward` (재화 조건 달성)
3. ✅ 현재 NPC가 스토리 NPC

### **결산 UI 연결 위치**

`StageStoryController.cs:127-137`에 TODO 주석으로 표시:

```csharp
// TODO: 결산 UI 연결 예정 위치
// ============================================
// 나중에 결산 UI를 추가할 때:
// 1. 여기서 결산 UI를 표시
// 2. 결산 UI에 "확인" 버튼 추가
// 3. 확인 버튼 클릭 시 → CheckAndTriggerStoryDialogue(success) 호출
//
// 현재는 결산 UI 없이 즉시 스토리 대화 진행
// ============================================
```

---

## 🧪 테스트 방법

### **1. Context Menu로 즉시 테스트**

#### StoryNPCConfig 검증
```
StoryNPCConfig 선택
→ 우클릭 → "Validate Config"
→ Console 확인
```

#### StageStoryController 상태 확인
```
StageStoryController GameObject 선택
→ Inspector → 우클릭 → "Log Current State"
→ Console에서 현재 스테이지, 재화, NPC 정보 확인
```

#### StoryNPCSpawnHelper 상태 확인
```
StoryNPCSpawnHelper가 있는 GameObject 선택
→ Inspector → 우클릭 → "Log Current State"
→ Console에서 스토리 NPC 교체 설정 확인
```

### **2. 실제 플레이 테스트**

1. **게임 시작**
2. **Console 확인:**
   - `[StoryNPCSpawnHelper] ✅ 마지막 NPC 교체 완료!`
3. **NPC 1~4 주문 완료** (랜덤 NPC)
4. **NPC 5 등장** (스토리 NPC)
   - StoryIntro 대화 확인
   - StoryOrder 대화 확인
5. **음식 제공**
   - StoryServedSuccess 대화 확인
6. **모든 NPC 완료 후 Console 확인:**
   - `[StageStoryController] ✅ 조건 만족! StoryAfterSummary 대화 트리거`
7. **StoryAfterSummary 대화 재생 확인**
   - Player ↔ NPC 대화 확인

---

## 📊 스테이지별 설정 예시

### **Stage 1 - 튜토리얼 스토리**
```
- 랜덤 NPC: 4명
- 스토리 NPC: 친절한 단골 손님
- 목표 재화: 400원
- StoryAfterSummary: 격려 대화
```

### **Stage 2 - 본격 스토리**
```
- 랜덤 NPC: 4명
- 스토리 NPC: 의문의 손님
- 목표 재화: 500원
- StoryAfterSummary: 스토리 전개 대화
```

### **Stage 3 - 메인 스토리**
```
- 랜덤 NPC: 4명
- 스토리 NPC: 핵심 인물
- 목표 재화: 600원
- StoryAfterSummary: 메인 스토리 클라이맥스 대화
```

---

## 🔧 트러블슈팅

### **Q: StoryAfterSummary 대화가 실행되지 않아요**

**확인 사항:**
1. ✅ `StageStoryController`가 씬에 있나요?
2. ✅ `StoryNPCConfig`가 할당되어 있나요?
3. ✅ `currentStageID`가 올바르게 설정되어 있나요?
4. ✅ 스토리 NPC의 DialogueSet에 StoryAfterSummary 대화가 있나요?
5. ✅ 재화 조건을 달성했나요? (totalReward >= targetReward)

**Console 확인:**
```
[StageStoryController] 재화 목표 미달성
→ 재화를 더 획득하세요

[StageStoryController] 현재 NPC는 스토리 NPC가 아닙니다
→ StoryNPCConfig 설정 확인

[NPCDialogueController] StoryAfterSummary 대화가 없습니다
→ DialogueSet에 StoryAfterSummary 추가
```

### **Q: 마지막 NPC가 스토리 NPC로 교체되지 않아요**

**확인 사항:**
1. ✅ `StoryNPCSpawnHelper`가 NPCSpawnManager와 같은 GameObject에 있나요?
2. ✅ `StoryNPCConfig`가 할당되어 있나요?
3. ✅ `Replace Last NPC With Story`가 체크되어 있나요?
4. ✅ `currentStageID`에 해당하는 스토리 NPC가 설정되어 있나요?

**Console 확인:**
```
[StoryNPCSpawnHelper] ✅ 마지막 NPC 교체 완료!
→ 정상 작동

[StoryNPCSpawnHelper] Stage X에 스토리 NPC가 설정되지 않았습니다
→ StoryNPCConfig에서 해당 스테이지 설정 추가
```

---

## 📚 추가 개발 시 참고

### **나중에 결산 UI 추가 시**

`StageStoryController.cs`의 `OnStageCompleted()` 메서드에서:

```csharp
private void OnStageCompleted(bool success)
{
    // 1. 결산 UI 표시
    ResultUI resultUI = FindFirstObjectByType<ResultUI>();
    if (resultUI != null)
    {
        resultUI.Show(success);
        resultUI.OnConfirmButtonClicked += () => {
            // 2. 확인 버튼 클릭 시 스토리 대화 트리거
            CheckAndTriggerStoryDialogue(success);
        };
    }
    else
    {
        // 결산 UI 없으면 즉시 실행 (현재 방식)
        CheckAndTriggerStoryDialogue(success);
    }
}
```

### **선택지 시스템 추가 시**

`DialogueLine.cs`에 선택지 관련 필드 추가:

```csharp
[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextDialogueIndex; // -1이면 대화 종료
}

public class DialogueLine
{
    // 기존 필드...

    public List<DialogueChoice> choices; // 선택지 (있으면 표시)
}
```

---

## ✅ 완료 체크리스트

- [ ] StoryNPCConfig ScriptableObject 생성
- [ ] 3개의 스토리 NPC 프리팹 준비
- [ ] 각 스토리 NPC에 NPCDialogueController 부착
- [ ] DialogueSet에 Story 타입 대화 작성
- [ ] StoryAfterSummary 대화 작성 (Player 대사 포함)
- [ ] StageStoryController를 씬에 추가
- [ ] StoryNPCSpawnHelper를 NPCSpawnManager에 부착
- [ ] 설정 검증 (Context Menu)
- [ ] 실제 플레이 테스트
- [ ] Console 로그 확인

---

## 📞 문의

구현 관련 문제나 추가 기능이 필요하면 언제든지 문의하세요!
