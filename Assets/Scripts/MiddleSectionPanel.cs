using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiddleSectionPanel : LobbyPanelBase
{
    [SerializeField] private Button joinRandomRoomBtn;
    [SerializeField] private Button joinRoomByArgBtn;
    [SerializeField] private Button createRoomBtn;

    [SerializeField] private TMP_InputField joinRoomByArgInputField;
    [SerializeField] private TMP_InputField createRoomInputField;

    private NetworkRunnerController networkRunnerController;

    public override void InitPanel(LobbyUIManager manager)
    {
        base.InitPanel(manager);

        joinRandomRoomBtn.onClick.AddListener(JoinRandomRoom);
        joinRoomByArgBtn.onClick.AddListener(() => CreateRoom(GameMode.Client, joinRoomByArgInputField));
        createRoomBtn.onClick.AddListener(() => CreateRoom(GameMode.Host, createRoomInputField));
        networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;
    }

    private void CreateRoom(GameMode gameMode, TMP_InputField field)
    {
        if (field.text.Length < 2) return;
        Debug.Log($"[MiddleSectionPanel] Start Game Mode: {gameMode}");
        networkRunnerController.StartGame(gameMode, field.text);
    }

    private void JoinRandomRoom()
    {
        Debug.Log($"[MiddleSectionPanel] Join Random Room");
        networkRunnerController.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }
}
