using System.IO;
using UnityEngine;

public static class DataManager
{
    private const int CharacterSize = 2;

    // 캐릭터 저장
    public static void SaveCharacter(CharacterData data, Texture2D image)
    {
        // PNG 배열, JSON 문자열로 변환
        byte[] pngData = image.EncodeToPNG();
        string jsonData = JsonUtility.ToJson(data, true); // prettyPrint

        // PNG, JSON 파일 경로 생성 (Application.persistentDataPath -> 운영체제가 허용한 저장 경로 AppData)
        string pngPath = Path.Combine(Application.persistentDataPath, data.imagePath);
        string jsonPath = Path.Combine(Application.persistentDataPath, $"{data.characterId}.json");

        // 파일 저장, 경로 표시
        File.WriteAllBytes(pngPath, pngData);
        Debug.Log($"Character image saved to: {pngPath}");
        File.WriteAllText(jsonPath, jsonData);
        Debug.Log($"Character stats saved to: {jsonPath}");
    }

    // 저장된 캐릭터 불러오기
    public static (CharacterData data, Sprite sprite) LoadCharacter(string characterId)
    {
        // JSON 파일에서 캐릭터 정보 불러옴
        string jsonPath = Path.Combine(Application.persistentDataPath, $"{characterId}.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"CharacterData not found: {jsonPath}");
            return (null, null);
        }
        string jsonData = File.ReadAllText(jsonPath);
        CharacterData data = JsonUtility.FromJson<CharacterData>(jsonData);

        // PNG 파일에서 캐릭터 이미지 불러옴
        string pngPath = Path.Combine(Application.persistentDataPath, data.imagePath);
        if (!File.Exists(pngPath))
        {
            Debug.LogError($"CharacterImage not found: {pngPath}");
            return (data, null);
        }
        byte[] pngData = File.ReadAllBytes(pngPath);
        Texture2D texture = new Texture2D(CharacterSize, CharacterSize);
        texture.LoadImage(pngData);

        // Texture를 Sprite로 변환, 캐릭터 반환
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f); // 스프라이트의 중앙
        Sprite sprite = Sprite.Create(texture, rect, pivot);
        return (data, sprite);
    }
}