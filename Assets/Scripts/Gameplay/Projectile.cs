using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] public float speed = 10f;
    [SerializeField] public float destroyTime = 2f;
    [SerializeField] public float attack = 1f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 재사용 될 때마다
    private void OnEnable()
    {
        rb.linearVelocity = -transform.up * speed; // Unity 기본은 Y축 방향 -> 캐릭터 기본은 아래를 봄

        // 계속 날아가거나 쌓이는 것 방지
        StartCoroutine(AutoDespawn());
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if(collision.TryGetComponent<Enemy>(out Enemy enemy))
            {
                enemy.TakeDamage(attack);
                ObjectPoolManager.Instance.Despawn(gameObject, PoolType.Projectile);
            }
        }
    }

    // Player의 공격력을 받아옴
    public void SetDamage(float damage)
    {
        this.attack = damage;
    }

    IEnumerator AutoDespawn()
    {
        yield return new WaitForSeconds(destroyTime);
        ObjectPoolManager.Instance.Despawn(gameObject, PoolType.Projectile);
    }
}