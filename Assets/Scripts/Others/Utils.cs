using Fusion;
using System.Collections;
using UnityEngine;

public static class Utils
{
    public static IEnumerator PlayAnimAndSetStateWhenFinished(GameObject parent, Animator animator, string clipName, bool activeStateAtTheEnd = true)
    {
        animator.Play(clipName);
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorClipInfo(0).Length);
        if(parent != null) parent.SetActive(activeStateAtTheEnd);
    }

    public static bool IsLocalPlayer(this NetworkObject networkObject)
    {
        return networkObject.IsValid == networkObject.HasInputAuthority;
    }
}
