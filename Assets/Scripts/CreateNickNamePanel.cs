using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateNickNamePanel : LobbyPanelBase
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button createNickNameBtn;

    private const int MAX_CHAR_FOR_NICKNAME = 2;

    public override void InitPanel(LobbyUIManager manager)
    {
        base.InitPanel(manager);
        createNickNameBtn.interactable = false;
        createNickNameBtn.onClick.AddListener(OnClickCreateNickName);
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string arg0)
    {
        createNickNameBtn.interactable = arg0.Length >= MAX_CHAR_FOR_NICKNAME;
    }

    private void OnClickCreateNickName()
    {
        var nickName = inputField.text;
        if(nickName.Length >= MAX_CHAR_FOR_NICKNAME)
        {
            lobbyUIManager.ShowPanel(LobbyPanelType.MiddleSectionPanel);
            GlobalManagers.Instance.PlayerData.SetNickName(nickName);
        }
    }
}
