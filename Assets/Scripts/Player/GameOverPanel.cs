using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private Button returnToLobby;
    [SerializeField] private GameObject childObj;
    [SerializeField] private TextMeshProUGUI rankText;

    private void Start() 
        => returnToLobby.onClick.AddListener(() => GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner());

    public void Show(int rank)
    {
        childObj.gameObject.SetActive(true);
        rankText.text = $"You are top #{rank}";
    }
}
