using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DataManager
{
    private const string GAME_RESULT = "GameResult.json";
    private const int MAX_SAVE_COUNT = 10;

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
        Texture2D texture = new Texture2D(2, 2); // 임시 값 2
        texture.LoadImage(pngData);              // 실제 값

        // Texture를 Sprite로 변환, 캐릭터 반환
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f); // 스프라이트의 중앙
        Sprite sprite = Sprite.Create(texture, rect, pivot);
        return (data, sprite);
    }

    public static RecordData LoadRecordData()
    {
        string path = Path.Combine(Application.persistentDataPath, GAME_RESULT);
        if (!File.Exists(path))
        {
            return new RecordData();
        }
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<RecordData>(json);
    }

    public static void SaveGameResult(CharacterData data, int finalStage, int finalLevel)
    {
        RecordData rd = LoadRecordData();

        // 새로운 기록 등록
        PlayData newRecord = new PlayData
        {
            characterId = data.characterId,
            className = data.className,
            grade = data.grade,
            playDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            maxStage = finalStage,
            level = finalLevel
        };
        rd.records.Add(newRecord);

        // 저장파일 정리
        ManageStorage(rd);

        // 파일로 저장
        string json = JsonUtility.ToJson(rd, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, GAME_RESULT), json);
        Debug.Log($"Game Record Saved");
    }

    private static void ManageStorage(RecordData rd)
    {
        // 정렬 순서
        rd.records.Sort((a, b) =>
        {
            if (a.maxStage != b.maxStage) return b.maxStage.CompareTo(a.maxStage); // 높은 스테이지 달성
            if (a.level != b.level) return b.level.CompareTo(a.level);             // 높은 레벨 달성
            return b.playDate.CompareTo(a.playDate);                               // 최근 플레이
        });

        // 10개 까지만 저장
        if (rd.records.Count > MAX_SAVE_COUNT)
        {
            List<PlayData> removeTargets = rd.records.GetRange(MAX_SAVE_COUNT, rd.records.Count - MAX_SAVE_COUNT);

            foreach (var target in removeTargets)
            {
                DeleteCharacterData(target.characterId);
            }
            rd.records = rd.records.GetRange(0, MAX_SAVE_COUNT);
        }
    }

    // Top10 외에 남아있는 파일 정리
    public static void InitStorage()
    {
        RecordData rd = LoadRecordData();
        HashSet<string> validIds = new HashSet<string>();
        if (rd != null)
        {
            foreach (var record in rd.records)
            {
                validIds.Add(record.characterId);
            }
        }

        string[] jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");
        foreach (string jsonFile in jsonFiles)
        {
            string fullFileName = Path.GetFileName(jsonFile);

            if (fullFileName == GAME_RESULT) continue;

            string fileId = Path.GetFileNameWithoutExtension(jsonFile);

            if (!validIds.Contains(fileId))
            {
                DeleteCharacterData(fileId);
            }
        }
    }

    private static void DeleteCharacterData(string characterId)
    {
        string pngPath = Path.Combine(Application.persistentDataPath, $"{characterId}.png");
        string jsonPath = Path.Combine(Application.persistentDataPath, $"{characterId}.json");

        if (File.Exists(pngPath)) File.Delete(pngPath);
        if (File.Exists(jsonPath)) File.Delete(jsonPath);
        Debug.Log($"Game Record Deleted");
    }
}