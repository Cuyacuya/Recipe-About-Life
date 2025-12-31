# 결산 UI 및 스토리 연출 설정 가이드

## 🎯 완성 흐름

```
5명 NPC 완료
   ↓
결산 페이지 표시
   - 획득 재화: 450원
   - 목표 재화: 400원
   - ✅ 목표 달성!
   ↓
[확인] 버튼 클릭
   ↓
스토리 NPC 확대 연출
   - 배경 + NPC 이미지 페이드 인
   ↓
StoryAfterSummary 대화
   - Player ↔ NPC 대화
   ↓
연출 종료 (페이드 아웃)
```

---

## 📋 Unity 설정 단계

### **Step 1: 결산 UI 만들기**

#### 1-1. Canvas 생성

```
Hierarchy 우클릭
→ UI → Canvas
→ 이름: "ResultCanvas"

Inspector:
├─ Render Mode: Screen Space - Overlay
├─ Canvas Scaler:
│  └─ UI Scale Mode: Scale With Screen Size
│     └─ Reference Resolution: 1920 x 1080
└─ (나머지 기본값)
```

#### 1-2. 결산 Panel 생성

```
ResultCanvas 우클릭
→ UI → Panel
→ 이름: "ResultPanel"

Inspector (Image):
├─ Color: 검은색, Alpha 200 (반투명 배경)
└─ (전체 화면 크기)
```

#### 1-3. 재화 정보 텍스트 추가

```
ResultPanel 우클릭
→ UI → Text - TextMeshPro
→ 이름: "TotalRewardText"

Inspector:
├─ Text: "획득 재화: 0원"
├─ Font Size: 48
├─ Alignment: Center
├─ Color: 흰색
└─ Position: 화면 중앙 위쪽
```

```
복사해서:
→ "TargetRewardText"
→ Text: "목표 재화: 400원"
→ Position: TotalRewardText 아래
```

```
복사해서:
→ "ResultMessageText"
→ Text: "✅ 목표 달성!"
→ Font Size: 64
→ Position: 가장 위
```

#### 1-4. 확인 버튼 추가

```
ResultPanel 우클릭
→ UI → Button - TextMeshPro
→ 이름: "ConfirmButton"

Inspector:
├─ Position: 화면 하단 중앙
├─ Width: 300, Height: 80
└─ Text: "확인"
   └─ Font Size: 36
```

#### 1-5. ResultUIController 스크립트 부착

```
ResultCanvas 선택
→ Add Component
→ "ResultUIController" 검색 후 추가

Inspector:
├─ Result Panel: [ResultPanel 드래그]
├─ Total Reward Text: [TotalRewardText 드래그]
├─ Target Reward Text: [TargetRewardText 드래그]
├─ Result Message Text: [ResultMessageText 드래그]
└─ Confirm Button: [ConfirmButton 드래그]
```

#### 1-6. 초기 상태 설정

```
ResultPanel 선택
→ Inspector 체크박스 해제 (비활성화)
```

---

### **Step 2: 스토리 NPC 확대 연출 UI 만들기**

#### 2-1. Canvas 생성

```
Hierarchy 우클릭
→ UI → Canvas
→ 이름: "StoryZoomCanvas"

Inspector:
├─ Render Mode: Screen Space - Overlay
├─ Sort Order: 10 ⭐ (결산 UI보다 위)
└─ Canvas Scaler:
   └─ UI Scale Mode: Scale With Screen Size
      └─ Reference Resolution: 1920 x 1080
```

#### 2-2. Zoom Panel 생성

```
StoryZoomCanvas 우클릭
→ UI → Panel
→ 이름: "ZoomPanel"

Inspector (Image):
├─ Color: 검은색, Alpha 255 (완전 불투명)
└─ (전체 화면 크기)
```

#### 2-3. 배경 이미지 추가

```
ZoomPanel 우클릭
→ UI → Image
→ 이름: "BackgroundImage"

Inspector:
├─ Anchor: Stretch (전체 화면)
├─ Source Image: [배경 이미지 스프라이트]
│  (예: 핫도그 가게 내부, 흐린 배경 등)
├─ Color: 흰색, Alpha 255
└─ Preserve Aspect: ✅ 체크
```

#### 2-4. NPC 이미지 추가

```
ZoomPanel 우클릭
→ UI → Image
→ 이름: "NPCImage"

Inspector:
├─ Width: 600, Height: 800 (NPC 크기)
├─ Position: 화면 중앙
├─ Source Image: [임시 NPC 스프라이트]
│  (실제로는 코드에서 동적 변경됨)
├─ Color: 흰색, Alpha 255
└─ Preserve Aspect: ✅ 체크
```

#### 2-5. StoryNPCZoomController 스크립트 부착

```
StoryZoomCanvas 선택
→ Add Component
→ "StoryNPCZoomController" 검색 후 추가

Inspector:
├─ Zoom Panel: [ZoomPanel 드래그]
├─ Background Image: [BackgroundImage 드래그]
├─ NPC Image: [NPCImage 드래그]
├─ Fade In Duration: 0.5
└─ Fade Out Duration: 0.5
```

#### 2-6. 초기 상태 설정

```
ZoomPanel 선택
→ Inspector 체크박스 해제 (비활성화)
```

---

### **Step 3: 기존 시스템 확인**

#### 3-1. StageStoryController 확인

```
Hierarchy에서 "StageStoryController" 선택

Inspector:
├─ Story NPC Config: [StoryNPCConfig] ✅
├─ Current Stage ID: 1
└─ Delay Before Story Dialogue: 1
```

**이미 설정되어 있어야 합니다!**

---

## 🎮 테스트 방법

### **방법 1: 에디터에서 UI만 테스트**

#### ResultUI 테스트:

```
1. ResultCanvas 선택
2. Inspector → ResultUIController
3. 우클릭 → "Test: Show Success"
4. 결산 페이지 표시 확인
5. [확인] 버튼 클릭
6. 숨겨지는지 확인
```

#### StoryZoom 테스트:

```
1. StoryZoomCanvas 선택
2. Inspector → StoryNPCZoomController
3. 우클릭 → "Test: Show NPC"
4. 페이드 인 연출 확인
5. 우클릭 → "Test: Hide NPC"
6. 페이드 아웃 연출 확인
```

---

### **방법 2: 실제 게임 플레이 테스트**

#### 준비 사항:

1. ✅ NPC8_Ajeossi에 StoryAfterSummary 대화 설정
2. ✅ StoryNPCConfig에 Ajeossi 등록
3. ✅ NPCSpawnManager에 StoryNPCSpawnHelper 부착
4. ✅ ResultCanvas 생성
5. ✅ StoryZoomCanvas 생성

#### 게임 실행:

```
1. Play 버튼 클릭
2. NPC 1~4 서빙
3. NPC 5 (Ajeossi) 서빙
4. ⭐ 결산 페이지 표시!
   - "획득 재화: XXX원"
   - "목표 재화: 400원"
5. [확인] 버튼 클릭
6. ⭐ 스토리 NPC 확대 연출!
   - 배경 + Ajeossi 이미지 페이드 인
7. ⭐ StoryAfterSummary 대화!
   - 말풍선에 대화 표시
8. 대화 종료
9. 페이드 아웃
```

#### Console 로그 확인:

```
[ScoreManager] 모든 NPC 완료! 총 재화: 450원 / 목표: 400원
[StageStoryController] 결산 UI 표시
[ResultUIController] 결산 페이지 표시 - 성공: True, 재화: 450/400
[ResultUIController] 확인 버튼 클릭 - 성공: True
[StageStoryController] 결산 UI 확인 버튼 클릭 - 성공: True
[StageStoryController] ✅ 조건 만족! StoryAfterSummary 대화 트리거
[StageStoryController] 스토리 NPC 확대 연출 시작
[StoryNPCZoomController] 스토리 NPC 확대 표시
[StoryNPCZoomController] 페이드 인 완료
[StageStoryController] StoryAfterSummary 대화 시작!
[NPCDialogueController] [NPC] 축하해요! 목표 금액을 달성하셨네요!
[NPCDialogueController] [Player] 감사합니다! 정말 힘들었어요.
...
[StageStoryController] StoryAfterSummary 대화 종료!
[StoryNPCZoomController] 스토리 NPC 숨김
[StoryNPCZoomController] 페이드 아웃 완료
```

---

## 🎨 UI 커스터마이징

### **결산 페이지 스타일**

```css
ResultPanel:
- 배경색: #000000DD (반투명 검정)
- 테두리: 금색 Border

TotalRewardText:
- 색상: #FFD700 (금색)
- 크기: 48px

TargetRewardText:
- 색상: #FFFFFF (흰색)
- 크기: 36px

ResultMessageText (성공):
- 색상: #00FF00 (초록)
- 크기: 64px

ResultMessageText (실패):
- 색상: #FF0000 (빨강)
- 크기: 64px
```

### **확대 연출 커스터마이징**

```
BackgroundImage:
- 배경 이미지 교체 (가게 내부, 흐린 배경 등)
- Blur 효과 추가 가능

NPCImage:
- 크기 조정 (더 크게/작게)
- 위치 조정 (왼쪽/오른쪽)
- Shadow 효과 추가 가능

Fade Duration:
- Fade In: 0.5초 → 1초 (느린 연출)
- Fade Out: 0.5초 → 1초
```

---

## 🔍 문제 해결

### **Q: 결산 페이지가 안 나와요**

**확인:**
```
Console:
[StageStoryController] ResultUIController를 찾을 수 없습니다
→ ResultCanvas가 씬에 있는지 확인
→ ResultUIController 스크립트가 부착되어 있는지 확인
```

### **Q: [확인] 버튼을 눌러도 반응이 없어요**

**확인:**
```
1. ResultUIController Inspector
   → Confirm Button이 할당되어 있는지 확인
2. Button에 On Click 이벤트가 자동 등록되는지 확인
   (스크립트에서 자동 등록됨)
```

### **Q: NPC 확대 연출이 안 나와요**

**확인:**
```
Console:
[StageStoryController] StoryNPCZoomController를 찾을 수 없습니다
→ StoryZoomCanvas가 씬에 있는지 확인

[StageStoryController] NPC 스프라이트를 찾을 수 없습니다
→ NPC 프리팹에 SpriteRenderer가 있는지 확인
```

### **Q: 대화가 안 나와요**

**확인:**
```
[NPCDialogueController] StoryAfterSummary 대화가 없습니다
→ NPC_Ajeossi_Set.asset에 StoryAfterSummary 대화 추가했는지 확인
```

---

## ✅ 체크리스트

- [ ] ResultCanvas 생성
- [ ] ResultPanel + 텍스트 + 버튼 생성
- [ ] ResultUIController 스크립트 부착 및 참조 연결
- [ ] StoryZoomCanvas 생성
- [ ] ZoomPanel + 배경 + NPC 이미지 생성
- [ ] StoryNPCZoomController 스크립트 부착 및 참조 연결
- [ ] StageStoryController 씬에 있는지 확인
- [ ] NPC8_Ajeossi에 StoryAfterSummary 대화 설정
- [ ] 게임 플레이로 전체 흐름 테스트

---

## 🎉 완료!

이제 게임을 실행하면:
1. 5명 완료 → 결산 페이지
2. [확인] 클릭 → NPC 확대
3. StoryAfterSummary 대화
4. 연출 종료

순서로 진행됩니다!
