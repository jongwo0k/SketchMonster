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
    }

    // GameOver
    public void GameIsOver()
    {
        gameOver.SetActive(true);
        int finalStage = MapController.Instance.stageLevel;
        gameOverText.text = "Stage: " + finalStage;
        Time.timeScale = 0.0001f;
    }

    // NextStage
    public void StageIsClear()
    {
        nextStage.SetActive(true);
        Time.timeScale = 0.0001f;
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
        Time.timeScale = 0.0001f;
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
        SceneManager.LoadScene("Menu");
    }
}