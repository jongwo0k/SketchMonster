using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private float attack = 1f;
    [SerializeField] private PoolType poolType = PoolType.Projectile; // Boss Projectile 별도

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 재사용 될 때마다
    private void OnEnable()
    {
        // 계속 날아가거나 쌓이는 것 방지
        StartCoroutine(AutoDespawn());
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(attack);
            ObjectPoolManager.Instance.Despawn(gameObject, poolType);
        }
    }

    // Player의 공격력을 받아옴
    public void SetDamage(float damage, float playerSpeed = 0) // MainTower의 Projectile은 추가속도0
    {
        this.attack = damage;
        rb.linearVelocity = -transform.up * (GameConfig.Data.projectileSpeed + playerSpeed); // Unity 기본은 Y축 방향 -> 캐릭터 기본은 아래를 봄
    }

    IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(GameConfig.Data.projectileDestroyTime);
        ObjectPoolManager.Instance.Despawn(gameObject, poolType);
    }
}