using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boss : Enemy
{
    private string bossClassName;
    private bool isRuntimeSprite = false; // Record

    protected override PoolType PoolKind => PoolType.Boss; // Pool 사용 안함

    // 임시
    // Boss Attack
    private const float towerAttackRange = 10f;   // 정지
    private const float towerFireCooldown = 1.5f;
    private const float chaseFireCooldown = 2f;
    private const float hysteresisRatio = 0.8f;   // 타겟 변경 시 떨림 방지
    private const float playerPriorityRatio = 1.1f;

    // Boss Skill
    private const float skillCooldown = 8f;
    private const float castDelay = 1f;           // 스킬 사용 전 딜레이
    private const float skillDamageMultiplier = 1.5f;

    // Bird
    private const float dashPower = 15f;
    private const float dashDuration = 0.5f;

    // Dog
    private const float barkRadius = 10f;
    private const float barkAngle = 90f;

    // Fish
    private const float bubbleDuration = 3f;
    private const float bubbleRadius = 4f;
    private const float bubbleTick = 0.5f;

    private Transform currentTarget;
    private float fireTimer;
    private float skillTimer;
    private bool isCasting = false;
    private bool isDashing = false;

    // Bark 범위
    private readonly List<Collider2D> barkHits = new();
    private ContactFilter2D playerFilter;

    protected override void Awake()
    {
        base.Awake();

        playerFilter = new ContactFilter2D();
        playerFilter.SetLayerMask(LayerMask.GetMask(ConstString.LAYER_PLAYER, ConstString.LAYER_TOWER));
        playerFilter.useLayerMask = true;
        playerFilter.useTriggers = true;
    }

    public void InitializeBoss(Sprite bossSprite, string className, float hp, float bossAttack, float bossSpeed, bool runtimeSprite)
    {
        base.Initialize(bossSprite, hp, bossAttack, bossSpeed);
        bossClassName = className;
        isRuntimeSprite = runtimeSprite;
        HP_Bar.gameObject.SetActive(false);
        skillTimer = skillCooldown;
    }

    protected override void MoveToTarget()
    {
        // 스킬 사용 전 정지
        if (isCasting)
        {
            if (!isDashing) rb.linearVelocity = Vector2.zero;
            return;
        }

        UpdateTarget();

        if (currentTarget == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        bool targetIsTower = (currentTarget == targetTower);
        float sqrDist = (currentTarget.position - transform.position).sqrMagnitude;

        if (targetIsTower && sqrDist <= towerAttackRange * towerAttackRange)
        {
            // 타워 도착 (기본 공격만)
            rb.linearVelocity = Vector2.zero;
            FaceDirection(currentTarget.position.x - transform.position.x);
            TickFire(towerFireCooldown);
        }
        else
        {
            // 직진
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
            FaceDirection(direction.x);

            if (!targetIsTower)
            {
                // 플레이어 추적
                TickFire(chaseFireCooldown); // 공격 속도 Tower > Player

                skillTimer -= Time.fixedDeltaTime;
                if (skillTimer <= 0f)
                {
                    skillTimer = skillCooldown;
                    StartCoroutine(CastSkill());
                }
            }
        }
    }

    // 타겟 변경
    private void UpdateTarget()
    {
        if (currentTarget == null)
        {
            currentTarget = FindClosestTarget();
            return;
        }

        Transform other = (currentTarget == targetPlayer) ? targetTower : targetPlayer;
        if (other == null) return;

        float sqrCur = (currentTarget.position - transform.position).sqrMagnitude;
        float sqrOther = (other.position - transform.position).sqrMagnitude;

        // Player 우선
        float threshold = (other == targetPlayer) ? (sqrCur * playerPriorityRatio * playerPriorityRatio) : (sqrCur * hysteresisRatio * hysteresisRatio);
        if (sqrOther < threshold)
        {
            currentTarget = other;
        }
    }

    private void FaceDirection(float dx)
    {
        if (dx > 0.1f) sr.flipX = true;
        else if (dx < -0.1f) sr.flipX = false;
    }

    private void TickFire(float cooldown)
    {
        fireTimer -= Time.fixedDeltaTime;
        if (fireTimer > 0f) return;

        fireTimer = cooldown;
        FireAt(currentTarget);
    }

    private void FireAt(Transform target)
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle + 90f);

        GameObject proj = ObjectPoolManager.Instance.Spawn(PoolType.BossProjectile, transform.position, rot);
        if (proj != null && proj.TryGetComponent<Projectile>(out var p))
        {
            p.SetDamage(attack);
        }
    }

    private IEnumerator CastSkill()
    {
        isCasting = true;
        rb.linearVelocity = Vector2.zero;

        // 사용 전 방향 고정
        Vector2 lockedDir = (targetPlayer.position - transform.position).normalized;
        Vector3 lockedPos = targetPlayer.position;

        yield return new WaitForSeconds(castDelay);

        if (isDead) { isCasting = false; yield break; } // 시전 중 사망 = 취소

        switch (bossClassName)
        {
            case "Bird": yield return DashSkill(lockedDir); break;
            case "Dog": BarkSkill(lockedDir); break;
            case "Fish": BubbleSkill(lockedPos); break;
        }

        isCasting = false;
    }

    // Player와 동일 스킬
    // Bird Boss
    private IEnumerator DashSkill(Vector2 dir)
    {
        SoundManager.Instance.PlayBirdSkill();
        isDashing = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * dashPower, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }

    // Dog Boss
    private void BarkSkill(Vector2 dir)
    {
        SoundManager.Instance.PlayDogSkill();
        float skillDamage = attack * skillDamageMultiplier;

        float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion effectRot = Quaternion.Euler(0f, 0f, rotZ);
        GameObject effectObj = ObjectPoolManager.Instance.Spawn(PoolType.BarkEffect, transform.position, effectRot);
        if (effectObj != null && effectObj.TryGetComponent<BarkEffect>(out var effect))
        {
            effect.PlayEffect(barkRadius, barkAngle, transform, effectRot, Vector3.zero, 0.3f);
        }

        Physics2D.OverlapCircle(transform.position, barkRadius, playerFilter, barkHits);
        foreach (var hit in barkHits)
        {
            Vector2 toTarget = (hit.transform.position - transform.position).normalized;
            if (Vector2.Angle(dir, toTarget) <= barkAngle / 2f && hit.TryGetComponent<IDamageable>(out var t))
            {
                t.TakeDamage(skillDamage);
            }
        }
    }

    // Fish Boss
    private void BubbleSkill(Vector3 pos)
    {
        SoundManager.Instance.PlayFishSkill();

        GameObject obj = ObjectPoolManager.Instance.Spawn(PoolType.BubbleSkill, pos, Quaternion.identity);
        if (obj != null && obj.TryGetComponent<Bubble>(out var bubble))
        {
            bubble.Initialize(attack, skillDamageMultiplier, bubbleDuration, bubbleRadius, bubbleTick, LayerMask.GetMask(ConstString.LAYER_PLAYER));
        }
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

    // 보스 사망 (경로 우회)
    protected override void Die(bool playSound = true)
    {
        isDead = true;
        StopAllCoroutines();

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

    // 범위 확인
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, towerAttackRange);

        if (currentTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
#endif
}