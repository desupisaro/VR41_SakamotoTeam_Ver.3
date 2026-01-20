using Unity.VisualScripting;
using UnityEngine;

public class GameSEManager : MonoBehaviour
{
    public AudioClip boalSE1;
    public AudioClip boalSE2;
    public AudioClip boalSE3;
    private int seed = 0;

    public void PlayTestSE()
    {
        seed = Random.RandomRange(0, 100);
        if (seed < 30)
        {
            SoundManager.Instance.PlaySE(boalSE1);
//            Debug.Log("1:" + seed);
        }
        else if (seed < 60 && seed >= 30)
        {
            SoundManager.Instance.PlaySE(boalSE2);
//            Debug.Log("2:" + seed);
        }
        else
        {
            SoundManager.Instance.PlaySE(boalSE3);
//            Debug.Log("3:" + seed);
        }
    }
}
