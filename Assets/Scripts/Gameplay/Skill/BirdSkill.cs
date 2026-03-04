using UnityEngine;
using System.Collections;

public class BirdSkill : Skill
{
    private DashSkillStats stats;

    public override void Initialize(PlayerController _player)
    {
        base.Initialize(_player);
        stats = GameConfig.Data.dashStats;
        this.cooldown = stats.cooldown;
    }

    protected override void Execute()
    {
        StartCoroutine(DashSkill());
    }

    // Dash: 순간 돌진, 돌진 중 무적, 충돌한 적 처치
    private IEnumerator DashSkill()
    {
        // 진행방향 유지
        Vector2 dashDir = player.CurrentMovement.normalized;
        if (dashDir == Vector2.zero)
        {
            dashDir = player.LastAimDirection;
        }

        SoundManager.Instance.PlayBirdSkill();

        // 대쉬 중
        player.SetDashState(true);

        player.Rb.linearVelocity = Vector2.zero;
        player.Rb.AddForce(dashDir * stats.dashPower, ForceMode2D.Impulse);

        yield return new WaitForSeconds(stats.duration);

        if (player != null)
        {
            player.SetDashState(false); // 끝
        }
    }
}