using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LakeMiniGameManager : MonoBehaviour
{
    public static LakeMiniGameManager Instance;

    [Header("프리팹 설정")]
    public GameObject obstaclePrefab;
    public GameObject warningPrefab;

    [Header("스폰 범위 기준 Transform(좌하단, 우하단)")]
    public Transform spawnLeftBottom;
    public Transform spawnRightBottom;

    [Header("스폰 타이밍")]
    public float spawnInterval = 1.2f;
    public float warningTime = 0.6f;

    [Header("중앙 금지 구간(가운데 안 나오게)")]
    public float centerX = 0f;
    public float centerBlockWidth = 2f;

    [Header("게임 시간")]
    public float gameDuration = 30f;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;

    [Header("체력(0~100)")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Tier Popup UI (Dino03)")]
    public Image tierPopupImage;
    public Sprite highSprite; // Dino03_01
    public Sprite midSprite;  // Dino03_02
    public Sprite lowSprite;  // Dino03_03
    public Vector2 popupSize = new Vector2(900, 450);
    public float popupSeconds = 1.2f;

    [Header("Scene Transition")]
    public bool loadNextSceneAfterPopup = true;

    [SceneName]
    public string nextSceneName; // Build Settings에 있는 씬 이름 드롭다운

    private float timeLeft;
    private bool isGameOver = false;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;

        if (spawnInterval < warningTime + 0.05f)
            spawnInterval = warningTime + 0.05f;

        timeLeft = gameDuration;
        UpdateTimeUI();
        UpdateHPAsScoreText();

        if (tierPopupImage != null)
            tierPopupImage.gameObject.SetActive(false);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void Update()
    {
        if (isGameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        UpdateTimeUI();

        if (timeLeft <= 0f)
            EndGame();
    }

    private IEnumerator SpawnLoop()
    {
        while (!isGameOver)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnObstacle()
    {
        if (obstaclePrefab == null || spawnLeftBottom == null || spawnRightBottom == null)
            return;

        float minX = spawnLeftBottom.position.x;
        float maxX = spawnRightBottom.position.x;

        float leftMax = centerX - centerBlockWidth * 0.5f;
        float rightMin = centerX + centerBlockWidth * 0.5f;

        leftMax = Mathf.Clamp(leftMax, minX, maxX);
        rightMin = Mathf.Clamp(rightMin, minX, maxX);

        float spawnX = (Random.value < 0.5f)
            ? Random.Range(minX, leftMax)
            : Random.Range(rightMin, maxX);

        float spawnY = spawnLeftBottom.position.y;

        StartCoroutine(SpawnWithWarning(new Vector3(spawnX, spawnY, 0f)));
    }

    private IEnumerator SpawnWithWarning(Vector3 pos)
    {
        GameObject warn = null;

        if (warningPrefab != null)
        {
            warn = Instantiate(warningPrefab, pos + Vector3.up * 0.10f, Quaternion.identity);
            warn.transform.localScale = Vector3.one * 0.35f;

            var blink = warn.GetComponent<WarningBlink>();
            if (blink != null)
                blink.SetBaseScale(warn.transform.localScale);
        }

        yield return new WaitForSeconds(warningTime);

        if (!isGameOver)
            Instantiate(obstaclePrefab, pos, Quaternion.identity);

        if (warn != null)
            Destroy(warn);
    }

    public void TakeDamage(int dmg)
    {
        if (isGameOver) return;

        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;

        UpdateHPAsScoreText();

        if (currentHealth <= 0)
            EndGame();
    }

    private void UpdateHPAsScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {currentHealth}";
    }

    private void UpdateTimeUI()
    {
        if (timeText == null) return;
        int sec = Mathf.CeilToInt(timeLeft);
        timeText.text = $"00:{sec:00}";
    }

    private void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        StopSpawning();
        UpdateTimeUI();

        int finalScore = MiniGameProgress.ClampScore100(currentHealth);
        MiniGameProgress.SetLakeResult(finalScore);

        Debug.Log($"[LAKE] End. Score={finalScore}, Tier={MiniGameProgress.ScoreToTier(finalScore)}");

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

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
}
