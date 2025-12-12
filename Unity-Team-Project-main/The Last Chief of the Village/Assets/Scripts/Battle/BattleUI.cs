using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;

    public Button attackButton;
    public Button defendButton;

    public void Show(bool active)
    {
        panel.SetActive(active);
    }

    public void SetMessage(string msg)
    {
        messageText.text = msg;
        Debug.Log($"[UI 메시지] {msg}");
    }

    // 플레이어가 선택할 수 있는 상태 (버튼 활성화)
    public void ShowPlayerTurn()
    {
        Show(true);
        SetMessage("플레이어는 무엇을 할 지 선택하세요!");
        SetButtonsInteractable(true);
    }

    // 적 턴 표시 (버튼 비활성화)
    public void ShowEnemyTurn()
    {
        Show(true);
        SetMessage("적의 차례입니다!");
        SetButtonsInteractable(false);
    }

    void SetButtonsInteractable(bool value)
    {
        if (attackButton != null) attackButton.interactable = value;
        if (defendButton != null) defendButton.interactable = value;
    }

    public void OnAttackButton()
    {
        GameManager.Instance.PlayerAttack();
    }

    public void OnDefendButton()
    {
        GameManager.Instance.PlayerDefend();
    }

    public void ShowResult(bool playerWon)
    {
        Show(true);                    // 패널은 보이게
        SetButtonsInteractable(false); // 버튼은 비활성화

        if (playerWon)
        {
            SetMessage("승리했습니다! 적을 쓰러뜨렸어요!");
        }
        else
        {
            SetMessage("패배했습니다... 공룡이 쓰러졌어요.");
        }
    }

}
