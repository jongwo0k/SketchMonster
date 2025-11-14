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
        Time.timeScale = 0f;
    }

    // NextStage
    public void StageIsClear()
    {
        nextStage.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Next_Button()
    {
        nextStage.SetActive(false);
        Time.timeScale = 1f;
        MapController.Instance.StartNextStage();
    }

    // LevelUp
    public void LevelUP()
    {
        levelUp.SetActive(true);
        Time.timeScale = 0f;
    }

    public void LevelUP_Button()
    {
        levelUp.SetActive(false);
        Time.timeScale = 1f;
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
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }
}