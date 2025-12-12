using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f;      // 기본 속도
    public float runSpeed = 6f;       // 달릴 때 속도 (Shift)

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    float moveX;
    bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");   // A:-1, D:+1

        // 걷기 애니메이션
        bool isWalking = Mathf.Abs(moveX) > 0.01f;
        anim.SetBool("isWalk", isWalking);

        // flip
        if (moveX > 0 && !isFacingRight)
            Flip();
        else if (moveX < 0 && isFacingRight)
            Flip();
    }

    void FixedUpdate()
    {
        // Shift를 누르면 runSpeed, 아니면 moveSpeed
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(moveX * currentSpeed, rb.linearVelocity.y);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        sr.flipX = !sr.flipX;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (FieldMiniGame.Instance != null)
            {
                FieldMiniGame.Instance.AddScore(1);
            }
            Destroy(other.gameObject);
        }
    }
}
