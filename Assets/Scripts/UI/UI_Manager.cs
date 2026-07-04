using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    // UI Panel 관리
    [Header("Stage Panels")]
    [SerializeField] private Slider stageSlider;
    [SerializeField] private TextMeshProUGUI stageText;

    // 결과 창
    [Header("Canvas")]
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject levelUp;
    [SerializeField] private GameObject nextStage;
    [SerializeField] private TextMeshProUGUI gameOverText;

    // 일시정지
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Slider volumeSlider;
    private bool isPaused = false;

    // 스테이지 관리
    private int finalStage;
    private int finalLevel = 1;

    // 마우스 커서 관리
    private float cursorDelay = 2f;
    private float cursorTimer;
    private Vector3 lastMousePosition;

    private void Start()
    {
        Time.timeScale = 1f;
        volumeSlider.value = SoundManager.Instance.GetVolume();
        lastMousePosition = Input.mousePosition;
        cursorTimer = 0f;
        Cursor.visible = false;
    }

    // 이벤트 구독
    private void OnEnable()
    {
        EventManager.OnGameOver += GameIsOver;
        EventManager.OnStageClear += StageIsClear;
        EventManager.OnLevelUp += LevelUP;
        EventManager.OnStagePanel += UpdateStagePanel;
        EventManager.OnStageSlider += UpdateStageSlider;
    }

    // 이벤트 해제
    private void OnDisable()
    {
        EventManager.OnGameOver -= GameIsOver;
        EventManager.OnStageClear -= StageIsClear;
        EventManager.OnLevelUp -= LevelUP;
        EventManager.OnStagePanel -= UpdateStagePanel;
        EventManager.OnStageSlider -= UpdateStageSlider;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ESC
        {
            // 다른 창이 열려있는 경우 제외
            if (!gameOver.activeSelf && !levelUp.activeSelf && !nextStage.activeSelf)
            {
                Pause();
            }
        }

        CursorVisibility();
    }

    // 마우스 커서 관리
    private void CursorVisibility()
    {
        bool isActiveUI = isPaused || gameOver.activeSelf || levelUp.activeSelf || nextStage.activeSelf;

        if (isActiveUI)
        {
            Cursor.visible = true;
            lastMousePosition = Input.mousePosition;
            cursorTimer = 0f;
        }
        else
        {
            if (Input.mousePosition != lastMousePosition)
            {
                Cursor.visible = true;
                cursorTimer = cursorDelay;
                lastMousePosition = Input.mousePosition;
            }

            if (cursorTimer > 0f)
            {
                cursorTimer -= Time.unscaledDeltaTime;
            }

            if (cursorTimer <= 0f)
            {
                Cursor.visible = false;
            }
        }
    }

    // 일시 정지
    public void Pause()
    {
        isPaused = !isPaused; // Toggle

        if (isPaused)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            pauseMenu.SetActive(false);
        }
    }

    // 볼륨 조절
    public void VolumeChange(float value)
    {
        SoundManager.Instance.SetVolume(value);
    }

    // 종료
    public void ExitMenu_Button()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        GameSession.CleanSession();
        SceneManager.LoadScene(ConstString.SCENE_MENU);
    }

    public void ExitGame_Button()
    {
#if !UNITY_WEBGL
        Application.Quit();
#endif
    }

    // GameOver
    private void GameIsOver()
    {
        if (gameOver.activeSelf) return;
        levelUp.SetActive(false);
        nextStage.SetActive(false);

        gameOver.SetActive(true);
        gameOverText.text = "Stage: " + finalStage;

#if !UNITY_WEBGL // Web 에선 record X
        var (data, _) = DataManager.LoadCharacter(GameSession.SelectedCharacterId);
        if (data != null)
        {
            DataManager.SaveGameResult(data, finalStage, finalLevel);
        }
#endif

        SoundManager.Instance.PlayGameOver();
        Time.timeScale = 0f;
    }

    // NextStage
    private void StageIsClear()
    {
        nextStage.SetActive(true);
        SoundManager.Instance.PlayStageClear();
        Time.timeScale = 0f;
    }

    public void Next_Button()
    {
        Time.timeScale = 1f;
        nextStage.SetActive(false);
        EventManager.NextStage();
    }

    // LevelUp
    private void LevelUP(int level)
    {
        if (gameOver.activeSelf) return;

        finalLevel = level;
        levelUp.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Player_LevelUP_Button()
    {
        PlayerController.Instance.PlayerLevelUP();
        Time.timeScale = 1f;
        levelUp.SetActive(false);
    }

    public void Tower_LevelUP_Button()
    {
        MainTower.Instance.TowerLevelUP();
        Time.timeScale = 1f;
        levelUp.SetActive(false);
    }

    public void Recover_HP_Button()
    {
        PlayerController.Instance.RecoverHP();
        MainTower.Instance.RecoverHP();
        Time.timeScale = 1f;
        levelUp.SetActive(false);
    }

    // Update UI Panels
    private void UpdateStagePanel(int stageLevel)
    {
        finalStage = stageLevel;
        stageText.text = "Stage: " + stageLevel;
    }

    private void UpdateStageSlider(float value)
    {
        stageSlider.value = value;
    }

    // 재시작
    public void Retry_Button()
    {
        Time.timeScale = 1f;
        gameOver.SetActive(false);
        GameSession.CleanSession();
        SceneManager.LoadScene(ConstString.SCENE_MENU);
    }
}