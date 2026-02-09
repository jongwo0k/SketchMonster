using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour
{
    private float damage;
    private float duration;
    private float radius;
    private float tickInterval = 0.5f; // 데미지 간격

    public void Initialize(float playerAttack, float skillDuration, float skillRadius)
    {
        this.damage = playerAttack * 50f; // 즉사
        this.duration = skillDuration;
        this.radius = skillRadius;

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
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hit in hits)
        {
            // 적이 범위 내에 있으면 처치
            if (hit.CompareTag("Enemy"))
            {
                if (hit.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    enemy.TakeDamage(damage);
                }
            }
        }
    }

    // gizmo 범위 확인
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}