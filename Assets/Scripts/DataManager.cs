using UnityEngine;
using System.IO;

public static class DataManager
{
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

    // 다시 불러오기
}