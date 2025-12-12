using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;      // 따라갈 대상 (Player)
    public float smoothTime = 0.15f;

    // 맵 안에서만 움직이도록 제한할 범위
    public Vector2 minBounds;     // (왼, 아래)
    public Vector2 maxBounds;     // (오른, 위)

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (!target) return;

        // z 값은 카메라 원래 z 유지
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        // 맵 범위 안으로 Clamp
        targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);

        // 부드럽게 따라가기
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}
