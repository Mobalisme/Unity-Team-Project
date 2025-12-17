using UnityEngine;
using System.Collections;

public class CameraIntroZoom : MonoBehaviour
{
    public Transform target;

    public float startSize = 13f;
    public float endSize = 6f;

    public float waitTime = 1.5f;

    [Header("부드러운 이동")]
    public float moveDuration = 0.4f;

    [Header("줌")]
    public float zoomDuration = 1.5f;

    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = startSize;
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        // 처음 장면 잠깐 보여주기
        yield return new WaitForSeconds(waitTime);

        // 플레이어 위치로 부드럽게 이동
        yield return StartCoroutine(SmoothMoveToTarget());

        // 이동 끝난 위치 고정
        Vector3 fixedPos = transform.position;

        // 줌만 진행
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            float t = elapsed / zoomDuration;
            cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);
            transform.position = fixedPos; // 위치 고정

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.orthographicSize = endSize;
        transform.position = fixedPos;
    }

    IEnumerator SmoothMoveToTarget()
    {
        if (target == null) yield break;

        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            float t = elapsed / moveDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // 자연스럽게

            transform.position = Vector3.Lerp(startPos, endPos, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }
}
