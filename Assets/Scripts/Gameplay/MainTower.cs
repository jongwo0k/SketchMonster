using UnityEngine;
using UnityEngine.UI;

public class MainTower : MonoBehaviour, IDamageable
{
    public static MainTower Instance { get; private set; }

    [SerializeField] private SpriteRenderer sketchField;

    bool isUpgrade = false;

    // Tower 능력치
    private float maxHP;
    private float HP;
    private float attack;
    private float attackCoolTime;
    private float fireTimer;
    private int towerLevel = 1;

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
        // Tower 능력치 값 불러오기
        maxHP = GameConfig.Data.towerBaseHp;
        attack = GameConfig.Data.towerBaseAttack;
        attackCoolTime = GameConfig.Data.towerAttackCooldown;
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
            EventManager.GameOver();
        }

        ObjectPoolManager.Instance.Spawn(PoolType.HitParticle, transform.position, Quaternion.identity);
        SoundManager.Instance.PlayPlayerHit();
    }

    // 레벨업 선택지
    public void TowerLevelUP()
    {
        isUpgrade = true;

        fireTimer = 0f;
        attackCoolTime = Mathf.Max(GameConfig.Data.towerMinCooldown, attackCoolTime * GameConfig.Data.towerCooldownReduction);
        maxHP += towerLevel * GameConfig.Data.towerHpPerLevel;
        HP += towerLevel * GameConfig.Data.towerHpPerLevel;
        attack += towerLevel * GameConfig.Data.towerAttackPerLevel;
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
        GameObject projInstance = ObjectPoolManager.Instance.Spawn(PoolType.Projectile, transform.position, rotation);
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

    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if(sketchField != null && sketchField.sprite != null)
        {
            Destroy(sketchField.sprite); // sprite만
        }
    }
}