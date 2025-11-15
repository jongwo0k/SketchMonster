using UnityEngine;

public class MapController : MonoBehaviour

{
    public static MapController Instance { get; private set; }

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;
    // [SerializeField] private EnemySpawner enemySpawner;

    [SerializeField] private CameraController mainCamera;

    // Stage 包府
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
        // Player 积己
        GameObject playerObject = playerSpawner.SpawnPlayer();

        // 积己等 Player俊 墨皋扼 楷搬
        mainCamera.target = playerObject.transform;

        float cameraHeight = Camera.main.orthographicSize;
        float cameraWidth = cameraHeight * Camera.main.aspect;

        RectInt mapBounds = mapGenerator.MapBounds;
        float minX = mapBounds.xMin + cameraWidth;
        float maxX = mapBounds.xMax - cameraWidth;
        float minY = mapBounds.yMin + cameraHeight;
        float maxY = mapBounds.yMax - cameraHeight;

        mainCamera.MapRange(minX, maxX, minY, maxY);

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

    private void StartNewStage()
    {
        mapGenerator.ClearMap();
        mapGenerator.GenerateMap();

        remainTime = stageDuration;

        UI_Manager.Instance.UpdateStagePanel(stageLevel);
        UI_Manager.Instance.UpdateStageSlider(0f);
    }

    public void StartNextStage()
    {
        stageLevel++;
        StartNewStage();
    }
}