using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager Instance { get; private set; }

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

    private void Start()
    {
        Time.timeScale = 1f;
        volumeSlider.value = SoundManager.Instance.GetVolume();
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
        SceneManager.LoadScene("Menu");
    }

    public void ExitGame_Button()
    {
        Application.Quit();
    }

    // GameOver
    public void GameIsOver()
    {
        gameOver.SetActive(true);
        int finalStage = MapController.Instance.stageLevel;
        gameOverText.text = "Stage: " + finalStage;

        var (data, _) = DataManager.LoadCharacter(GameSession.SelectedCharacterId);
        if(data != null)
        {
            int finalLevel = PlayerController.Instance.level;
            DataManager.SaveGameResult(data, finalStage, finalLevel);
        }
        
        SoundManager.Instance.PlayGameOver();
        Time.timeScale = 0f;
    }

    // NextStage
    public void StageIsClear()
    {
        nextStage.SetActive(true);
        SoundManager.Instance.PlayStageClear();
        Time.timeScale = 0f;
    }

    public void Next_Button()
    {
        Time.timeScale = 1f;
        nextStage.SetActive(false);
        MapController.Instance.StartNextStage();
    }

    // LevelUp
    public void LevelUP()
    {
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
    public void UpdateStagePanel(int stageLevel)
    {
        stageText.text = "Stage: " + stageLevel;
    }

    public void UpdateStageSlider(float value)
    {
        stageSlider.value = value;
    }

    // 재시작
    public void Retry_Button()
    {
        Time.timeScale = 1f;
        gameOver.SetActive(false);
        GameSession.CleanSession();
        SceneManager.LoadScene("Menu");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}