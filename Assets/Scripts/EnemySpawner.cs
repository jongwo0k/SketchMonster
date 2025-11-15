using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Template")]
    [SerializeField] private GameObject enemyTemplate;

    // 생성 빈도
    [Header("Spawn Interval")]
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private float spawnIntervalDecrease = 0.1f;

    private MapGenerator mapGenerator;
    private float currentSpawnInterval;

    public void StartSpawnEnemy(int stageLevel)
    {
        // 맵 정보 불러오기
        mapGenerator = GetComponent<MapGenerator>();

        // 스테이지에 따라 생성 빈도 조정
        currentSpawnInterval = Mathf.Max(0.5f, initialSpawnInterval - ((stageLevel - 1) * spawnIntervalDecrease));

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
        GameObject enemyInstance = Instantiate(enemyTemplate, spawnPos, Quaternion.identity);
        Enemy enemyScript = enemyInstance.GetComponent<Enemy>();

        // 세션에서 외형 불러옴
        Texture2D enemyTexture = GameSession.EnemyTextures[Random.Range(0, GameSession.EnemyTextures.Count)];
        Sprite enemySprite = ConvertTextureToSprite(enemyTexture);

        // Stage에 따라 능력치 상승
        int currentStage = MapController.Instance.stageLevel;
        float hp = 50f + (currentStage * 15f);
        float speed = 5f + (currentStage * 0.1f);

        // 외형, 능력치 부여
        enemyScript.Initialize(enemySprite, hp, speed);

    }

    // 끝에서 랜덤 생성
    private Vector2 GetRandomSpawnPosition()
    {
        RectInt bounds = mapGenerator.MapBounds;
        int side = Random.Range(0, 4); // 1234(상하좌우 순)

        Vector2 spawnPos = Vector2.zero;

        // Wall 타일 바로 앞에서 생성
        if (side == 0)
        {
            spawnPos = new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMax - 2);
        }
        else if (side == 1)
        {
            spawnPos = new Vector2(Random.Range(bounds.xMin, bounds.xMax), bounds.yMin + 2);
        }
        else if (side == 2)
        {
            spawnPos = new Vector2(bounds.xMin + 2, Random.Range(bounds.yMin, bounds.yMax));
        }
        else
        {
            spawnPos = new Vector2(bounds.xMax - 2, Random.Range(bounds.yMin, bounds.yMax));
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
}