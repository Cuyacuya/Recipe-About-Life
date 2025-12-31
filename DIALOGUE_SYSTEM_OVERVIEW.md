# 대화 시스템 전체 구조 - 한눈에 보기

## 🎯 핵심 요약

### 실제로 사용하는 시스템: **3개**

1. **NPCDialogueController** - NPC 개별 대화 (등장~퇴장)
2. **DialogueManager** - 플레이어와 대화 (재화 달성 시)
3. **StageStoryController** - 스토리 NPC 특별 대화 (재화 달성 시)

### 삭제 가능한 시스템: **2개**

1. ❌ **StageDialogueController** - StageStoryController와 중복
2. ❌ **StageDialogueData** - 사용되지 않음

---

## 📊 전체 구조도

```
┌──────────────────────────────────────────────────────────────┐
│                      게임 플레이 흐름                           │
└──────────────────────────────────────────────────────────────┘

【NPC 1명의 라이프사이클】
═══════════════════════════════════════════════════════════════

NPC 등장
   │
   ├─ NPCMovement (이동)
   │     OnArrived()
   │        ↓
   ├─ 【NPCDialogueController】
   │     StartDialogue(Intro)
   │     "안녕하세요!" (말풍선)
   │        ↓
   ├─ NPCOrderController
   │     RequestOrder()
   │        ↓
   ├─ 【NPCDialogueController】
   │     StartDialogue(Order)
   │     "이걸로 주세요!" (말풍선)
   │        ↓
   │  [플레이어가 음식 제공]
   │        ↓
   ├─ ScoreManager
   │     OnRecipeCompleted()
   │        ↓
   ├─ 【NPCDialogueController】
   │     StartDialogue(ServedSuccess/Fail)
   │     "맛있겠네요!" / "이게 아닌데요?" (말풍선)
   │        ↓
   ├─ NPCMovement
   │     OnOrderComplete()
   │        ↓
   ├─ 【NPCDialogueController】
   │     StartDialogue(Exit)
   │     "잘 먹겠습니다!" (말풍선)
   │        ↓
   └─ NPC 퇴장


【스테이지 완료 후】
═══════════════════════════════════════════════════════════════

5명의 NPC 모두 완료
   │
   ├─ ScoreManager
   │     OnStageCompleted(success)
   │        │
   │        ├─────────────────┬─────────────────┐
   │        │                 │                 │
   │        ▼                 ▼                 ▼
   │  ┌──────────┐   ┌──────────────┐   ┌─────────┐
   │  │재화 달성?│   │마지막 NPC가  │   │ 둘 다   │
   │  │         │   │스토리 NPC?   │   │ 아니면? │
   │  └──────────┘   └──────────────┘   └─────────┘
   │        │                 │                 │
   │       Yes               Yes               No
   │        │                 │                 │
   │        ▼                 ▼                 ▼
   │  ┌──────────┐   ┌──────────────┐   ┌─────────┐
   │  │Dialogue  │   │StageStory    │   │  종료   │
   │  │Manager   │   │Controller    │   └─────────┘
   │  └──────────┘   └──────────────┘
   │        │                 │
   │        ▼                 ▼
   │  플레이어와     StoryAfterSummary
   │  대화 시작     대화 시작
   │  (별도 UI)     (NPC 말풍선)
```

---

## 🎭 대화 타입별 사용처

### **일반 NPC 대화** (NPCDialogueController 사용)

```
DialogueType 사용
├─ Intro              → NPC 등장 시
├─ Order              → 주문 시
├─ ServedSuccess      → 주문 성공 시
├─ ServedFail         → 주문 실패 시
└─ Exit               → 퇴장 시
```

### **스토리 NPC 대화** (NPCDialogueController 사용)

```
DialogueType 사용
├─ StoryIntro         → 스토리 NPC 등장 시 (일반보다 길음)
├─ StoryOrder         → 스토리 NPC 주문 시
├─ StoryServedSuccess → 스토리 NPC 주문 성공 시
├─ StoryServedFail    → 스토리 NPC 주문 실패 시
└─ StoryAfterSummary  → ⭐ 재화 달성 후 추가 대화 (StageStoryController가 트리거)
```

### **플레이어 대화** (DialogueManager 사용)

```
하드코딩된 메시지 배열
├─ "잘했어! 오늘 정말 바빴지?"
├─ "5명의 손님을 모두 대접했네!"
└─ "목표 금액도 달성했고, 정말 수고했어!"
```

---

## 🔄 실제 게임 시나리오

### **시나리오 1: 일반 스테이지 (랜덤 NPC만)**

```
랜덤 NPC 1~5 등장
   각 NPC:
   - Intro → Order → Served → Exit

5명 완료 후:
   재화 달성?
   ├─ Yes → DialogueManager 실행 (플레이어 대화)
   └─ No  → 종료
```

### **시나리오 2: 스토리 스테이지 (마지막이 스토리 NPC)**

```
랜덤 NPC 1~4 등장
   각 NPC:
   - Intro → Order → Served → Exit

스토리 NPC 5 등장
   - StoryIntro → StoryOrder → StoryServed → (대기)

5명 완료 후:
   재화 달성?
   ├─ Yes → StageStoryController 실행
   │        └─ StoryAfterSummary 대화
   │           (Player ↔ NPC 대화)
   └─ No  → 종료
```

---

## 📂 파일별 역할

### ✅ **실제 사용 중**

| 파일명 | 타입 | 역할 |
|--------|------|------|
| `DialogueEnums.cs` | Enum | 대화 타입 정의 |
| `DialogueLine.cs` | Data | 대화 한 줄 데이터 |
| `DialogueBubbleUI.cs` | UI | 말풍선 UI |
| `NPCDialogueSet.cs` | ScriptableObject | NPC별 대화 세트 |
| `NPCDialogueController.cs` | MonoBehaviour | NPC 대화 실행 |
| `DialogueManager.cs` | MonoBehaviour | 플레이어 대화 |
| `StageStoryController.cs` | MonoBehaviour | 스토리 대화 트리거 |
| `StoryNPCConfig.cs` | ScriptableObject | 스토리 NPC 설정 |
| `StoryNPCSpawnHelper.cs` | MonoBehaviour | 스토리 NPC 스폰 |

### ❌ **사용되지 않음 (삭제 가능)**

| 파일명 | 이유 |
|--------|------|
| `StageDialogueController.cs` | StageStoryController와 중복 |
| `StageDialogueData.cs` | 실제로 사용되지 않음 |

---

## 🎮 컴포넌트 배치

### **NPC GameObject**

```
NPC Prefab (일반 NPC)
├─ NPCMovement
├─ NPCOrderController
├─ NPCDialogueController ⭐
│  └─ Dialogue Set: RandomNPC_Dialogue
└─ Canvas
   └─ DialogueBubbleUI

NPC Prefab (스토리 NPC)
├─ NPCMovement
├─ NPCOrderController
├─ NPCDialogueController ⭐
│  └─ Dialogue Set: StoryNPC_A_Dialogue
└─ Canvas
   └─ DialogueBubbleUI
```

### **씬 설정**

```
Scene Hierarchy
├─ GameManager
├─ ScoreManager
├─ DialogueManager ⭐
├─ NPCSpawnManager
│  └─ StoryNPCSpawnHelper ⭐ (같은 GameObject)
└─ StageStoryController ⭐
```

---

## 🔍 각 시스템이 사용하는 UI

| 시스템 | UI 위치 | UI 타입 |
|--------|---------|---------|
| NPCDialogueController | NPC 위 (World Space) | DialogueBubbleUI |
| DialogueManager | 화면 중앙 (Screen Space) | 별도 캐릭터 UI |
| StageStoryController | NPC 위 (World Space) | DialogueBubbleUI (마지막 NPC 것 사용) |

---

## 💡 핵심 포인트

### 1. NPCDialogueController는 만능
- **모든 NPC 대화** (일반, 스토리 모두) 실행
- **DialogueType으로 구분**
  - 일반: Intro, Order, Exit
  - 스토리: StoryIntro, StoryOrder, StoryAfterSummary

### 2. DialogueManager vs StageStoryController
- **DialogueManager**: 플레이어 캐릭터와 대화 (별도 UI)
- **StageStoryController**: 마지막 NPC와 추가 대화 (NPC UI)

### 3. 중복 시스템
- **StageDialogueController** ❌
  - StageStoryController와 같은 일을 함
  - 단, StageDialogueData를 사용 (비효율적)
  - **삭제 권장**

---

## 🚀 다음에 할 일

1. ❌ **StageDialogueController.cs 삭제**
2. ❌ **StageDialogueData.cs 삭제**
3. ✅ **StoryNPCConfig 생성** (ScriptableObject)
4. ✅ **스토리 NPC 프리팹 준비**
5. ✅ **StageStoryController 씬에 배치**
6. ✅ **StoryNPCSpawnHelper 배치**

---

## 📞 질문?

- NPCDialogueController: NPC 대화 전부
- DialogueManager: 플레이어 대화
- StageStoryController: 스토리 추가 대화

**삭제:**
- StageDialogueController
- StageDialogueData
