using UnityEngine;

public enum BossClass
{
    Bird,
    Dog,
    Fish
}

[System.Serializable]
public struct BossTemplate
{
    public Sprite sprite;
    public BossClass bossClass;
}

public class BossSpawner : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private BossTemplate[] bossTemplates;

    [Header("Spawn")]
    [SerializeField] private EnemySpawner enemySpawner;

    public void SpawnBoss(int stageLevel)
    {
        Sprite bossSprite;
        string bossClassName;
        bool runtimeSprite = false;

#if UNITY_WEBGL
        // Web에선 record 없음, template 이미지 사용
        SelectTemplate(out bossSprite, out bossClassName);
#else
        if (!TrySelectFromRecord(stageLevel, out bossSprite, out bossClassName, out runtimeSprite)) // record 없는 초회차
        {
            SelectTemplate(out bossSprite, out bossClassName);
        }
#endif

        // 능력치 (임시)
        float hp = 500f + stageLevel * 100f;
        float attack = 15f + stageLevel * 2f;
        float speed = 3f;

        // 생성 위치
        Vector3 spawnPos = enemySpawner.GetRandomSpawnPosition();

        GameObject bossObject = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        Boss boss = bossObject.GetComponent<Boss>();
        boss.InitializeBoss(bossSprite, hp, attack, speed, runtimeSprite);

        Debug.Log($"[Boss] Stage {stageLevel} - Class: {bossClassName}, HP: {hp}, Sprite: {(runtimeSprite ? "record" : "template")}");
    }

    private void SelectTemplate(out Sprite sprite, out string className)
    {
        BossTemplate t = bossTemplates[Random.Range(0, bossTemplates.Length)];
        sprite = t.sprite;
        className = t.bossClass.ToString();
    }

#if !UNITY_WEBGL
    // record 아래서부터 보스로 등장
    private bool TrySelectFromRecord(int stageLevel, out Sprite sprite, out string className, out bool runtimeSprite)
    {
        sprite = null;
        className = null;
        runtimeSprite = false;

        RecordData rd = DataManager.LoadRecordData();
        if (rd == null || rd.records.Count == 0) return false;

        int bossIndex = stageLevel / 5;
        int rankFromBottom = (bossIndex - 1) % rd.records.Count;
        PlayData pick = rd.records[rd.records.Count - 1 - rankFromBottom];

        Debug.Log($"[Boss] record - {rankFromBottom + 1} (characterId: {pick.characterId})");

        var (data, loadedSprite) = DataManager.LoadCharacter(pick.characterId);
        if (data == null || loadedSprite == null) return false;

        sprite = loadedSprite;
        className = data.className;
        runtimeSprite = true;
        return true;
    }
#endif

#if UNITY_EDITOR
    [ContextMenu("Spawn Boss (Test)")]
    private void TestSpawn() => SpawnBoss(5);
#endif
}