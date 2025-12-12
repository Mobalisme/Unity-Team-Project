using System.Collections;
using UnityEngine;

public class BattleShaker : MonoBehaviour
{
    public float duration = 0.2f;  // 흔들리는 시간
    public float magnitude = 10f;  // 흔들리는 세기 (픽셀)

    RectTransform rect;
    Vector3 originalPos;
    Coroutine shakeCo;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (rect != null)
            originalPos = rect.anchoredPosition;
        else
            originalPos = transform.localPosition;
    }

    public void Shake()
    {
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            if (rect != null)
                rect.anchoredPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            else
                transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원위치
        if (rect != null)
            rect.anchoredPosition = originalPos;
        else
            transform.localPosition = originalPos;
    }
}
