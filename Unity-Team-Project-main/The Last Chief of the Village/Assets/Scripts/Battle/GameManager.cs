using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerHP = 100;
    public int enemyHP = 100;

    public int playerMaxHP = 100;
    public int enemyMaxHP = 100;

    public BattleUI battleUI;

    public BattleShaker playerShaker;  
    public BattleShaker enemyShaker;

    public TMP_Text playerHPText;
    public TMP_Text enemyHPText;



    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PrintHP();
        PlayerTurn();
    }

    // ----- 플레이어 턴 시작 -----
    public void PlayerTurn()
    {
        Debug.Log("=== 플레이어 턴 시작 ===");
        battleUI.ShowPlayerTurn();
    }


    // 공격 선택 시
    public void PlayerAttack()
    {
        battleUI.Show(false);

        enemyHP -= 20;
        if (enemyHP < 0) enemyHP = 0;

        UpdateHPText();

        if (enemyShaker != null)
            enemyShaker.Shake();

        Debug.Log($"플레이어 공격! → 적 HP: {enemyHP}");
        CheckEnemyDead();
    }


    // 방어 선택 시
    public void PlayerDefend()
    {
        battleUI.Show(false);

        playerHP += 15;
        if (playerHP > playerMaxHP) playerHP = playerMaxHP;

        UpdateHPText();

        Debug.Log($"플레이어 방어! → 플레이어 HP 회복: {playerHP}");
        Invoke(nameof(EnemyTurn), 1f);
    }

    // ----- 적 턴 -----
    private void EnemyTurn()
    {
        Debug.Log("=== 적 턴 시작 ===");
        battleUI.ShowEnemyTurn();

        playerHP -= 15;
        if (playerHP < 0) playerHP = 0;

        UpdateHPText();

        if (playerShaker != null)
            playerShaker.Shake();

        Debug.Log($"적의 공격! → 플레이어 HP: {playerHP}");
        CheckPlayerDead();
    }



    // ----- 체력 체크 -----
    void CheckEnemyDead()
    {
        if (enemyHP <= 0)
        {
            Debug.Log("적을 쓰러뜨렸습니다! 승리!");
            battleUI.ShowResult(true);  // 플레이어 승리
            return;                     // 더 이상 턴 안 넘어가게
        }

        Invoke(nameof(EnemyTurn), 1f);
    }

    void CheckPlayerDead()
    {
        if (playerHP <= 0)
        {
            Debug.Log("플레이어 사망... 패배했습니다.");
            battleUI.ShowResult(false);  // 플레이어 패배
            return;
        }

        Invoke(nameof(PlayerTurn), 1.2f);
    }

    void PrintHP()
    {
        Debug.Log($"플레이어 HP: {playerHP} / {playerMaxHP}");
        Debug.Log($"적 HP: {enemyHP} / {enemyMaxHP}");
        UpdateHPText(); 
    }


    void UpdateHPText()
    {
        if (playerHPText != null)
            playerHPText.text = $"Player HP: {playerHP} / {playerMaxHP}";

        if (enemyHPText != null)
            enemyHPText.text = $"Enemy HP: {enemyHP} / {enemyMaxHP}";
    }





}
