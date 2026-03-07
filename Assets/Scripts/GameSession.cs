// Scene이 바뀔 때 데이터 유지
using UnityEngine;
using System.Collections.Generic;

public static class GameSession
{
    // 고정
    public static string SelectedCharacterId;

#if UNITY_WEBGL // Web 에선 파일로 저장 X
    public static CharacterData SelectedCharacterData;
    public static Sprite SelectedCharacterSprite;
#endif

    // 달라짐
    public static Texture2D OriginalSketch;
    public static List<Texture2D> EnemyTextures = new List<Texture2D>();

    public static void CleanSession()
    {
        SelectedCharacterId = null;

#if UNITY_WEBGL
        SelectedCharacterData = null;
        if (SelectedCharacterSprite != null)
        {
            if (SelectedCharacterSprite.texture != null)
            {
                Object.Destroy(SelectedCharacterSprite.texture);
            }
            Object.Destroy(SelectedCharacterSprite);
            SelectedCharacterSprite = null;
        }
#endif

        // 원본 스케치
        if (OriginalSketch != null)
        {
            Object.Destroy(OriginalSketch);
            OriginalSketch = null;
        }

        // Enemy
        if (EnemyTextures != null)
        {
            foreach (var tex in EnemyTextures)
            {
                if (tex != null) Object.Destroy(tex);
            }
            EnemyTextures.Clear();
        }

        // Event
        EventManager.ClearAllEvents();

        Debug.Log("Clear Complete");
    }
}