using System;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button returnToLobby;
    [SerializeField] private GameObject childObj;

    private void Start()
    {
        returnToLobby.onClick.AddListener(() => GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner());
    }

    private void GameManager_OnGameStateChange()
    {
            childObj.gameObject.SetActive(true);
    }
}
