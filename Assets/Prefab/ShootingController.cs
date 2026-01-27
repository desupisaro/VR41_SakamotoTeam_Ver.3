using UnityEngine;

public class ShootingController : MonoBehaviour
{
    // インスペクターから設定するための公開変数
    public GameObject bulletPrefab; // 1. 弾のプレハブを格納
    public float launchSpeed = 1000f; // 2. 弾を発射する速さ

    // ★追加: 発射位置を動かすための空のオブジェクトを割り当てる
    public Transform shotPoint;
    // ★追加: 発射位置が移動するスピード
    public float moveSpeed = 5f;

    void Update()
    {
        // --- 発射位置の移動処理 ---
        // Vector3.up (上下), Vector3.right (左右) を使って移動させる
        if (Input.GetKey(KeyCode.W)) shotPoint.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.S)) shotPoint.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.A)) shotPoint.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) shotPoint.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        // スペースキーが押された瞬間を検出
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // ★修正: 生成位置を transform.position ではなく shotPoint.position に変更
        GameObject bullet = Instantiate(
            bulletPrefab,
            shotPoint.position,
            shotPoint.rotation
        );

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            // 発射方向も発射口の向きに合わせる
            bulletRb.AddForce(shotPoint.forward * launchSpeed);
        }

        Destroy(bullet, 4f);
    }
}
