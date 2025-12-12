using UnityEngine;

public class FieldMiniGame : MonoBehaviour
{
    public static FieldMiniGame Instance;

    [Header("Obstacle 설정")]
    public GameObject obstaclePrefab;
    public Transform spawnLeftTop;
    public Transform spawnRightTop;
    public float spawnInterval = 0.7f;

    [Header("게임 시간 설정")]
    public float gameDuration = 30f;

    [Header("점수")]
    public int score = 0;

    float gameTimer;
    float spawnTimer;
    bool isPlaying = false;

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
        score = 0;
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying) return;

        gameTimer -= Time.deltaTime;
        if (gameTimer <= 0f)
        {
            isPlaying = false;
            Debug.Log("Game Over! Final Score = " + score);
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnObstacle();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || spawnLeftTop == null || spawnRightTop == null)
        {
            Debug.LogWarning("Spawn 설정이 비어 있음!");
            return;
        }

        float randomX = Random.Range(spawnLeftTop.position.x, spawnRightTop.position.x);
        float y = spawnLeftTop.position.y;

        Vector3 spawnPos = new Vector3(randomX, y, 0f);
        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
    }

}
