using UnityEngine;

public class FieldObstacle : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 3f;     // 아래로 떨어지는 속도
    public float destroyY = -15f; // 이 Y보다 아래로 내려가면 삭제

    void Update()
    {
        // 아래 방향으로 이동
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 너무 아래로 떨어지면 삭제
        if (transform.position.y < destroyY)
        {
            Destroy(gameObject);
        }
    }
}
