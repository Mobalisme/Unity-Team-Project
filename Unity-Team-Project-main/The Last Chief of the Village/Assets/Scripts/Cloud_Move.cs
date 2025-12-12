using UnityEngine;

public class SimpleBackgroundScroll : MonoBehaviour
{
    [Tooltip("오른쪽으로 이동하는 속도 (월드 유닛/초)")]
    public float speed = 1f;

    void Update()
    {
        // 부모 오브젝트를 오른쪽으로 천천히 이동
        transform.position += Vector3.right * speed * Time.deltaTime;
    }
}
