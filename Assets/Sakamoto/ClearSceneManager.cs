using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearSceneManager : MonoBehaviour
{
    // 戻りたいシーン名（最初のゲーム画面など）
    public string nextSceneName = "GameScene";

    void Update()
    {
        // QuestコントローラーのAボタン、またはキーボードのスペースキー
        if (OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}