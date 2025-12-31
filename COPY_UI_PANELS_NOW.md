# ✅ UI 패널 복사 - 지금 바로 실행하세요!

백업 씬 파일이 준비되었습니다: `Assets/Scenes/GamePlayScene_NPCTalk_Backup.unity`

---

## 🎯 Unity 에디터에서 실행할 단계

### 1단계: 현재 GamePlayScene 열기

1. **Unity 에디터 열기**
2. **Project 창**에서 `Assets/Scenes/GamePlayScene.unity` **더블클릭**
   - 현재 작업 중인 GamePlayScene이 열립니다

---

### 2단계: 백업 씬 추가 로드 (Additive)

1. **메뉴**: `File` > `Open Scene Additive` 클릭

2. **씬 선택 창**이 열리면:
   - `Assets/Scenes` 폴더로 이동
   - **`GamePlayScene_NPCTalk_Backup.unity`** 선택
   - **`Open`** 버튼 클릭

3. **Hierarchy 창 확인**:
   ```
   Hierarchy:
   ├─ GamePlayScene (현재 씬)
   │  └─ Canvas
   │     └─ ... (현재 UI들)
   └─ GamePlayScene_NPCTalk_Backup (백업 씬) ✅
      └─ Canvas
         ├─ FramePanel ✅
         ├─ PlayerDialoguePanel ✅
         ├─ NPCDialoguePanel ✅
         ├─ ResultPanel ✅
         └─ FadePanel ✅
   ```

---

### 3단계: UI 패널 복사 (5개)

**중요**: 반드시 순서대로 하나씩 복사하세요!

#### 3.1 FramePanel 복사

1. **Hierarchy**에서:
   - `GamePlayScene_NPCTalk_Backup` > `Canvas` > **`FramePanel`** 찾기
2. **`FramePanel` 우클릭** > **`Copy`** 클릭
3. **`GamePlayScene`** > **`Canvas` 우클릭** > **`Paste as Child`** 클릭
4. ✅ **복사 완료** - Hierarchy에서 `GamePlayScene/Canvas/FramePanel` 확인

#### 3.2 PlayerDialoguePanel 복사

1. **Hierarchy**에서:
   - `GamePlayScene_NPCTalk_Backup` > `Canvas` > **`PlayerDialoguePanel`** 찾기
2. **`PlayerDialoguePanel` 우클릭** > **`Copy`**
3. **`GamePlayScene`** > **`Canvas` 우클릭** > **`Paste as Child`**
4. ✅ **복사 완료**

#### 3.3 NPCDialoguePanel 복사

1. **Hierarchy**에서:
   - `GamePlayScene_NPCTalk_Backup` > `Canvas` > **`NPCDialoguePanel`** 찾기
2. **`NPCDialoguePanel` 우클릭** > **`Copy`**
3. **`GamePlayScene`** > **`Canvas` 우클릭** > **`Paste as Child`**
4. ✅ **복사 완료**

#### 3.4 ResultPanel 복사

1. **Hierarchy**에서:
   - `GamePlayScene_NPCTalk_Backup` > `Canvas` > **`ResultPanel`** 찾기
2. **`ResultPanel` 우클릭** > **`Copy`**
3. **`GamePlayScene`** > **`Canvas` 우클릭** > **`Paste as Child`**
4. ✅ **복사 완료**

#### 3.5 FadePanel 복사

1. **Hierarchy**에서:
   - `GamePlayScene_NPCTalk_Backup` > `Canvas` > **`FadePanel`** 찾기
2. **`FadePanel` 우클릭** > **`Copy`**
3. **`GamePlayScene`** > **`Canvas` 우클릭** > **`Paste as Child`**
4. ✅ **복사 완료**

---

### 4단계: 백업 씬 언로드

1. **Hierarchy**에서 **`GamePlayScene_NPCTalk_Backup`** (씬) **우클릭**
2. **`Unload Scene`** 클릭
3. ✅ **백업 씬 언로드 완료**

---

### 5단계: 현재 씬 저장

1. **메뉴**: `File` > `Save` 클릭
   - 또는 **`Ctrl + S`** (Windows) / **`Cmd + S`** (Mac)
2. ✅ **씬 저장 완료**

---

### 6단계: 복사된 패널 확인

**Hierarchy**에서 `GamePlayScene/Canvas` 아래에 다음 GameObject들이 있는지 확인:

```
GamePlayScene
└─ Canvas
   ├─ ... (기존 UI들)
   ├─ FramePanel ✅
   ├─ PlayerDialoguePanel ✅
   ├─ NPCDialoguePanel ✅
   ├─ ResultPanel ✅
   └─ FadePanel ✅
```

---

### 7단계: 참조 확인 (중요!)

각 패널을 선택하고 Inspector에서 컴포넌트 참조가 연결되어 있는지 확인:

#### FramePanel 확인
1. **Hierarchy**에서 `FramePanel` 선택
2. **Inspector**에서 **`FramePanelUI`** 컴포넌트 확인
3. **Frame Panel** 필드: `FramePanel` 연결되어 있는지 확인
4. ✅ **참조 OK**

#### PlayerDialoguePanel 확인
1. **Hierarchy**에서 `PlayerDialoguePanel` 선택
2. **Inspector**에서 **`PlayerDialogueUI`** 컴포넌트 확인
3. **Dialogue Panel**: `PlayerDialoguePanel` 연결 확인
4. **Dialogue Text**: `PlayerDialogueText` 연결 확인
5. ✅ **참조 OK**

#### NPCDialoguePanel 확인
1. **Hierarchy**에서 `NPCDialoguePanel` 선택
2. **Inspector**에서 **`NPCDialogueUI`** 컴포넌트 확인
3. **Dialogue Panel**: `NPCDialoguePanel` 연결 확인
4. **NPC Image**: `NPCImage` 연결 확인
5. **NPC Name Text**: `NPCNameText` 연결 확인
6. **Dialogue Text**: `NPCDialogueText` 연결 확인
7. ✅ **참조 OK**

#### ResultPanel 확인
1. **Hierarchy**에서 `ResultPanel` 선택
2. **Inspector**에서 **`ResultUIController`** 컴포넌트 확인
3. **Result Panel**: `ResultPanel` 연결 확인
4. **Total Reward Text**: `TotalRewardText` 연결 확인
5. **Target Reward Text**: `TargetRewardText` 연결 확인
6. **Result Message Text**: `ResultMessageText` 연결 확인
7. **Confirm Button**: `ConfirmButton` 연결 확인
8. ✅ **참조 OK**

#### FadePanel 확인
1. **Hierarchy**에서 `FadePanel` 선택
2. **Inspector**에서 **`FadeUI`** 컴포넌트 확인
3. **Fade Image**: `FadeImage` 연결 확인
4. **Guide Text**: `GuideText` 연결 확인
5. ✅ **참조 OK**

---

### 8단계 (선택사항): 백업 씬 파일 삭제

UI 패널 복사가 완료되고 모든 참조가 정상이면, 백업 파일을 삭제할 수 있습니다:

1. **Project 창**에서 `Assets/Scenes/GamePlayScene_NPCTalk_Backup.unity` 찾기
2. **우클릭** > **`Delete`** 클릭
3. **확인 창**에서 **`Delete`** 클릭
4. ✅ **백업 파일 삭제 완료**

또는 나중을 위해 백업 파일을 보관해도 됩니다.

---

## ✅ 완료!

이제 모든 UI 패널이 현재 GamePlayScene에 복사되었습니다!

**예상 소요 시간**: 5-10분

**다음 단계**:
1. StageStoryController GameObject 추가
2. StoryNPCConfig ScriptableObject 생성
3. 전체 프로세스 테스트

---

## ⚠️ 문제가 발생한 경우

### 문제 1: 백업 씬이 보이지 않음
**해결**: Unity 에디터 재시작 후 다시 시도

### 문제 2: 참조가 끊어짐 (Missing Reference)
**해결**:
1. 해당 패널 선택
2. Inspector에서 누락된 필드 찾기
3. Hierarchy에서 해당 GameObject를 드래그하여 연결

### 문제 3: 복사한 패널이 작동하지 않음
**해결**:
1. 백업 씬 언로드 확인
2. 씬 저장 (`Ctrl + S`) 확인
3. Unity 재시작

---

## 📞 도움이 필요하면

- 스크린샷과 함께 어떤 단계에서 문제가 발생했는지 알려주세요
- Console 창의 에러 메시지를 복사해서 알려주세요
