using UnityEngine;

// 기본 능력치
[System.Serializable]
public struct ClassStats
{
    public float hp;
    public float attack;
    public float speed;
}

// 스킬 능력치
[System.Serializable]
public struct DashSkillStats // Bird
{
    public float cooldown;
    public float dashPower;
    public float duration;
}

[System.Serializable]
public struct BarkSkillStats // Dog
{
    public float cooldown;
    public float damageMultiplier; // 기본 공격 * 계수
    public float radius;
    public float angle;
    public float effectDuration;
}

[System.Serializable]
public struct BubbleSkillStats // Fish
{
    public float cooldown;
    public float damageMultiplier;
    public float duration;
    public float radius;
    public float tickInterval;
}

// 보스 능력치
[System.Serializable]
public class BossStats
{
    // 등장
    [Header("Stage")]
    public int bossStageInterval = 5;
    public float spawnDelay = 2f; // 등장 전 대기시간
    public float dieDelay = 1f;

    // 기본 능력치
    [Header("Boss Stats")]
    public float baseHP = 500f;
    public float hpPerStage = 100f;
    public float baseAttack = 10f;
    public float attackPerStage = 3f;
    public float speed = 3f;

    // 보스전
    [Header("Combat")]
    public float maxHitRatio = 0.1f; // 단일 데미지 상한 (즉사 방지)
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.5f;
    public float contactCooldown = 1f;

    // 보스 패턴
    [Header("Behavior")]
    public float towerAttackRange = 10f;   // tower 앞 정지 거리
    public float towerFireCooldown = 1.5f;
    public float chaseFireCooldown = 2f;
    public float hysteresisRatio = 0.8f;   // playerPriorityRatio * hysteresisRatio < 1 (타겟 변경 떨림 방지)
    public float playerPriorityRatio = 1.1f;

    // 보스 스킬
    [Header("Skill")]
    public float skillCooldown = 8f;
    public float castDelay = 1f;
    public float skillDamageMultiplier = 1.5f;
    public float dashPower = 15f;
    public float dashDuration = 0.5f;
    public float barkRadius = 10f;
    public float barkAngle = 90f;
    public float barkEffectDuration = 0.3f;
    public float bubbleDuration = 3f;
    public float bubbleRadius = 5f;
    public float bubbleTick = 0.5f;
    public float bubbleFirstTickDelay = 0.5f;
}

[CreateAssetMenu(fileName = "BalanceData", menuName = "ScriptableObject/BalanceData")]
public class BalanceData : ScriptableObject
{
    // 기본 능력치
    [Header("Base Stats")]
    public ClassStats birdStats;
    public ClassStats dogStats;
    public ClassStats fishStats;

    // 등급 확률
    [Header("Grade Rate")]
    public float gradeS = 10f;
    public float gradeA = 30f;
    public float gradeB = 40f;
    // 나머지 C (20%)

    // 등급 계수
    [Header("Grade Multiplier")]
    public float gradeMultiplierS = 1.5f;
    public float gradeMultiplierA = 1.2f;
    public float gradeMultiplierB = 1.0f;
    public float gradeMultiplierC = 0.8f;

    // 스케치 보너스
    [Header("Sketch Bonus")]
    public float strokeHpFactor = 5f;
    public float timeSpeedFactor = 0.2f;
    public float maxHpBonus = 200f;

    // Projectile
    [Header("Projectile Life Time")]
    public float projectileSpeed = 10f;
    public float projectileDestroyTime = 2f;

    // Skill
    [Header("Skill Stats")]
    public DashSkillStats dashStats;
    public BarkSkillStats barkStats;
    public BubbleSkillStats bubbleStats;
    
    // 경험치
    [Header("Experience")]
    public float expValue = 10f;
    public float maxXP = 100f;

    // 플레이어 레벨업 증가량
    [Header("Player LevelUp")]
    public float playerHpPerLevel = 10f;
    public float playerAttackPerLevel = 1.1f;
    public float xpMultiplier = 1.1f;

    // 적 기본 능력치
    [Header("Enemy Base Stats")]
    public float enemyBaseHp = 50f;
    public float enemyHpPerStage = 15f;
    public float enemyBaseAttack = 5f;
    public float enemyAttackPerStage = 1.1f;
    public float enemyBaseSpeed = 5f;
    public float enemySpeedPerStage = 0.1f;

    // 적 타입 별 능력치
    [Header("Enemy Type Multiplier")]
    public float runnerHpMultiplier = 0.8f;
    public float runnerAttackMultiplier = 0.8f;
    public float runnerSpeedMultiplier = 1.2f;
    public float tankerHpMultiplier = 1.4f;
    public float tankerAttackMultiplier = 1.2f;
    public float tankerSpeedMultiplier = 0.6f;

    // 적 스폰률
    [Header("Enemy Spawn")]
    public float initialSpawnInterval = 3f;
    public float spawnIntervalDecrease = 0.1f;
    public float minSpawnInterval = 0.5f;

    // 보스 능력치
    [Header("Boss")]
    public BossStats bossStats;

    // 타워 기본 능력치
    [Header("Tower Base Stats")]
    public float towerBaseHp = 100f;
    public float towerBaseAttack = 15f;
    public float towerAttackCooldown = 3f;

    // 타워 레벨업 증가량
    [Header("Tower LevelUp")]
    public float towerHpPerLevel = 15f;
    public float towerAttackPerLevel = 1.1f;
    public float towerCooldownReduction = 0.9f; // 공격속도
    public float towerMinCooldown = 0.5f;

    // 타이머
    [Header("Duration")]
    public float sketchDuration = 20f;
    public float stageDuration = 60f;

    // 맵 크기
    [Header("Map Size")]
    public int width = 140;
    public int height = 68;

    // 클래스별 스탯 가져오기
    public ClassStats GetClassStats(string className)
    {
        return className switch
        {
            "Bird" => birdStats,
            "Dog" => dogStats,
            "Fish" => fishStats,
            _ => dogStats
        };
    }

    // 등급 계수 가져오기
    public float GetGradeMultiplier(string grade)
    {
        return grade switch
        {
            "S" => gradeMultiplierS,
            "A" => gradeMultiplierA,
            "B" => gradeMultiplierB,
            "C" => gradeMultiplierC,
            _ => 1.0f
        };
    }

    // 등급 부여 (랜덤)
    public string GetRandomGrade()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < gradeS) return "S";                    // 10% (0 ~ 9.99)
        if (randomValue < gradeS + gradeA) return "A";           // 30% (10 ~ 39.99)
        if (randomValue < gradeS + gradeA + gradeB) return "B";  // 40% (40 ~ 79.99)
        return "C";                                              // 20% (80 ~ 99.99)
    }
}