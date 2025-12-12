using UnityEngine;
using System.Collections;

public class PlayerMovement2 : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float leftLimit = -8f;
    public float rightLimit = 8f;

    [Header("피격 설정")]
    public float hurtCooldown = 0.4f;   // 한 번 맞고 또 맞기까지 시간(초)

    Rigidbody2D rb;
    Animator anim;
    bool isHurting = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float inputX = 0f;

        if (Input.GetKey(KeyCode.A)) inputX = -1f;
        else if (Input.GetKey(KeyCode.D)) inputX = 1f;

        Vector2 pos = rb.position;
        pos.x += inputX * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);

        rb.MovePosition(pos);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 디버그: 실제로 충돌이 들어오는지 먼저 확인
        Debug.Log("OnTriggerEnter2D with: " + other.name);

        if (isHurting) return;
        if (!other.CompareTag("LakeObstacle")) return;

        StartCoroutine(HurtRoutine());
        //  물줄기 안 없애고 그대로 놔둔다
    }

    IEnumerator HurtRoutine()
    {
        isHurting = true;

        if (LakeMiniGameManager.Instance != null)
        {
            LakeMiniGameManager.Instance.TakeDamage(5);
        }

        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        yield return new WaitForSeconds(hurtCooldown);
        isHurting = false;
    }
}
