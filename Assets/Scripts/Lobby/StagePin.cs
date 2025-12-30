using UnityEngine;
using System.Collections;

namespace RecipeAboutLife.Lobby
{
    /// <summary>
    /// 스테이지 핀 - 위아래로 움직이고 클릭 가능
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class StagePin : MonoBehaviour
    {
        [Header("스테이지 설정")]
        [SerializeField] private int stageIndex = 1;
        [SerializeField] private bool isUnlocked = true;

        [Header("스프라이트")]
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite lockedSprite;

        [Header("부유 애니메이션")]
        [SerializeField] private float floatHeight = 0.15f;
        [SerializeField] private float floatSpeed = 2f;

        private SpriteRenderer spriteRenderer;
        private Vector3 startPosition;

        public int StageIndex => stageIndex;
        public bool IsUnlocked => isUnlocked;
        public System.Action<StagePin> OnClicked;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPosition = transform.position;
        }

        private void Start()
        {
            UpdateSprite();
            StartCoroutine(FloatAnimation());
        }

        private void OnMouseDown()
        {
            Debug.Log($"[StagePin] Stage {stageIndex} 클릭!");
            OnClicked?.Invoke(this);
        }

        public void SetUnlocked(bool unlocked)
        {
            isUnlocked = unlocked;
            UpdateSprite();
        }

        private void UpdateSprite()
        {
            if (spriteRenderer == null) return;

            if (isUnlocked && unlockedSprite != null)
            {
                spriteRenderer.sprite = unlockedSprite;
                spriteRenderer.color = Color.white;
            }
            else if (!isUnlocked && lockedSprite != null)
            {
                spriteRenderer.sprite = lockedSprite;
                spriteRenderer.color = Color.white;
            }
            else if (!isUnlocked)
            {
                spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        private IEnumerator FloatAnimation()
        {
            float time = Random.Range(0f, Mathf.PI * 2f);

            while (true)
            {
                time += Time.deltaTime * floatSpeed;
                float yOffset = Mathf.Sin(time) * floatHeight;
                transform.position = startPosition + new Vector3(0, yOffset, 0);
                yield return null;
            }
        }
    }
}
