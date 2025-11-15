using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    bool isDead = false;

    // 능력치
    [Header("Ability")]
    private float HP = 100f; // 테스트용 (Stage마다 변경)
    private float maxHP;
    private float speed = 5f;

    // Prefabs
    [SerializeField] private GameObject experienceOrb;
    private Transform targetTower;
    private Transform targetPlayer;

    // UI
    [SerializeField] private Slider HP_Bar;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        GameObject tower = GameObject.Find("MainTower"); // MainTower는 처음부터 존재
        targetTower = tower.transform;
        
        HP_Bar.gameObject.SetActive(false);
    }

    void Start()
    {
        targetPlayer = PlayerController.Instance.transform; // Player는 생성 됨
    }

    void FixedUpdate()
    {
        if (Time.timeScale == 0f || isDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // 목표를 향해 이동
        Transform currentTarget = FindClosestTarget();
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    // 초기화, EnemySpawner가 실행
    public void Initialize(Sprite enemySprite, float hp, float enemySpeed)
    {
        sr.sprite = enemySprite;
        maxHP = hp;
        HP = hp;
        speed = enemySpeed;

        HP_Bar.value = 1;
    }

    // MainTower / Player 중 가까운 대상 찾기
    private Transform FindClosestTarget()
    {
        float distToTower = Vector2.Distance(transform.position, targetTower.position);
        float distToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        if (distToPlayer < distToTower)
        {
            return targetPlayer;
        }
        else
        {
            return targetTower; // 동일하면 Tower 우선
        }
    }

    // 데미지 처리
    public void TakeDamage(int damage)
    {
        HP -= damage;

        HP_Bar.value = HP / maxHP;
        if (HP_Bar.gameObject.activeSelf == false)
        {
            HP_Bar.gameObject.SetActive(true);
        }

        if (HP <= 0)
        {
            Die();
        }
    }

    // 사망
    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        HP_Bar.gameObject.SetActive(false);

        // 경험치 오브 떨어트림
        Instantiate(experienceOrb, transform.position, Quaternion.identity);

        Destroy(gameObject, 0.5f);
    }
}