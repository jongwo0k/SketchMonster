using UnityEngine;

public class MapController : MonoBehaviour

{
    public static MapController Instance { get; private set; }

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private PlayerSpawner playerSpawner;
    // [SerializeField] private EnemySpawner enemySpawner;

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
        // Map, Player 鉴辑肺 积己
        mapGenerator.GenerateMap();
        playerSpawner.SpawnPlayer();

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