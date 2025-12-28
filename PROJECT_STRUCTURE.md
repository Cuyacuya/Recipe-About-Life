# RECIPE ABOUT LIFE - 프로젝트 구조 문서

## 1. 게임 개요

| 항목 | 내용 |
|------|------|
| 게임명 | RECIPE ABOUT LIFE (인생에 대한 레시피) |
| 장르 | 힐링 요리 시뮬레이션 + 대화형 스토리 |
| 플랫폼 | 모바일 (세로) / 싱글 플레이 |
| 개발 기간 | 2025.11.18 ~ 2025.12.31 (약 2달) |
| 개발 인원 | 프로그래머 2, 아트 2, 사운드 1 |

### 한 줄 소개
> 아내를 잃고 무너진 요리사가 푸드트럭을 끌고 전국을 여행하며 핫도그를 팔고 사람들과 이야기를 나누는 과정에서 삶의 색을 되찾는 힐링 요리 스토리 게임.

---

## 2. 핵심 게임 루프

```
로비 → 요리(손님 5명) → 마지막 손님 대화 → 결과 → 로비
```

### 스테이지 구성
- 1 스테이지 = 손님 5명
- Stage 1 → 스토리 대화
- Stage 2 → 스토리 대화  
- Stage 3 → 재료 해금 NPC + 다음 맵 해금

### 클리어 조건
- 목표 재화 달성 → 스토리 진행
- 미달성 → 다시 도전

---

## 3. 요리 시스템 (핫도그 조리 6단계)

| 단계 | 이름 | 설명 |
|------|------|------|
| 1 | 꼬치 들기 | 꼬치 클릭 시 손에 든 상태 |
| 2 | 속재료 끼우기 | 소시지/치즈/반반 선택 |
| 3 | 반죽 묻히기 | 회전하여 게이지 채우기 |
| 4 | 튀기기 | 색 변화 타이밍 맞추기 (황금색 최적) |
| 5 | 설탕/소스 | 설탕 → 소스 순서 필수 |
| 6 | 완성 및 제공 | 손님에게 드래그해서 제공 |

### 튀김 색상 (시간 기반)
```
0~3초: Raw (생것)
3~7초: Yellow (덜 익음)
7~9초: Golden (최적!)
9~11초: Brown (약간 탐)
11초+: Burnt (탐)
```

---

## 4. 멘탈 시스템

- 멘탈 범위: 0 ~ 3
- 스테이지 시작: 멘탈 3 (채도 50%)
- 실수 시 멘탈 감소 → 화면 무채색화
- 스테이지 클리어 시 채도 증가

### 멘탈 감소 조건
- 속재료 틀림 → 멘탈 -1
- 설탕 틀림 → 멘탈 -1
- 소스→설탕 순서 실수 → 멘탈 -1
- 튀김 태움(Burnt) 또는 생것(Raw) → 멘탈 -1

---

## 5. 프로젝트 폴더 구조

```
Assets/Scripts/
├── Core/                           # 🔴 공유 데이터 (둘 다 사용)
│   ├── Constants.cs
│   ├── GameSessionConnector.cs
│   └── Data/
│       ├── CustomerResult.cs
│       ├── GameSessionData.cs      # ⭐ 핵심 공유 ScriptableObject
│       └── StageData.cs
│
├── Cooking/                        # 🔵 정우현 담당 (요리 시스템)
│   ├── Core/
│   │   ├── ICookingStep.cs
│   │   ├── CookingDataModels.cs
│   │   └── CookingManager.cs
│   └── Data/
│       └── RecipeConfigSO.cs
│
├── Events/                         # 🔵 정우현 담당 (이벤트 시스템)
│   └── EventSystem.cs
│
├── NPC/                            # 🟢 최혁도 담당 (NPC 시스템)
│   ├── NPCController.cs            # NPC 이동 및 주문 관리
│   ├── NPCData.cs                  # NPC 데이터 모델
│   ├── NPCSpawner.cs               # NPC 생성 및 스폰
│   └── Data/
│       └── NPCConfigSO.cs          # NPC 설정 ScriptableObject
│
├── Dialogue/                       # 🟢 최혁도 담당 (대화 시스템)
│   ├── DialogueManager.cs          # 대화 진행 관리
│   ├── DialogueData.cs             # 대화 데이터 모델
│   └── Data/
│       └── DialogueSO.cs           # 대화 데이터 ScriptableObject
│
├── Systems/                        # 🟢 최혁도 담당
│   ├── MentalManager.cs
│   └── InventoryManager.cs
│
└── UI/                             # 🟢 최혁도 담당
    ├── Panels/
    │   ├── MainMenuPanel.cs
    │   ├── LobbyPanel.cs
    │   ├── ResultPanel.cs
    │   └── DialoguePanel.cs
    └── HUD/
        └── ScoreHUD.cs
```

---

## 6. 역할 분담

### 정우현 (요리 플레이)
- CookingManager (요리 단계 FSM)
- CookingDataModels (HotdogRecipe, CustomerOrder 등)
- EventSystem (GameEvents)
- RecipeConfigSO (요리 설정)
- 6개 Step 클래스 (StickPickup, Ingredient, Batter, Frying, Topping, Completion)

### 최혁도 (NPC/대화/UI/결과)
- GameSessionData (공유 데이터)
- **NPCController** (NPC 이동 및 주문)
- **NPCSpawner** (NPC 생성 관리)
- **DialogueManager** (NPC 대화 시스템)
- **DialogueSO** (대화 데이터)
- MentalManager (멘탈/채도)
- ResultPanel (결과 화면)
- DialoguePanel (대화 UI)
- ScoreHUD (실시간 점수)
- MainMenu, Lobby UI

---

## 7. 데이터 흐름

```
┌─────────────────────────────────────────────────────────────┐
│                      정우현 영역                             │
│                                                             │
│   CookingManager → HotdogRecipe 완성                        │
│         ↓                                                   │
│   GameEvents.TriggerRecipeCompleted(recipe)                 │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      공유 영역                               │
│                                                             │
│   GameSessionConnector (이벤트 구독)                         │
│         ↓                                                   │
│   GameSessionData.AddResultFromRecipe(recipe)               │
│         ↓                                                   │
│   CustomerResult 생성 및 저장                                │
└─────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      최혁도 영역                             │
│                                                             │
│   sessionData.GetStageSummary() → ResultPanel 표시          │
│   sessionData.GetCurrentMental() → MentalManager 채도 조절   │
└─────────────────────────────────────────────────────────────┘
```

---

## 8. 핵심 클래스 설명

### GameSessionData (ScriptableObject)
```csharp
// 스테이지 진행 중 모든 데이터 저장
public class GameSessionData : ScriptableObject
{
    public int currentMental;                    // 현재 멘탈 (0~3)
    public int totalMoney;                       // 총 획득 재화
    public List<CustomerResult> customerResults; // 손님별 결과
    
    // 정우현이 호출
    public void AddResultFromRecipe(HotdogRecipe recipe);
    
    // 최혁도가 호출
    public StageResultSummary GetStageSummary();
}
```

### CustomerResult
```csharp
// 손님 한 명의 결과 (HotdogRecipe에서 변환)
public class CustomerResult
{
    public FillingType servedFilling;    // 제공한 속재료
    public FryingColor servedFryingColor; // 튀김 상태
    public float quality;                 // 품질 점수 (0~100)
    public int moneyEarned;              // 획득 재화
    public int mentalChange;             // 멘탈 변화량
    
    // HotdogRecipe → CustomerResult 변환
    public static CustomerResult FromRecipe(HotdogRecipe recipe, CustomerOrder order);
}
```

### GameEvents (이벤트 시스템)
```csharp
// 매니저 간 통신
public static class GameEvents
{
    // 요리 완성 시 (정우현 발행 → 최혁도 구독)
    public static event Action<HotdogRecipe> OnRecipeCompleted;
    
    // 실수 발생 시
    public static event Action OnMistakeMade;
    
    // 멘탈 변경 시
    public static event Action<int> OnMentalChanged;
    
    // 모든 손님 완료 시
    public static event Action OnAllCustomersServed;
}
```

---

## 9. 씬 구성

| 씬 이름 | 설명 |
|---------|------|
| MainMenuScene | 메인 화면 (Start, Setting, Exit) |
| LobbyScene | 스테이지 선택 |
| GamePlayScene | 요리 플레이 |

---

## 10. 주요 Enum 정리

### FillingType (속재료)
```csharp
public enum FillingType
{
    Sausage,  // 소시지
    Cheese,   // 치즈
    Mixed     // 반반
}
```

### FryingColor (튀김 상태)
```csharp
public enum FryingColor
{
    Raw,      // 생것 (0~3초)
    Yellow,   // 덜 익음 (3~7초)
    Golden,   // 최적 (7~9초)
    Brown,    // 약간 탐 (9~11초)
    Burnt     // 탐 (11초+)
}
```

### CookingStepType (요리 단계)
```csharp
public enum CookingStepType
{
    None,
    StickPickup,   // 1. 꼬치 들기
    Ingredient,    // 2. 속재료 끼우기
    Batter,        // 3. 반죽 묻히기
    Frying,        // 4. 튀기기
    Topping,       // 5. 토핑/소스
    Completed      // 6. 완성
}
```

---

## 11. NPC 대화 시스템

### 11.1 NPC 등장 및 주문 프로세스

```
1. NPC 등장: 화면 오른쪽에서 왼쪽으로 걷기
2. 중간 지점 도착: 멈춰서기
3. 주문하기: 말풍선으로 주문 표시 (속재료, 소스 등)
4. 요리 대기: 애니메이션 루프
5. 요리 완성: 손님에게 드래그해서 제공
```

### 11.2 스테이지 클리어 후 대화 분기

```
스테이지 완료
    ↓
재화 체크
    ├── 목표 달성 ✅
    │       ↓
    │   특별한 손님 등장 (10명 중 랜덤)
    │       ↓
    │   의미있는 스토리 대화
    │       ↓
    │   Stage 1,2: 스토리 진행
    │   Stage 3: 재료 해금 알림
    │
    └── 목표 미달 ❌
            ↓
        일반 손님 등장
            ↓
        일상적인 대화
            ↓
        로비로 복귀
```

### 11.3 NPC 데이터 구조

```csharp
// NPC 기본 정보
public class NPCData
{
    public int npcID;                    // NPC 고유 ID (1~10)
    public string npcName;               // NPC 이름
    public Sprite npcSprite;             // NPC 스프라이트
    public float walkSpeed;              // 걷기 속도
    public Vector3 stopPosition;         // 멈출 위치

    // 주문 정보
    public FillingType preferredFilling;
    public SauceType preferredSauce;
    public bool wantsSugar;
}

// NPC 설정 ScriptableObject
[CreateAssetMenu(fileName = "NPCConfig", menuName = "Game/NPC Config")]
public class NPCConfigSO : ScriptableObject
{
    public List<NPCData> allNPCs;        // 10명의 NPC 데이터
    public int moneyThreshold;           // 특별 대화 재화 기준
}
```

### 11.4 대화 데이터 구조

```csharp
// 대화 한 줄
[System.Serializable]
public class DialogueLine
{
    public string speakerName;           // 화자 이름
    public string text;                  // 대화 내용
    public Sprite speakerSprite;         // 화자 스프라이트 (선택)
}

// 대화 묶음
[CreateAssetMenu(fileName = "Dialogue", menuName = "Game/Dialogue")]
public class DialogueSO : ScriptableObject
{
    public int npcID;                    // 연결된 NPC ID
    public int stageNumber;              // 스테이지 번호 (1, 2, 3)
    public bool isSpecialDialogue;       // true: 목표 달성 시, false: 실패 시
    public List<DialogueLine> lines;     // 대화 내용
    public bool unlocksIngredient;       // 재료 해금 여부 (Stage 3)
    public string unlockedIngredientName; // 해금 재료 이름
}
```

### 11.5 주요 클래스 설명

#### NPCController.cs
```csharp
// NPC 한 명의 행동 제어
public class NPCController : MonoBehaviour
{
    public NPCData npcData;

    private void Start()
    {
        // 오른쪽에서 등장
        WalkToPosition(npcData.stopPosition);
    }

    private void WalkToPosition(Vector3 target)
    {
        // 이동 애니메이션 + DOTween/Coroutine
    }

    public void ShowOrder()
    {
        // 주문 말풍선 표시
    }

    public void ReceiveFood(HotdogRecipe recipe)
    {
        // 음식 받기 → 평가 → 퇴장
    }
}
```

#### NPCSpawner.cs
```csharp
// NPC 생성 및 관리
public class NPCSpawner : MonoBehaviour
{
    public NPCConfigSO npcConfig;
    public Transform spawnPoint;         // 오른쪽 스폰 위치
    public Transform exitPoint;          // 왼쪽 퇴장 위치

    public void SpawnRandomNPC()
    {
        // 랜덤 NPC 생성
    }

    public void SpawnStoryNPC(int npcID)
    {
        // 특정 NPC 생성 (스토리용)
    }
}
```

#### DialogueManager.cs
```csharp
// 대화 진행 관리
public class DialogueManager : MonoBehaviour
{
    private DialogueSO currentDialogue;
    private int currentLineIndex;

    public void StartDialogue(int npcID, int stage, bool isSuccess)
    {
        // 조건에 맞는 대화 로드
        // DialoguePanel UI 활성화
    }

    public void ShowNextLine()
    {
        // 다음 대화 라인 표시
    }

    public void EndDialogue()
    {
        // 대화 종료 → 재료 해금 처리 → 로비 복귀
    }
}
```

### 11.6 이벤트 연동

```csharp
// GameEvents에 추가될 이벤트들
public static event Action<NPCData> OnNPCOrderReady;        // NPC 주문 준비 완료
public static event Action<int> OnNPCServed;                // NPC에게 서빙 완료
public static event Action<bool> OnStageDialogueStart;      // 대화 시작 (성공/실패)
public static event Action<string> OnIngredientUnlocked;    // 재료 해금
```

---

## 12. 개발 일정 (최혁도)

| 작업 | 기간 | 상태 |
|------|------|------|
| 메인화면 | 11/20 ~ 11/22 | ✅ 완료 |
| 공유 데이터 구조 | 11/22 ~ 11/23 | 🔄 진행중 |
| NPC 대화시스템 | 11/22 ~ 11/26 | 예정 |
| 발주/재고 | 11/26 ~ 12/04 | 예정 |
| 멘탈/점수 | 12/05 ~ 12/12 | 예정 |
| 스토리컷씬 | 12/13 ~ 12/18 | 예정 |
| 환경설정 | 12/18 ~ 12/19 | 예정 |

---

## 12. Git 브랜치 전략

```
main
  └── develop
        ├── feature/cooking-system (정우현)
        └── feature/ui-system (최혁도)
```

---

## 13. 참고 사항

- Unity 버전: 2022.3 LTS 권장
- 이벤트 기반 통신으로 느슨한 결합 유지
- ScriptableObject로 데이터와 코드 분리
- 각자 독립적으로 테스트 가능한 구조
