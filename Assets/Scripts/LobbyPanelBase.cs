using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPanelBase : MonoBehaviour
{
    [SerializeField] private LobbyPanelType panelType;
    [SerializeField] private Animator panelAnimator;

    public LobbyPanelType PanelType => panelType;

    protected LobbyUIManager lobbyUIManager;

    public enum LobbyPanelType
    {
        None,
        CreateNickNamePanel,
        MiddleSectionPanel
    }

    private enum AnimatorParameter
    {
        In, Out
    }

    public virtual void InitPanel(LobbyUIManager manager)
    {
        lobbyUIManager = manager;
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
        this.panelAnimator.Play(AnimatorParameter.In.ToString());
    }

    public void ClosePanel()
    {
        StartCoroutine(Utils.PlayAnimAndSetStateWhenFinished(gameObject, panelAnimator, AnimatorParameter.Out.ToString(), false));
    }
}
