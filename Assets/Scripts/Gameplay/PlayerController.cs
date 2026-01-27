using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    // 공격
    [Header("Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform directionIndicator; // 조준 방향
    [SerializeField] private Transform firePoint;          // 발사 지점

    // 움직임
    [Header("Movement")]
    [SerializeField] public float HP = 100f; // 테스트용 (캐릭터 스탯으로 업데이트)
    [SerializeField] public float XP = 1f;
    [SerializeField] public float attack = 10f;
    [SerializeField] public float speed = 5f;
    [SerializeField] public int level = 1;
    private float maxHP;
    private float maxXP = 100f;

    // UI
    [Header("UI")]
    [SerializeField] private Slider HP_Bar;
    [SerializeField] private Slider XP_Bar;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movement;
    private Vector2 lastAimDirection; // 조준 방향을 저장

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

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        lastAimDirection = Vector2.down; // default: 아래쪽 방향
    }

    void Update()
    {
        // 키보드 움직임 입력 (WASD, 방향키 모두)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 바라보는 방향으로 스프라이트 반전
        // GAN이 어느 방향을 바라보는 이미지를 생성할지 알 수 없음, 현재 오른쪽을 보는 이미지가 정상 동작
        if (movement.x > 0.1f)
        {
            sr.flipX = false;
        }
        else if (movement.x < -0.1f)
        {
            sr.flipX = true;
        }

        // 멈춰도 직전 방향 유지
        if (movement.sqrMagnitude > 0.1f)
        {
            lastAimDirection = movement.normalized;
            UpdateDirectionIndicator();
        }

        // 기본 공격 발사 (Space)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    // 물리처리
    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * speed; // 대각선 보정
    }

    // 충돌
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ExperienceOrb")) // 경험치 구슬
        {
            float xpValue = collision.GetComponent<ExpOrb>().expValue;
            GetXP(xpValue);
            Destroy(collision.gameObject);
        }
        
    }

    // 방향 표시
    private void UpdateDirectionIndicator()
    {
        // 방향 벡터 -> 각도
        float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;
        directionIndicator.rotation = Quaternion.Euler(0f, 0f, angle + 90f); // 캐릭터는 아래를 보고 있는 것이 기본값
    }

    // templete을 선택된 캐릭터로 교체
    public void Initialize(CharacterData data, Sprite characterSprite)
    {
        // 능력치 적용
        HP = data.hp;
        maxHP = data.hp;
        attack = data.attack;
        speed = data.speed;
        level = data.level;

        // 이미지 적용
        sr.sprite = characterSprite;

        // UI 초기화
        HP_Bar.value = HP / maxHP;
        XP_Bar.value = XP / maxXP;
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
            gameObject.SetActive(false);
        }
    }

    // Projectile 발사
    public void Fire()
    {
        GameObject projectileObject = Instantiate(projectile, firePoint.position, directionIndicator.rotation);
        Projectile projectileScript = projectileObject.GetComponent<Projectile>();
        projectileScript.SetDamage(this.attack);
    }

    // 경험치 획득
    public void GetXP(float xp)
    {
        XP += xp;
        if (XP >= maxXP)
        {
            level++;
            XP -= maxXP;
            maxXP *= 1.1f; // 필요 경험치 증가

            UI_Manager.Instance.LevelUP();
        }
        XP_Bar.value = XP / maxXP;
    }

    // 레벨업 선택지
    public void PlayerLevelUP()
    {
        maxHP += level * 10f;
        attack += level * 1.1f;

        HP_Bar.value = HP / maxHP;
    }

    public void RecoverHP()
    {
        HP = maxHP;
        HP_Bar.value = HP / maxHP;
    }

    // 종료 시 이번 회차 데이터 제거
    void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (sr != null && sr.sprite != null && sr.sprite.texture != null)
        {
            Destroy(sr.sprite.texture);
        }
    }
}