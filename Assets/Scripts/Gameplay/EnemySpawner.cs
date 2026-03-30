using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // 방향
    private enum MapSide
    {
        Top,
        Bottom,
        Left,
        Right,
    }

    private MapGenerator mapGenerator;
    private float currentSpawnInterval;
    private int currentStage;

    // 스프라이트 미리 저장
    private List<Sprite> enemySprites = new List<Sprite>();

    void Start()
    {
        if (GameSession.EnemyTextures != null)
        {
            foreach (Texture2D texture in GameSession.EnemyTextures)
            {
                // 캐싱
                Sprite unselectSprite = ConvertTextureToSprite(texture);
                enemySprites.Add(unselectSprite);
            }
        }
    }

    public void StartSpawnEnemy(int stageLevel)
    {
        currentStage = stageLevel;

        // 맵 정보 불러오기
        mapGenerator = GetComponent<MapGenerator>();

        // 스테이지에 따라 생성 빈도 조정
        currentSpawnInterval = Mathf.Max(GameConfig.Data.minSpawnInterval, GameConfig.Data.initialSpawnInterval - ((stageLevel - 1) * GameConfig.Data.spawnIntervalDecrease));

        Debug.Log($"[Stage {currentStage}] Spawn Interval: {currentSpawnInterval}");

        // 이전 스테이지 종료, 새로 시작
        StopAllCoroutines();
        StartCoroutine(SpawnLoop());
    }
    
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 간격만큼 대기 후 생성
            yield return new WaitForSeconds(currentSpawnInterval);

            if (Time.timeScale == 0f)
            {
                yield return null;
                continue;
            }

            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        // 생성 위치 설정
        Vector2 spawnPos = GetRandomSpawnPosition();

        // EnemyTemplate 가져옴
        GameObject enemyObject = ObjectPoolManager.Instance.Spawn(PoolType.Enemy, spawnPos, Quaternion.identity);
        Enemy enemyScript = enemyObject.GetComponent<Enemy>();

        // Stage에 따라 능력치 상승
        float HP = GameConfig.Data.enemyBaseHp + (currentStage * GameConfig.Data.enemyHpPerStage);
        float attack = GameConfig.Data.enemyBaseAttack + (currentStage * GameConfig.Data.enemyAttackPerStage);
        float speed = GameConfig.Data.enemyBaseSpeed + (currentStage * GameConfig.Data.enemySpeedPerStage);

        // enemy type 분리 (느리고 강한 적, 빠르고 약한 적)
        int enemyType = Random.Range(0, enemySprites.Count);
        Sprite enemySprite = enemySprites[enemyType];

        float hpMultiplier, attackMultiplier, speedMultiplier;
        switch (enemyType)
        {
            case 0: // runner type
                hpMultiplier = GameConfig.Data.runnerHpMultiplier;
                attackMultiplier = GameConfig.Data.runnerAttackMultiplier;
                speedMultiplier = GameConfig.Data.runnerSpeedMultiplier;
                break;
            case 1: // tanker type
                hpMultiplier = GameConfig.Data.tankerHpMultiplier;
                attackMultiplier = GameConfig.Data.tankerAttackMultiplier;
                speedMultiplier = GameConfig.Data.tankerSpeedMultiplier;
                break;
            default:
                hpMultiplier = 1f;
                attackMultiplier = 1f;
                speedMultiplier = 1f;
                break;
        }

        // 외형, 능력치 부여
        enemyScript.Initialize(enemySprite, HP * hpMultiplier, attack * attackMultiplier, speed * speedMultiplier);
    }

    // 끝에서 랜덤 생성
    private Vector2 GetRandomSpawnPosition()
    {
        RectInt bounds = mapGenerator.MapBounds;
        MapSide side = (MapSide)Random.Range(0, 4); // 형 변환

        Vector2 spawnPos = Vector2.zero;

        // Wall 타일 바로 앞에서 생성
        switch (side)
        {
            case MapSide.Top:
                spawnPos = new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMax - 2);
                break;
            case MapSide.Bottom:
                spawnPos = new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMin + 2);
                break;
            case MapSide.Left:
                spawnPos = new Vector2(bounds.xMin + 2, Random.Range(bounds.yMin, bounds.yMax));
                break;
            case MapSide.Right:
                spawnPos = new Vector2(bounds.xMax - 2, Random.Range(bounds.yMin, bounds.yMax));
                break;
        }

        return spawnPos;
    }

    // Textrue -> Sprite
    private Sprite ConvertTextureToSprite(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        float pixelsPerUnit = Mathf.Max(texture.width, texture.height);

        Sprite sprite = Sprite.Create(texture, rect, pivot, pixelsPerUnit);
        return sprite;
    }

    void OnDestroy()
    {
        foreach (var sprite in enemySprites)
        {
            if (sprite != null)
                Destroy(sprite);
        }
        enemySprites.Clear();
    }
}