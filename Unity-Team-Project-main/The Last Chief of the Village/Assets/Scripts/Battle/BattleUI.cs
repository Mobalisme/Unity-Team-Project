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
        SetMessage("플레이어는 무엇을 할 지 선택하세요!");
        SetButtonsInteractable(true);
    }

    public void ShowEnemyTurn()
    {
        Show(true);
        SetMessage("적의 차례입니다...");
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
            GameManager.Instance.PlayerDefend(); // Recover
    }

    public void ShowResult(bool playerWon)
    {
        Show(true);
        SetButtonsInteractable(false);
        SetMessage(playerWon ? "승리했습니다! 보스를 쓰러뜨렸어요!" : "패배했습니다... 파티가 전멸했어요.");
    }

    // GameManager에서 버튼 잠그려고 호출하는 함수
    public void SetButtonsInteractable(bool value)
    {
        if (attackButton != null) attackButton.interactable = value;
        if (defendButton != null) defendButton.interactable = value;
    }
}
