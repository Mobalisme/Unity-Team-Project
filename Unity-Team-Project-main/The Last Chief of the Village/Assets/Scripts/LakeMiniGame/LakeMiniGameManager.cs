// LakeMiniGameManager.cs
// 주석: 한글 / 게임에 표시되는 문자열: 영어

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LakeMiniGameManager : MonoBehaviour
{
    public static LakeMiniGameManager Instance;

    [Header("프리팹 설정")]
    public GameObject obstaclePrefab;   // 물줄기 프리팹
    public GameObject warningPrefab;    // 경고(느낌표) 프리팹

    [Header("스폰 범위 기준 Transform")]
    public Transform spawnLeftBottom;   // 왼쪽 기준점
    public Transform spawnRightBottom;  // 오른쪽 기준점

    [Header("스폰 타이밍")]
    public float spawnInterval = 1.2f;  // 물줄기 생성 간격
    public float warningTime = 0.6f;    // 몇 초 전에 예고할지

    [Header("중앙 금지 구간(가운데 안 나오게)")]
    public float centerX = 0f;          // 가운데 X (필요시 조정)
    public float centerBlockWidth = 2f; // 중앙 금지 폭

    [Header("게임 시간")]
    public float gameDuration = 30f;

    [Header("UI")]
    public TextMeshProUGUI timeText;    // 00:30 표시 텍스트
    public TextMeshProUGUI scoreText;   // Score: 0 표시 텍스트

    [Header("점수(옵션)")]
    public int score = 0;

    [Header("체력(옵션)")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Tier Popup UI (Dino03)")]
    public Image tierPopupImage;        // Canvas의 UI Image
    public Sprite highSprite;           // Dino03_01
    public Sprite midSprite;            // Dino03_02
    public Sprite lowSprite;            // Dino03_03
    public Vector2 popupSize = new Vector2(900, 450);
    public float popupSeconds = 1.2f;

    [Header("Scene Flow (Optional)")]
    public bool loadNextSceneAfterPopup = false;
    public string nextSceneName = "Battle";

    float timeLeft;
    bool isGameOver = false;

    Coroutine spawnRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;

        if (spawnInterval < warningTime + 0.05f)
            spawnInterval = warningTime + 0.05f;

        timeLeft = gameDuration;
        UpdateTimeUI();
        UpdateHPOnScoreText();

        if (tierPopupImage != null)
            tierPopupImage.gameObject.SetActive(false);

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        if (isGameOver) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft < 0f) timeLeft = 0f;

        UpdateTimeUI();

        if (timeLeft <= 0f)
        {
            EndGame();
        }
    }

    IEnumerator SpawnLoop()
    {
        while (!isGameOver)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObstacle()
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

    IEnumerator SpawnWithWarning(Vector3 pos)
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

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        if (score < 0) score = 0;
        UpdateScoreUI();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHPOnScoreText();
    }

    void UpdateHPOnScoreText()
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {currentHealth}";
    }

    void UpdateTimeUI()
    {
        if (timeText == null) return;
        int sec = Mathf.CeilToInt(timeLeft);
        timeText.text = $"00:{sec:00}";
    }

    void UpdateScoreUI()
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {score}";
    }

    void EndGame()
    {
        if (isGameOver) return;

        isGameOver = true;
        StopSpawning();
        UpdateTimeUI();

        // Lake: 남은 체력(0~100)을 최종 점수로 사용
        int finalScore = MiniGameProgress.ClampScore100(currentHealth);
        MiniGameProgress.SetLakeResult(finalScore);

        StartCoroutine(EndFlow(finalScore));
    }

    IEnumerator EndFlow(int finalScore)
    {
        yield return ShowTierPopup(MiniGameProgress.ScoreToTier(finalScore));

        if (loadNextSceneAfterPopup && !string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator ShowTierPopup(MiniGameProgress.Tier123 tier)
    {
        if (tierPopupImage == null) yield break;

        tierPopupImage.gameObject.SetActive(true);
        tierPopupImage.preserveAspect = true;

        Sprite s = null;
        if (tier == MiniGameProgress.Tier123.High_3) s = highSprite;
        else if (tier == MiniGameProgress.Tier123.Mid_2) s = midSprite;
        else s = lowSprite;

        if (s != null) tierPopupImage.sprite = s;

        // 크게 보이게
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
