using UnityEngine;
using System.Collections.Generic;

public class DogSkill : Skill
{
    private BarkSkillStats stats;

    private readonly List<Collider2D> hitList = new List<Collider2D>();
    private ContactFilter2D enemyFilter;

    public override void Initialize(PlayerController _player)
    {
        base.Initialize(_player);
        stats = GameConfig.Data.barkStats;
        this.cooldown = stats.cooldown;

        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask(ConstString.LAYER_ENEMY));
        enemyFilter.useLayerMask = true;
        enemyFilter.useTriggers = true;
    }

    protected override void Execute()
    {
        BarkSkill();
    }

    // Bark: 제자리에서 전방 공격, 범위 내 적에게 데미지
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
        GameObject effectObj = ObjectPoolManager.Instance.Spawn(PoolType.BarkEffect, player.FirePoint.position, effectRot);
        if (effectObj != null)
        {
            BarkEffect effect = effectObj.GetComponent<BarkEffect>();
            if (effect != null)
            {
                Vector3 firePoint = player.FirePoint.localPosition;
                effect.PlayEffect(stats.radius, stats.angle, player.transform, effectRot, firePoint, stats.effectDuration);
            }
        }

        BarkDamage(barkDir);
    }

    private void BarkDamage(Vector2 direction)
    {
        Vector3 origin = player.FirePoint.position;
        float currentDamage = player.Attack * stats.damageMultiplier;

        // 주변 collider 탐색
        Physics2D.OverlapCircle(origin, stats.radius, enemyFilter, hitList);

        foreach (var hit in hitList)
        {
            // 적이 범위 내에 있으면 데미지
            Vector2 toEnemy = (hit.transform.position - origin).normalized;
            float angleToEnemy = Vector2.Angle(direction, toEnemy);

            if (angleToEnemy <= stats.angle / 2f)
            {
                if (hit.TryGetComponent<IDamageable>(out IDamageable target))
                {
                    target.TakeDamage(currentDamage);
                }
            }
        }
    }
}