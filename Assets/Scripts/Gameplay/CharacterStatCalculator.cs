using UnityEngine;

public static class CharacterStatCalculator // (완성 후 밸런스를 고려해 계수 수정 필요)
{
    // 추가 계수 (스케치 정보) Max 제한?
    private const float StrokeHpFactor = 5f;
    private const float TimeSpeedFactor = 0.2f;

    // 능력치 설정
    public static CharacterData Calculate(string className, int strokeCount, int remainTime)
    {
        var tickId = System.DateTime.Now.Ticks.ToString(); // PNG, JSON의 id 통일

        CharacterData data = new()
        {
            characterId = tickId,
            imagePath = $"{tickId}.png",
            className = className,
            grade = GetRandomGrade(),
            level = 1
        };

        // 클래스별 기본 능력치
        float baseHp = 100f, baseAttack = 10f, baseSpeed = 5f;
        switch (className)
        {
            case "Bird":            // 낮은 스펙, 빠름
                baseHp = 80f;
                baseAttack = 8f;
                baseSpeed = 7f;
                break;
            case "Dog":             // 밸런스
                baseHp = 100f;
                baseAttack = 10f;
                baseSpeed = 5f;
                break;
            case "Fish":            // 높은 스펙, 느림
                baseHp = 120f;
                baseAttack = 12f;
                baseSpeed = 3f;
                break;
        }

        // 등급 계수
        float gradeMultiplier = 1.0f;
        switch (data.grade)
        {
            case "S": gradeMultiplier = 1.5f; break;
            case "A": gradeMultiplier = 1.2f; break;
            case "B": gradeMultiplier = 1.0f; break;
            case "C": gradeMultiplier = 0.8f; break;
        }

        // 최종 능력치 계산
        float hpBonus = strokeCount * StrokeHpFactor;
        float speedBonus = remainTime * TimeSpeedFactor;

        data.hp = (baseHp * gradeMultiplier) + hpBonus;
        data.attack = baseAttack * gradeMultiplier;
        data.speed = (baseSpeed * gradeMultiplier) + speedBonus;

        return data;
    }

    // 등급 부여 (랜덤)
    private static string GetRandomGrade()
    {
        float randomValue = Random.Range(0f, 100f);

        if (randomValue < 10f) return "S";  // 10% (0 ~ 9.99)
        if (randomValue < 40f) return "A";  // 30% (10 ~ 39.99)
        if (randomValue < 80f) return "B";  // 40% (40 ~ 79.99)
        return "C";                         // 20% (80 ~ 99.99)
    }
}