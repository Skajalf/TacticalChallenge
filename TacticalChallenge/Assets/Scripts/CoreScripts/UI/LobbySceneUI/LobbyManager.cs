using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public CanvasGroup[] menuButtons;

    public float fadeInDuration = 1.0f;
    public float buttonInterval = 0.2f;

    void Start()
    {
        DOTween.Init();

        StartCoroutine(AnimateButtonsSequentially());
    }

    IEnumerator AnimateButtonsSequentially()
    {
        if (menuButtons == null || menuButtons.Length == 0)
        {
            yield break;
        }

        foreach (CanvasGroup buttonGroup in menuButtons)
        {
            buttonGroup.alpha = 0;
            buttonGroup.DOFade(1, fadeInDuration);

            yield return new WaitForSeconds(buttonInterval);
        }
    }
}