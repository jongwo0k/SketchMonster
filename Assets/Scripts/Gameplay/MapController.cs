using UnityEngine;

public class MapController : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private EnemySpawner enemySpawner;

    [SerializeField] private CameraController mainCamera;

    // Stage 관리
    [Header("Stage")]
    private float stageDuration;
    private float remainTime;
    private int stageLevel = 1;

    void Start()
    {
        // Player 생성
        GameObject playerObject = playerSpawner.SpawnPlayer();

        // 생성된 Player에 카메라 연결
        mainCamera.target = playerObject.transform;

        // 진행도 캐싱
        stageDuration = GameConfig.Data.stageDuration;

        StartNewStage();
    }

    // 이벤트 구독
    private void OnEnable()
    {
        EventManager.OnNextStage += StartNextStage;
    }

    // 이벤트 해제
    private void OnDisable()
    {
        EventManager.OnNextStage -= StartNextStage;
    }

    // Update UI
    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

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

        remainTime = stageDuration;

        EventManager.StagePanel(stageLevel);
        EventManager.StageSlider(0f);
        enemySpawner.StartSpawnEnemy(stageLevel);
    }

    private void StartNextStage()
    {
        stageLevel++;
        StartNewStage();
    }

    // 이전 스테이지에 남은 enemy, projectile 제거
    private void ClearRemainObjects()
    {
        ObjectPoolManager.Instance.ClearObjects();
    }
}