using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private Dictionary<string, int> hitCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        // ★ Rキーが押されたらリセットを実行
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetAllTargets();
        }
    }

    void ResetAllTargets()
    {
        // シーン内のすべての TargetIdentifier を探してリセット
        TargetIdentifier[] targets = FindObjectsOfType<TargetIdentifier>();
        foreach (TargetIdentifier t in targets)
        {
            t.ResetTarget();
        }

        // 必要であればスコア（hitCounts）もリセット
        hitCounts.Clear();
        Debug.Log("すべての的をリセットしました");
    }

    public void RecordHit(string targetName)
    {
        if (hitCounts.ContainsKey(targetName)) hitCounts[targetName]++;
        else hitCounts.Add(targetName, 1);

        Debug.Log($"{targetName} ヒット! 合計: {hitCounts[targetName]}");
    }
}