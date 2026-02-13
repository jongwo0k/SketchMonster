using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject howtoplayPanel;
    [SerializeField] private GameObject recordPanel;

    [Header("Volume")]
    [SerializeField] private Slider volumeSlider;

    void Start()
    {
        if (howtoplayPanel != null) howtoplayPanel.SetActive(false);
        if (recordPanel != null) recordPanel.SetActive(false);
        volumeSlider.value = SoundManager.Instance.GetVolume();
        DataManager.InitStorage();
    }

    public void OnNewGameButtonClicked()
    {
        // New Game 버튼 클릭 -> 그림판으로 이동, 캐릭터 생성 파이프라인 ("StartScene" 씬)을 로드
        SceneManager.LoadScene("StartScene");
    }

    
    public void OnRecordButtonClicked()
    {
        recordPanel.SetActive(true);
    }

    /*
    public void OnHowToPlayPanelButtonClicked()
    {
        howtoplayPanel.SetActive(true);
    }
    */

    public void OnExitButtonClicked()
    {
        // Exit 버튼 클릭 -> 에디터에서는 동작X, 빌드된 게임에서는 종료
        Application.Quit();
    }

    
    public void CloseButtonClicked()
    {
        // howtoplayPanel.SetActive(false);
        recordPanel.SetActive(false);
    }
    

    public void VolumeChange(float value)
    {
        SoundManager.Instance.SetVolume(value);
    }
}