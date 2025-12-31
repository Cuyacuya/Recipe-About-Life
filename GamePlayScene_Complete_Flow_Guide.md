# GamePlayScene 완전한 프로세스 가이드

## ✅ 현재 통합 상태 확인

병합 전 NPCTalk의 **전체 프로세스**가 현재 통합 버전에 **완전히 포함**되어 있습니다!

- ✅ NPC 5명 순차 스폰 및 주문
- ✅ 각 NPC에게 요리 제공
- ✅ **5명 완료 후 결산 페이지 표시**
- ✅ **AfterStory 대화 (스토리 NPC)**
- ✅ **로비로 이동**

---

## 완전한 게임플레이 플로우

```
1. 게임 시작 (GamePlayScene 로드)
   ↓
2. NPCSpawnManager.StartStage()
   → 랜덤 5명의 NPC 선택
   ↓
3. 첫 번째 NPC 스폰 및 이동
   ↓
4. NPCMovement.OnArrived()
   → Intro 대화 표시
   ↓
5. Intro 대화 종료
   → NPCOrderController.OnNPCStopped()
   → RequestOrder()
   → Order 대화 표시
   ↓
6. NPCOrderController.SendOrderToCookingSystem()
   → ScoreManager.SetActiveOrder(orderData)
   ↓
7. [플레이어 요리 - 6단계]
   Phase 1: Stick Pickup → 꼬치 들기
   Phase 2: Ingredient → 재료 2개 선택
   Phase 3: Batter → 반죽 입히기
   Phase 4: Frying → 튀김 (색상 변화)
   Phase 5: Topping → 설탕 뿌리기
   Phase 6: Sauce → 소스 그리기
   ↓
8. "서빙하기" 버튼 클릭
   → SimpleCookingManager.ServeHotdog()
   → SimpleCookingManager.OnHotdogServed 이벤트
   ↓
9. ScoreManager.OnHotdogServed()
   → OrderValidator.Validate(hotdog, order)
   → HotdogRecipe 생성 (quality, matchesOrder)
   → GameEvents.TriggerRecipeCompleted(recipe)
   ↓
10. ScoreManager.OnRecipeCompleted(recipe)
    → 보상 계산 및 저장
    → NPCOrderController.OnFoodServed(isSuccess) 호출
    ↓
11. NPCOrderController.OnFoodServed()
    → ServedSuccess/ServedFail 대화 표시
    ↓
12. 서빙 대화 종료
    → NPCMovement.OnOrderComplete()
    → Exit 대화 시작
    ↓
13. Exit 대화 종료
    → NPC 퇴장 이동
    ↓
14. NPCMovement.OnExitComplete()
    → NPCSpawnManager.OnNPCOrderComplete()
    → 현재 NPC 제거
    → 다음 NPC 스폰 (2-14 반복, 5번까지)
    ↓
15. ===== 5번째 NPC 완료 후 =====
    ScoreManager.OnRecipeCompleted()
    → currentNPCIndex >= maxNPCCount (5)
    → OnAllNPCsServed() 호출
    ↓
16. ScoreManager.OnAllNPCsServed()
    → 총 재화 계산 (totalReward)
    → 목표 달성 여부 확인 (success = totalReward >= targetTotalReward)
    → OnStageCompleted.Invoke(success) 🔥
    ↓
17. StageStoryController.OnStageCompleted(success)
    → FadeUI.FadeIn() (검은 화면)
    ↓
18. 페이드 인 완료 후
    → ResultUIController.Show(success) 🎯
    ↓
19. ===== 결산 페이지 =====
    - 획득 재화 표시 (totalReward)
    - 목표 재화 표시 (targetReward)
    - 성공/실패 메시지 표시
    - 확인 버튼 대기
    ↓
20. 확인 버튼 클릭
    → StageStoryController.OnResultUIConfirmed(success)
    → TransitionToStoryDialogue(success) 코루틴 시작
    ↓
21. ===== 스토리 대화 전환 =====
    (검은 화면 상태 유지)
    → ResultUIController.Hide()
    → FadeUI.ShowText("잠시만요") (2초)
    → FadeUI.HideText()
    → FramePanelUI.Show()
    → NPCDialogueUI.Show() (빈 텍스트)
    → PlayerDialogueUI.Show() (빈 텍스트)
    → FadeUI.FadeOut() (화면 밝아짐)
    ↓
22. ===== AfterStory 대화 시작 =====
    StageStoryController.StartStoryDialogue()
    → StoryNPCConfig에서 스토리 NPC 데이터 로드
    → DialogueType.StoryAfterSummary 대화 재생
    → NPC/Player 대화 교차 표시
    → 터치 입력으로 진행
    ↓
23. AfterStory 대화 종료
    → StageStoryController.OnStoryDialogueCompleted()
    → AfterStory 전용 NPC 있는지 확인 (Stage 3의 경우 추가 대화)
    ↓
24. ===== 로비 이동 시퀀스 =====
    TransitionToLobbyCoroutine()
    → FadeUI.FadeIn() (검은 화면)
    → UI 패널들 숨김 (NPCDialogueUI, PlayerDialogueUI, FramePanelUI)
    → FadeUI.ShowText("로비로")
    → 터치 입력 무한 대기 ⏳
    ↓
25. 터치 입력 감지
    → FadeUI.HideText()
    → SceneManager.LoadScene("LobbyScene") 🏠
```

---

## 필수 컴포넌트 체크리스트

### 1. Managers (Hierarchy)

```
✅ GameManager
   - 컴포넌트: GameManager.cs
   - DontDestroyOnLoad

✅ ScoreManager
   - 컴포넌트: ScoreManager.cs
   - Max NPC Count: 5
   - Target Total Reward: 500 (또는 원하는 목표 재화)
   - 이벤트: OnStageCompleted

✅ SimpleCookingManager
   - 컴포넌트: SimpleCookingManager.cs
   - 모든 프리팹 할당
   - 이벤트: OnHotdogServed

✅ NPCSpawnManager
   - 컴포넌트: NPCSpawnManager.cs
   - NPC Prefabs: 10개 (랜덤 5명 선택)
   - NPCs Per Stage: 5

✅ StageStoryController 🔥
   - 컴포넌트: StageStoryController.cs
   - Story NPC Config: StoryNPCConfig ScriptableObject 할당
   - Current Stage ID: 1 (또는 해당 스테이지 번호)
   - Fade Duration: 1
   - Wait Text Duration: 2
   - Line Display Time: 3
   - Line Pause Duration: 0.5
```

### 2. UI Components (Canvas 하위)

```
✅ ResultPanel (결산 페이지)
   - GameObject: ResultPanel
   - 컴포넌트: ResultUIController.cs
   - 자식 요소:
     * TotalRewardText (TextMeshProUGUI)
     * TargetRewardText (TextMeshProUGUI)
     * ResultMessageText (TextMeshProUGUI)
     * ConfirmButton (Button)
   - 초기 상태: 비활성화 (SetActive(false))

✅ FadePanel (페이드 효과)
   - GameObject: FadePanel
   - 컴포넌트: FadeUI.cs
   - Image: 검은색 (Alpha 0)
   - GuideText: TextMeshProUGUI (화면 중앙)

✅ FramePanel (대화 프레임)
   - GameObject: FramePanel
   - 컴포넌트: FramePanelUI.cs
   - Image: 대화 배경 프레임
   - 초기 상태: 비활성화

✅ NPCDialoguePanel
   - GameObject: NPCDialoguePanel
   - 컴포넌트: NPCDialogueUI.cs
   - 자식 요소:
     * NPCNameText (TextMeshProUGUI)
     * NPCDialogueText (TextMeshProUGUI)
     * NPCImage (Image)
   - 초기 상태: 비활성화

✅ PlayerDialoguePanel
   - GameObject: PlayerDialoguePanel
   - 컴포넌트: PlayerDialogueUI.cs
   - 자식 요소:
     * PlayerDialogueText (TextMeshProUGUI)
   - 초기 상태: 비활성화
```

### 3. ScriptableObjects

```
✅ StoryNPCConfig
   - Create > RecipeAboutLife > Story NPC Config
   - Stage별 스토리 NPC 설정:
     * Stage 1: NPC #5 (5번째 손님) + DialogueSet + Sprite
     * Stage 2: NPC #5 + DialogueSet + Sprite
     * Stage 3: NPC #5 + 추가 AfterStory NPC (Ajeossi) + DialogueSets + Sprites

✅ NPCDialogueSet (각 NPC마다)
   - DialogueType.StoryAfterSummary 대화 추가:
     * SpeakerType: NPC / Player 교차
     * Text: 스토리 대화 내용
     * Display Duration: 3초 (또는 원하는 시간)

✅ OrderData (주문 데이터)
   - 최소 5개 이상 생성
   - 재료, 토핑, 소스 조합 다양하게

✅ NPC Prefabs
   - 최소 5개 이상
   - 컴포넌트: NPCMovement, NPCDialogueController, NPCOrderController
   - DialogueBubbleUI 자식 오브젝트
```

---

## 이벤트 흐름도

```
SimpleCookingManager.OnHotdogServed
           ↓
ScoreManager.OnHotdogServed()
           ↓
GameEvents.OnRecipeCompleted
           ↓
ScoreManager.OnRecipeCompleted()
           ↓
(5명 완료 시) ScoreManager.OnAllNPCsServed()
           ↓
ScoreManager.OnStageCompleted.Invoke(success) 🔥
           ↓
StageStoryController.OnStageCompleted(success)
           ↓
ResultUIController.Show(success) 🎯
           ↓
(확인 버튼 클릭) ResultUIController.OnConfirmClicked.Invoke(success)
           ↓
StageStoryController.OnResultUIConfirmed(success)
           ↓
TransitionToStoryDialogue(success)
           ↓
StoryAfterSummary 대화 재생
           ↓
TransitionToLobbyCoroutine()
           ↓
SceneManager.LoadScene("LobbyScene") 🏠
```

---

## 중요한 설정 포인트

### 1. StageStoryController 설정

StageStoryController는 **반드시 Hierarchy에 GameObject로 추가**해야 합니다!

```
Hierarchy:
├─ ...
├─ StageStoryController (GameObject)
│  └─ StageStoryController.cs 컴포넌트
│     - Story NPC Config: StoryNPCConfig_Stage1 (할당 필수!)
│     - Current Stage ID: 1
│     - Fade Duration: 1
│     - Wait Text Duration: 2
└─ ...
```

### 2. StoryNPCConfig 생성

```
1. Project 창 우클릭
2. Create > RecipeAboutLife > Story NPC Config
3. 이름: StoryNPCConfig_Stage1
4. 설정:
   Stage 1:
   - Story NPC Index: 4 (5번째 NPC, 0-based index)
   - Story Dialogue Set: NPCDialogueSet_005 (StoryAfterSummary 대화 포함)
   - Story NPC Sprite: NPC_005_Sprite
   - Has After Story Only NPC: false (Stage 3만 true)
```

### 3. NPCDialogueSet에 StoryAfterSummary 추가

```
NPCDialogueSet_005 (5번째 NPC):
- Intro Dialogues: [...]
- Order Dialogues: [...]
- Served Success Dialogues: [...]
- Served Fail Dialogues: [...]
- Exit Dialogues: [...]
- Story After Summary Dialogues: 🔥
  [0]
    - Speaker: NPC
    - Text: "오늘 하루도 수고 많으셨어요!"
    - Display Duration: 3
  [1]
    - Speaker: Player
    - Text: "감사합니다. 덕분에 즐거웠어요."
    - Display Duration: 3
  [2]
    - Speaker: NPC
    - Text: "내일도 좋은 하루 되세요~"
    - Display Duration: 3
```

### 4. ResultPanel UI 설정

```
Canvas/ResultPanel:
├─ Background (Image)
├─ TitleText: "결산" (TextMeshProUGUI)
├─ TotalRewardText: "획득 재화: 450원" (TextMeshProUGUI)
├─ TargetRewardText: "목표 재화: 500원" (TextMeshProUGUI)
├─ ResultMessageText: "✅ 목표 달성!" (TextMeshProUGUI)
└─ ConfirmButton (Button)
   └─ Text: "확인"

ResultUIController 컴포넌트 설정:
- Result Panel: ResultPanel (GameObject)
- Total Reward Text: TotalRewardText
- Target Reward Text: TargetRewardText
- Result Message Text: ResultMessageText
- Confirm Button: ConfirmButton
```

### 5. FadePanel UI 설정

```
Canvas/FadePanel:
├─ FadeImage (Image)
│  - Color: Black (0, 0, 0, 0) - 초기 Alpha 0
│  - Raycast Target: true
├─ GuideText (TextMeshProUGUI)
   - Text: "" (빈 문자열)
   - Alignment: Center
   - Font Size: 48
   - Color: White

FadeUI 컴포넌트 설정:
- Fade Image: FadeImage
- Guide Text: GuideText
- Fade Duration: 1
```

---

## 테스트 체크리스트

### Phase 1: NPC 주문 (1-5명)
- [ ] 첫 번째 NPC 스폰 및 이동
- [ ] Intro 대화 표시
- [ ] Order 대화 및 주문 전달
- [ ] 요리 6단계 정상 작동
- [ ] 서빙 및 검증
- [ ] ServedSuccess/Fail 대화 표시
- [ ] Exit 대화 및 퇴장
- [ ] 2-5번째 NPC도 동일 프로세스 (5명 반복)

### Phase 2: 결산 페이지
- [ ] 5번째 NPC 퇴장 완료 후
- [ ] 페이드 인 (검은 화면)
- [ ] **결산 페이지 표시** 🎯
  - [ ] 획득 재화 표시 (totalReward)
  - [ ] 목표 재화 표시 (targetReward)
  - [ ] 성공/실패 메시지 정확히 표시
- [ ] 확인 버튼 클릭 가능

### Phase 3: AfterStory 대화
- [ ] 확인 버튼 클릭 후
- [ ] 결산 UI 숨김
- [ ] "잠시만요" 텍스트 표시 (2초)
- [ ] 텍스트 숨김
- [ ] FramePanel, NPCDialogueUI, PlayerDialogueUI 표시
- [ ] 페이드 아웃 (화면 밝아짐)
- [ ] **StoryAfterSummary 대화 시작** 🔥
  - [ ] NPC 대화 표시 (이름, 이미지, 텍스트)
  - [ ] Player 대화 표시
  - [ ] 터치로 다음 대화 진행
  - [ ] 모든 대화 라인 재생 완료

### Phase 4: 로비 이동
- [ ] AfterStory 대화 종료 후
- [ ] 페이드 인 (검은 화면)
- [ ] 모든 대화 UI 패널 숨김
- [ ] "로비로" 텍스트 표시
- [ ] **터치 입력 무한 대기** ⏳
- [ ] 터치 후 텍스트 숨김
- [ ] **LobbyScene 로드** 🏠

---

## 자주 발생하는 문제 해결

### 문제 1: 결산 페이지가 표시되지 않음
**원인**: ResultUIController가 없거나 ScoreManager.OnStageCompleted 이벤트를 StageStoryController가 구독하지 않음
**해결**:
1. Hierarchy에 ResultPanel GameObject 있는지 확인
2. ResultUIController 컴포넌트 있는지 확인
3. StageStoryController가 OnEnable에서 ScoreManager.OnStageCompleted 구독하는지 확인

### 문제 2: AfterStory 대화가 시작되지 않음
**원인**: StoryNPCConfig가 설정되지 않았거나 DialogueSet에 StoryAfterSummary 대화가 없음
**해결**:
1. StageStoryController에 Story NPC Config 할당되어 있는지 확인
2. StoryNPCConfig에 해당 Stage의 스토리 NPC 설정되어 있는지 확인
3. NPCDialogueSet에 DialogueType.StoryAfterSummary 대화 추가되어 있는지 확인

### 문제 3: "로비로" 텍스트가 표시되지 않음
**원인**: FadeUI가 없거나 GuideText가 설정되지 않음
**해결**:
1. Hierarchy에 FadePanel GameObject 있는지 확인
2. FadeUI 컴포넌트 있는지 확인
3. FadeUI의 Guide Text 필드에 TextMeshProUGUI 할당되어 있는지 확인

### 문제 4: 로비로 이동하지 않음
**원인**: LobbyScene이 Build Settings에 없거나 씬 이름이 다름
**해결**:
1. File > Build Settings 열기
2. LobbyScene이 Scenes In Build 목록에 있는지 확인
3. 씬 이름이 "LobbyScene"과 정확히 일치하는지 확인
4. 또는 StageStoryController.LoadLobbyScene() 메서드에서 씬 이름 수정

### 문제 5: 5번째 NPC 후에도 다음 NPC가 스폰됨
**원인**: ScoreManager의 maxNPCCount가 5가 아님
**해결**:
1. ScoreManager Inspector에서 Max NPC Count = 5 확인
2. NPCSpawnManager의 NPCs Per Stage = 5 확인

---

## 전체 프로세스 요약

```
[게임 시작]
    ↓
[NPC 1-5명 순차 스폰 및 주문 → 요리 → 서빙]
    ↓
[5번째 NPC 퇴장 완료]
    ↓
[페이드 인 → 결산 페이지 표시] 🎯
    ↓
[확인 버튼 클릭]
    ↓
["잠시만요" 텍스트 → 페이드 아웃]
    ↓
[AfterStory 대화 시작] 🔥
    ↓
[AfterStory 대화 종료]
    ↓
[페이드 인 → "로비로" 텍스트 → 터치 대기] ⏳
    ↓
[터치 입력 → LobbyScene 로드] 🏠
```

---

## 완료!

이제 GamePlayScene에서 **완전한 게임플레이 플로우**가 작동합니다!

1. **NPC 5명 주문 및 서빙** ✅
2. **결산 페이지 표시** ✅
3. **AfterStory 대화** ✅
4. **로비로 이동** ✅

모든 컴포넌트가 이미 통합되어 있으므로, 위의 설정 가이드를 따라 GamePlayScene을 구축하고 테스트하세요!
