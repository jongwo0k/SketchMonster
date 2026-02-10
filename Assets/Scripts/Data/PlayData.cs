using System;
using System.Collections.Generic;

[Serializable]
public class PlayData
{
    // 기본 정보
    public string characterId;
    public string className;
    public string grade;
    public string playDate;

    // 최종 결과
    public int maxStage;
    public int level;
}

[System.Serializable]
public class RecordData
{
    public List<PlayData> records = new List<PlayData>();
}