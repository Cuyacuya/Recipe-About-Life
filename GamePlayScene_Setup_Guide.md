# GamePlayScene 설정 가이드

이 가이드는 NPC 대화 시스템과 food_system_v2 요리 시스템이 통합된 GamePlayScene을 설정하는 방법을 설명합니다.

## 목차
1. [필수 매니저 오브젝트](#1-필수-매니저-오브젝트)
2. [NPC 시스템 설정](#2-npc-시스템-설정)
3. [요리 시스템 설정](#3-요리-시스템-설정)
4. [UI 설정](#4-ui-설정)
5. [카메라 설정](#5-카메라-설정)
6. [테스트 체크리스트](#6-테스트-체크리스트)

---

## 1. 필수 매니저 오브젝트

Hierarchy에 다음 GameObject들을 생성하고 각각에 필요한 컴포넌트를 추가하세요:

### 1.1 GameManager
```
GameObject 이름: GameManager
컴포넌트: GameManager.cs
설정:
  - Singleton으로 작동 (자동)
```

### 1.2 ScoreManager
```
GameObject 이름: ScoreManager
컴포넌트: ScoreManager.cs (RecipeAboutLife.Managers)
설정:
  - Stage Index: 0 (첫 스테이지)
  - Total NPCs Per Stage: 5 (스테이지당 NPC 수)
  - Base Reward Per Recipe: 100
```

**중요**: ScoreManager는 이제 주문 관리 기능도 담당합니다!
- NPCOrderController가 주문을 ScoreManager.SetActiveOrder()로 전달
- SimpleCookingManager.OnHotdogServed 이벤트를 구독
- HotdogData를 HotdogRecipe로 변환 및 검증
- GameEvents.OnRecipeCompleted 이벤트 발생

### 1.3 SimpleCookingManager
```
GameObject 이름: SimpleCookingManager
컴포넌트: SimpleCookingManager.cs (RecipeAboutLife.Cooking)
설정:
  ===== Prefabs =====
  - Stick Prefab: 꼬치 프리팹 (Prefabs/Cooking/Stick)
  - Sausage Prefab: 소시지 프리팹 (Prefabs/Cooking/Sausage)
  - Cheese Prefab: 치즈 프리팹 (Prefabs/Cooking/Cheese)
  - Batter Container Prefab: 반죽 컨테이너 (Prefabs/Cooking/BatterContainer)
  - Oil Container Prefab: 기름통 (Prefabs/Cooking/OilContainer)
  - Sugar Container Prefab: 설탕통 (Prefabs/Cooking/SugarContainer)

  ===== UI References =====
  - Ingredient Popup: 재료 선택 팝업 UI (Canvas 하위)
  - Batter UI: 반죽 UI (Canvas 하위)
  - Frying Timer UI: 튀김 타이머 UI (Canvas 하위)
  - Serve Button: 서빙 버튼 (Canvas 하위)
```

### 1.4 NPCSpawnManager
```
GameObject 이름: NPCSpawnManager
컴포넌트: NPCSpawnManager.cs (RecipeAboutLife.Managers)
설정:
  ===== NPC Prefabs =====
  - Npc Prefabs: NPC 프리팹 배열 (5-10개 정도)
    예: Prefabs/NPCs/NPC_001, NPC_002, NPC_003...

  ===== Spawn/Exit Points =====
  - Spawn Point: NPC 스폰 위치 (예: (-10, 0, 0))
  - Target Position: NPC 이동 목표 위치 (예: (0, 0, 0))
  - Exit Point: NPC 퇴장 위치 (예: (10, 0, 0))

  ===== Settings =====
  - Max NPCs Per Stage: 5
```

**NPC Prefab 구조**:
각 NPC Prefab은 다음 컴포넌트들을 가져야 합니다:
```
NPC_001 (프리팹)
├─ SpriteRenderer (NPC 외형)
├─ Collider2D (상호작용용)
├─ NPCMovement.cs
├─ NPCDialogueController.cs
├─ NPCOrderController.cs
├─ NPCDialogueSet (ScriptableObject 참조)
└─ DialogueBubbleUI (자식 GameObject)
   └─ Canvas
      └─ DialogueBubble
         ├─ Background (Image)
         └─ DialogueText (TextMeshPro)
```

---

## 2. NPC 시스템 설정

### 2.1 NPC Prefab 생성 예시

1. **새 GameObject 생성**: `NPC_001`
2. **컴포넌트 추가**:
   - `SpriteRenderer`: NPC 스프라이트 설정
   - `BoxCollider2D` 또는 `CircleCollider2D`: 클릭 감지용
   - `NPCMovement.cs`
   - `NPCDialogueController.cs`
   - `NPCOrderController.cs`

3. **NPCMovement 설정**:
   ```
   - Move Speed: 2.0
   - Arrival Distance: 0.1
   ```

4. **NPCDialogueController 설정**:
   ```
   - Dialogue Set: NPCDialogueSet_001 (ScriptableObject)
   - Type Speed: 0.05
   - Wait Before Auto Continue: 2.0
   ```

5. **NPCOrderController 설정**:
   - Dialogue Set, Dialogue Controller는 자동 검색됨
   - Dialogue Bubble UI는 자식 오브젝트에서 자동 찾기

### 2.2 DialogueBubbleUI 자식 오브젝트 생성

NPC_001 하위에 다음 구조 생성:

```
NPC_001
└─ DialogueBubbleUI (GameObject)
   ├─ Canvas
   │  ├─ Render Mode: World Space
   │  ├─ Sorting Layer: UI
   │  └─ Order in Layer: 100
   └─ DialogueBubble (Panel)
      ├─ Background (Image - 말풍선 이미지)
      └─ DialogueText (TextMeshProUGUI)
         - Font Size: 24
         - Alignment: Center
         - Color: Black
```

DialogueBubbleUI GameObject에 `DialogueBubbleUI.cs` 컴포넌트 추가:
```
- Dialogue Bubble: DialogueBubble Panel
- Dialogue Text: DialogueText (TextMeshProUGUI)
- Fade Duration: 0.3
```

### 2.3 NPCDialogueSet ScriptableObject 생성

1. Project 창에서 우클릭
2. `Create > RecipeAboutLife > NPC Dialogue Set`
3. 이름: `NPCDialogueSet_001`
4. 설정:
   ```
   ===== NPC Info =====
   - NPC ID: 1
   - NPC Name: "손님 A"

   ===== Order Data =====
   - NPC Order: OrderData_SausageHotdog (ScriptableObject)

   ===== Dialogues =====
   - Intro Dialogues:
     "안녕하세요!"
     "핫도그 하나 주문하고 싶어요."

   - Order Dialogues:
     "소시지 핫도그 주세요!"
     "설탕 뿌려주시고, 케찹도 부탁드려요."

   - Served Success Dialogues:
     "와, 맛있겠어요!"
     "감사합니다!"

   - Served Fail Dialogues:
     "음... 제가 주문한 게 이게 아닌 것 같은데요?"
     "다시 만들어 주세요."

   - Exit Dialogues:
     "잘 먹었습니다!"
     "다음에 또 올게요!"
   ```

### 2.4 OrderData ScriptableObject 생성

1. Project 창에서 우클릭
2. `Create > RecipeAboutLife > Order`
3. 이름: `OrderData_SausageHotdog`
4. 설정:
   ```
   ===== 주문 정보 =====
   - Order Name: "소시지 핫도그"

   ===== 속재료 (2칸 고정) =====
   - Filling Slot 1: HalfSausage
   - Filling Slot 2: HalfSausage

   ===== 토핑 =====
   - Need Sugar: true (체크)

   ===== 소스 요구사항 =====
   - Sauce Requirements (리스트):
     [0] Sauce Type: Ketchup, Min Amount: Medium
   ```

---

## 3. 요리 시스템 설정

### 3.1 요리 영역 설정

Hierarchy에 다음 오브젝트들 생성:

```
===== Phase 1: Stick Pickup =====
StickContainer (GameObject)
├─ SpriteRenderer (꼬치통 이미지)
├─ BoxCollider2D
└─ StickPickupHandler.cs
   - Cutting Board Zone: CuttingBoard의 드롭존 참조

===== Phase 2: Ingredient Selection =====
CuttingBoard (GameObject)
├─ SpriteRenderer (도마 이미지)
└─ SimpleDropZone.cs
   - Zone Type: CuttingBoard

Canvas > IngredientPopup (GameObject)
├─ IngredientPopupHandler.cs
│  - Stick Display: 팝업 내 꼬치 표시 Transform
│  - Ingredient1 Pos: 첫 번째 재료 위치 Transform
│  - Ingredient2 Pos: 두 번째 재료 위치 Transform
│  - Sausage Source: IngredientSource (소시지)
│  - Cheese Source: IngredientSource (치즈)
│  - Stick Drop Zone: 드롭존 참조
│  - Sausage Prefab: 소시지 프리팹
│  - Cheese Prefab: 치즈 프리팹
└─ (UI 요소들...)

===== Phase 3: Batter =====
BatterContainer (GameObject)
├─ SpriteRenderer (반죽통 이미지)
├─ BoxCollider2D
└─ BatterHandler.cs
   - Batter Drop Zone: 드롭존 참조
   - Batter Stage Duration: 2.0

BatterDropZone (GameObject)
├─ SpriteRenderer (반죽 영역 표시)
└─ SimpleDropZone.cs
   - Zone Type: Batter

===== Phase 4: Frying =====
OilContainer (GameObject)
├─ SpriteRenderer (기름통 이미지)
├─ BoxCollider2D
└─ FryingHandler.cs
   - Frying Drop Zone: 드롭존 참조
   - Frying Times:
     * Time To Yellow: 3.0
     * Time To Golden: 5.0
     * Time To Brown: 8.0
     * Time To Burnt: 12.0

FryingDropZone (GameObject)
├─ SpriteRenderer (기름통 영역)
└─ SimpleDropZone.cs
   - Zone Type: Frying

===== Phase 5: Toppings =====
SugarContainer (GameObject)
├─ SpriteRenderer (설탕통 이미지)
├─ BoxCollider2D
└─ ToppingHandler.cs
   - Topping Type: Sugar
   - Drop Zone: 드롭존 참조

===== Phase 6: Sauce =====
KetchupBottle (GameObject)
├─ SpriteRenderer (케첩 병)
├─ BoxCollider2D
└─ SauceButton.cs
   - Sauce Type: Ketchup

MustardBottle (GameObject)
├─ SpriteRenderer (머스타드 병)
├─ BoxCollider2D
└─ SauceButton.cs
   - Sauce Type: Mustard

SauceDrawArea (GameObject)
├─ BoxCollider2D (소스 그릴 영역)
└─ SauceDrawer.cs
   - Ketchup Dot Sprite: 케첩 점 스프라이트
   - Mustard Dot Sprite: 머스타드 점 스프라이트
   - Dot Spacing: 0.1
   - Dot Scale: 0.3
   - Draw Area: 자신의 Collider2D
```

### 3.2 UI 요소 설정

Canvas 하위에 다음 UI 생성:

```
Canvas (Canvas)
├─ IngredientPopup (Panel - 처음엔 비활성화)
│  └─ ... (재료 선택 UI)
├─ BatterUI (Panel)
│  └─ BatterProgressBar (Slider)
├─ FryingTimerUI (Panel)
│  └─ TimerText (TextMeshProUGUI)
├─ ServeButton (Button)
│  └─ Text: "서빙하기"
└─ PhaseIndicatorText (TextMeshProUGUI)
   - Text: "Phase: Stick Pickup"
```

---

## 4. UI 설정

### 4.1 DialogueBubbleUI 설정 (각 NPC Prefab마다)

위의 [2.2 DialogueBubbleUI 자식 오브젝트 생성](#22-dialoguebubbleui-자식-오브젝트-생성) 참조

### 4.2 요리 UI 설정

SimpleCookingManager에서 UI 참조 연결:
```
- Ingredient Popup: Canvas/IngredientPopup
- Batter UI: Canvas/BatterUI
- Frying Timer UI: Canvas/FryingTimerUI
- Serve Button: Canvas/ServeButton
```

---

## 5. 카메라 설정

```
Main Camera (GameObject)
├─ Camera
│  ├─ Projection: Orthographic
│  ├─ Size: 5
│  └─ Position: (0, 0, -10)
└─ CameraZoomController.cs (선택사항)
   - Min Size: 3
   - Max Size: 10
   - Zoom Speed: 2
```

---

## 6. 테스트 체크리스트

설정이 완료되면 다음을 테스트하세요:

### 6.1 NPC 스폰 및 이동
- [ ] Play 시작 시 첫 번째 NPC가 스폰됨
- [ ] NPC가 Target Position으로 이동
- [ ] 도착 시 Intro 대화 표시됨

### 6.2 주문 시스템
- [ ] Intro 대화 종료 후 Order 대화 시작
- [ ] Order 대화에 주문 내용이 표시됨
- [ ] 주문이 ScoreManager에 전달됨 (콘솔 로그 확인)

### 6.3 요리 시스템 (6단계)
- [ ] **Phase 1 (Stick Pickup)**: 꼬치통 클릭 시 꼬치 생성, 도마에 드롭 가능
- [ ] **Phase 2 (Ingredient)**: 재료 팝업 표시, 소시지/치즈 2개 선택 가능
- [ ] **Phase 3 (Batter)**: 반죽통 드래그로 반죽 입히기, 진행바 표시
- [ ] **Phase 4 (Frying)**: 기름통에 드롭 시 튀김 시작, 색상 변화 확인
- [ ] **Phase 5 (Topping)**: 설탕통 클릭 시 설탕 추가
- [ ] **Phase 6 (Sauce)**: 케첩/머스타드 클릭 후 드래그로 소스 그리기

### 6.4 서빙 및 검증
- [ ] "서빙하기" 버튼 클릭 시 HotdogData 생성
- [ ] OrderValidator가 주문과 비교 검증 (콘솔 로그 확인)
- [ ] ScoreManager가 HotdogRecipe 생성
- [ ] GameEvents.OnRecipeCompleted 이벤트 발생
- [ ] NPCOrderController가 OnFoodServed 호출됨

### 6.5 NPC 피드백 및 퇴장
- [ ] 주문 일치 시: ServedSuccess 대화 표시
- [ ] 주문 불일치 시: ServedFail 대화 표시
- [ ] 서빙 대화 종료 후 Exit 대화 시작
- [ ] Exit 대화 종료 후 NPC가 Exit Point로 이동
- [ ] NPC 퇴장 후 다음 NPC 스폰 (최대 5명)

### 6.6 점수 및 보상
- [ ] ScoreManager에서 품질 점수 계산 (콘솔 로그)
- [ ] 보상 코인 지급 (UI 업데이트 확인)
- [ ] 5명 완료 후 스테이지 종료 이벤트 발생

---

## 7. 자주 발생하는 문제 해결

### 문제 1: NPC가 스폰되지 않음
**해결**:
- NPCSpawnManager에 NPC Prefab이 할당되어 있는지 확인
- Spawn Point가 카메라 뷰 밖에 있는지 확인
- ScoreManager의 Total NPCs Per Stage가 1 이상인지 확인

### 문제 2: 주문 대화가 표시되지 않음
**해결**:
- NPCDialogueSet에 Order Dialogues가 입력되어 있는지 확인
- NPCDialogueSet에 NPC Order (OrderData)가 할당되어 있는지 확인
- NPCOrderController가 NPCDialogueController를 찾았는지 콘솔 확인

### 문제 3: 요리 단계가 진행되지 않음
**해결**:
- SimpleCookingManager에 모든 프리팹이 할당되어 있는지 확인
- 각 Handler(StickPickupHandler, BatterHandler 등)가 드롭존 참조를 가지고 있는지 확인
- SimpleDropZone의 Zone Type이 올바르게 설정되어 있는지 확인

### 문제 4: 서빙 후 NPC 반응이 없음
**해결**:
- SimpleCookingManager.OnHotdogServed 이벤트가 발생하는지 콘솔 로그 확인
- ScoreManager가 SimpleCookingManager.OnHotdogServed를 구독했는지 확인 (Start 메서드)
- NPCOrderController.IsWaitingForFood()가 true인지 확인

### 문제 5: FillingType, Camera 등 컴파일 에러
**해결**:
- 이미 수정 완료됨:
  - EventSystem.cs: 삭제된 클래스 참조 주석 처리
  - ScoreManager.cs: FillingType 네임스페이스 명시 (Cooking.FillingType, Orders.FillingType)
  - Cooking 스크립트들: Camera → UnityEngine.Camera로 수정

---

## 8. 완전한 게임플레이 플로우 요약

```
1. 게임 시작
   ↓
2. NPCSpawnManager.SpawnNextNPC()
   ↓
3. NPC 이동 → Target Position 도착
   ↓
4. NPCMovement.OnArrived() → Intro 대화
   ↓
5. Intro 대화 종료 → NPCOrderController.OnNPCStopped()
   ↓
6. NPCOrderController.RequestOrder() → Order 대화 + 주문 데이터 가져오기
   ↓
7. NPCOrderController.SendOrderToCookingSystem()
   → ScoreManager.SetActiveOrder(orderData)
   ↓
8. [플레이어 요리 - 6단계 진행]
   Phase 1: Stick Pickup → 꼬치 들기
   Phase 2: Ingredient → 재료 2개 선택
   Phase 3: Batter → 반죽 입히기
   Phase 4: Frying → 튀김 (색상 변화)
   Phase 5: Topping → 설탕 뿌리기
   Phase 6: Sauce → 소스 그리기
   ↓
9. "서빙하기" 버튼 클릭
   → SimpleCookingManager.ServeHotdog()
   → SimpleCookingManager.OnHotdogServed 이벤트 발생
   ↓
10. ScoreManager.OnHotdogServed()
    → OrderValidator.Validate(hotdog, order) 검증
    → HotdogRecipe 생성 (quality, matchesOrder)
    → GameEvents.TriggerRecipeCompleted(recipe)
    ↓
11. ScoreManager.OnRecipeCompleted(recipe)
    → 보상 계산 및 지급
    → NPCOrderController.OnFoodServed(isSuccess) 호출
    ↓
12. NPCOrderController.OnFoodServed()
    → ServedSuccess/ServedFail 대화 표시
    ↓
13. 서빙 대화 종료 → NPCMovement.OnOrderComplete()
    → Exit 대화 시작
    ↓
14. Exit 대화 종료 → NPC 퇴장 이동
    ↓
15. NPCMovement.OnExitComplete()
    → NPCSpawnManager.OnNPCOrderComplete()
    → 다음 NPC 스폰 (2번으로 돌아감)
    ↓
16. 5명 완료 시 스테이지 종료 이벤트 발생
```

---

## 9. 필수 파일 체크리스트

다음 파일들이 프로젝트에 존재하는지 확인하세요:

### Scripts (새로 생성된 파일)
- [x] `Assets/Scripts/Cooking/HotdogRecipe.cs`
- [x] `Assets/Scripts/Utilities/OrderValidator.cs`

### Scripts (수정된 파일)
- [x] `Assets/Scripts/Events/EventSystem.cs`
- [x] `Assets/Scripts/Managers/ScoreManager.cs`
- [x] `Assets/Scripts/NPC/NPCOrderController.cs`
- [x] `Assets/Scripts/Cooking/SimpleDraggable.cs`
- [x] `Assets/Scripts/Cooking/SauceDrawer.cs`
- [x] `Assets/Scripts/Cooking/IngredientPopupHandler.cs`
- [x] `Assets/Scripts/Cooking/StickPickupHandler.cs`

### ScriptableObjects (생성 필요)
- [ ] `Assets/ScriptableObjects/Orders/OrderData_SausageHotdog.asset`
- [ ] `Assets/ScriptableObjects/Orders/OrderData_CheeseHotdog.asset`
- [ ] `Assets/ScriptableObjects/Orders/OrderData_MixedHotdog.asset`
- [ ] `Assets/ScriptableObjects/NPCDialogueSets/NPCDialogueSet_001.asset`
- [ ] `Assets/ScriptableObjects/NPCDialogueSets/NPCDialogueSet_002.asset`
- [ ] (더 많은 주문/대화 세트 생성...)

### Prefabs (생성 필요)
- [ ] `Assets/Prefabs/NPCs/NPC_001.prefab`
- [ ] `Assets/Prefabs/NPCs/NPC_002.prefab`
- [ ] `Assets/Prefabs/Cooking/Stick.prefab`
- [ ] `Assets/Prefabs/Cooking/Sausage.prefab`
- [ ] `Assets/Prefabs/Cooking/Cheese.prefab`
- [ ] `Assets/Prefabs/Cooking/BatterContainer.prefab`
- [ ] `Assets/Prefabs/Cooking/OilContainer.prefab`
- [ ] `Assets/Prefabs/Cooking/SugarContainer.prefab`

---

## 완료!

이제 GamePlayScene이 완전히 설정되었습니다. Play 버튼을 눌러 전체 플로우를 테스트하세요!

**콘솔 로그 확인 팁**:
- `[NPCSpawnManager]`: NPC 스폰/퇴장 로그
- `[NPCMovement]`: NPC 이동 상태
- `[NPCOrderController]`: 주문 전달 로그
- `[NPCDialogueController]`: 대화 진행 로그
- `[SimpleCookingManager]`: 요리 단계 진행 로그
- `[OrderValidator]`: 주문 검증 결과
- `[ScoreManager]`: 점수/보상 계산 로그
