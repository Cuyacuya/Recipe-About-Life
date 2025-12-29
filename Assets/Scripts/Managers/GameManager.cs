using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public void LoadLobbyScene()
    {
        Debug.Log("[GameManager] 시작 버튼 클릭됨 - LobbyScene 로드");
        SceneManager.LoadScene("LobbyScene");
    }

    public void OpenSetting()
    {
        Debug.Log("[GameManager] 설정 버튼 클릭됨");
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] 종료 버튼 클릭됨 - 게임 종료");
        Application.Quit();
    }
}
