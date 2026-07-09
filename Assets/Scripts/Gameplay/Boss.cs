using UnityEngine;
using System.Collections;

public class Boss : Enemy
{
    private bool isRuntimeSprite = false;

    protected override PoolType PoolKind => PoolType.Boss; // Pool 사용 안함

    public void InitializeBoss(Sprite bossSprite, float hp, float bossAttack, float bossSpeed, bool runtimeSprite)
    {
        base.Initialize(bossSprite, hp, bossAttack, bossSpeed);
        isRuntimeSprite = runtimeSprite;
        HP_Bar.gameObject.SetActive(false);
    }

    // 보스 체력 = 상단 슬라이더 (타이머)
    protected override void OnHPChanged()
    {
        EventManager.StageSlider(HP / maxHP);
    }

    // 접촉 (넉백)
    protected override void OnContact(IDamageable target)
    {
        if (target is PlayerController player)
        {
            player.TakeDamage(attack);
            Vector2 dir = (player.transform.position - transform.position).normalized;
            player.ApplyKnockback(dir, 10f, 0.5f); // 임시
        }
        else // 타워
        {
            target.TakeDamage(attack);
        }
    }

    // 보스는 대시(float.MaxValue)로 즉사X
    public override void TakeDamage(float damage)
    {
        float clamped = Mathf.Min(damage, maxHP * 0.1f); // 임시
        base.TakeDamage(clamped);
    }

    // Despawn 경로 우회
    protected override void Die(bool playSound = true)
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        ObjectPoolManager.Instance.Spawn(PoolType.DieParticle, transform.position, Quaternion.identity);
        if (playSound)
        {
            SoundManager.Instance.PlayEnemyDie();
        }

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        // timeScale=0에서도
        yield return new WaitForSecondsRealtime(1f);
        EventManager.BossDefeated();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // record에서 로드한 것만 (template은 X)
        if (isRuntimeSprite && sr != null && sr.sprite != null)
        {
            Texture2D tex = sr.sprite.texture;
            Destroy(sr.sprite);
            if (tex != null) Destroy(tex);
        }
    }
}