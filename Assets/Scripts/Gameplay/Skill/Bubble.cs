using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bubble : MonoBehaviour
{
    private float damage;
    private float duration;
    private float radius;
    private float tickInterval; // 데미지 간격

    private readonly List<Collider2D> hitList = new List<Collider2D>();
    private ContactFilter2D enemyFilter;

    private void Awake()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask(ConstString.LAYER_ENEMY));
        enemyFilter.useLayerMask = true;
    }

    public void Initialize(float playerAttack, float damageMultiplier, float skillDuration, float skillRadius, float interval)
    {
        this.damage = playerAttack * damageMultiplier;
        this.duration = skillDuration;
        this.radius = skillRadius;
        this.tickInterval = interval;

        // 크기 조절
        transform.localScale = Vector3.one * (radius * 2f);

        StopAllCoroutines();
        StartCoroutine(BubbleSkill());
    }

    private IEnumerator BubbleSkill()
    {
        float elapsedTime = 0f;         // 전체 지속 시간
        float tickTimer = tickInterval; // 데미지 간격 (설치 시점부터 데미지 입히고 시작)

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                BubbleDamage();
                tickTimer = 0f;
            }

            yield return null;
        }

        ObjectPoolManager.Instance.Despawn(gameObject, PoolType.BubbleSkill); // 끝
    }

    private void BubbleDamage()
    {
        // 주변 collider 탐색
        Physics2D.OverlapCircle(transform.position, radius, enemyFilter, hitList);

        foreach (var hit in hitList)
        {
            // 적이 범위 내에 있으면 데미지
            if (hit.TryGetComponent<IDamageable>(out IDamageable target))
            {
                target.TakeDamage(damage);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}