using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bubble : MonoBehaviour
{
    private float damage;
    private float duration;
    private float radius;
    private float tickInterval; // 데미지 간격
    private float firstTickDelay;

    private readonly List<Collider2D> hitList = new List<Collider2D>();
    private ContactFilter2D enemyFilter;
    private SpriteRenderer sr;
    private Color defaultColor;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(LayerMask.GetMask(ConstString.LAYER_ENEMY));
        enemyFilter.useLayerMask = true;
        enemyFilter.useTriggers = true;
    }

    public void Initialize(float baseAttack, float damageMultiplier, float skillDuration, float skillRadius, float interval, LayerMask? targetMask = null, float firstTickDelay = 0f, Color? overrideColor = null)
    {
        this.damage = baseAttack * damageMultiplier;
        this.duration = skillDuration;
        this.radius = skillRadius;
        this.tickInterval = interval;
        this.firstTickDelay = firstTickDelay;

        enemyFilter.SetLayerMask(targetMask ?? LayerMask.GetMask(ConstString.LAYER_ENEMY));

        sr.color = overrideColor.HasValue ? new Color(overrideColor.Value.r, overrideColor.Value.g, overrideColor.Value.b, defaultColor.a) : defaultColor;

        // 크기 조절
        transform.localScale = Vector3.one * (radius * 2f);

        StopAllCoroutines();
        StartCoroutine(BubbleSkill());
    }

    private IEnumerator BubbleSkill()
    {
        float elapsedTime = 0f;                          // 전체 지속 시간
        float tickTimer = tickInterval - firstTickDelay; // 데미지 간격 (보스는 설치 시점엔 데미지 입히지 않음)

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