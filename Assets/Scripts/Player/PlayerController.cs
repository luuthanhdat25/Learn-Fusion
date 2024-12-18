using Fusion;
using TMPro;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 6;
    [SerializeField] private GameObject cam;

    private float horizontal;
    private Rigidbody2D rigidbody2D;
    private PlayerWeaponController playerWeaponController;
    private PlayerVisualController playerVisualController;

    [Networked] private NetworkButtons buttonPrev { get; set; }
    [Networked(OnChanged = nameof(OnNickNameChange))] private NetworkString<_8> playerName { get; set; }

    private static void OnNickNameChange(Changed<PlayerController> changed)
    {
        var nickName = changed.Behaviour.playerName;
        changed.Behaviour.SetPlayerNickName(nickName);
    }

    private void SetPlayerNickName(NetworkString<_8> nickName)
    {
        playerNameText.text = nickName + " " + Object.InputAuthority.PlayerId;
    }

    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerWeaponController = GetComponent<PlayerWeaponController>();
        playerVisualController = GetComponent<PlayerVisualController>();
        SetLocalObjects();
    }

    private void SetLocalObjects()
    {
        if (Runner.LocalPlayer == Object.HasInputAuthority)
        {
            cam.SetActive(true);

            //Update new join player nickname to all client
            var nickName = GlobalManagers.Instance.NetworkRunnerController.LocalPlayerNickName;
            RpcSetNickName(nickName);
        }
    }

    //Send RPC to Host
    //RpcTargets defines on which it is executed!
    [Rpc(sources:RpcSources.InputAuthority, RpcTargets.StateAuthority)] //Client send to Server
    private void RpcSetNickName(NetworkString<_8> nickName)
    {
        playerName = nickName;
    }

    public void BeforeUpdate()
    {
        if(Runner.LocalPlayer == Object.HasInputAuthority)
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out PlayerData input))
        {
            rigidbody2D.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigidbody2D.velocity.y);

            CheckJumpInput(input);
        }

        playerVisualController.UpdateScaleTransform(rigidbody2D.velocity);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(buttonPrev);
        if(pressed.WasPressed(buttonPrev, PlayerInputButtons.Jump))
        {
            rigidbody2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        buttonPrev = input.NetworkButtons;
    }

    public override void Render()
    {
        playerVisualController.RendererVisuals(rigidbody2D.velocity, playerWeaponController.IsHoldingShootingKey);
    }

    public PlayerData GetPlayerDataInput()
    {
        PlayerData playerData = new PlayerData();
        playerData.HorizontalInput = horizontal;
        playerData.GunPivotRotation = playerWeaponController.LocalQuaternionPivotRot;
        playerData.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        playerData.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        return playerData;
    }
}
