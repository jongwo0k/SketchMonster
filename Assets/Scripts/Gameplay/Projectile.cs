using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] public float speed = 10f;
    [SerializeField] public float destroyTime = 2f;
    [SerializeField] public float attack = 1f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = -transform.up * speed; // Unity 기본은 Y축 방향 -> 캐릭터 기본은 아래를 봄

        // 계속 날아가거나 쌓이는 것 방지
        Destroy(gameObject, destroyTime);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.GetComponent<Enemy>().TakeDamage(attack);
            Destroy(gameObject);
        }
    }

    // Player의 공격력을 받아옴
    public void SetDamage(float damage)
    {
        this.attack = damage;
    }
}