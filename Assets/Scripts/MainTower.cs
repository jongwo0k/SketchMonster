using UnityEngine;

public class MainTower : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sketchField;

    // Tower 체력
    public float maxHealth = 100f;
    public float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        // 텍스쳐 불러오기
        Texture2D sketchTexture = GameSession.OriginalSketch;

        if (sketchTexture != null)
        {
            // texture -> sprite
            Rect rect = new Rect(0, 0, sketchTexture.width, sketchTexture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float pixelsPerUnit = Mathf.Max(sketchTexture.width, sketchTexture.height); // 크기 고정
            Sprite sketchSprite = Sprite.Create(sketchTexture, rect, pivot, pixelsPerUnit);

            // 스케치로 교체
            sketchField.sprite = sketchSprite;

            // 정사각형으로
            float Width = sketchTexture.width / pixelsPerUnit;
            float Height = sketchTexture.height / pixelsPerUnit;
            float scaleX = (Width > 0) ? 0.9f / Width : 0.9f;
            float scaleY = (Height > 0) ? 0.9f / Height : 0.9f;

            sketchField.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            UI_Manager.Instance.GameIsOver();
        }
    }
}