# UI 패널 수동 설정 가이드

복사가 안되는 경우, 이 가이드를 따라 처음부터 만드세요.

---

## 1. FadePanel (페이드 효과)

### 1.1 GameObject 생성

```
Hierarchy:
Canvas (Canvas)
└─ FadePanel (GameObject)
   ├─ FadeImage (Image)
   └─ GuideText (TextMeshProUGUI)
```

### 1.2 FadePanel 설정

1. **Canvas 선택** → 우클릭 → `Create Empty`
2. **이름 변경**: `FadePanel`
3. **RectTransform 설정**:
   - Anchors: Stretch (좌하단 Anchor Presets에서 Shift+Alt+클릭으로 Stretch 선택)
   - Left: 0, Top: 0, Right: 0, Bottom: 0
   - Width/Height: 자동 계산됨

### 1.3 FadeImage 설정

1. **FadePanel 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `FadeImage`
3. **RectTransform 설정**:
   - Anchors: Stretch
   - Left: 0, Top: 0, Right: 0, Bottom: 0

4. **Image 컴포넌트 설정**:
   - Source Image: None (비워둠)
   - Color: **Black (R:0, G:0, B:0, A:0)** ⚠️ Alpha를 0으로!
   - Raycast Target: **✅ 체크** (중요!)

### 1.4 GuideText 설정

1. **FadePanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `GuideText`
3. **RectTransform 설정**:
   - Anchors: Center
   - Pos X: 0, Pos Y: 0
   - Width: 800, Height: 200

4. **TextMeshProUGUI 컴포넌트 설정**:
   - Text: (빈 문자열)
   - Font: 원하는 폰트
   - Font Size: **48**
   - Alignment: **Center (가운데 정렬)**
   - Color: **White (R:255, G:255, B:255, A:255)**
   - Vertex Color: White
   - Enable Auto Sizing: ❌ 체크 해제

### 1.5 FadeUI 컴포넌트 추가

1. **FadePanel 선택**
2. **Inspector** → `Add Component`
3. **검색**: `FadeUI` 입력
4. **FadeUI.cs** 선택

5. **참조 연결**:
   - Fade Image: `FadeImage` 드래그
   - Guide Text: `GuideText` 드래그
   - Fade Duration: **1** (1초)

---

## 2. ResultPanel (결산 페이지)

### 2.1 GameObject 생성

```
Canvas
└─ ResultPanel (GameObject)
   ├─ Background (Image)
   ├─ TitleText (TextMeshProUGUI) "결산"
   ├─ TotalRewardText (TextMeshProUGUI)
   ├─ TargetRewardText (TextMeshProUGUI)
   ├─ ResultMessageText (TextMeshProUGUI)
   └─ ConfirmButton (Button)
      └─ Text (TextMeshProUGUI) "확인"
```

### 2.2 ResultPanel 설정

1. **Canvas 선택** → 우클릭 → `Create Empty`
2. **이름 변경**: `ResultPanel`
3. **RectTransform 설정**:
   - Anchors: Stretch
   - Left: 0, Top: 0, Right: 0, Bottom: 0

4. **초기 상태**: Inspector에서 **왼쪽 상단 체크박스 해제 (비활성화)**

### 2.3 Background 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `Background`
3. **RectTransform**: Stretch (Left:0, Top:0, Right:0, Bottom:0)
4. **Image**:
   - Color: 반투명 검은색 (R:0, G:0, B:0, A:200)

### 2.4 TitleText 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `TitleText`
3. **RectTransform**:
   - Anchors: Top Center
   - Pos X: 0, Pos Y: -100
   - Width: 400, Height: 100
4. **TextMeshProUGUI**:
   - Text: "결산"
   - Font Size: 60
   - Alignment: Center
   - Color: White

### 2.5 TotalRewardText 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `TotalRewardText`
3. **RectTransform**:
   - Anchors: Center
   - Pos X: 0, Pos Y: 50
   - Width: 600, Height: 80
4. **TextMeshProUGUI**:
   - Text: "획득 재화: 0원"
   - Font Size: 40
   - Alignment: Center
   - Color: White

### 2.6 TargetRewardText 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `TargetRewardText`
3. **RectTransform**:
   - Anchors: Center
   - Pos X: 0, Pos Y: -30
   - Width: 600, Height: 80
4. **TextMeshProUGUI**:
   - Text: "목표 재화: 500원"
   - Font Size: 40
   - Alignment: Center
   - Color: Yellow

### 2.7 ResultMessageText 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `ResultMessageText`
3. **RectTransform**:
   - Anchors: Center
   - Pos X: 0, Pos Y: -110
   - Width: 600, Height: 80
4. **TextMeshProUGUI**:
   - Text: ""
   - Font Size: 50
   - Alignment: Center
   - Color: Green

### 2.8 ConfirmButton 설정

1. **ResultPanel 선택** → 우클릭 → `UI` > `Button - TextMeshPro`
2. **이름 변경**: `ConfirmButton`
3. **RectTransform**:
   - Anchors: Bottom Center
   - Pos X: 0, Pos Y: 100
   - Width: 300, Height: 80
4. **Button 컴포넌트**: 기본 설정 유지
5. **자식 Text**:
   - Text: "확인"
   - Font Size: 40
   - Alignment: Center

### 2.9 ResultUIController 컴포넌트 추가

1. **ResultPanel 선택**
2. **Inspector** → `Add Component` → `ResultUIController`
3. **참조 연결**:
   - Result Panel: `ResultPanel` (자기 자신)
   - Total Reward Text: `TotalRewardText` 드래그
   - Target Reward Text: `TargetRewardText` 드래그
   - Result Message Text: `ResultMessageText` 드래그
   - Confirm Button: `ConfirmButton` 드래그

---

## 3. FramePanel (대화 프레임)

### 3.1 GameObject 생성

```
Canvas
└─ FramePanel (Image)
```

### 3.2 FramePanel 설정

1. **Canvas 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `FramePanel`
3. **RectTransform 설정**:
   - Anchors: Stretch
   - Left: 0, Top: 0, Right: 0, Bottom: 0

4. **Image 컴포넌트 설정**:
   - Source Image: 대화 프레임 이미지 (있으면 사용, 없으면 None)
   - Color: 반투명 검은색 (R:0, G:0, B:0, A:150)

5. **초기 상태**: **비활성화** (왼쪽 상단 체크박스 해제)

### 3.3 FramePanelUI 컴포넌트 추가

1. **FramePanel 선택**
2. **Inspector** → `Add Component` → `FramePanelUI`
3. **참조 연결**:
   - Frame Panel: `FramePanel` (자기 자신)

---

## 4. NPCDialoguePanel

### 4.1 GameObject 생성

```
Canvas
└─ NPCDialoguePanel (Image)
   ├─ NPCImage (Image)
   ├─ NPCNameText (TextMeshProUGUI)
   └─ NPCDialogueText (TextMeshProUGUI)
```

### 4.2 NPCDialoguePanel 설정

1. **Canvas 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `NPCDialoguePanel`
3. **RectTransform**:
   - Anchors: Bottom Stretch (하단 중앙에 펼침)
   - Left: 100, Right: 100, Bottom: 50
   - Height: 200
   - Pos Y: 125 (하단에서 125만큼 위로)

4. **Image**:
   - Color: 반투명 흰색 (R:255, G:255, B:255, A:200)

5. **초기 상태**: **비활성화**

### 4.3 NPCImage 설정

1. **NPCDialoguePanel 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `NPCImage`
3. **RectTransform**:
   - Anchors: Left Center
   - Pos X: 100, Pos Y: 0
   - Width: 150, Height: 150

4. **Image**:
   - Source Image: None (스크립트에서 동적 할당)
   - Preserve Aspect: ✅ 체크

### 4.4 NPCNameText 설정

1. **NPCDialoguePanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `NPCNameText`
3. **RectTransform**:
   - Anchors: Top Left
   - Pos X: 220, Pos Y: -20
   - Width: 300, Height: 50

4. **TextMeshProUGUI**:
   - Text: ""
   - Font Size: 32
   - Alignment: Left
   - Color: Black

### 4.5 NPCDialogueText 설정

1. **NPCDialoguePanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `NPCDialogueText`
3. **RectTransform**:
   - Anchors: Stretch (NPCDialoguePanel 기준)
   - Left: 220, Top: 70, Right: 50, Bottom: 30

4. **TextMeshProUGUI**:
   - Text: ""
   - Font Size: 28
   - Alignment: Left & Top
   - Color: Black
   - Overflow: Overflow
   - Wrapping: Enabled

### 4.6 NPCDialogueUI 컴포넌트 추가

1. **NPCDialoguePanel 선택**
2. **Inspector** → `Add Component` → `NPCDialogueUI`
3. **참조 연결**:
   - Dialogue Panel: `NPCDialoguePanel` (자기 자신)
   - NPC Image: `NPCImage` 드래그
   - NPC Name Text: `NPCNameText` 드래그
   - Dialogue Text: `NPCDialogueText` 드래그
   - Fade Duration: 0.3

---

## 5. PlayerDialoguePanel

### 5.1 GameObject 생성

```
Canvas
└─ PlayerDialoguePanel (Image)
   └─ PlayerDialogueText (TextMeshProUGUI)
```

### 5.2 PlayerDialoguePanel 설정

1. **Canvas 선택** → 우클릭 → `UI` > `Image`
2. **이름 변경**: `PlayerDialoguePanel`
3. **RectTransform**:
   - Anchors: Bottom Stretch
   - Left: 100, Right: 100, Bottom: 260
   - Height: 150
   - Pos Y: 335

4. **Image**:
   - Color: 반투명 파란색 (R:100, G:150, B:255, A:200)

5. **초기 상태**: **비활성화**

### 5.3 PlayerDialogueText 설정

1. **PlayerDialoguePanel 선택** → 우클릭 → `UI` > `Text - TextMeshPro`
2. **이름 변경**: `PlayerDialogueText`
3. **RectTransform**:
   - Anchors: Stretch
   - Left: 30, Top: 30, Right: 30, Bottom: 30

4. **TextMeshProUGUI**:
   - Text: ""
   - Font Size: 28
   - Alignment: Left & Top
   - Color: White
   - Wrapping: Enabled

### 5.4 PlayerDialogueUI 컴포넌트 추가

1. **PlayerDialoguePanel 선택**
2. **Inspector** → `Add Component` → `PlayerDialogueUI`
3. **참조 연결**:
   - Dialogue Panel: `PlayerDialoguePanel` (자기 자신)
   - Dialogue Text: `PlayerDialogueText` 드래그
   - Fade Duration: 0.3

---

## 레이아웃 참고

### 화면 배치 (1920x1080 기준)

```
┌─────────────────────────────────────────────┐
│           (게임플레이 영역)                    │
│                                             │
│                                             │
│            FramePanel (전체)                 │
│                                             │
│         [PlayerDialoguePanel]               │ ← 상단 (Y: 335)
│         플레이어 대화 표시                     │
│                                             │
│    [NPCImage] [NPCDialoguePanel]            │ ← 하단 (Y: 125)
│      NPC     NPC 대화 표시                   │
└─────────────────────────────────────────────┘
```

---

## 최종 체크리스트

### FadePanel
- [ ] FadeImage Alpha = 0
- [ ] GuideText는 빈 문자열
- [ ] FadeUI 컴포넌트 참조 연결

### ResultPanel
- [ ] 초기 상태 비활성화
- [ ] 5개 텍스트 모두 있음
- [ ] ConfirmButton 있음
- [ ] ResultUIController 참조 연결

### FramePanel
- [ ] 초기 상태 비활성화
- [ ] FramePanelUI 컴포넌트 있음

### NPCDialoguePanel
- [ ] 초기 상태 비활성화
- [ ] NPCImage, NPCNameText, NPCDialogueText 있음
- [ ] NPCDialogueUI 참조 연결

### PlayerDialoguePanel
- [ ] 초기 상태 비활성화
- [ ] PlayerDialogueText 있음
- [ ] PlayerDialogueUI 참조 연결

---

## 완료!

이제 모든 UI 패널이 설정되었습니다!

**예상 소요 시간**: 약 30-40분

**더 빠른 방법**: `UI_Panel_Import_Guide.md`의 방법 1 사용 (5분!)
