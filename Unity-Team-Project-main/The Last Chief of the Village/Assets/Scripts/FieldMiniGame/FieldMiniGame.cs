using UnityEngine;
using TMPro;
using System.Collections;

public class FieldMiniGame : MonoBehaviour
{
    public static FieldMiniGame Instance;

    [Header("프리팹 (3종)")]
    public GameObject applePrefab;
    public GameObject grapePrefab;
    public GameObject bombPrefab;

    [Header("스폰 위치")]
    public Transform spawnLeftTop;
    public Transform spawnRightTop;
    public float spawnInterval = 0.7f;

    [Header("스폰 가중치")]
    public int appleWeight = 45;
    public int grapeWeight = 20;
    public int bombWeight = 35;

    [Header("게임 시간")]
    public float gameDuration = 30f;

    Color timeNormalColor = Color.white;


    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("폭탄 효과 - 화면 흔들림")]
    public Camera targetCamera;            
    public float shakeDuration = 0.18f;
    public float shakeStrength = 0.08f; // 흔들림 크기

    [Header("카메라 경계 (Shake에도 적용)")]
    public Vector2 camMinBounds;   // 예: (-5, -3)
    public Vector2 camMaxBounds;   // 예: ( 5,  3)


    float gameTimer;
    float spawnTimer;
    bool isPlaying;

    Coroutine shakeCo;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameTimer = gameDuration;
        spawnTimer = 0f;
        isPlaying = true;

        score = 0;
        RefreshScoreUI();
        RefreshTimeUI();

        if (timeText != null)
            timeNormalColor = timeText.color;

    }

    [Header("점수")]
    public int score = 0;

    private void Update()
    {
        if (!isPlaying) return;

        // 타이머
        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            isPlaying = false;
            RefreshTimeUI();

            Debug.Log("Game Over! Final Score = " + score);

            // 게임 끝나면 화면에 있는 아이템 전부 삭제
            ClearAllItems();
            return;
        }

        // 스폰
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnItem();
            spawnTimer = spawnInterval;
        }

        RefreshTimeUI();
    }

    void SpawnItem()
    {
        if (spawnLeftTop == null || spawnRightTop == null) return;

        GameObject prefab = PickByWeight();
        if (prefab == null) return;

        float randomX = Random.Range(spawnLeftTop.position.x, spawnRightTop.position.x);
        float y = spawnLeftTop.position.y;

        Vector3 spawnPos = new Vector3(randomX, y, 0f);
        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    GameObject PickByWeight()
    {
        int total = appleWeight + grapeWeight + bombWeight;
        if (total <= 0) return applePrefab;

        int r = Random.Range(0, total);

        if (r < appleWeight) return applePrefab;
        r -= appleWeight;

        if (r < grapeWeight) return grapePrefab;
        return bombPrefab;
    }

    public void AddScore(int amount)
    {
        score += amount;
        RefreshScoreUI();
    }

    void RefreshScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    void RefreshTimeUI()
    {
        if (timeText == null) return;

        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(gameTimer));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
    }


    void ClearAllItems()
    {
        var items = Object.FindObjectsByType<FallingItem>(FindObjectsSortMode.None);
        foreach (var it in items)
            Destroy(it.gameObject);

    }

    // 폭탄 효과: 카메라 흔들기
    public void ShakeCamera()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(cam));
    }

    IEnumerator ShakeRoutine(Camera cam)
    {
        Vector3 origin = cam.transform.position;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector2 offset = Random.insideUnitCircle * shakeStrength;
            Vector3 pos = origin + new Vector3(offset.x, offset.y, 0f);

            // Clamp 적용 (x, y만)
            pos.x = Mathf.Clamp(pos.x, camMinBounds.x, camMaxBounds.x);
            pos.y = Mathf.Clamp(pos.y, camMinBounds.y, camMaxBounds.y);

            cam.transform.position = pos;

            yield return null;
        }

        // 마지막에도 Clamp 한 번 더
        Vector3 finalPos = origin;
        finalPos.x = Mathf.Clamp(finalPos.x, camMinBounds.x, camMaxBounds.x);
        finalPos.y = Mathf.Clamp(finalPos.y, camMinBounds.y, camMaxBounds.y);
        cam.transform.position = finalPos;
    }

}
