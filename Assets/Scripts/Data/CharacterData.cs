using System;

[Serializable] // JSON 변환
public class CharacterData
{
    // 기본 정보
    public string characterId;   // 고유 ID: "character_1234"  -> 이름 지정?
    public string imagePath;     // 이미지 파일 경로: "character_1234.png"
    public string className;     // "Bird", "Dog", "Fish"
    public string grade;         // S(10) A(30) B(40) C(20)
    // public string characterName 직접 지정?

    // 계수 적용된 최종 능력치 (class + grade + sketch)
    public float hp;
    public float attack;
    public float speed;

    // Level, Skill
    public int level;
    // public List<Skill> skills; // class별 고유 스킬 추가
}