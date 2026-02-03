using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

//StageManager
public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    //[Header("VR Settings")]
    //[SerializeField] private OVRScreenFade screenFade;

    [Header("Game Status")]
    [SerializeField] private int totalTargets = 0;
    [SerializeField] private int currentHitCount = 0;

    [SerializeField] private string sceneSet;

    private void Awake()
    {
        // シングルトンの型チェックも変更
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        TargetIdentifier[] targets = FindObjectsByType<TargetIdentifier>(FindObjectsSortMode.None);
        totalTargets = targets.Length;
        currentHitCount = 0;
    }

    public void RecordHit(string targetName)
    {
        currentHitCount++;
        // 必要ならデバッグログなど

        if (currentHitCount >= totalTargets)
        {
            StartCoroutine(TransitionToClearScene());
        }
    }

    private IEnumerator TransitionToClearScene()
    {
        //if (screenFade != null)
        //{
        //    screenFade.FadeOut();
        //}
            yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene(sceneSet);
    }
}