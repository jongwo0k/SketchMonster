using UnityEngine;

public class DogSkill : Skill
{
    private BarkSkillStats stats;

    public override void Initialize(PlayerController _player)
    {
        base.Initialize(_player);
        stats = GameConfig.Data.barkStats;
        this.cooldown = stats.cooldown;
        this.damage *= stats.damageMultiplier;
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

        // 범위 표시
        GameObject effectObj = ObjectPoolManager.Instance.Spawn(PoolType.BarkEffect, player.firePoint.position, effectRot);
        if (effectObj != null)
        {
            BarkEffect effect = effectObj.GetComponent<BarkEffect>();
            if (effect != null)
            {
                Vector3 firePoint = player.firePoint.localPosition;
                effect.PlayEffect(stats.radius, stats.angle, player.transform, effectRot, firePoint, stats.effectDuration);
            }
        }

        BarkDamage(barkDir);
    }

    private void BarkDamage(Vector2 direction)
    {
        Vector3 origin = player.firePoint.position;

        // 주변 collider 탐색
        int enemyLayer = LayerMask.GetMask(ConstString.LAYER_ENEMY); // 팀킬 방지
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, stats.radius, enemyLayer);

        foreach (var hit in hits)
        {
            // 적이 범위 내에 있으면 처치
            Vector2 toEnemy = (hit.transform.position - origin).normalized;
            float angleToEnemy = Vector2.Angle(direction, toEnemy);

            if (angleToEnemy <= stats.angle / 2f)
            {
                if (hit.TryGetComponent<IDamageable>(out IDamageable target))
                {
                    target.TakeDamage(damage);
                }
            }
        }
    }
}