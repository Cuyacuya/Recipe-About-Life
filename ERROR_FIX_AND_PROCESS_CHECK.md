# 에러 수정 및 전체 프로세스 체크

## 🔴 현재 에러: SerializedObjectNotCreatableException

### 원인
이 에러는 **null 오브젝트를 Inspector에서 선택**했을 때 발생합니다.

### 해결 방법

#### 방법 1: Unity 에디터 재시작 (가장 간단)
1. **Unity 에디터 완전 종료**
2. **Unity 에디터 재시작**
3. **GamePlayScene 다시 열기**
4. ✅ 에러 사라짐 (대부분의 경우)

#### 방법 2: 문제 오브젝트 찾기 및 제거
1. **Console 창 Clear** (우측 상단 Clear 버튼)
2. **Hierarchy에서 모든 GameObject 선택 해제** (빈 공간 클릭)
3. **Project 창 클릭** (씬이 아닌 다른 곳)
4. **에러가 다시 나타나면**:
   - Hierarchy에서 GameObject를 하나씩 클릭
   - 에러가 나타나는 GameObject 찾기
   - 해당 GameObject의 잘못된 컴포넌트 제거 또는 GameObject 삭제 후 재생성

#### 방법 3: 씬 재로드
1. **File > Save** (현재 씬 저장)
2. **File > New Scene** (새 씬 열기)
3. **File > Open Scene** > `GamePlayScene.unity` 다시 열기

---

## ✅ 전체 프로세스 체크리스트

### Phase 1: 컴파일 확인

#### 1.1 컴파일 에러 확인
- [ ] **Console 창 열기** (Ctrl + Shift + C)
- [ ] **컴파일 에러 0개 확인** (Console 창 우측 상단 빨간색 에러 개수)
- [ ] ❌ **에러가 있으면**: 에러 메시지를 복사해서 알려주세요

#### 1.2 필수 스크립트 존재 확인
- [ ] `Assets/Scripts/Cooking/HotdogRecipe.cs`
- [ ] `Assets/Scripts/Utilities/OrderValidator.cs`
- [ ] `Assets/Scripts/Managers/ScoreManager.cs`
- [ ] `Assets/Scripts/NPC/NPCSpawnManager.cs`
- [ ] `Assets/Scripts/Dialogue/StageStoryController.cs`
- [ ] `Assets/Scripts/UI/ResultUIController.cs`
- [ ] `Assets/Scripts/UI/FadeUI.cs`
- [ ] `Assets/Scripts/UI/NPCDialogueUI.cs`
- [ ] `Assets/Scripts/UI/PlayerDialogueUI.cs`
- [ ] `Assets/Scripts/UI/FramePanelUI.cs`

---

### Phase 2: Hierarchy 구조 확인

#### 2.1 필수 Manager GameObject 확인

**Hierarchy에 다음 GameObject들이 있는지 확인**:

```
GamePlayScene
├─ GameManager ✅
│  └─ GameManager.cs
├─ ScoreManager ✅
│  └─ ScoreManager.cs
├─ SimpleCookingManager ✅
│  └─ SimpleCookingManager.cs
├─ NPCSpawnManager ✅
│  └─ NPCSpawnManager.cs
├─ StageStoryController ⚠️ (없으면 추가 필요!)
│  └─ StageStoryController.cs
└─ Canvas
   ├─ FramePanel ✅
   ├─ PlayerDialoguePanel ✅
   ├─ NPCDialoguePanel ✅
   ├─ ResultPanel ✅
   ├─ FadePanel ✅
   └─ ... (요리 관련 UI들)
```

#### 2.2 StageStoryController 추가 (없는 경우)

**없으면 지금 바로 추가**:

1. **Hierarchy 우클릭** > `Create Empty`
2. **이름 변경**: `StageStoryController`
3. **Inspector** > `Add Component` > `StageStoryController` 검색 후 추가
4. **컴포넌트 설정**:
   - Story NPC Config: ⚠️ **나중에 설정** (ScriptableObject 생성 후)
   - Current Stage ID: **1**
   - Delay Before Story Dialogue: **1**
   - Line Display Time: **3**
   - Line Pause Duration: **0.5**
   - Fade Duration: **1**
   - Wait Text Duration: **2**

---

### Phase 3: UI 패널 참조 확인

#### 3.1 ResultPanel 확인

1. **Hierarchy**에서 `Canvas/ResultPanel` 선택
2. **Inspector**에서 `ResultUIController` 컴포넌트 확인:
   - [ ] Result Panel: `ResultPanel` 연결
   - [ ] Total Reward Text: `TotalRewardText` 연결
   - [ ] Target Reward Text: `TargetRewardText` 연결
   - [ ] Result Message Text: `ResultMessageText` 연결
   - [ ] Confirm Button: `ConfirmButton` 연결
3. **초기 상태**: 왼쪽 상단 체크박스 **해제 (비활성화)**

#### 3.2 FadePanel 확인

1. **Hierarchy**에서 `Canvas/FadePanel` 선택
2. **Inspector**에서 `FadeUI` 컴포넌트 확인:
   - [ ] Fade Image: `FadeImage` 연결
   - [ ] Guide Text: `GuideText` 연결
   - [ ] Fade Duration: **1**
3. **FadeImage 확인**:
   - Color Alpha: **0** (투명)
   - Raycast Target: **✅ 체크**

#### 3.3 NPCDialoguePanel 확인

1. **Hierarchy**에서 `Canvas/NPCDialoguePanel` 선택
2. **Inspector**에서 `NPCDialogueUI` 컴포넌트 확인:
   - [ ] Dialogue Panel: `NPCDialoguePanel` 연결
   - [ ] NPC Image: `NPCImage` 연결
   - [ ] NPC Name Text: `NPCNameText` 연결
   - [ ] Dialogue Text: `NPCDialogueText` 연결
3. **초기 상태**: **비활성화**

#### 3.4 PlayerDialoguePanel 확인

1. **Hierarchy**에서 `Canvas/PlayerDialoguePanel` 선택
2. **Inspector**에서 `PlayerDialogueUI` 컴포넌트 확인:
   - [ ] Dialogue Panel: `PlayerDialoguePanel` 연결
   - [ ] Dialogue Text: `PlayerDialogueText` 연결
3. **초기 상태**: **비활성화**

#### 3.5 FramePanel 확인

1. **Hierarchy**에서 `Canvas/FramePanel` 선택
2. **Inspector**에서 `FramePanelUI` 컴포넌트 확인:
   - [ ] Frame Panel: `FramePanel` 연결
3. **초기 상태**: **비활성화**

---

### Phase 4: ScoreManager 설정 확인

1. **Hierarchy**에서 `ScoreManager` 선택
2. **Inspector**에서 확인:
   - [ ] Max NPC Count: **5**
   - [ ] Base Reward Per Recipe: **100**
   - [ ] Target Total Reward: **500** (또는 원하는 목표)
   - [ ] Current NPC Index: **0**

---

### Phase 5: NPCSpawnManager 설정 확인

1. **Hierarchy**에서 `NPCSpawnManager` 선택
2. **Inspector**에서 확인:
   - [ ] NPC Prefabs: **최소 5개 이상** 할당
   - [ ] NPCs Per Stage: **5**
   - [ ] Auto Start On Awake: **✅ 체크**

⚠️ **NPC Prefab이 없으면**:
- NPC Prefab을 먼저 생성해야 합니다
- `GamePlayScene_Setup_Guide.md`의 "2. NPC 시스템 설정" 참고

---

### Phase 6: SimpleCookingManager 설정 확인

1. **Hierarchy**에서 `SimpleCookingManager` 선택
2. **Inspector**에서 확인:
   - [ ] Stick Prefab: 할당
   - [ ] Sausage Prefab: 할당
   - [ ] Cheese Prefab: 할당
   - [ ] Batter Container Prefab: 할당 (선택)
   - [ ] Oil Container Prefab: 할당 (선택)
   - [ ] Ingredient Popup: 할당
   - [ ] Serve Button: 할당

---

### Phase 7: 이벤트 구독 확인 (중요!)

**Play 모드 진입 후 Console 로그 확인**:

1. **Play 버튼 클릭**
2. **Console 창 확인**:
   ```
   [ScoreManager] SimpleCookingManager.OnHotdogServed 구독 완료 ✅
   [StageStoryController] ... (에러 없음) ✅
   [NPCSpawnManager] 스테이지 시작! ... ✅
   ```

3. **에러 확인**:
   - ❌ **NullReferenceException**: 참조가 끊어짐 → 해당 GameObject 참조 재연결
   - ❌ **MissingComponentException**: 컴포넌트 누락 → 컴포넌트 추가

---

### Phase 8: ScriptableObject 생성 (필수!)

현재 없는 경우 생성해야 합니다.

#### 8.1 StoryNPCConfig 생성

1. **Project 창** → `Assets/ScriptableObjects` 폴더 (없으면 생성)
2. **우클릭** > `Create` > `RecipeAboutLife` > `Story NPC Config`
3. **이름**: `StoryNPCConfig_Stage1`
4. **설정**:
   - Story NPC Indices: [4] (5번째 NPC, 0-based)
   - Story Dialogue Sets: [크기 1]
     - Element 0: (NPCDialogueSet 할당 - 생성 필요)
   - Story NPC Sprites: [크기 1]
     - Element 0: (NPC 스프라이트 할당)
   - After Story Only Indices: (비워둠)
   - After Story Dialogue Sets: (비워둠)
   - After Story NPC Sprites: (비워둠)

5. **StageStoryController에 할당**:
   - Hierarchy > `StageStoryController` 선택
   - Inspector > Story NPC Config: `StoryNPCConfig_Stage1` 드래그

#### 8.2 NPCDialogueSet 생성 (StoryAfterSummary 포함)

1. **Project 창** > 우클릭 > `Create` > `RecipeAboutLife` > `NPC Dialogue Set`
2. **이름**: `NPCDialogueSet_005` (5번째 NPC용)
3. **설정**:
   - NPC ID: **5**
   - NPC Display Name: "손님 5"
   - Intro Dialogues: [추가]
   - Order Dialogues: [추가]
   - Served Success Dialogues: [추가]
   - Served Fail Dialogues: [추가]
   - Exit Dialogues: [추가]
   - **Story After Summary Dialogues**: [추가] ⚠️ **필수!**
     ```
     [0]
       Speaker: NPC
       Text: "오늘 하루도 수고 많으셨어요!"
       Display Duration: 3
     [1]
       Speaker: Player
       Text: "감사합니다. 덕분에 즐거웠어요."
       Display Duration: 3
     [2]
       Speaker: NPC
       Text: "내일도 좋은 하루 되세요~"
       Display Duration: 3
     ```

#### 8.3 OrderData 생성 (최소 5개)

각 NPC마다 다른 주문을 할당하려면 OrderData가 필요합니다.

1. **Project 창** > 우클릭 > `Create` > `RecipeAboutLife` > `Order`
2. **이름**: `OrderData_001`, `OrderData_002`, ... (5개 이상)
3. **각각 다르게 설정**:
   - Order Name: "소시지 핫도그", "치즈 핫도그", ...
   - Filling Slot 1/2: HalfSausage / HalfCheese
   - Need Sugar: true/false
   - Sauce Requirements: 추가

---

### Phase 9: NPC Prefab 생성 (최소 5개)

**NPC Prefab 구조**:

```
NPC_001 (Prefab)
├─ SpriteRenderer
├─ Collider2D
├─ NPCMovement.cs
├─ NPCDialogueController.cs
│  - Dialogue Set: NPCDialogueSet_001
├─ NPCOrderController.cs
└─ DialogueBubbleUI (GameObject)
   └─ Canvas
      └─ ... (대화 말풍선 UI)
```

자세한 생성 방법은 `GamePlayScene_Setup_Guide.md`의 "2.1 NPC Prefab 생성" 참고

---

### Phase 10: 전체 플로우 테스트

#### 10.1 기본 테스트 (NPC 1명)

1. **Play 버튼 클릭**
2. **확인**:
   - [ ] 첫 번째 NPC 스폰
   - [ ] NPC 이동 (Spawn Point → Target Position)
   - [ ] Intro 대화 표시
   - [ ] Order 대화 표시
   - [ ] 주문이 ScoreManager에 전달됨 (Console 로그)

3. **요리 6단계 진행**:
   - [ ] Phase 1: Stick Pickup
   - [ ] Phase 2: Ingredient (재료 2개 선택)
   - [ ] Phase 3: Batter
   - [ ] Phase 4: Frying
   - [ ] Phase 5: Topping
   - [ ] Phase 6: Sauce

4. **서빙**:
   - [ ] "서빙하기" 버튼 클릭
   - [ ] OrderValidator 검증 (Console 로그)
   - [ ] ServedSuccess/Fail 대화 표시
   - [ ] Exit 대화 표시
   - [ ] NPC 퇴장

5. **다음 NPC**:
   - [ ] 2번째 NPC 스폰

#### 10.2 5명 완료 테스트

**5명 모두 서빙 완료 후**:

1. **5번째 NPC 퇴장 확인**
2. **페이드 인 (검은 화면)** ✅
3. **결산 페이지 표시** ✅
   - [ ] "획득 재화: XXX원" 표시
   - [ ] "목표 재화: 500원" 표시
   - [ ] 성공/실패 메시지 표시
4. **확인 버튼 클릭**
5. **"잠시만요" 텍스트 표시** (2초) ✅
6. **페이드 아웃 (화면 밝아짐)** ✅
7. **AfterStory 대화 시작** ✅
   - [ ] NPC 이름 및 이미지 표시
   - [ ] 대화 내용 표시
   - [ ] 터치로 진행
8. **AfterStory 대화 종료**
9. **페이드 인 (검은 화면)** ✅
10. **"로비로" 텍스트 표시** ✅
11. **터치 입력 대기** (무한 대기) ✅
12. **화면 클릭**
13. **LobbyScene 로드** ✅

---

## 🔍 현재 상태 진단

### 체크 1: ScoreManager.OnStageCompleted 이벤트 확인

**Play 모드에서 Console 로그 확인**:

```
[ScoreManager] 모든 NPC 완료! 총 재화: XXX원 / 목표: 500원
```

이 로그가 나타나면:
- ✅ **OnAllNPCsServed() 정상 호출**
- ✅ **OnStageCompleted.Invoke(success) 발생**
- ✅ StageStoryController가 구독 중이면 결산 페이지 표시

### 체크 2: StageStoryController.OnStageCompleted 구독 확인

**Play 모드 진입 후 Console 로그**:

```
[StageStoryController] 스테이지 완료! 성공: true, Stage ID: 1
```

이 로그가 나타나지 않으면:
- ❌ **StageStoryController가 Hierarchy에 없음**
- ❌ **OnEnable에서 이벤트 구독 실패**

**해결**: StageStoryController GameObject 추가 (Phase 2.2)

### 체크 3: ResultUIController.Instance 확인

**Play 모드 진입 후 Console 로그**:

```
[StageStoryController] 결산 UI 표시
```

이 로그가 나타나지 않고 대신:

```
[StageStoryController] ResultUIController를 찾을 수 없습니다.
```

나타나면:
- ❌ **ResultPanel이 없거나 ResultUIController 컴포넌트 없음**

**해결**: ResultPanel 확인 (Phase 3.1)

### 체크 4: FadeUI.Instance 확인

```
[StageStoryController] FadeUI를 찾을 수 없습니다.
```

나타나면:
- ❌ **FadePanel이 없거나 FadeUI 컴포넌트 없음**

**해결**: FadePanel 확인 (Phase 3.2)

---

## ⚠️ 자주 발생하는 문제

### 문제 1: NPC가 스폰되지 않음
**원인**: NPCSpawnManager에 NPC Prefab이 할당되지 않음
**해결**:
1. NPCSpawnManager 선택
2. NPC Prefabs 배열에 최소 5개 프리팹 할당

### 문제 2: 결산 페이지가 표시되지 않음
**원인**: StageStoryController가 Hierarchy에 없음
**해결**:
1. Hierarchy 우클릭 > Create Empty
2. 이름: StageStoryController
3. Add Component: StageStoryController.cs

### 문제 3: AfterStory 대화가 시작되지 않음
**원인**: StoryNPCConfig가 설정되지 않음 또는 DialogueSet에 StoryAfterSummary 없음
**해결**:
1. StoryNPCConfig 생성 (Phase 8.1)
2. NPCDialogueSet에 Story After Summary Dialogues 추가 (Phase 8.2)

### 문제 4: "로비로" 텍스트가 표시되지 않음
**원인**: FadeUI의 Guide Text 참조가 끊어짐
**해결**:
1. FadePanel 선택
2. FadeUI 컴포넌트의 Guide Text 필드에 GuideText 드래그

### 문제 5: 로비로 이동하지 않음
**원인**: LobbyScene이 Build Settings에 없음
**해결**:
1. File > Build Settings
2. LobbyScene을 Scenes In Build에 추가

---

## 📋 최종 체크리스트

### 필수 항목 (반드시 있어야 함)

- [ ] **StageStoryController GameObject** (Hierarchy)
- [ ] **StoryNPCConfig ScriptableObject** 생성 및 할당
- [ ] **NPCDialogueSet (StoryAfterSummary 포함)** 생성
- [ ] **ResultPanel** (Canvas 하위)
- [ ] **FadePanel** (Canvas 하위)
- [ ] **NPCDialoguePanel** (Canvas 하위)
- [ ] **PlayerDialoguePanel** (Canvas 하위)
- [ ] **FramePanel** (Canvas 하위)
- [ ] **NPC Prefabs (최소 5개)**
- [ ] **OrderData (최소 5개)**
- [ ] **LobbyScene** (Build Settings에 추가)

### 선택 항목 (있으면 좋음)

- [ ] StoryNPCConfig에 AfterStory 전용 NPC 추가 (Stage 3)
- [ ] 다양한 주문 조합 (10개 이상)
- [ ] NPC 스프라이트 (표정 변화)

---

## ✅ 완료!

**모든 체크리스트를 통과하면 전체 프로세스가 정상 작동합니다!**

1. ✅ NPC 5명 순차 주문 및 서빙
2. ✅ 결산 페이지 표시
3. ✅ AfterStory 대화
4. ✅ 로비로 이동

---

## 🆘 문제가 발생하면

**다음 정보를 알려주세요**:

1. **어느 단계에서 멈췄는지**
2. **Console 창의 에러 메시지** (빨간색 에러)
3. **Console 창의 마지막 로그** (회색/흰색 로그)
4. **Hierarchy 스크린샷** (StageStoryController 부분)
5. **Inspector 스크린샷** (문제가 있는 GameObject)

그러면 정확한 해결 방법을 알려드리겠습니다!
