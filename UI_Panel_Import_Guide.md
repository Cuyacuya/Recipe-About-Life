# UI 패널 가져오기 가이드

## 방법 1: 병합 전 씬에서 복사 (가장 빠름!)

### 단계별 가이드

#### 1. 병합 전 GamePlayScene 임시 저장

```bash
# Git Bash 또는 터미널에서 실행
cd "C:\Users\CUYA\Desktop\Github\Recipe-About-Life"

# 병합 전 NPCTalk의 GamePlayScene을 임시 파일로 저장
git show ec0aac3:Assets/Scenes/GamePlayScene.unity > Assets/Scenes/GamePlayScene_NPCTalk_Backup.unity
```

#### 2. Unity에서 두 씬 동시에 열기

1. **Unity 에디터 열기**

2. **현재 GamePlayScene 열기**:
   - `Assets/Scenes/GamePlayScene.unity` 더블클릭

3. **백업 씬 Hierarchy에 추가 로드**:
   - 메뉴: `File` > `Open Scene Additive`
   - 선택: `Assets/Scenes/GamePlayScene_NPCTalk_Backup.unity`
   - 또는 `GamePlayScene_NPCTalk_Backup.unity` 파일을 Hierarchy 창에 드래그

#### 3. UI 패널 복사

Hierarchy에서 백업 씬의 Canvas를 펼치고 다음 GameObject들을 찾아서 복사:

```
GamePlayScene_NPCTalk_Backup (씬)
└─ Canvas
   ├─ FramePanel ✅
   ├─ PlayerDialoguePanel ✅
   ├─ NPCDialoguePanel ✅
   ├─ ResultPanel ✅
   └─ FadePanel ✅
```

**복사 방법**:
1. `FramePanel` 우클릭 → `Copy`
2. 현재 `GamePlayScene`의 `Canvas` 우클릭 → `Paste as Child`
3. 나머지 4개 패널도 동일하게 복사

#### 4. 백업 씬 언로드 및 저장

1. **백업 씬 언로드**:
   - Hierarchy에서 `GamePlayScene_NPCTalk_Backup` 씬 우클릭
   - `Unload Scene` 클릭

2. **현재 씬 저장**:
   - `Ctrl + S` 또는 `File` > `Save`

3. **백업 파일 삭제** (선택사항):
   - Project 창에서 `GamePlayScene_NPCTalk_Backup.unity` 삭제

#### 5. 참조 연결 확인

복사한 UI 패널들의 스크립트 참조를 확인:

```
ResultPanel:
- ResultUIController 컴포넌트의 참조가 모두 연결되어 있는지 확인

FadePanel:
- FadeUI 컴포넌트의 Fade Image, Guide Text 참조 확인

NPCDialoguePanel:
- NPCDialogueUI 컴포넌트의 참조 확인

PlayerDialoguePanel:
- PlayerDialogueUI 컴포넌트의 참조 확인

FramePanel:
- FramePanelUI 컴포넌트의 참조 확인
```

**참조가 끊어진 경우**:
- 해당 패널을 선택하고 Inspector에서 누락된 참조를 수동으로 연결

---

## 방법 2: Git으로 특정 파일만 복원

더 깔끔한 방법입니다.

```bash
# 현재 브랜치 확인
git branch

# 병합 전 NPCTalk의 GamePlayScene만 복원 (임시)
git show ec0aac3:Assets/Scenes/GamePlayScene.unity > temp_scene.unity

# Unity에서 temp_scene.unity 열어서 UI 패널 복사 후
# temp_scene.unity 삭제
```

---

## 방법 3: 처음부터 만들기 (시간이 많이 걸림)

만약 위 방법이 작동하지 않으면, 아래 상세 가이드를 따라 처음부터 만들 수 있습니다.

---

## 완료!

방법 1을 사용하면 **5분 안에** 모든 UI 패널을 가져올 수 있습니다!
