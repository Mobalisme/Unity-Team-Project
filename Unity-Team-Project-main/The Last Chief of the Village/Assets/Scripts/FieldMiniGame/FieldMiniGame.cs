using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("스폰 주기")]
    public float spawnInterval = 0.7f;

    [Header("스폰 가중치")]
    public int appleWeight = 45;
    public int grapeWeight = 20;
    public int bombWeight = 35;

    [Header("게임 시간")]
    public float gameDuration = 30f;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;

    [Header("폭탄 효과 - 화면 흔들림")]
    public Camera targetCamera;
    public float shakeDuration = 0.18f;
    public float shakeStrength = 0.08f;

    [Header("카메라 경계 (Shake에도 적용)")]
    public Vector2 camMinBounds;
    public Vector2 camMaxBounds;

    [Header("Tier Popup UI (Dino02)")]
    public Image tierPopupImage;
    public Sprite highSprite; // Dino02_01
    public Sprite midSprite;  // Dino02_02
    public Sprite lowSprite;  // Dino02_03
    public Vector2 popupSize = new Vector2(900, 450);
    public float popupSeconds = 1.2f;

    [Header("Scene Transition")]
    public bool loadNextSceneAfterPopup = true;

    [SceneName]
    public string nextSceneName;

    [Header("점수")]
    public int score = 0;

    private float gameTimer;
    private float spawnTimer;
    private bool isPlaying;
    private bool ended = false;
    private Coroutine shakeCo;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartGame();
        if (tierPopupImage != null)
            tierPopupImage.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        gameTimer = gameDuration;
        spawnTimer = 0f;
        isPlaying = true;
        ended = false;

        score = 0;
        RefreshScoreUI();
        RefreshTimeUI();
    }

    private void Update()
    {
        if (!isPlaying) return;

        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0f)
        {
            gameTimer = 0f;
            isPlaying = false;
            RefreshTimeUI();
            EndGame();
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnItem();
            spawnTimer = spawnInterval;
        }

        RefreshTimeUI();
    }

    private void SpawnItem()
    {
        if (spawnLeftTop == null || spawnRightTop == null) return;

        GameObject prefab = PickByWeight();
        if (prefab == null) return;

        float x = Random.Range(spawnLeftTop.position.x, spawnRightTop.position.x);
        float y = spawnLeftTop.position.y;

        Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
    }

    private GameObject PickByWeight()
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
        if (ended) return;
        score += amount;
        RefreshScoreUI();
    }

    private void RefreshScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void RefreshTimeUI()
    {
        if (timeText == null) return;

        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(gameTimer));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void EndGame()
    {
        if (ended) return;
        ended = true;

        ClearAllItems();

        int finalScore = MiniGameProgress.ClampScore100(score);
        MiniGameProgress.SetFieldResult(finalScore);

        Debug.Log($"[FIELD] End. Score={finalScore}, Tier={MiniGameProgress.ScoreToTier(finalScore)}");

        StartCoroutine(EndFlow(finalScore));
    }

    private IEnumerator EndFlow(int finalScore)
    {
        yield return ShowTierPopup(MiniGameProgress.ScoreToTier(finalScore));

        if (loadNextSceneAfterPopup && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator ShowTierPopup(MiniGameProgress.Tier123 tier)
    {
        if (tierPopupImage == null) yield break;

        tierPopupImage.gameObject.SetActive(true);
        tierPopupImage.preserveAspect = true;

        Sprite s = null;
        if (tier == MiniGameProgress.Tier123.High_3) s = highSprite;
        else if (tier == MiniGameProgress.Tier123.Mid_2) s = midSprite;
        else s = lowSprite;

        if (s != null) tierPopupImage.sprite = s;

        tierPopupImage.rectTransform.anchoredPosition = Vector2.zero;
        tierPopupImage.rectTransform.sizeDelta = popupSize;

        yield return new WaitForSeconds(popupSeconds);

        tierPopupImage.gameObject.SetActive(false);
    }

    private void ClearAllItems()
    {
        // 경고 제거: FindObjectsOfType 대신 FindObjectsByType 사용
        var items = Object.FindObjectsByType<FallingItem>(FindObjectsSortMode.None);
        foreach (var it in items)
            Destroy(it.gameObject);
    }

    public void ShakeCamera()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        if (shakeCo != null) StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(cam));
    }

    private IEnumerator ShakeRoutine(Camera cam)
    {
        Vector3 origin = cam.transform.position;
        float t = 0f;

        while (t < shakeDuration)
        {
            t += Time.deltaTime;

            Vector2 offset = Random.insideUnitCircle * shakeStrength;
            Vector3 pos = origin + new Vector3(offset.x, offset.y, 0f);

            pos.x = Mathf.Clamp(pos.x, camMinBounds.x, camMaxBounds.x);
            pos.y = Mathf.Clamp(pos.y, camMinBounds.y, camMaxBounds.y);

            cam.transform.position = pos;
            yield return null;
        }

        Vector3 finalPos = origin;
        finalPos.x = Mathf.Clamp(finalPos.x, camMinBounds.x, camMaxBounds.x);
        finalPos.y = Mathf.Clamp(finalPos.y, camMinBounds.y, camMaxBounds.y);
        cam.transform.position = finalPos;
    }
}
