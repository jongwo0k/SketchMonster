using UnityEngine;

public static class CharacterStatCalculator
{

    // 능력치 설정
    public static CharacterData Calculate(string className, int strokeCount, int remainTime)
    {
        var uniqueId = System.Guid.NewGuid().ToString(); // PNG, JSON의 id 통일

        CharacterData data = new()
        {
            characterId = uniqueId,
            imagePath = $"{uniqueId}.png",
            className = className,
            grade = GameConfig.Data.GetRandomGrade(),
            level = 1
        };

        // 클래스별 기본 능력치
        ClassStats baseStats = GameConfig.Data.GetClassStats(className);
        float gradeMultiplier = GameConfig.Data.GetGradeMultiplier(data.grade);

        // 보너스 스탯
        float hpBonus = Mathf.Min(strokeCount * GameConfig.Data.strokeHpFactor, GameConfig.Data.maxHpBonus);
        float speedBonus = remainTime * GameConfig.Data.timeSpeedFactor;

        // 최종 능력치 계산
        data.hp = (baseStats.hp * gradeMultiplier) + hpBonus;
        data.attack = baseStats.attack * gradeMultiplier;
        data.speed = (baseStats.speed * gradeMultiplier) + speedBonus;

        return data;
    }
}