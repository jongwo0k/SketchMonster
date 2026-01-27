// Scene이 바뀔 때 데이터 유지
using UnityEngine;
using System.Collections.Generic;

public static class GameSession
{
    // 고정
    public static string SelectedCharacterId;

    // 달라짐
    public static Texture2D OriginalSketch;
    public static List<Texture2D> EnemyTextures = new List<Texture2D>();

    public static void CleanSession()
    {
        SelectedCharacterId = null;

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
        
        Debug.Log("Clear Complete");
    }
}