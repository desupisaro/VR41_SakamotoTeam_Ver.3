using UnityEngine;

public class TargetIdentifier : MonoBehaviour
{
    public string targetName = "Target_A";
    public float clearDistance = 0.5f;

    private Vector3 initialPosition;
    private Quaternion initialRotation; // ��]���ۑ�
    private Rigidbody rb;
    private bool isHitMeasured = false;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isHitMeasured) return;

        // ���ɔ�񂾔���i�����̓V�[���ɍ��킹�Ē������Ă��������j
        if (transform.position.z > initialPosition.z + clearDistance)
        {
            isHitMeasured = true;
            GameManager.Instance.RecordHit(targetName);
        }
    }

    // �����Z�b�g����
    public void ResetTarget()
    {
        isHitMeasured = false;
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;        // �����Ă���͂��~�߂�
            rb.angularVelocity = Vector3.zero; // ��]���Ă���͂��~�߂�
        }
    }

    public string TargetName => targetName;
}