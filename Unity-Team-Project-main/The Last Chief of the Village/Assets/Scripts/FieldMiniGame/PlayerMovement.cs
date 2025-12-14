using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 3f;
    public float runSpeed = 6f;

    [Header("폭탄 맞았을 때 - 플레이어 깜빡임")]
    public float blinkDuration = 0.6f;
    public float blinkInterval = 0.08f;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;

    Vector2 input;
    Coroutine blinkCo;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null) sr = GetComponent<SpriteRenderer>();

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        float x = 0f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        else if (Input.GetKey(KeyCode.D)) x = 1f;

        input = new Vector2(x, 0f);

        bool isWalk = Mathf.Abs(x) > 0.01f;
        if (anim != null)
            anim.SetBool("isWalk", isWalk);

        if (sr != null && x != 0)
            sr.flipX = x < 0;
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : moveSpeed;
        rb.linearVelocity = new Vector2(input.x * speed, rb.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        FallingItem item = other.GetComponent<FallingItem>();
        if (item == null) return;

        // 점수 반영
        if (FieldMiniGame.Instance != null)
            FieldMiniGame.Instance.AddScore(item.points);

        // 폭탄이면 효과
        if (item.type == ItemType.Bomb || item.points < 0)
        {
            if (FieldMiniGame.Instance != null)
                FieldMiniGame.Instance.ShakeCamera();

            StartBlink();
        }

        Destroy(other.gameObject);
    }

    void StartBlink()
    {
        if (sr == null) return;

        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        float t = 0f;
        bool visible = true;

        while (t < blinkDuration)
        {
            t += blinkInterval;
            visible = !visible;
            sr.enabled = visible;
            yield return new WaitForSeconds(blinkInterval);
        }

        sr.enabled = true;
    }
}
