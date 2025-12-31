# NPC 프리팹 체크리스트

## ✅ 결론: 프리팹 수정 필요 없음!

**오늘 추가한 시스템은 모두 씬 레벨에서 동작하므로**
**기존 NPC 프리팹을 수정할 필요가 없습니다.**

---

## 📋 NPC 프리팹 필수 구성 (기존과 동일)

### **일반 NPC 프리팹** (랜덤 NPC용)

```
NPC GameObject (예: NPC1_HighschoolGirl)
├─ SpriteRenderer
├─ NPCMovement ✅ (기존)
├─ NPCOrderController ✅ (기존)
├─ NPCDialogueController ✅ (어제 추가)
│  └─ Dialogue Set: RandomNPC_Dialogue (할당 필요)
└─ Canvas (World Space)
   └─ DialogueBubbleUI ✅ (기존)
```

### **스토리 NPC 프리팹** (예: NPC8_Ajeossi)

```
NPC GameObject
├─ SpriteRenderer
├─ NPCMovement ✅ (기존)
├─ NPCOrderController ✅ (기존)
├─ NPCDialogueController ✅ (어제 추가)
│  └─ Dialogue Set: StoryNPC_A_Dialogue (할당 필요) ⭐
└─ Canvas (World Space)
   └─ DialogueBubbleUI ✅ (기존)
```

**차이점:** DialogueSet만 다름!

---

## 🆕 오늘 추가한 시스템 (NPC 프리팹 수정 불필요)

### **씬 레벨 컴포넌트들**

| 컴포넌트 | 위치 | NPC 프리팹 수정? |
|----------|------|------------------|
| StoryNPCConfig | ScriptableObject | ❌ 아님 |
| StoryNPCSpawnHelper | NPCSpawnManager GameObject | ❌ 아님 |
| StageStoryController | 씬에 별도 GameObject | ❌ 아님 |

**→ NPC 프리팹은 건드릴 필요 없음!**

---

## ✅ 확인해야 할 것 (프리팹 열어서 확인)

### **1. NPCDialogueController가 있는지 확인**

모든 NPC 프리팹을 열어서:

```
Inspector에서 확인:
├─ NPCMovement ✅
├─ NPCOrderController ✅
├─ NPCDialogueController ✅ ← 이게 있는지 확인!
└─ Canvas > DialogueBubbleUI ✅
```

**없다면:**
- `Add Component` → `NPCDialogueController` 추가

---

### **2. Dialogue Set이 할당되어 있는지 확인**

NPCDialogueController Inspector:

```
┌─ NPC Dialogue Controller ──────┐
│ 대화 데이터                      │
│ └─ Dialogue Set:               │
│    [  ] ← 여기가 비어있으면 안됨! │
└─────────────────────────────────┘
```

**비어있다면:**
- NPCDialogueSet ScriptableObject를 만들어서 할당 필요

---

### **3. 스토리 NPC의 DialogueSet에 StoryAfterSummary 대화가 있는지 확인**

스토리 NPC (NPC8_Ajeossi 등)의 DialogueSet 열어서:

```
NPCDialogueSet Inspector
└─ Dialogue Groups
   ├─ Intro
   ├─ Order
   ├─ ServedSuccess
   ├─ ServedFail
   ├─ Exit
   ├─ StoryIntro
   ├─ StoryOrder
   ├─ StoryServedSuccess
   ├─ StoryServedFail
   └─ StoryAfterSummary ⭐ ← 이게 있어야 함!
      └─ Lines:
         ├─ "축하해요! 목표 달성했어요!"
         ├─ "정말 열심히 하셨네요!"
         └─ ...
```

**없다면:**
- Dialogue Groups 크기 늘리기
- Type을 `StoryAfterSummary`로 설정
- Lines 추가

---

## 🎯 프리팹별 체크리스트

### **일반 NPC (10개)**

프리팹 목록:
- NPC1_HighschoolGirl
- NPC2_GrandPa
- NPC3_GrandMa
- NPC4_Man1
- NPC5_UniMan
- NPC6_Woman1
- NPC7_Woman2
- NPC9_Man2
- NPC10_Woman3
- NPC11_OldWoman
- NPC13_Man3

확인 사항:
- [ ] NPCDialogueController 컴포넌트 있음?
- [ ] Dialogue Set 할당되어 있음?
- [ ] DialogueSet에 기본 대화 있음? (Intro, Order, Exit)

---

### **스토리 NPC (3개 예정)**

현재 있는 것:
- NPC8_Ajeossi (StoryNPC 폴더)

추가로 만들어야 할 것:
- 스토리 NPC B (Stage 2용)
- 스토리 NPC C (Stage 3용)

확인 사항:
- [ ] NPCDialogueController 컴포넌트 있음?
- [ ] Dialogue Set 할당되어 있음?
- [ ] DialogueSet에 Story 타입 대화 있음?
  - [ ] StoryIntro
  - [ ] StoryOrder
  - [ ] StoryServedSuccess
  - [ ] StoryServedFail
  - [ ] **StoryAfterSummary** ⭐ (가장 중요!)

---

## 🚫 하지 말아야 할 것

### ❌ 프리팹에 추가하면 안 되는 컴포넌트:

- StoryNPCSpawnHelper (NPCSpawnManager에만 부착)
- StageStoryController (씬에 별도 GameObject로 배치)

### ❌ 프리팹에서 수정하면 안 되는 것:

- NPCMovement
- NPCOrderController
- DialogueBubbleUI

→ 이것들은 기존 그대로 유지!

---

## 📝 정리

### **수정 필요 없음:**
- ✅ 프리팹 구조는 그대로
- ✅ 컴포넌트 추가 불필요
- ✅ 기존 시스템 변경 없음

### **확인만 하면 됨:**
- ✅ NPCDialogueController 있는지
- ✅ Dialogue Set 할당되어 있는지
- ✅ 스토리 NPC의 StoryAfterSummary 대화 있는지

### **만약 없다면:**
- NPCDialogueController 추가
- NPCDialogueSet 생성 후 할당
- StoryAfterSummary 대화 작성

---

## 🔍 빠른 확인 방법

Unity 에디터에서:

1. **프리팹 열기**
   ```
   Project 창 → Prefabs/NPC/ → 프리팹 더블클릭
   ```

2. **Inspector에서 확인**
   ```
   Components 목록:
   ✅ NPCMovement
   ✅ NPCOrderController
   ✅ NPCDialogueController ← 이게 있는지!
   ```

3. **Dialogue Set 확인**
   ```
   NPCDialogueController
   └─ Dialogue Set: [할당되어 있는지 확인]
   ```

4. **스토리 NPC만 추가 확인**
   ```
   Dialogue Set 더블클릭
   → Dialogue Groups에서 StoryAfterSummary 확인
   ```

---

## ✅ 최종 결론

**NPC 프리팹 수정 필요 없음!**

단, 다음만 확인:
1. NPCDialogueController 컴포넌트 있는지
2. Dialogue Set 할당되어 있는지
3. 스토리 NPC의 StoryAfterSummary 대화 있는지

모두 **어제 작업**에서 이미 완료되어야 할 내용입니다.
오늘 추가한 시스템은 **씬 레벨**에서만 동작하므로
프리팹은 건드릴 필요가 없습니다! 🎉
