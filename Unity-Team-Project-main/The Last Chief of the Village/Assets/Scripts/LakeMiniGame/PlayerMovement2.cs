using UnityEngine;
using System.Collections;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;          // 기본 이동 속도
    public float runMultiplier = 1.6f;    // Shift 누를 때 배속
    public float leftLimit = -8f;
    public float rightLimit = 8f;

    [Header("피격 설정")]
    public float hurtCooldown = 0.5f;     // 연속 피격 방지 시간

    [Header("카메라 흔들림 (피격 시)")]
    public float shakeDuration = 0.18f;
    public float shakeStrength = 0.18f;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    bool isHurting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // --- 이동 입력 ---
        float inputX = 0f;
        if (Input.GetKey(KeyCode.A)) inputX = -1f;
        else if (Input.GetKey(KeyCode.D)) inputX = 1f;

        // --- Shift 달리기 ---
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            speed *= runMultiplier;

        // --- 위치 이동 ---
        Vector2 pos = rb.position;
        pos.x += inputX * speed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
        rb.MovePosition(pos);

        // --- 좌우 방향 Flip ---
        if (sr != null)
        {
            if (inputX < 0) sr.flipX = true;
            else if (inputX > 0) sr.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 이미 피격 중이면 무시
        if (isHurting) return;

        // 물줄기만 반응
        if (!other.CompareTag("LakeObstacle")) return;

        StartCoroutine(HurtRoutine());
    }

    IEnumerator HurtRoutine()
    {
        isHurting = true;

        // (옵션) 데미지 처리
        if (LakeMiniGameManager.Instance != null)
            LakeMiniGameManager.Instance.TakeDamage(5);

        // 화면 흔들림
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeDuration, shakeStrength);

        // Hurt 애니메이션
        if (anim != null)
            anim.SetTrigger("Hurt");

        yield return new WaitForSeconds(hurtCooldown);
        isHurting = false;
    }
}
