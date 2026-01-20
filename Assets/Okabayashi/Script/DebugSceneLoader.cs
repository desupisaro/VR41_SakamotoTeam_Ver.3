using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugSceneLoader : MonoBehaviour
{
    public void LoadTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void LoadResult()
    {
        SceneManager.LoadScene("ResultScene");
    }
}
