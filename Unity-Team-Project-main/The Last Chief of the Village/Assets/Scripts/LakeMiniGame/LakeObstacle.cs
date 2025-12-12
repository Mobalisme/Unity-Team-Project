using UnityEngine;

public class LakeObstacle : MonoBehaviour
{
    [Header("수명 설정")]
    public float lifeTime = 0.6f;   // 물줄기가 화면에 남아있는 시간 (초)

    void Start()
    {
        // lifeTime이 지나면 자동으로 사라지게
        Destroy(gameObject, lifeTime);
    }
}
