using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private float moveSpeed = 6;

    private float horizontal;
    private Rigidbody2D rigidbody2D;

    public override void Spawned()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
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
        }
    }

    public PlayerData GetPlayerDataInput()
    {
        PlayerData playerData = new PlayerData();
        playerData.HorizontalInput = horizontal;
        return playerData;
    }
}
