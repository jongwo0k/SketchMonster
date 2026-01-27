using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnNewGameButtonClicked()
    {
        // New Game 버튼 클릭 -> 그림판으로 이동, 캐릭터 생성 파이프라인 ("StartScene" 씬)을 로드
        SceneManager.LoadScene("StartScene");
    }

    /*
    public void OnLoadGameButtonClicked()
    {

    }

    public void OnCharacterButtonClicked()
    {

    }
    */

    public void OnExitButtonClicked()
    {
        // Exit 버튼 클릭 -> 에디터에서는 동작X, 빌드된 게임에서는 종료
        Application.Quit();
    }
}