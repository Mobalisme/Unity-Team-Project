// BattleUI.cs
// 주석: 한글 / 게임에 표시되는 문자열: 영어

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("Root")]
    public GameObject panel;

    [Header("Text")]
    public TMP_Text messageText;

    [Header("Buttons")]
    public Button attackButton;
    public Button defendButton; // Recover 버튼으로 사용

    public void Show(bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    public void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
    }

    public void ShowPlayerTurn()
    {
        Show(true);
        SetMessage("Choose an action (Attack / Recover)");
        SetButtonsInteractable(true);
    }

    public void ShowEnemyTurn()
    {
        Show(true);
        SetMessage("Enemy turn...");
        SetButtonsInteractable(false);
    }

    public void OnAttackButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerAttack();
    }

    public void OnDefendButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDefend();
    }

    public void ShowResult(bool playerWon)
    {
        Show(true);
        SetButtonsInteractable(false);
        SetMessage(playerWon ? "Victory! The boss is defeated!" : "Defeat... Your party is wiped out.");
    }

    public void SetButtonsInteractable(bool value)
    {
        if (attackButton != null) attackButton.interactable = value;
        if (defendButton != null) defendButton.interactable = value;
    }

    // ===== (추가) 구버전 GameManager 호환용 =====
    public void RefreshAll()
    {
        // 구버전 코드가 호출해도 에러 안 나게만 처리
        // 실제 UI 갱신은 GameManager가 HUD 텍스트를 갱신하는 구조로 이미 되어있음
    }

    public void SetBossPhase(int phase)
    {
        // 구버전 코드가 호출해도 에러 안 나게만 처리
    }
}
