using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public void LoadLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void OpenSetting()
    {

    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
