using UnityEngine;
using System.Collections;

public class LakeMiniGameManager : MonoBehaviour
{
    public static LakeMiniGameManager Instance;

    [Header("물줄기 프리팹 설정")]
    public GameObject obstaclePrefab;
    public Transform spawnLeftBottom;
    public Transform spawnRightBottom;
    public float spawnInterval = 0.7f;

    [Header("물줄기 이동 설정")]
    public float obstacleSpeed = 3f;

    [Header("게임 시간 설정")]
    public float gameDuration = 20f;   // 게임 진행 시간(초)
    float elapsedTime;
    bool isPlaying;

    [Header("플레이어 체력")]
    public int maxHealth = 100;
    public int currentHealth;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        elapsedTime = 0f;
        isPlaying = true;
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        if (!isPlaying) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= gameDuration)
        {
            isPlaying = false;
            Debug.Log("Lake Game Clear! Time over");
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (isPlaying)
        {
            SpawnObstacle();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || spawnLeftBottom == null || spawnRightBottom == null)
            return;

        // 0 또는 1 중 하나 랜덤
        int lane = Random.Range(0, 2);

        Transform spawnPoint = (lane == 0) ? spawnLeftBottom : spawnRightBottom;

        // 선택된 왼쪽 / 오른쪽 위치에서만 생성
        Vector3 spawnPos = spawnPoint.position;

        Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
    }



    public void TakeDamage(int amount)
    {
        if (!isPlaying) return;

        currentHealth -= amount;
        Debug.Log("HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isPlaying = false;
            Debug.Log("Lake Game Over! HP 0");
        }
    }
}
