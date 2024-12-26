using Fusion;
using UnityEngine;
using TMPro;

public class PlayerDataNetworked : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [Networked, HideInInspector] public NetworkString<_16> NickName { get; private set; }

    private ChangeDetector _changeDetector;
    
    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        
        // --- Client
        // Find the local non-networked PlayerData to read the data and communicate it to the Host via a single RPC 
        if (Object.IsLocalPlayer())
        {
            var nickName = GlobalManagers.Instance.PlayerData.GetNickName();
            Rpc_SetNickName(nickName);
        }

        GlobalManagers.Instance.PlayerSpawnerController.AddEntry(Object.InputAuthority, Object);
        UpdateNickNameUI();
    }

    // RPC used to send player information to the Host
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    private void Rpc_SetNickName(string nickName)
    {
        if (string.IsNullOrEmpty(nickName)) return;
        NickName = nickName;
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(NickName):
                    UpdateNickNameUI();
                    break;
            }
        }
    }

    private void UpdateNickNameUI()
    {
        playerNameText.text = $"[{NickName}] [Id:{Object.InputAuthority.PlayerId}]";
    }
}
