using UnityEngine;

public class FishSkill : Skill
{
    private BubbleSkillStats stats;

    public override void Initialize(PlayerController _player)
    {
        base.Initialize(_player);
        stats = GameConfig.Data.bubbleStats;
        this.cooldown = stats.cooldown;
    }

    protected override void Execute()
    {
        // Bubble: 플레이어 위치에 장판 생성, 장판 위의 적 처치? 지속 데미지?
        GameObject bubbleObj = ObjectPoolManager.Instance.Spawn(PoolType.BubbleSkill, player.firePoint.position, Quaternion.identity);

        if (bubbleObj != null)
        {
            Bubble bubble = bubbleObj.GetComponent<Bubble>();
            if (bubble != null)
            {
                bubble.Initialize(player.attack, stats.damageMultiplier, stats.duration, stats.radius, stats.tickInterval);
            }
        }

        SoundManager.Instance.PlayFishSkill();
    }
}