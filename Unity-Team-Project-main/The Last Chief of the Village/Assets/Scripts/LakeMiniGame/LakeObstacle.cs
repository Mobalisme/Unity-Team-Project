using UnityEngine;

public class LakeObstacle : MonoBehaviour
{
    [Header("수명 설정")]
    public float lifeTime = 0.6f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
