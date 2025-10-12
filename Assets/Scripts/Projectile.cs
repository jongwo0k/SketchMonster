using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] public float speed = 10f;
    [SerializeField] public float destroy = 2f;

    public Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = -transform.up * speed; // Unity 기본은 Y축 방향 -> 캐릭터 기본은 아래를 봄

        // 계속 날아가거나 쌓이는 것 방지
        Destroy(gameObject, destroy);
    }
}