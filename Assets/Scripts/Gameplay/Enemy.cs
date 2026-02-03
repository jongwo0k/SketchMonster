using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour
{
    bool isDead = false;
    bool isDefeat = false;

    // 능력치
    [Header("Ability")]
    private float HP = 100f; // 테스트용 (Stage마다 변경)
    private float maxHP;
    private float attack = 10f;
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
        
        HP_Bar.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (PlayerController.Instance != null)
        {
            targetPlayer = PlayerController.Instance.transform;
        }
        if (MainTower.Instance != null)
        {
            targetTower = MainTower.Instance.transform;
        }
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
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
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    // 충돌 처리
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("MainTower")) 
        {
            MainTower.Instance.TakeDamage(attack);
            Die(false);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(attack);
            Die(false);
        }
    }

    // 초기화, EnemySpawner가 실행
    public void Initialize(Sprite enemySprite, float hp, float enemyAttack, float enemySpeed)
    {
        sr.sprite = enemySprite;
        maxHP = hp;
        HP = hp;
        attack = enemyAttack;
        speed = enemySpeed;

        // 상태 초기화
        isDead = false;
        isDefeat = false;
        col.enabled = true;

        // HP UI
        HP_Bar.gameObject.SetActive(true);
        HP_Bar.value = 1;
    }

    // MainTower / Player 중 가까운 대상 찾기
    private Transform FindClosestTarget()
    {
        // 파괴 될경우?

        // 거리 계산
        float distToTower = Vector2.Distance(transform.position, targetTower.position);
        float distToPlayer = Vector2.Distance(transform.position, targetPlayer.position);

        // 가까운 쪽 타겟팅
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
            isDefeat = true;
            Die(true);
        }
        else
        {
            ObjectPoolManager.Instance.Spawn(PoolType.EnemyHitParticle, transform.position, Quaternion.identity);
            SoundManager.Instance.PlayEnemyHit();
        }
    }

    // 사망
    private void Die(bool playSound = true)
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        HP_Bar.gameObject.SetActive(false);

        // 처치된 경우에만 경험치 오브 떨어트림
        if (isDefeat)
        {
            ObjectPoolManager.Instance.Spawn(PoolType.ExpOrb, transform.position, Quaternion.identity);
            ObjectPoolManager.Instance.Spawn(PoolType.DieParticle, transform.position, Quaternion.identity);
        }
        StartCoroutine(DespawnDelay(0.5f));
        if (playSound)
        {
            SoundManager.Instance.PlayEnemyDie();
        }
    }

    IEnumerator DespawnDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolManager.Instance.Despawn(gameObject, PoolType.Enemy);
    }
}