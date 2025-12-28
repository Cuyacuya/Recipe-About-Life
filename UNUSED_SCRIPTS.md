# 사용되지 않는 스크립트 목록

## ❌ 삭제 가능한 파일

### 1. StageDialogueController.cs
- **경로:** `Assets/Scripts/Dialogue/StageDialogueController.cs`
- **이유:** `StageStoryController.cs`와 역할이 완전히 중복
- **차이점:**
  - StageDialogueController: StageDialogueData 사용
  - StageStoryController: NPCDialogueSet 사용 (더 일관성 있음)
- **대체:** `StageStoryController.cs` 사용

### 2. StageDialogueData.cs
- **경로:** `Assets/Scripts/Dialogue/StageDialogueData.cs`
- **이유:** 실제로 사용되지 않음
- **대체:** NPCDialogueSet의 StoryAfterSummary 타입 사용

---

## ⚠️ 삭제 전 확인 사항

삭제하기 전에 다음을 확인하세요:

### 1. 씬에서 사용 확인
```
Hierarchy에서 검색:
- "StageDialogueController"
- 있으면 GameObject 삭제 또는 컴포넌트 제거
```

### 2. ScriptableObject 에셋 확인
```
Project 창에서 검색:
- "StageDialogueData"
- ".asset" 파일이 있으면 삭제
```

### 3. 참조 확인
```
Project 창에서:
- StageDialogueController.cs 우클릭
- "Find References in Scene"
- 참조가 없으면 안전하게 삭제 가능
```

---

## 🔧 삭제 후 작업

### 1. StageStoryController 사용으로 전환

StageDialogueController를 사용하고 있었다면:

**Before:**
```csharp
StageDialogueController controller = StageDialogueController.Instance;
controller.StartStageFinalDialogue(stageID, isSuccess);
```

**After:**
```csharp
// 자동으로 처리됨!
// StageStoryController가 ScoreManager.OnStageCompleted 이벤트를 구독하여
// 재화 조건 만족 시 자동으로 StoryAfterSummary 대화 실행
```

### 2. 데이터 이관

StageDialogueData를 사용했다면:

**Before:**
```
StageDialogueData
├─ stageID: 1
├─ finalSuccessDialogue: [...]
└─ finalFailDialogue: [...]
```

**After:**
```
NPCDialogueSet (스토리 NPC용)
└─ Dialogue Groups
   └─ StoryAfterSummary
      └─ lines: [...]
```

---

## ✅ 삭제 체크리스트

- [ ] 씬에서 StageDialogueController 컴포넌트 제거
- [ ] StageDialogueData .asset 파일 삭제
- [ ] StageDialogueController.cs 삭제
- [ ] StageDialogueData.cs 삭제
- [ ] 에디터에서 스크립트 컴파일 오류 없는지 확인
- [ ] StageStoryController로 정상 작동 확인

---

## 📝 참고

이 파일들은 이전에 작업하던 중 만들어진 것이며,
현재는 더 나은 구조인 StageStoryController로 대체되었습니다.

삭제해도 게임에 영향을 주지 않습니다.
