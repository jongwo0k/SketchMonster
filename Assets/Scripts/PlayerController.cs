using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 공격
    [Header("Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private Transform directionIndicator; // 조준 방향
    [SerializeField] private Transform firePoint;          // 발사 지점

    // 움직임
    [Header("Movement")]
    [SerializeField]  public float speed = 5f; // 테스트용 (캐릭터 스탯으로 업데이트)

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 lastAimDirection; // 조준 방향을 저장

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastAimDirection = Vector2.down; // default: 아래쪽 방향
    }

    void Update()
    {
        // 키보드 움직임 입력 (WASD, 방향키 모두)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

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

    // 방향 표시
    private void UpdateDirectionIndicator()
    {
        // 방향 벡터 -> 각도
        float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;
        directionIndicator.rotation = Quaternion.Euler(0f, 0f, angle + 90f); // 캐릭터는 아래를 보고 있는 것이 기본값
    }

    // 발사
    private void Fire()
    {
        Instantiate(projectile, firePoint.position, directionIndicator.rotation);
    }

    // 물리처리
    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * speed; // 대각선 보정
    }

    // 캐릭터 데이터로 교체
}