using System.Collections;
using System;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private Animator fadeAnimator;

    /// <summary>
    /// Animates fade out then completes an action once completed.
    /// </summary>
    internal void FadeOut(Action onComplete)
    {
        StartCoroutine(FadeRoutine("Start", onComplete));
    }

    /// <summary>
    /// Animates fade back into screen.
    /// </summary>
    internal void FadeIn()
    {
        fadeAnimator.SetTrigger("Exit");
    }

    /// <summary>
    /// Sets the trigger for the fade then completes an action 
    /// </summary>
    private IEnumerator FadeRoutine(string trigger, Action onComplete)
    {
        fadeAnimator.SetTrigger(trigger);
        // Debug.Log("Fade Completed");
        yield return new WaitForSecondsRealtime(1f);
        // Debug.Log("Should invoke action");
        onComplete?.Invoke();
    }
}
