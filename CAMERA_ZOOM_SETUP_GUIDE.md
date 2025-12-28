# 카메라 줌 연출 설정 가이드

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
🎥 카메라가 NPC에게 줌 인
   - NPC가 있는 위치로 이동
   - 카메라 크기 축소 (확대 효과)
   - 1초 동안 부드럽게 이동
   ↓
💬 StoryAfterSummary 대화
   - NPC 위 말풍선에 표시
   - Player ↔ NPC 대화
   ↓
대화 종료
   ↓
🎥 카메라 줌 아웃
   - 원래 위치/크기로 복귀
   - 1초 동안 부드럽게 이동
```

---

## 📋 Unity 설정 단계

### **Step 1: CameraZoomController 추가**

#### 1-1. Main Camera 선택

```
Hierarchy에서:
└─ Main Camera 선택
```

#### 1-2. CameraZoomController 스크립트 부착

```
Inspector:
└─ Add Component
   └─ "CameraZoomController" 검색 후 추가
```

#### 1-3. 설정

```
┌─ Camera Zoom Controller ────────────────┐
│ 줌 설정                                   │
│ ├─ Zoom In Size: 2 ⭐                    │
│ │  (작을수록 더 확대, 기본 5에서 2로)      │
│ ├─ Zoom In Duration: 1                  │
│ │  (줌 인 시간, 초 단위)                  │
│ ├─ Zoom Out Duration: 1                 │
│ │  (줌 아웃 시간, 초 단위)                │
│ └─ Target Y Offset: 0.5                 │
│    (NPC 위치에서 위로 얼마나)             │
└───────────────────────────────────────────┘
```

**설정 설명:**
- **Zoom In Size: 2**
  - 기본 카메라 크기가 5라면, 2로 줌 인하면 2.5배 확대
  - 더 작은 값 = 더 많이 확대 (예: 1 = 5배 확대)

- **Zoom Duration: 1초**
  - 빠른 연출: 0.5초
  - 느린 연출: 2초

- **Target Y Offset: 0.5**
  - NPC 발 위치가 아닌 얼굴 부근을 중심으로 줌

---

### **Step 2: 기존 설정 확인**

#### 2-1. StageStoryController 확인

```
Hierarchy에서:
└─ StageStoryController 선택

Inspector 확인:
├─ Story NPC Config: [할당됨] ✅
├─ Current Stage ID: 1
└─ Delay Before Story Dialogue: 1
```

**이미 설정되어 있어야 합니다!**

#### 2-2. ResultUIController 확인

```
Hierarchy에서:
└─ ResultCanvas 선택

Inspector 확인:
├─ Result Panel: [할당됨] ✅
├─ Total Reward Text: [할당됨] ✅
├─ Target Reward Text: [할당됨] ✅
├─ Result Message Text: [할당됨] ✅
└─ Confirm Button: [할당됨] ✅
```

---

## 🎮 테스트 방법

### **방법 1: Context Menu로 개별 테스트**

#### 카메라 줌 테스트:

```
1. 게임 플레이 (Play 버튼)
2. NPC가 등장할 때까지 대기
3. Main Camera 선택
4. Inspector → CameraZoomController
5. 우클릭 → "Test: Zoom In"
6. ⭐ 카메라가 NPC에게 줌 인!
7. 우클릭 → "Test: Zoom Out"
8. ⭐ 카메라가 원래대로 복귀!
```

---

### **방법 2: 실제 게임 플레이 테스트**

#### 준비 사항:

- [x] ResultUIController 설정 완료
- [x] Main Camera에 CameraZoomController 부착
- [x] StageStoryController 씬에 있음
- [x] StoryNPCConfig 설정 완료
- [x] NPC8_Ajeossi에 StoryAfterSummary 대화 설정

#### 게임 실행:

```
1. Play 버튼 클릭
2. NPC 1~4 서빙
3. NPC 5 (Ajeossi) 서빙
4. ⭐ 결산 페이지 표시
5. [확인] 버튼 클릭
6. ⭐ 카메라가 Ajeossi에게 줌 인!
   - 1초 동안 부드럽게 이동
7. ⭐ StoryAfterSummary 대화 시작
   - 말풍선에 대화 표시
8. 대화 종료
9. ⭐ 카메라 줌 아웃
   - 원래 위치로 복귀
```

#### Console 로그 확인:

```
[ResultUIController] 확인 버튼 클릭 - 성공: True
[StageStoryController] ✅ 조건 만족! StoryAfterSummary 대화 트리거
[StageStoryController] 카메라 줌 인 시작 → NPC
[CameraZoomController] NPC(NPC8_Ajeossi)에게 줌 인 시작
[CameraZoomController] 줌 인 완료 - 크기: 2
[StageStoryController] StoryAfterSummary 대화 시작!
[NPCDialogueController] [NPC] 축하해요! 목표 금액을 달성하셨네요!
[NPCDialogueController] [Player] 감사합니다! 정말 힘들었어요.
...
[StageStoryController] StoryAfterSummary 대화 종료!
[StageStoryController] 카메라 줌 아웃 시작
[CameraZoomController] 줌 아웃 완료 - 원래 크기: 5
```

---

## 🎨 커스터마이징

### **줌 정도 조정**

```
Zoom In Size 값 변경:

├─ 3: 약간 확대 (1.67배)
├─ 2: 중간 확대 (2.5배) ⭐ 권장
├─ 1.5: 많이 확대 (3.33배)
└─ 1: 매우 확대 (5배)
```

### **줌 속도 조정**

```
Zoom Duration 값 변경:

├─ 0.3: 매우 빠름 (급격한 연출)
├─ 0.5: 빠름
├─ 1: 보통 ⭐ 권장
├─ 1.5: 느림 (부드러운 연출)
└─ 2: 매우 느림
```

### **카메라 위치 조정**

```
Target Y Offset 값 변경:

├─ 0: NPC 발 위치
├─ 0.5: NPC 중간 ⭐ 권장
├─ 1: NPC 머리 위
└─ 1.5: NPC 위쪽 공간
```

---

## 🎬 연출 효과

### **현재 구현된 효과:**

1. **Smooth Step (부드러운 가속/감속)**
   - 시작: 천천히
   - 중간: 빠르게
   - 끝: 천천히

2. **위치 + 크기 동시 변경**
   - 카메라가 이동하면서 동시에 확대
   - 자연스러운 연출

3. **원래 위치 자동 저장**
   - Awake()에서 초기 카메라 위치/크기 저장
   - 줌 아웃 시 정확히 복귀

### **추가 가능한 효과:**

#### 배경 흐림 효과 (Post Processing)

```
Main Camera에 Post Processing Volume 추가:
└─ Depth of Field 효과
   ├─ Focus Distance: NPC까지 거리
   └─ Aperture: 5.6 (배경 흐림)
```

#### 비네팅 효과

```
Post Processing Volume:
└─ Vignette 효과
   ├─ Intensity: 0.3
   └─ Smoothness: 0.5
   (화면 가장자리 어둡게)
```

#### 색 보정

```
Post Processing Volume:
└─ Color Grading 효과
   └─ Temperature: -10 (차가운 톤)
   또는
   └─ Temperature: +10 (따뜻한 톤)
```

---

## 🔍 문제 해결

### **Q: 카메라 줌이 안 돼요**

**확인:**
```
Console:
[CameraZoomController] 메인 카메라를 찾을 수 없습니다
→ Main Camera에 "MainCamera" 태그가 있는지 확인

[StageStoryController] CameraZoomController를 찾을 수 없습니다
→ Main Camera에 CameraZoomController 스크립트가 부착되어 있는지 확인
```

### **Q: 줌이 너무 빠르거나 느려요**

**조정:**
```
Main Camera 선택
→ CameraZoomController
→ Zoom In Duration: 값 변경
   (0.5 = 빠름, 1 = 보통, 2 = 느림)
```

### **Q: NPC가 화면 밖으로 나가요**

**조정:**
```
Zoom In Size를 더 크게 (예: 2 → 3)
또는
Target Y Offset 조정 (예: 0.5 → 0)
```

### **Q: 줌 아웃 후 위치가 이상해요**

**해결:**
```
Main Camera 선택
→ CameraZoomController
→ 우클릭 → "Test: Reset"
→ 강제로 원래 위치로 복귀
```

---

## 📊 설정 비교

### **시나리오별 권장 설정:**

#### **빠른 연출 (긴박감)**
```
Zoom In Size: 1.5 (많이 확대)
Zoom In Duration: 0.5 (빠르게)
Zoom Out Duration: 0.3 (매우 빠르게)
```

#### **부드러운 연출 (감성적)** ⭐ 권장
```
Zoom In Size: 2 (적당히 확대)
Zoom In Duration: 1 (보통)
Zoom Out Duration: 1 (보통)
```

#### **드라마틱 연출 (극적)**
```
Zoom In Size: 1 (매우 확대)
Zoom In Duration: 2 (천천히)
Zoom Out Duration: 1.5 (천천히)
```

---

## ✅ 체크리스트

- [ ] Main Camera에 CameraZoomController 부착
- [ ] Zoom In Size 설정 (권장: 2)
- [ ] Zoom Duration 설정 (권장: 1초)
- [ ] Context Menu로 개별 테스트
- [ ] 실제 게임 플레이로 전체 흐름 테스트
- [ ] Console 로그 확인

---

## 🎉 완료!

이제 카메라가 NPC에게 자연스럽게 줌 인/아웃하며 스토리 대화를 연출합니다!

### **이전 방식과 차이:**
- ❌ 별도 Canvas UI 불필요
- ✅ 실제 게임 월드에서 연출
- ✅ 설정 간단 (Main Camera에만 스크립트 부착)
- ✅ 자연스러운 카메라 워크

### **다음 단계:**
- Post Processing으로 배경 흐림 효과 추가
- 음향 효과 추가
- 카메라 셰이크 효과 등
