using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    Vector3 originalPos;
    Coroutine shakeCo;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    void OnEnable()
    {
        // 씬 재진입/비활성화 후 재활성화 대비
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(duration, strength));
    }

    IEnumerator ShakeRoutine(float duration, float strength)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCo = null;
    }
}
