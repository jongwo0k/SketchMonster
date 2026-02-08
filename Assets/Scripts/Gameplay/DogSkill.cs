using UnityEngine;

public class DogSkill : Skill
{
    public float radius = 8f;
    public float angle = 90f;

    public override void Initialize(PlayerController _player)
    {
        base.Initialize(_player);
        this.cooldown = 1f;
    }

    protected override void Execute()
    {
        BarkSkill();
    }

    // Bark: 제자리에서 전방 공격, 범위 내 적 처치
    private void BarkSkill()
    {
        // 진행방향 유지
        Vector2 barkDir = player.CurrentMovement.normalized;
        if (barkDir == Vector2.zero)
        {
            barkDir = player.LastAimDirection;
        }

        SoundManager.Instance.PlayDogSkill();

        float rotZ = Mathf.Atan2(barkDir.y, barkDir.x) * Mathf.Rad2Deg - 90; // 위
        Quaternion effectRot = Quaternion.Euler(0f, 0f, rotZ);

        ObjectPoolManager.Instance.Spawn(PoolType.BarkParticle, player.firePoint.position, effectRot);
        
        BarkDamage(barkDir);
    }

    private void BarkDamage(Vector2 direction)
    {
        Vector3 origin = player.firePoint.position;

        // 주변 collider 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);

        foreach (var hit in hits)
        {
            // 적이 범위 내에 있으면 처치
            if (hit.CompareTag("Enemy"))
            {
                Vector2 toEnemy = (hit.transform.position - origin).normalized;
                float angleToEnemy = Vector2.Angle(direction, toEnemy);

                if (angleToEnemy <= angle / 2f)
                {
                    if (hit.TryGetComponent<Enemy>(out Enemy enemy))
                    {
                        enemy.TakeDamage(float.MaxValue);
                    }
                }
            }
        }
    }

    // gizmo 범위 확인
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Vector3 origin = player.firePoint.position;
        Vector3 direction = Application.isPlaying ? (Vector3)player.LastAimDirection : Vector3.down; // 아래

        Gizmos.color = Color.red;

        // 원
        Gizmos.DrawWireSphere(origin, radius);
        // 부채꼴 (데미지 범위)
        Vector3 leftDir = Quaternion.Euler(0, 0, angle / 2f) * direction;
        Vector3 rightDir = Quaternion.Euler(0, 0, -angle / 2f) * direction;

        Gizmos.DrawLine(origin, origin + leftDir * radius);
        Gizmos.DrawLine(origin, origin + rightDir * radius);
    }
}