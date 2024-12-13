using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] private LoadingCanvasController loadingCanvasControllerPrefab;
    [SerializeField] private LobbyPanelBase[] lobbyPanels;

    private void Start()
    {
        foreach (var lobby in lobbyPanels)
        {
            lobby.InitPanel(this);
        }

        Instantiate(loadingCanvasControllerPrefab);
    }

    public void ShowPanel(LobbyPanelBase.LobbyPanelType panelType)
    {
        foreach (var panel in lobbyPanels)
        {
            if (panel.PanelType == panelType)
            {
                panel.ShowPanel();
            }
            else if(panel.gameObject.activeSelf)
            {
                panel.ClosePanel();
            }
        }
    }
}
