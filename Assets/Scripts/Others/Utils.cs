using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static IEnumerator PlayAnimAndSetStateWhenFinished(GameObject parent, Animator animator, string clipName, bool activeStateAtTheEnd = true)
    {
        animator.Play(clipName);
        yield return new WaitForSecondsRealtime(animator.GetCurrentAnimatorClipInfo(0).Length);
        if(parent != null) parent.SetActive(activeStateAtTheEnd);
    }
}
