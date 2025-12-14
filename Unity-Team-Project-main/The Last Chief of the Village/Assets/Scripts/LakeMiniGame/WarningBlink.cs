using UnityEngine;

public class WarningBlink : MonoBehaviour
{
    SpriteRenderer sr;
    float t;

    Vector3 baseScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;  // 현재 스케일을 기본으로 저장
    }

    void OnEnable()
    {
        // Instantiate 직후 스케일이 바뀌는 경우 대비
        baseScale = transform.localScale;
    }

    public void SetBaseScale(Vector3 s)
    {
        baseScale = s;
        transform.localScale = s;
    }

    void Update()
    {
        t += Time.deltaTime * 8f;

        float pulse = (Mathf.Sin(t) + 1f) * 0.5f;

        float mul = Mathf.Lerp(0.9f, 1.05f, pulse);
        transform.localScale = baseScale * mul;

        if (sr != null)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(0.4f, 0.9f, pulse);
            sr.color = c;
        }
    }
}
