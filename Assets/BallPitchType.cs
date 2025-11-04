using System.Collections.Generic;
using UnityEngine;

public class BallPitchType : MonoBehaviour
{
    public Rigidbody rb;

    // 投球タイプの選択
    public enum PitchType
    {
        Straight,
        Fork,
        Slider // 以前の例のスライダー
    }
    public PitchType currentPitchType = PitchType.Straight; // インスペクターで選択可能

    private List<Vector3> pathPoints = new List<Vector3>();

    public float targetSpeed = 100f;     // 目標とする速度
    public float forceMagnitude = 50f;  // 目標点へ向かう力の強さ
    public float arrivalThreshold = 0.5f; // 目標点に到達したとみなす距離

    private int currentPathIndex = 0;
    private Vector3 initialPosition; // 開始位置を保存

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbodyがアタッチされていません！");
            enabled = false;
            return;
        }

        initialPosition = rb.position; // 現在の配置位置を開始点とする

        // 最初の経路を生成
        GeneratePathForPitchType(currentPitchType);

        // ボールを初期位置に配置してリセット
        ResetBall();
    }
    // キー入力を検出するためにUpdateメソッドを追加
    void Update()
    {
        // 1キーでストレート
        if (Input.GetKeyDown(KeyCode.Alpha1)) // キーボードの1を押す
        {
            Debug.Log("ストレートに切り替え");
            currentPitchType = PitchType.Straight;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
        // 2キーでフォーク
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // キーボードの2を押す
        {
            Debug.Log("フォークに切り替え");
            currentPitchType = PitchType.Fork;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
        // 3キーでスライダー
        else if (Input.GetKeyDown(KeyCode.Alpha3)) // キーボードの3を押す
        {
            Debug.Log("スライダーに切り替え");
            currentPitchType = PitchType.Slider;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
    }


    void FixedUpdate()
    {

        Vector3 currentTargetPosition = pathPoints[currentPathIndex];
        Vector3 directionToTarget = (currentTargetPosition - rb.position).normalized;

        Vector3 desiredVelocity = directionToTarget * targetSpeed;
        Vector3 force = (desiredVelocity - rb.linearVelocity) * forceMagnitude;

        rb.AddForce(force, ForceMode.Force);

        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f);
        }

        if (Vector3.Distance(rb.position, currentTargetPosition) < arrivalThreshold)
        {
            currentPathIndex++;
            // 最終目標点に到達した場合
            if (currentPathIndex >= pathPoints.Count)
            {
                Debug.Log("最終目標点に到達しました！");
                // 最終到達後も、キー入力でリセットされるまで待機
            }
        }
    }

    void OnDrawGizmos()
    {
        if (pathPoints != null && pathPoints.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < pathPoints.Count; i++)
            {
                Gizmos.DrawWireSphere(pathPoints[i], arrivalThreshold);
                if (i < pathPoints.Count - 1)
                {
                    Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                }
            }
        }
    }


    /// <summary>
    /// 選択されたピッチタイプに基づいて経路を生成する
    /// </summary>
    void GeneratePathForPitchType(PitchType type)
    {
        pathPoints.Clear(); // 既存の経路点をクリア
        pathPoints.Add(initialPosition); // 開始点を追加

        switch (type)
        {
            case PitchType.Straight:
                BallStraight();
                break;
            case PitchType.Fork:
                BallFork();
                break;
            case PitchType.Slider:
                BallSlider();
                break;
        }
        // 経路点が開始点のみで終わらないようにチェック
        if (pathPoints.Count <= 1)
        {
            Debug.LogWarning($"選択されたピッチタイプ ({type}) の経路点が少なすぎます。追加の経路点を生成してください。");
            // 少なくとももう1点追加して、動き出せるようにする
            pathPoints.Add(initialPosition + Vector3.forward * 0.1f);
        }
    }

    /// <summary>
    /// ボールを初期状態に戻す
    /// </summary>
    void ResetBall()
    {
        rb.position = initialPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentPathIndex = 0; // 経路インデックスをリセット
        // スクリプトが無効になっている可能性があるので再度有効化
        enabled = true;
        Debug.Log("ボールをリセットし、新しい経路を開始します。");
    }

    /// <summary>
    /// スライダーの場合の関数
    /// </summary>
    void BallSlider()
    {
        // --- ここで経路点を定義します ---
        // 最初の数点は直線的に配置

        pathPoints.Add(initialPosition + new Vector3(0, 0, 10)); // 1: まっすぐ前方へ10m
        pathPoints.Add(initialPosition + new Vector3(0, 0, 40)); // 2:


        pathPoints.Add(initialPosition + new Vector3(-5, 0, 55)); // 3: 
        pathPoints.Add(initialPosition + new Vector3(-10, 0, 75)); // 3: 
        pathPoints.Add(initialPosition + new Vector3(-20, 0, 100)); // 5: 
        pathPoints.Add(initialPosition + new Vector3(-40, 0, 150)); // 5: 
    }
    void BallStraight()
    {
        // --- ここで経路点を定義します ---
        // 最初の数点は直線的に配置
        pathPoints.Add(initialPosition + new Vector3(0, 0, 10)); // 1: 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 50)); // 1: 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 80)); // 1: 
        pathPoints.Add(initialPosition + new Vector3(0, -1, 100)); // 2:

    }
    void BallFork()
    {
        // --- ここで経路点を定義します ---
        // 最初の数点は直線的に配置

        pathPoints.Add(initialPosition + new Vector3(0, 0, 10)); // 1: 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 50)); // 1: 
        pathPoints.Add(initialPosition + new Vector3(0, -30, 200)); // 2:

    }



}