using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button returnToLobby;
    [SerializeField] private GameObject childObj;

    private void Start()
    {
        GlobalManagers.Instance.GameManager.OnGameOver += GameManager_OnGameOver;
        returnToLobby.onClick.AddListener(() => GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner());
    }

    private void GameManager_OnGameOver()
    {
        childObj.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        GlobalManagers.Instance.GameManager.OnGameOver -= GameManager_OnGameOver;
    }
}
