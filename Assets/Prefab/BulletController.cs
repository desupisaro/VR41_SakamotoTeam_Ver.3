using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float explosionForce = 500f; // 爆発の力（吹き飛ばす強さ）
    public float explosionRadius = 5f; // 爆発の影響範囲

void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        // ★修正点：ここでのカウント処理（RecordHit）を削除しました
        // 当たったかどうかは的（TargetIdentifier）自身が動きを見て判断します

        if (rb != null)
        {
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            //Destroy(gameObject); // 弾は消す
        }
    }
}

