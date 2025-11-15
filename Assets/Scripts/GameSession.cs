// Scene이 바뀔 때 데이터 유지
using UnityEngine;
using System.Collections.Generic;

public static class GameSession
{
    // 고정
    public static string SelectedCharacterId;

    // 달라짐
    public static Texture2D OriginalSketch;
    public static List<Texture2D> EnemyTextures;
}