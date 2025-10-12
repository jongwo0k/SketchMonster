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
    [SerializeField] public float hp = 100f; // 테스트용 (캐릭터 스탯으로 업데이트)
    [SerializeField] public float attack = 10f;
    [SerializeField] public float speed = 5f;
    [SerializeField] public int level = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 movement;
    private Vector2 lastAimDirection; // 조준 방향을 저장

    void Awake()
    {
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
            Instantiate(projectile, firePoint.position, directionIndicator.rotation);
        }
    }

    // 방향 표시
    private void UpdateDirectionIndicator()
    {
        // 방향 벡터 -> 각도
        float angle = Mathf.Atan2(lastAimDirection.y, lastAimDirection.x) * Mathf.Rad2Deg;
        directionIndicator.rotation = Quaternion.Euler(0f, 0f, angle + 90f); // 캐릭터는 아래를 보고 있는 것이 기본값
    }

    // 물리처리
    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * speed; // 대각선 보정
    }

    // templete을 선택된 캐릭터로 교체
    public void Initialize(CharacterData data, Sprite characterSprite)
    {
        // 능력치 적용
        hp = data.hp;
        attack = data.attack;
        speed = data.speed;
        level = data.level;

        // 이미지 적용
        sr.sprite = characterSprite;
    }
}