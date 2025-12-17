using UnityEngine;
using System.Collections;

public class AlphaBlinkSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float visibleTime = 0.6f;   // 보이는 시간
    [SerializeField] private float hiddenTime  = 0.6f;   // 숨는 시간

    private Coroutine co;

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        co = StartCoroutine(Blink());
    }

    private void OnDisable()
    {
        if (co != null) StopCoroutine(co);
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            SetAlpha(1f);
            yield return new WaitForSeconds(visibleTime);

            SetAlpha(0f);
            yield return new WaitForSeconds(hiddenTime);
        }
    }

    private void SetAlpha(float a)
    {
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
