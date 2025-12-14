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

    // 플레이어 턴(선택 단계) 표시
    public void ShowPlayerTurn()
    {
        Show(true);
        SetMessage("Choose an action (Attack / Recover)");
        SetButtonsInteractable(true);
    }

    // 적 턴 표시(버튼 잠금)
    public void ShowEnemyTurn()
    {
        Show(true);
        SetMessage("Enemy turn...");
        SetButtonsInteractable(false);
    }

    // Attack 버튼 클릭 시 호출(Inspector OnClick에 연결)
    public void OnAttackButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerAttack();
    }

    // Recover 버튼 클릭 시 호출(Inspector OnClick에 연결)
    public void OnDefendButton()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDefend();
    }

    // 승패 표시
    public void ShowResult(bool playerWon)
    {
        Show(true);
        SetButtonsInteractable(false);
        SetMessage(playerWon ? "Victory! The boss is defeated!" : "Defeat... Your party is wiped out.");
    }

    // GameManager에서 입력을 잠그기 위해 호출
    public void SetButtonsInteractable(bool value)
    {
        if (attackButton != null) attackButton.interactable = value;
        if (defendButton != null) defendButton.interactable = value;
    }
}
