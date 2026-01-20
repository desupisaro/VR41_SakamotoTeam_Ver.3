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

    // --- Input System 用の追加部分 ---
    [Header("グリップボタンとトリガーボタンの割り当て")]
    public InputActionProperty gripButtonAction;    // Inspectorでグリップボタンのアクションを割り当てる
    public InputActionProperty triggerButtonAction; // Inspectorでトリガーボタンのアクションを割り当てる


    [Header("決定（発射）ボタンの割り当て")]
    public InputActionProperty launchButtonAction; // Inspectorで決定ボタンのアクションを割り当てる


    [Header("リセットボタン割り当て")]
    public InputActionProperty resetButton;// Inspectorでリセットボタンを割り当てる


    [Header("投げ手の変更")]
    public InputActionProperty leftButtonAction;
    public InputActionProperty rightButtonAction;



    private bool isBallLaunched = false; // ボールが発射されたかどうかを追跡するフラグ


    // 右投げか左投げを変更する
    public enum ThrowType
    {
        Right,
        Left
    }
    [Header("現在の投げ手")]
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
        launchButtonAction.action.Enable();

        resetButton.action.Enable();
        leftButtonAction.action.Enable();
        rightButtonAction.action.Enable();

        // performedイベントにハンドラを登録
        // グリップボタンのみ -> ストレート
        gripButtonAction.action.performed += OnGripPerformed;
        // トリガーボタンのみ -> スライダー
        triggerButtonAction.action.performed += OnTriggerPerformed;
        // 決定ボタンのイベントを登録
        launchButtonAction.action.performed += OnLaunchPerformed;
        // フォーク（グリップとトリガーの両方）はUpdateで検出します

        // リセット
        resetButton.action.performed += OnReset;
        leftButtonAction.action.performed += OnLeftThrow;
        rightButtonAction.action.performed += OnRightThrow;
        
    }

    void OnDisable()
    {
        // イベントハンドラの登録解除
        gripButtonAction.action.performed -= OnGripPerformed;
        triggerButtonAction.action.performed -= OnTriggerPerformed;
        launchButtonAction.action.performed -= OnLaunchPerformed; // 決定ボタンのイベントを解除
        resetButton.action.performed -= OnReset;
        leftButtonAction.action.performed -= OnLeftThrow;
        rightButtonAction.action.performed -= OnRightThrow;


        // 各アクションを無効化
        gripButtonAction.action.Disable();
        triggerButtonAction.action.Disable();
        launchButtonAction.action.Disable();
        resetButton.action.Disable();
        leftButtonAction.action.Disable();
        rightButtonAction.action.Disable();
    }

    // --- Input Systemイベントハンドラ ---
    private void OnGripPerformed(InputAction.CallbackContext context)
    {
        // このハンドラは球種切り替えに使うため、ここでは何もしません。
        // 実際の切り替え処理はUpdateで行われます。
    }

    private void OnTriggerPerformed(InputAction.CallbackContext context)
    {
        // このハンドラは球種切り替えに使うため、ここでは何もしません。
        // 実際の切り替え処理はUpdateで行われます。
    }

    // 決定ボタンが押されたときに呼び出されるメソッド
    private void OnLaunchPerformed(InputAction.CallbackContext context)
    {
        // ボールがまだ発射されていない場合に発射する
        if (!isBallLaunched)
        {
            Debug.Log("ボールを発射！");
            isBallLaunched = true;

            rb.isKinematic = false;
        }
    }

    private void OnReset(InputAction.CallbackContext context) 
    {
        ResetBall();
    }

    private void OnLeftThrow(InputAction.CallbackContext context)
    {
        // 発射されている場合変更できなくする
        if (isBallLaunched)
        {
            return;
        }
        currentThrowType = ThrowType.Left;
    }
    private void OnRightThrow(InputAction.CallbackContext context)
    {
        // 発射されている場合変更できなくする
        if (isBallLaunched)
        {
            return;
        }
        currentThrowType = ThrowType.Right;
    }
    // --- ここまで Input Systemイベントハンドラ ---

    // 同時押しを検出するためにUpdateを使用
    void Update()
    {
        bool isGripPressed = gripButtonAction.action.IsPressed();
        bool isTriggerPressed = triggerButtonAction.action.IsPressed();

        // 既にボールが発射されている場合は、球種を変更できないようにする
        if (isBallLaunched)
        {
            return;
        }

        if (currentPitchType != PitchType.Fork && isGripPressed && isTriggerPressed)
        {
            Debug.Log("フォークに切り替え");
            currentPitchType = PitchType.Fork;
            GeneratePathForPitchType(currentPitchType);
        }
        else if (currentPitchType != PitchType.Straight && isGripPressed && !isTriggerPressed)
        {
            Debug.Log("ストレートに切り替え");
            currentPitchType = PitchType.Straight;
            GeneratePathForPitchType(currentPitchType);
        }
        else if (currentPitchType != PitchType.Slider && !isGripPressed && isTriggerPressed)
        {
            Debug.Log("スライダーに切り替え");
            currentPitchType = PitchType.Slider;
            GeneratePathForPitchType(currentPitchType);
        }
    }

    void FixedUpdate()
    {

        // isBallLaunchedフラグがtrueの場合のみボールを動かす
        if (!isBallLaunched)
        {
            return; // 発射されていなければ何もしない
        }


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

                Invoke("ResetBall", 5.0f);
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
        rb.isKinematic = true;
        rb.position = initialPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentPathIndex = 0;

        isBallLaunched = false; // 発射フラグをリセット

        enabled = true;
        Debug.Log("ボールをリセットし、発射待機状態になります。");
    }

    // （BallSlider, BallStraight, BallFork メソッドは変更ありません）
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