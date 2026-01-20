using Unity.VisualScripting;
using UnityEngine;

public class ResultSEManager : MonoBehaviour
{
    public AudioClip resultSE;
    public void Start()
    {
        SoundManager.Instance.PlaySE(resultSE);
    }
}