using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
using TMPro;

public class IntroManager : MonoBehaviour
{
    public Image fadePanel;
    public TextMeshProUGUI clickToStartText;
    public string lobbySceneName = "LobbyScene";
    public float fadeDuration = 1.0f;
    public float minLogoDisplayTime = 3.0f;
    public VideoPlayer logoVideoPlayer;
    public VideoPlayer titleVideoPlayer;

    private const float LOGO_TRANSITION_FADE_DURATION = 1.0f;

    private AsyncOperation asyncLoad;
    private bool isReadyToStart = false;
    private bool lobbyLoaded = false;
    private bool titlePlaying = false;
    private float logoPlayStartTime = 0f;

    void Start()
    {
        if (clickToStartText != null) clickToStartText.gameObject.SetActive(false);

        if (titleVideoPlayer != null) StartCoroutine(PrepareTitleVideo());

        if (logoVideoPlayer != null) logoVideoPlayer.Prepare();

        StartCoroutine(LoadLobbySceneAsync());
        StartCoroutine(WaitForVideoPreparationAndStart());
        if (logoVideoPlayer != null) logoVideoPlayer.loopPointReached += OnLogoVideoFinished;

        if (fadePanel != null)
        {
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 1f);
            StartCoroutine(FadeIn(fadeDuration));
        }
    }

    void Update()
    {
        if (isReadyToStart && Input.anyKeyDown)
        {
            StartLobbyTransition();
        }
    }

    IEnumerator PrepareTitleVideo()
    {
        if (titleVideoPlayer == null) yield break;

        titleVideoPlayer.Prepare();

        while (!titleVideoPlayer.isPrepared)
        {
            yield return null;
        }
    }

    IEnumerator WaitForVideoPreparationAndStart()
    {
        while (logoVideoPlayer != null && !logoVideoPlayer.isPrepared)
        {
            yield return null;
        }
        if (logoVideoPlayer != null)
        {
            logoPlayStartTime = Time.time;
            logoVideoPlayer.Play();
        }
    }

    void OnLogoVideoFinished(VideoPlayer vp)
    {
        if (logoVideoPlayer != null) logoVideoPlayer.loopPointReached -= OnLogoVideoFinished;

        StartCoroutine(EnforceMinTimeAndTransition());
    }

    IEnumerator EnforceMinTimeAndTransition()
    {
        float elapsedTime = Time.time - logoPlayStartTime;
        float timeToWait = minLogoDisplayTime - elapsedTime;

        if (timeToWait > 0f)
        {
            yield return new WaitForSeconds(timeToWait);
        }

        StartCoroutine(FadeLogoToTitle());
    }

    IEnumerator FadeLogoToTitle()
    {
        if (fadePanel == null)
        {
            StartCoroutine(ImmediateTitleTransition());
            yield break;
        }

        yield return StartCoroutine(FadeOut(LOGO_TRANSITION_FADE_DURATION));

        if (logoVideoPlayer != null) logoVideoPlayer.gameObject.SetActive(false);

        if (titleVideoPlayer != null)
        {
            titleVideoPlayer.gameObject.SetActive(true);
            while (!titleVideoPlayer.isPrepared)
            {
                yield return null;
            }
            titleVideoPlayer.isLooping = true;
            titleVideoPlayer.Play();
            titlePlaying = true;
        }

        yield return StartCoroutine(FadeIn(LOGO_TRANSITION_FADE_DURATION));

        TryEnableClick();
    }

    IEnumerator ImmediateTitleTransition()
    {
        if (logoVideoPlayer != null) logoVideoPlayer.gameObject.SetActive(false);

        if (titleVideoPlayer != null)
        {
            titleVideoPlayer.gameObject.SetActive(true);
            while (!titleVideoPlayer.isPrepared)
            {
                yield return null;
            }
            titleVideoPlayer.isLooping = true;
            titleVideoPlayer.Play();
            titlePlaying = true;
        }
        TryEnableClick();
    }

    IEnumerator LoadLobbySceneAsync()
    {
        asyncLoad = SceneManager.LoadSceneAsync(lobbySceneName);
        if (asyncLoad == null) yield break;
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }
        lobbyLoaded = true;
        TryEnableClick();
    }

    void TryEnableClick()
    {
        if (lobbyLoaded && titlePlaying && !isReadyToStart)
        {
            isReadyToStart = true;
            if (clickToStartText != null) clickToStartText.gameObject.SetActive(true);
            if (fadePanel != null) fadePanel.raycastTarget = false;
        }
    }

    void StartLobbyTransition()
    {
        if (!isReadyToStart) return;
        isReadyToStart = false;
        StartCoroutine(FadeOutAndLoad(fadeDuration));
    }

    IEnumerator FadeOutAndLoad(float duration)
    {
        if (fadePanel == null)
        {
            if (asyncLoad != null) asyncLoad.allowSceneActivation = true;
            yield break;
        }

        yield return StartCoroutine(FadeOut(duration));

        if (asyncLoad != null) asyncLoad.allowSceneActivation = true;
    }

    IEnumerator FadeIn(float duration)
    {
        if (fadePanel == null) yield break;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / duration);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0f);
        if (fadePanel != null) fadePanel.raycastTarget = false;
    }

    IEnumerator FadeOut(float duration)
    {
        if (fadePanel == null) yield break;
        if (fadePanel != null) fadePanel.raycastTarget = true;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, alpha);
            yield return null;
        }
        fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 1f);
    }
}