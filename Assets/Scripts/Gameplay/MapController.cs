using UnityEngine;

public class MapController : MonoBehaviour

{
    public static MapController Instance { get; private set; }

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private EnemySpawner enemySpawner;

    [SerializeField] private CameraController mainCamera;

    // Stage 관리
    [Header("Stage")]
    public float remainTime;
    public float stageDuration = 60f;
    public int stageLevel = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Player 생성
        GameObject playerObject = playerSpawner.SpawnPlayer();

        // 생성된 Player에 카메라 연결
        mainCamera.target = playerObject.transform;

        StartNewStage();
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

            UI_Manager.Instance.UpdateStageSlider(value);
        }
        else
        {
            UI_Manager.Instance.StageIsClear();
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

        UI_Manager.Instance.UpdateStagePanel(stageLevel);
        UI_Manager.Instance.UpdateStageSlider(0f);
        enemySpawner.StartSpawnEnemy(stageLevel);
    }

    public void StartNextStage()
    {
        stageLevel++;
        StartNewStage();
    }

    // 이전 스테이지에 남은 enemy, projectile 제거
    private void ClearRemainObjects()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); // List로 변경 or 다른 함수 object pooling
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject proj in projectiles)
        {
            Destroy(proj);
        }

        GameObject[] orbs = GameObject.FindGameObjectsWithTag("ExperienceOrb");
        foreach (GameObject orb in orbs)
        {
            Destroy(orb);
        }
    }
}