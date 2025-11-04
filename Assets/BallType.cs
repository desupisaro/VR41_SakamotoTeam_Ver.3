using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Input Systemを使用するために必要

public class BallType : MonoBehaviour
{
    [Header("球のRigidbody")]
    public Rigidbody rb;

    public enum PitchType
    {
        Straight,
        Fork,
        Slider
    }
    [Header("球種")]
    public PitchType currentPitchType = PitchType.Straight; // インスペクターで選択可能

    private List<Vector3> pathPoints = new List<Vector3>();

    [Header("--球速設定--")]
    public float targetSpeed = 120f;     // 目標とする速度
    public float forceMagnitude = 25f;  // 目標点へ向かう力の強さ
    public float arrivalThreshold = 2f; // 目標点に到達したとみなす距離

    private int currentPathIndex = 0;
    private Vector3 initialPosition; // 開始位置を保存

    // --- Input System 用の追加部分 (グリップとトリガー) ---
    [Header("グリップボタンとトリガーボタンの割り当て")]
    public InputActionProperty gripButtonAction;    // Inspectorでグリップボタンのアクションを割り当てる
    public InputActionProperty triggerButtonAction; // Inspectorでトリガーボタンのアクションを割り当てる

    // 右投げか左投げを変更する
    public enum ThrowType
    {
        Right,
        Left
    }
    [Header("投げる手の変更")]
    public ThrowType currentThrowType = ThrowType.Right;// インスペクターでも変更可能

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

    void OnEnable()
    {
        // 各アクションを有効化
        gripButtonAction.action.Enable();
        triggerButtonAction.action.Enable();

        // performedイベントにハンドラを登録
        // グリップボタンのみ -> ストレート
        gripButtonAction.action.performed += OnGripPerformed;
        // トリガーボタンのみ -> スライダー
        triggerButtonAction.action.performed += OnTriggerPerformed;

        // フォーク（グリップとトリガーの両方）はUpdateで検出します
    }

    void OnDisable()
    {
        // イベントハンドラの登録解除
        gripButtonAction.action.performed -= OnGripPerformed;
        triggerButtonAction.action.performed -= OnTriggerPerformed;

        // 各アクションを無効化
        gripButtonAction.action.Disable();
        triggerButtonAction.action.Disable();
    }

    // --- Input Systemイベントハンドラ ---
    private void OnGripPerformed(InputAction.CallbackContext context)
    {
        // グリップのみが押されたか、または両方が押されたがトリガーのイベントがまだ来ていないか
        // このイベントはグリップが押された瞬間に発生。
        // フォークの条件（両方押し）はUpdateで最終的に判断するため、ここでは保留し、
        // フォークでなければストレートとして処理。
        // より正確な同時押し検出のために、Updateで処理を集中させる。
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        // トリガーのみが押されたか、または両方が押されたがグリップのイベントがまだ来ていないか
        // このイベントはトリガーが押された瞬間に発生。
        // フォークの条件（両方押し）はUpdateで最終的に判断するため、ここでは保留し、
        // フォークでなければスライダーとして処理。
    }
    // --- ここまで Input Systemイベントハンドラ ---

    // 同時押しを検出するためにUpdateを使用
    void Update()
    {
        // Test用 右投げ左投げ変更
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentThrowType = ThrowType.Right;
            Debug.Log("右投げに変更");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentThrowType |= ThrowType.Left;
            Debug.Log("左投げに変更");
        }

        bool isGripPressed = gripButtonAction.action.IsPressed();
        bool isTriggerPressed = triggerButtonAction.action.IsPressed();

        // フォーク: グリップとトリガーの両方が押された瞬間
        // performedイベントでは「押された瞬間」しか検出できないため、Updateで両方のボタンの状態を監視し、
        // かつ、以前の状態との比較で「同時に押された瞬間」を検出する必要がある。
        // 簡単のために、ここでは「現在両方押されている」状態で、まだ投球タイプが切り替わっていない場合に設定。
        // ただし、これだと「押しっぱなし」で何度も切り替わる可能性がある。
        // 実際に切り替えが発生したかを追跡するフラグが必要。

        // フラグを使って、一度切り替えたらボタンが離れるまで再切り替えしないように。
        if (currentPitchType != PitchType.Fork && isGripPressed && isTriggerPressed)
        {
            // フォークに切り替え（両方押し）
            Debug.Log("フォークに切り替え");
            currentPitchType = PitchType.Fork;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
        else if (currentPitchType != PitchType.Straight && isGripPressed && !isTriggerPressed)
        {
            // ストレートに切り替え（グリップのみ）
            Debug.Log("ストレートに切り替え");
            currentPitchType = PitchType.Straight;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
        else if (currentPitchType != PitchType.Slider && !isGripPressed && isTriggerPressed)
        {
            // スライダーに切り替え（トリガーのみ）
            Debug.Log("スライダーに切り替え");
            currentPitchType = PitchType.Slider;
            GeneratePathForPitchType(currentPitchType);
            ResetBall();
        }
        // ここでは、ボタンが離されたときの処理は特に必要ないが、
        // 厳密に「押された瞬間」のイベントとして扱いたい場合は、
        // グリップ/トリガーそれぞれのperformedイベントで条件分岐を行う方が良い。
    }

    void FixedUpdate()
    {
        if (currentPathIndex >= pathPoints.Count)
        {
            return;
        }

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
            if (currentPathIndex >= pathPoints.Count)
            {
                Debug.Log("最終目標点に到達しました！");
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
                if (Application.isPlaying && i == currentPathIndex)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(pathPoints[i], arrivalThreshold * 1.5f);
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.DrawWireSphere(pathPoints[i], arrivalThreshold);
                }

                if (i < pathPoints.Count - 1)
                {
                    Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                }
            }
        }
    }

    void GeneratePathForPitchType(PitchType type)
    {
        pathPoints.Clear();
        pathPoints.Add(initialPosition);

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
        if (pathPoints.Count <= 1)
        {
            Debug.LogWarning($"選択されたピッチタイプ ({type}) の経路点が少なすぎます。追加の経路点を生成してください。");
            pathPoints.Add(initialPosition + Vector3.forward * 0.1f);
        }
    }

    void ResetBall()
    {
        rb.position = initialPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentPathIndex = 0;
        enabled = true;
        Debug.Log("ボールをリセットし、新しい経路を開始します。");
    }

    void BallSlider()
    {
        // スライダー
        // 右投げ
        if (currentThrowType == ThrowType.Right)
        {
            // 最初の数点は比較的まっすぐ
            pathPoints.Add(initialPosition + new Vector3(0, 0, 5));  // 短い距離で前方へ
            pathPoints.Add(initialPosition + new Vector3(0, 0, 15)); // 更に前方へ

            // ここから徐々に横方向への変化を加える
            pathPoints.Add(initialPosition + new Vector3(-1, 0, 25)); // 少し左に
            pathPoints.Add(initialPosition + new Vector3(-3, 0, 35)); // さらに左に
            pathPoints.Add(initialPosition + new Vector3(-6, 0, 45)); // より左に
            pathPoints.Add(initialPosition + new Vector3(-10, 0, 55)); // はっきりと左に
            pathPoints.Add(initialPosition + new Vector3(-15, 0, 65)); // 
            pathPoints.Add(initialPosition + new Vector3(-20, 0, 75)); // 最終到達点近く
            pathPoints.Add(initialPosition + new Vector3(-25, 0, 85)); // 
            pathPoints.Add(initialPosition + new Vector3(-30, 0, 95)); // 
            pathPoints.Add(initialPosition + new Vector3(-35, 0, 105)); // 最終到達点
        }
        // 左投げ
        else if (currentThrowType == ThrowType.Left)
        {
            // 最初の数点は比較的まっすぐ
            pathPoints.Add(initialPosition + new Vector3(0, 0, 5));
            pathPoints.Add(initialPosition + new Vector3(0, 0, 15));

            // ここから徐々に横方向への変化を加える
            pathPoints.Add(initialPosition + new Vector3(1, 0, 25)); // 少し右に
            pathPoints.Add(initialPosition + new Vector3(3, 0, 35)); // さらに右に
            pathPoints.Add(initialPosition + new Vector3(6, 0, 45)); // より右に
            pathPoints.Add(initialPosition + new Vector3(10, 0, 55)); // はっきりと右に
            pathPoints.Add(initialPosition + new Vector3(15, 0, 65));
            pathPoints.Add(initialPosition + new Vector3(20, 0, 75));
            pathPoints.Add(initialPosition + new Vector3(25, 0, 85));
            pathPoints.Add(initialPosition + new Vector3(30, 0, 95));
            pathPoints.Add(initialPosition + new Vector3(35, 0, 105));
        }
    }
    void BallStraight()
    {
        // --- ここで経路点を定義します ---
        // 最初は直線的に配置
        pathPoints.Add(initialPosition + new Vector3(0, 0, 5));  // 短い距離で前方へ
        pathPoints.Add(initialPosition + new Vector3(0, 0, 15)); // 更に前方へ
        pathPoints.Add(initialPosition + new Vector3(0, 0, 25)); 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 35)); 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 45));
        pathPoints.Add(initialPosition + new Vector3(0, 0, 55)); 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 65)); 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 75)); 
        pathPoints.Add(initialPosition + new Vector3(0, 0, 85)); 
        pathPoints.Add(initialPosition + new Vector3(0, -1, 95)); 
        pathPoints.Add(initialPosition + new Vector3(0, -2, 105)); // 最終到達点
    }
    void BallFork()
    {
        pathPoints.Add(initialPosition + new Vector3(0, 0, 10));
        pathPoints.Add(initialPosition + new Vector3(0, 0, 50));
        pathPoints.Add(initialPosition + new Vector3(0, -30, 200));
    }
}