using UnityEngine;

namespace RecipeAboutLife.NPC
{
    /// <summary>
    /// NPC 외형 데이터
    /// 각 NPC의 스프라이트 정보를 저장
    /// </summary>
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "RecipeAboutLife/NPCData")]
    public class NPCData : ScriptableObject
    {
        [Header("NPC 정보")]
        [Tooltip("NPC 이름")]
        public string NPCName = "NPC";

        [Tooltip("NPC 스프라이트")]
        public Sprite NPCSprite;

        [Header("외형 설정 (선택사항)")]
        [Tooltip("스프라이트 색상")]
        public Color SpriteColor = Color.white;

        [Tooltip("스프라이트 크기 배율")]
        public Vector3 SpriteScale = Vector3.one;
    }
}