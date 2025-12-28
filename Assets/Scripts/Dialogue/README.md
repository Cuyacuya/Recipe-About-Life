# Dialogue System - 대화 시스템 확장

Recipe About Life 프로젝트의 ScriptableObject 기반 대화 시스템입니다.

## 📋 개요

이 시스템은 **기존 대화 시스템을 유지**하면서, **NPC별 대화와 스테이지 종료 대화**를 추가로 관리합니다.

### 기존 시스템 (유지)
- `DialogueManager.cs` - 스테이지 종료 후 플레이어와의 대화
- `DialogueBubbleUI.cs` - 말풍선 UI

### 새로운 시스템 (추가)
- **NPC별 대화** - 각 NPC마다 다른 대사 설정
- **스테이지 종료 대화** - 성공/실패에 따른 엔딩 대화
- **ScriptableObject 기반** - 대화 데이터를 에셋으로 관리

---

## 📁 파일 구조

```
Assets/Scripts/Dialogue/
├── DialogueEnums.cs           # 대화 타입, 발화자 enum
├── DialogueLine.cs            # 대화 한 줄 데이터 구조
├── DialogueManager.cs         # [기존] 플레이어 대화 매니저
├── NPCDialogueSet.cs          # [신규] NPC 대화 ScriptableObject
├── NPCDialogueController.cs   # [신규] NPC 대화 실행 컨트롤러
├── StageDialogueData.cs       # [신규] 스테이지 대화 ScriptableObject
└── StageDialogueController.cs # [신규] 스테이지 대화 실행 컨트롤러

Assets/ScriptableObjects/Dialogue/
└── (여기에 대화 데이터 ScriptableObject 생성)
```

---

## 🎭 대화 타입 (DialogueType)

NPC의 라이프사이클에 따른 5가지 대화 타입:

| 타입 | 설명 | 트리거 시점 |
|------|------|------------|
| `Intro` | 등장 인사 | NPC가 중앙에 도착했을 때 |
| `Order` | 주문 멘트 | 주문을 시작할 때 |
| `ServedSuccess` | 성공 대사 | 주문과 일치하는 음식을 받았을 때 |
| `ServedFail` | 실패 대사 | 주문과 다른 음식을 받았을 때 |
| `Exit` | 퇴장 인사 | 음식을 받고 떠날 때 |

---

## 🗣️ 발화자 타입 (SpeakerType)

| 타입 | 설명 |
|------|------|
| `NPC` | NPC가 말하는 대사 |
| `Player` | 플레이어가 말하는 대사 |
| `System` | 시스템 내레이션/설명 |

---

## 📝 사용 방법

### 1. NPC 대화 데이터 생성

**Unity 에디터에서:**
1. Project 창에서 우클릭
2. `Create > RecipeAboutLife > Dialogue > NPC Dialogue Set`
3. 파일명: `NPC_Businessman_Dialogue` (예시)
4. Inspector에서 설정:
   - `npcID`: "NPC_Businessman"
   - `npcDisplayName`: "회사원"
   - 각 대화 타입별 대화 라인 작성

**자동 생성되는 기본 대화:**
- OnValidate() 함수가 자동으로 5개 대화 그룹 생성
- 각 그룹에 기본 예시 대사 포함
- Inspector에서 수정하여 사용

### 2. 스테이지 대화 데이터 생성

**Unity 에디터에서:**
1. Project 창에서 우클릭
2. `Create > RecipeAboutLife > Dialogue > Stage Dialogue`
3. 파일명: `Stage1_Dialogue`
4. Inspector에서 설정:
   - `stageID`: 1
   - `stageName`: "Stage 1"
   - `finalSuccessDialogue`: 성공 시 대화 라인들
   - `finalFailDialogue`: 실패 시 대화 라인들

### 3. NPC 프리팹 설정

**NPC GameObject 구성:**
```
NPC GameObject
├── NPCMovement (기존)
├── NPCOrderController (기존)
├── NPCDialogueController (신규 추가)
│   └── Dialogue Set: NPC_Businessman_Dialogue (연결)
└── DialogueBubbleUI (기존, 주문 + 대화 겸용)
```

**컴포넌트 추가:**
1. NPC 프리팹에 `NPCDialogueController` 추가
2. Inspector에서 `Dialogue Set` 필드에 생성한 ScriptableObject 연결
3. `Dialogue Bubble` 필드에 DialogueBubbleUI 연결 (자동 검색됨)

### 4. 씬 설정 (StageDialogueController)

**Canvas 구성:**
```
Canvas (Screen Space)
└── DialoguePanel (GameObject)
   ├── Background (Image)
   ├── SpeakerNameText (TextMeshProUGUI)
   └── DialogueText (TextMeshProUGUI)
```

**컴포넌트 설정:**
1. 씬에 빈 GameObject 생성 → `StageDialogueController` 추가
2. Inspector에서 설정:
   - `Stage Dialogues`: 생성한 스테이지 대화 데이터들 추가
   - `Dialogue Panel`: 위에서 만든 DialoguePanel 연결
   - `Dialogue Text`: DialogueText 연결
   - `Speaker Name Text`: SpeakerNameText 연결 (선택)

---

## 🔌 기존 시스템과의 연결

### ⚠️ 중요: 기존 코드는 수정하지 않음

주문/스폰/점수 시스템(OrderData, OrderManager, NPCOrderController, NPCMovement, ScoreManager 등)은 **절대 수정하지 않습니다**.

대화 시스템은 기존 시스템과 **독립적**으로 동작합니다.

### 연결 포인트 (향후 구현 시 참고)

#### 1. NPC 등장 시 (Intro 대화)

**위치:** `NPCMovement.cs` - `OnArrived()` 또는 `Start()`

```csharp
// TODO: NPC 등장 대화 (현재 구현하지 않음)
NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
if (dialogueController != null)
{
    dialogueController.StartDialogue(DialogueType.Intro);
}
```

#### 2. 주문 시작 시 (Order 대화)

**위치:** `NPCOrderController.cs` - `RequestOrder()` 이후

```csharp
// TODO: 주문 대화 (현재 구현하지 않음)
NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
if (dialogueController != null)
{
    dialogueController.StartDialogue(DialogueType.Order);
}
```

#### 3. 음식 서빙 후 (ServedSuccess / ServedFail)

**위치:** `ScoreManager.cs` - `OnRecipeCompleted()`

```csharp
// TODO: 서빙 대화 (현재 구현하지 않음)
NPCSpawnManager spawnManager = FindFirstObjectByType<NPCSpawnManager>();
GameObject currentNPC = spawnManager.GetCurrentNPC();
NPCDialogueController dialogueController = currentNPC?.GetComponent<NPCDialogueController>();

if (dialogueController != null)
{
    DialogueType type = recipe.matchesOrder ? DialogueType.ServedSuccess : DialogueType.ServedFail;
    dialogueController.StartDialogue(type);
}
```

#### 4. NPC 퇴장 시 (Exit 대화)

**위치:** `NPCMovement.cs` - `OnOrderComplete()` 호출 전

```csharp
// TODO: 퇴장 대화 (현재 구현하지 않음)
NPCDialogueController dialogueController = GetComponent<NPCDialogueController>();
if (dialogueController != null)
{
    dialogueController.StartDialogue(DialogueType.Exit);

    // 대화 종료 후 퇴장
    dialogueController.OnDialogueEnded += (type) =>
    {
        if (type == DialogueType.Exit)
        {
            StartExit(); // 퇴장 시작
        }
    };
}
```

#### 5. 스테이지 종료 시 (마지막 손님 대화)

**자동 연결됨!**

`StageDialogueController`가 `ScoreManager.OnStageCompleted` 이벤트를 자동으로 구독하므로, **추가 코드 필요 없음**.

---

## 🎮 런타임 API

### NPCDialogueController

```csharp
// 대화 시작
bool success = dialogueController.StartDialogue(DialogueType.Intro);

// 대화 중단
dialogueController.StopDialogue();

// 상태 확인
bool isActive = dialogueController.IsDialogueActive();

// 대화 세트 변경
dialogueController.SetDialogueSet(newDialogueSet);
```

### StageDialogueController

```csharp
// 스테이지 종료 대화 시작
StageDialogueController.Instance.StartStageFinalDialogue(stageID: 1, isSuccess: true);

// 대화 중단
StageDialogueController.Instance.StopDialogue();

// 상태 확인
bool isActive = StageDialogueController.Instance.IsDialogueActive();
```

---

## 🧪 테스트 방법

### 1. 에디터 Context Menu 사용

**NPCDialogueController:**
- Hierarchy에서 NPC 선택
- Inspector에서 NPCDialogueController 찾기
- 우클릭 → Context Menu:
  - `Test: Play Intro`
  - `Test: Play Order`
  - `Test: Play Served Success`
  - `Test: Play Served Fail`
  - `Test: Play Exit`
  - `Test: Stop Dialogue`

**StageDialogueController:**
- Hierarchy에서 StageDialogueController 선택
- 우클릭 → Context Menu:
  - `Test: Play Success Dialogue`
  - `Test: Play Fail Dialogue`
  - `Test: Stop Dialogue`

### 2. ScriptableObject 검증

**NPCDialogueSet:**
- Project 창에서 대화 데이터 선택
- 우클릭 → Context Menu:
  - `Validate Dialogue Set` - 유효성 검증
  - `Log Dialogue Info` - 대화 정보 출력

**StageDialogueData:**
- Project 창에서 스테이지 대화 데이터 선택
- 우클릭 → Context Menu:
  - `Validate Stage Dialogue`
  - `Log Stage Dialogue Info`

---

## ⚙️ 설정 옵션

### NPCDialogueController

| 필드 | 설명 | 기본값 |
|------|------|--------|
| `dialogueSet` | NPC 대화 ScriptableObject | null |
| `dialogueBubble` | 말풍선 UI | 자동 검색 |
| `lineDisplayTime` | 각 대화 라인 표시 시간 (초) | 3.0 |
| `linePauseDuration` | 대화 라인 간 간격 (초) | 0.5 |

### StageDialogueController

| 필드 | 설명 | 기본값 |
|------|------|--------|
| `stageDialogues` | 스테이지 대화 데이터 목록 | 빈 리스트 |
| `dialoguePanel` | 대화 패널 (Screen Space) | null |
| `dialogueText` | 대화 텍스트 (TMP) | null |
| `speakerNameText` | 발화자 이름 (TMP, 선택) | null |
| `lineDisplayTime` | 각 대화 라인 표시 시간 (초) | 3.0 |
| `linePauseDuration` | 대화 라인 간 간격 (초) | 0.5 |

---

## 📊 데이터 구조

### DialogueLine
```csharp
public class DialogueLine
{
    public SpeakerType speaker;    // 발화자 (NPC, Player, System)
    public string text;            // 대화 내용
    public float displayDuration;  // 표시 시간 (0이면 기본값 사용)
}
```

### DialogueGroup
```csharp
public class DialogueGroup
{
    public DialogueType type;      // 대화 타입 (Intro, Order 등)
    public DialogueLine[] lines;   // 대화 라인들
}
```

### NPCDialogueSet
```csharp
public class NPCDialogueSet : ScriptableObject
{
    public string npcID;                        // NPC 식별자
    public string npcDisplayName;               // 표시 이름
    public List<DialogueGroup> dialogueGroups;  // 대화 그룹들
}
```

### StageDialogueData
```csharp
public class StageDialogueData : ScriptableObject
{
    public int stageID;                            // 스테이지 번호
    public string stageName;                       // 스테이지 이름
    public List<DialogueLine> finalSuccessDialogue; // 성공 대화
    public List<DialogueLine> finalFailDialogue;    // 실패 대화
}
```

---

## 🔮 TODO: 향후 구현 예정

### 대화 연출
- [ ] 대화 타이핑 효과 (한 글자씩 표시)
- [ ] 다음 대화로 넘기는 버튼/터치
- [ ] 대화 스킵 기능
- [ ] 캐릭터 표정/감정 표현
- [ ] 대화 중 효과음/배경음악

### 게임플레이 통합
- [ ] 대화 중 게임 입력 차단
- [ ] 대화 완료 후 보상 연출
- [ ] 대화 선택지 시스템
- [ ] 대화 기록 (로그)

### 편의 기능
- [ ] 대화 데이터 CSV/JSON 임포트
- [ ] 다국어 지원
- [ ] 대화 미리보기 에디터 툴

---

## 🐛 디버그 로그

모든 대화 시스템 로그는 `[NPCDialogueController]`, `[StageDialogueController]` 등의 태그로 시작합니다.

**로그 예시:**
```
[NPCDialogueController] NPC_Businessman Intro 대화 시작 (2 라인)
[NPCDialogueController] [NPC] 안녕하세요! 주문할게요.
[StageDialogueController] 스테이지 1 성공 대화 시작 (5 라인)
```

---

## 🔗 관련 시스템

- **주문 시스템**: OrderData, OrderManager, NPCOrderController
- **NPC 시스템**: NPCMovement, NPCSpawnManager
- **점수 시스템**: ScoreManager
- **이벤트 시스템**: GameEvents (EventSystem.cs)

**주의:** 위 시스템들은 **절대 수정하지 않습니다**. 대화 시스템은 독립적으로 동작합니다.

---

## 📞 문의

시스템 확장이나 버그 제보는 프로젝트 담당자에게 문의하세요.
