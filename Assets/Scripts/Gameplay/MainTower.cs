using UnityEngine;
using UnityEngine.UI;

public class MainTower : MonoBehaviour
{
    public static MainTower Instance { get; private set; }

    [SerializeField] private SpriteRenderer sketchField;
    [SerializeField] private GameObject projectileObject;

    bool isUpgrade = false;

    // Tower 능력치
    public float maxHP = 100f;
    public float HP;
    public float attack = 10f;
    public float attackCoolTime = 3f;
    public float fireTimer;
    public int towerLevel = 1;

    // UI
    [SerializeField] private Slider HP_Bar;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        HP = maxHP;

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

    void Update()
    {
        if (!isUpgrade || Time.timeScale == 0f)
        {
            return;
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = attackCoolTime;
        }
    }

    // 피격
    public void TakeDamage(float damage)
    {
        HP -= damage;

        HP_Bar.value = HP / maxHP;
        if (HP_Bar.gameObject.activeSelf == false)
        {
            HP_Bar.gameObject.SetActive(true);
        }

        if (HP <= 0)
        {
            UI_Manager.Instance.GameIsOver();
        }
    }

    // 레벨업 선택지
    public void TowerLevelUP()
    {
        isUpgrade = true;

        fireTimer = 0f;
        attackCoolTime = Mathf.Max(0.5f, attackCoolTime * 0.9f);
        maxHP += towerLevel * 15f;
        attack += towerLevel * 1.1f;
        towerLevel++;

        HP_Bar.value = HP / maxHP;
    }

    // 사방으로 Projectile 발사
    public void Fire()
    {
        Quaternion rotationUp = Quaternion.Euler(0, 0, 90f + 90f);
        Quaternion rotationDown = Quaternion.Euler(0, 0, -90f + 90f);
        Quaternion rotationRight = Quaternion.Euler(0, 0, 0f + 90f);
        Quaternion rotationLeft = Quaternion.Euler(0, 0, 180f + 90f);

        SpawnProjectile(rotationUp);
        SpawnProjectile(rotationDown);
        SpawnProjectile(rotationRight);
        SpawnProjectile(rotationLeft);
    }

    // Projectile 생성
    private void SpawnProjectile(Quaternion rotation)
    {
        GameObject projInstance = Instantiate(projectileObject, transform.position, rotation);
        Projectile projectileScript = projInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetDamage(this.attack);
        }
    }

    public void RecoverHP()
    {
        HP = maxHP;
        HP_Bar.value = HP / maxHP;
    }
}