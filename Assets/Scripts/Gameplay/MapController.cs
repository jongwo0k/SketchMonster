using UnityEngine;
using System.Collections;

public class MapController : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private BossSpawner bossSpawner;

    [SerializeField] private CameraController mainCamera;

    // Stage 관리
    [Header("Stage")]
    private float stageDuration;
    private float remainTime;
    private int stageLevel = 1;
    private bool isBossStage = false;

#if UNITY_EDITOR
    [Header("StageTest")]
    [SerializeField] private int startStage = 1;
#endif

    void Start()
    {
        // Player 생성
        GameObject playerObject = playerSpawner.SpawnPlayer();

        // 생성된 Player에 카메라 연결
        mainCamera.target = playerObject.transform;

        // 진행도 캐싱
        stageDuration = GameConfig.Data.stageDuration;

#if UNITY_EDITOR
        if (startStage > 1) stageLevel = startStage;
#endif

        StartNewStage();
    }

    // 이벤트 구독
    private void OnEnable()
    {
        EventManager.OnNextStage += StartNextStage;
        EventManager.OnBossDefeated += BossIsDefeated;
    }

    // 이벤트 해제
    private void OnDisable()
    {
        EventManager.OnNextStage -= StartNextStage;
        EventManager.OnBossDefeated -= BossIsDefeated;
    }

    // Update UI
    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        if (isBossStage) return; // 타이머 대신 Boss HP

        if (remainTime > 0)
        {
            remainTime -= Time.deltaTime;
            float value = (stageDuration - remainTime) / stageDuration;

            EventManager.StageSlider(value);
        }
        else
        {
            EventManager.StageClear();
        }
    }

    // 카메라 경계 설정
    private void SetCameraBoundary()
    {
        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        RectInt mapBounds = mapGenerator.MapBounds;
        float minX = mapBounds.xMin + cameraWidth;
        float maxX = mapBounds.xMax - cameraWidth;
        float minY = mapBounds.yMin + cameraHeight;
        float maxY = mapBounds.yMax - cameraHeight;

        mainCamera.MapRange(minX, maxX, minY, maxY);
    }

    private void StartNewStage()
    {
        mapGenerator.ClearMap();
        ClearRemainObjects();
        mapGenerator.GenerateMap();

        SetCameraBoundary();
        EventManager.StagePanel(stageLevel);

        BossStats bossStats = GameConfig.Data.bossStats;

        isBossStage = (stageLevel % bossStats.bossStageInterval == 0);
        if (isBossStage)
        {
            EventManager.StageSlider(1f); // 반대
            enemySpawner.StopSpawning();  // 보스전은 보스만
            StartCoroutine(SpawnBossDelayed());
        }
        else
        {
            remainTime = stageDuration;
            EventManager.StageSlider(0f);
            enemySpawner.StartSpawnEnemy(stageLevel);
        }
    }

    private void StartNextStage()
    {
        stageLevel++;
        StartNewStage();
    }

    // 보스 처치 = Clear
    private void BossIsDefeated()
    {
        EventManager.StageClear();
    }

    // 이전 스테이지에 남은 enemy, projectile 제거
    private void ClearRemainObjects()
    {
        ObjectPoolManager.Instance.ClearObjects();
    }

    private IEnumerator SpawnBossDelayed()
    {
        yield return new WaitForSeconds(GameConfig.Data.bossStats.spawnDelay);
        bossSpawner.SpawnBoss(stageLevel);
    }
}