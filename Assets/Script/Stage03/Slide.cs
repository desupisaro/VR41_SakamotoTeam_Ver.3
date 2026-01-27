/*
//-------------------------------------------------------------------------------------------------
作成日：2026/01/20 Tue 18:19
作者　：菅村

Slide.cs
障害物を左右に動かす為のスクリプト。
//-------------------------------------------------------------------------------------------------
 */


//-------------------------------------------------------------------------------------------------
// 名前空間の宣言。
//-------------------------------------------------------------------------------------------------
using UnityEngine;


//-------------------------------------------------------------------------------------------------
// class Slide
//-------------------------------------------------------------------------------------------------
public class Slide : MonoBehaviour
{
    //-------------------------------------------------------------------------------------------------
    // 変数宣言。
    //-------------------------------------------------------------------------------------------------

    // オブジェクトの移動タイプ。
    [System.Serializable]
    enum MoveType
    {
        UNMOVE,                     // 動かない。
        SIDEMOVE,                   // 左右移動。
        VERTICALMOVE,               // 上下移動。
        GOLDEN_ROTATIONAL_ENERGY,   // 回転。
    }

    [Header("オブジェクトの移動タイプ")]
    [SerializeField] private MoveType _type;

    [Header("オブジェクトの移動スピード")]
    [SerializeField] private float _sideMoveSpeed       = 1f;
    [SerializeField] private float _verticalMoveSpeed   = 1f;

    // 
    private Vector3 _pos = Vector3.zero;

    //
    private Quaternion _rotate = Quaternion.identity;

    // 
    private float sin = 0;


    //-------------------------------------------------------------------------------------------------
    // private void Awake()関数。
    //-------------------------------------------------------------------------------------------------
    private void Awake()
    {
        _pos    = this.transform.localPosition;
        _rotate = this.transform.localRotation;
    }


    /*

    //-------------------------------------------------------------------------------------------------
    // private void Start()関数。
    //-------------------------------------------------------------------------------------------------
    private void Start()
    {
        
    }

    */


    //-------------------------------------------------------------------------------------------------
    // private void Update()関数。
    //-------------------------------------------------------------------------------------------------
    private void Update()
    {
        switch ( _type )
        {
            case MoveType.UNMOVE:
                break;

            case MoveType.SIDEMOVE:
                float sin = Mathf.Sin(Time.time * _sideMoveSpeed);
                this.transform.localPosition = new Vector3(sin, _pos.y, _pos.z);
                break;

            case MoveType.VERTICALMOVE:
                float sinV = Mathf.Sin(Time.time * _verticalMoveSpeed);
                this.transform.localPosition = new Vector3(_pos.x, sinV + 1, _pos.z);
                break;

            case MoveType.GOLDEN_ROTATIONAL_ENERGY:
                sin = Mathf.Tan( Time.time );
                this.transform.localRotation = new Quaternion( _rotate.x, _rotate.y, sin, _rotate.w );
                break;

            default:
                break;
        }
    }
}