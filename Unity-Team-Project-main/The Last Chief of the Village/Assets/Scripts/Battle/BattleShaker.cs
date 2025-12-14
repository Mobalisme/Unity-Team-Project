using System.Collections;
using UnityEngine;

public class BattleShaker : MonoBehaviour
{
    public float duration = 0.2f; 
    public float magnitude = 10f; 

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

        if (rect != null)
            rect.anchoredPosition = originalPos;
        else
            transform.localPosition = originalPos;
    }
}
