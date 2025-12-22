using UnityEngine;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// NPC 이동 컨트롤러
    /// 오른쪽에서 왼쪽으로 걸어와서 목표 지점에서 정지
    /// </summary>
    public class NPCMovement : MonoBehaviour
    {
        [Header("이동 설정")]
        [SerializeField]
        [Tooltip("시작 위치 (오른쪽)")]
        private Vector3 startPosition = new Vector3(8.5f, 2.3f, 0f);

        [SerializeField]
        [Tooltip("목표 위치 (중간)")]
        private Vector3 targetPosition = new Vector3(-2f, 2.3f, 0f);

        [SerializeField]
        [Tooltip("이동 속도")]
        private float moveSpeed = 2f;

        [SerializeField]
        [Tooltip("목표 위치 도달 판정 거리")]
        private float arrivalDistance = 0.1f;

        [SerializeField]
        [Tooltip("도착 후 주문까지 대기 시간 (초)")]
        private float orderDelay = 1f;

        [SerializeField]
        [Tooltip("퇴장 위치 (왼쪽 끝)")]
        private Vector3 exitPosition = new Vector3(-8.6f, 2.55f, 0f);

        [Header("참조")]
        [SerializeField]
        [Tooltip("NPC 주문 컨트롤러")]
        private NPCOrderController orderController;

        [SerializeField]
        [Tooltip("NPC 대화 컨트롤러")]
        private Dialogue.NPCDialogueController dialogueController;

        [SerializeField]
        [Tooltip("스프라이트 렌더러 (좌우 반전용)")]
        private SpriteRenderer spriteRenderer;

        [Header("상태")]
        private bool isMoving = true;
        private bool hasArrived = false;
        private bool isExiting = false;
        private bool orderCompleted = false;

        private void Awake()
        {
            // 컴포넌트 자동 찾기
            if (orderController == null)
            {
                orderController = GetComponent<NPCOrderController>();
                if (orderController == null)
                {
                    Debug.LogWarning("[NPCMovement] NPCOrderController를 찾을 수 없습니다!");
                }
            }

            if (dialogueController == null)
            {
                dialogueController = GetComponent<Dialogue.NPCDialogueController>();
                if (dialogueController == null)
                {
                    Debug.LogWarning("[NPCMovement] NPCDialogueController를 찾을 수 없습니다!");
                }
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogWarning("[NPCMovement] SpriteRenderer를 찾을 수 없습니다!");
                }
            }
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            if (dialogueController != null)
            {
                dialogueController.OnDialogueEnded -= OnIntroDialogueEnded;
                dialogueController.OnDialogueEnded -= OnExitDialogueEnded;
            }
        }

        private void Start()
        {
            // 시작 위치로 이동
            transform.position = startPosition;

            // 왼쪽으로 이동할 것이므로 스프라이트 반전
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
            }

            Debug.Log("[NPCMovement] NPC 이동 시작!");
        }

        private void Update()
        {
            if (isExiting)
            {
                // 퇴장 중
                MoveTowardsExit();
            }
            else if (isMoving && !hasArrived)
            {
                // 입장 중
                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// 목표 지점으로 이동
        /// </summary>
        private void MoveTowardsTarget()
        {
            // 목표 위치로 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // 도착 확인
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance <= arrivalDistance)
            {
                OnArrived();
            }
        }

        /// <summary>
        /// 목표 지점 도착 시 호출
        /// </summary>
        private void OnArrived()
        {
            // 이미 도착 처리했으면 중복 실행 방지
            if (hasArrived)
            {
                return;
            }

            isMoving = false;
            hasArrived = true;

            Debug.Log($"[NPCMovement] NPC 도착!");

            // Intro 대화 시작
            if (dialogueController != null)
            {
                bool hasIntroDialogue = dialogueController.StartDialogue(Dialogue.DialogueType.Intro);

                if (hasIntroDialogue)
                {
                    // Intro 대화가 있으면 대화 종료 후 주문 시작
                    dialogueController.OnDialogueEnded += OnIntroDialogueEnded;
                }
                else
                {
                    // Intro 대화가 없으면 바로 주문 시작
                    Invoke(nameof(StartOrder), orderDelay);
                }
            }
            else
            {
                // DialogueController가 없으면 바로 주문 시작
                Invoke(nameof(StartOrder), orderDelay);
            }
        }

        /// <summary>
        /// Intro 대화 종료 시 호출
        /// </summary>
        private void OnIntroDialogueEnded(Dialogue.DialogueType type)
        {
            // Intro 대화가 끝났을 때만 처리
            if (type == Dialogue.DialogueType.Intro)
            {
                // 이벤트 구독 해제
                if (dialogueController != null)
                {
                    dialogueController.OnDialogueEnded -= OnIntroDialogueEnded;
                }

                // orderDelay 후 주문 시작
                Invoke(nameof(StartOrder), orderDelay);
            }
        }

        /// <summary>
        /// 주문 시작 (도착 후 대기 시간 후 호출)
        /// </summary>
        private void StartOrder()
        {
            // 주문 컨트롤러에 정지 알림
            if (orderController != null)
            {
                orderController.OnNPCStopped();
                Debug.Log("[NPCMovement] 주문 시작!");
            }
            else
            {
                Debug.LogWarning("[NPCMovement] OrderController가 없어서 주문을 시작할 수 없습니다!");
            }
        }

        /// <summary>
        /// 이동 재시작 (주문 완료 후 퇴장용)
        /// </summary>
        public void StartMoving()
        {
            isMoving = true;
        }

        /// <summary>
        /// 이동 중지
        /// </summary>
        public void StopMoving()
        {
            isMoving = false;
        }

        /// <summary>
        /// 새로운 목표 위치 설정
        /// </summary>
        public void SetTargetPosition(Vector3 newTarget)
        {
            targetPosition = newTarget;
            hasArrived = false;
        }

        /// <summary>
        /// 주문 완료 - 퇴장 시작
        /// </summary>
        public void OnOrderComplete()
        {
            if (orderCompleted)
            {
                Debug.LogWarning("[NPCMovement] 이미 주문이 완료되었습니다!");
                return;
            }

            orderCompleted = true;
            Debug.Log("[NPCMovement] 주문 완료! 퇴장 시작");

            // 주문 UI 숨기기
            if (orderController != null)
            {
                orderController.ClearOrder();
            }

            // 퇴장 시작
            StartExit();
        }

        /// <summary>
        /// 퇴장 시작
        /// </summary>
        private void StartExit()
        {
            // Exit 대화 시작
            if (dialogueController != null)
            {
                bool hasExitDialogue = dialogueController.StartDialogue(Dialogue.DialogueType.Exit);

                if (hasExitDialogue)
                {
                    // Exit 대화가 있으면 대화 종료 후 퇴장 이동 시작
                    dialogueController.OnDialogueEnded += OnExitDialogueEnded;
                }
                else
                {
                    // Exit 대화가 없으면 바로 퇴장 이동 시작
                    BeginExitMovement();
                }
            }
            else
            {
                // DialogueController가 없으면 바로 퇴장 이동 시작
                BeginExitMovement();
            }
        }

        /// <summary>
        /// Exit 대화 종료 시 호출
        /// </summary>
        private void OnExitDialogueEnded(Dialogue.DialogueType type)
        {
            // Exit 대화가 끝났을 때만 처리
            if (type == Dialogue.DialogueType.Exit)
            {
                // 이벤트 구독 해제
                if (dialogueController != null)
                {
                    dialogueController.OnDialogueEnded -= OnExitDialogueEnded;
                }

                // 퇴장 이동 시작
                BeginExitMovement();
            }
        }

        /// <summary>
        /// 퇴장 이동 시작 (Exit 대화 종료 후 호출)
        /// </summary>
        private void BeginExitMovement()
        {
            isExiting = true;
            isMoving = false;
            Debug.Log("[NPCMovement] NPC 퇴장 중...");
        }

        /// <summary>
        /// 퇴장 위치로 이동
        /// </summary>
        private void MoveTowardsExit()
        {
            // 퇴장 위치로 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                exitPosition,
                moveSpeed * Time.deltaTime
            );

            // 퇴장 완료 확인
            float distance = Vector3.Distance(transform.position, exitPosition);
            if (distance <= arrivalDistance)
            {
                OnExitComplete();
            }
        }

        /// <summary>
        /// 퇴장 완료 - NPC 제거 및 다음 NPC 스폰 알림
        /// </summary>
        private void OnExitComplete()
        {
            Debug.Log("[NPCMovement] NPC 퇴장 완료! SpawnManager에 알림");

            // SpawnManager에 완료 알림
            NPCSpawnManager spawnManager = FindObjectOfType<NPCSpawnManager>();
            if (spawnManager != null)
            {
                spawnManager.OnNPCOrderComplete();
            }
            else
            {
                Debug.LogWarning("[NPCMovement] SpawnManager를 찾을 수 없습니다!");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 주문 완료 여부 확인
        /// </summary>
        public bool IsOrderCompleted()
        {
            return orderCompleted;
        }
    }
}