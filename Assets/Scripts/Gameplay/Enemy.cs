using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour, IDamageable
{
    protected bool isDead = false;
    protected bool isDefeat = false;

    // 능력치
    [Header("Ability")]
    protected float HP = 100f; // 테스트용 (Stage마다 변경)
    protected float maxHP;
    protected float attack = 10f;
    protected float speed = 5f;

    // Prefabs
    protected Transform targetTower;
    protected Transform targetPlayer;

    // UI
    [SerializeField] protected Slider HP_Bar;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Collider2D col;

    protected virtual PoolType PoolKind => PoolType.Enemy;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        HP_Bar.gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
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

    protected virtual void OnDisable()
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

        MoveToTarget();
    }

    protected virtual void MoveToTarget()
    {
        // 목표를 향해 이동
        Transform currentTarget = FindClosestTarget();
        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;

        if (direction.x > 0.1f)
        {
            sr.flipX = true;
        }
        else if (direction.x < -0.1f)
        {
            sr.flipX = false;
        }
    }

    // 충돌 처리
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var target))
        {
            OnContact(target);
        }
    }

    protected virtual void OnContact(IDamageable target) // Enemy 즉사, Boss 넉백
    {
        target.TakeDamage(attack);
        Die(false);
    }

    // 초기화, EnemySpawner가 실행
    public virtual void Initialize(Sprite enemySprite, float hp, float enemyAttack, float enemySpeed)
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

#if UNITY_EDITOR
        Debug.Log($"[Enemy Stats] HP: {maxHP}, Attack: {attack}, Speed: {speed}");
#endif
    }

    // MainTower / Player 중 가까운 대상 찾기
    protected Transform FindClosestTarget()
    {
        // 파괴 될경우?

        // 거리 계산
        float sqrDistToTower = (transform.position - targetTower.position).sqrMagnitude;
        float sqrDistToPlayer = (transform.position - targetPlayer.position).sqrMagnitude;

        // 가까운 쪽 타겟팅
        if (sqrDistToPlayer < sqrDistToTower)
        {
            return targetPlayer;
        }
        else
        {
            return targetTower; // 동일하면 Tower 우선
        }
    }

    // 데미지 처리
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        HP -= damage;

        OnHPChanged();

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

    protected virtual void OnHPChanged() // Boss는 HP_Bar대신 타이머 슬라이더
    {
        HP_Bar.value = HP / maxHP;
        if (HP_Bar.gameObject.activeSelf == false)
        {
            HP_Bar.gameObject.SetActive(true);
        }
    }

    // 사망
    protected virtual void Die(bool playSound = true)
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
        ObjectPoolManager.Instance.Despawn(gameObject, PoolKind);
    }
}