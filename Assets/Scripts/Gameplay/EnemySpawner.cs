using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // 생성 빈도
    [Header("Spawn Interval")]
    [SerializeField] private float initialSpawnInterval = 3f;
    [SerializeField] private float spawnIntervalDecrease = 0.1f;

    private MapGenerator mapGenerator;
    private float currentSpawnInterval;

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
        GameObject enemyObject = ObjectPoolManager.Instance.Spawn(PoolType.Enemy, spawnPos, Quaternion.identity);
        Enemy enemyScript = enemyObject.GetComponent<Enemy>();

        // Stage에 따라 능력치 상승
        int currentStage = MapController.Instance.stageLevel;
        float HP = 50f + (currentStage * 15f);
        float attack = 5f + (currentStage * 1.1f);
        float speed = 5f + (currentStage * 0.1f);

        // 외형, 능력치 부여
        Sprite enemySprite = enemySprites[Random.Range(0, enemySprites.Count)];
        enemyScript.Initialize(enemySprite, HP, attack, speed);
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