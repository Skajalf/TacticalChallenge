using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Michsky.MUIP;

public class LoadingSceneUI : MonoBehaviour
{
    private static string nextScene;
    private static string nextGameMode;

    [Header("로딩 시간 설정")]
    [SerializeField] private float minLoadTime = 1.0f;

    [Header("UI 요소")]
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    private const string LoadingTextBase = "Loading";

    private float noiseTimeOffset;
    private float noiseScale = 0.5f;
    private float targetFakeProgress;

    public static void LoadScene(string sceneName, string gameMode = null)
    {
        nextScene = sceneName;
        nextGameMode = gameMode;
        SceneManager.LoadScene("LoadingScene");
    }

    public static string GetNextGameMode()
    {
        return nextGameMode;
    }

    public static float GetMinLoadingDuration()
    {
        return 1.0f;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("로드할 다음 씬 이름(nextScene)이 설정되지 않았습니다. 로비로 돌아갑니다.");
            SceneManager.LoadScene("LobbyScene");
            return;
        }

        if (progressBar != null)
        {
            progressBar.maxValue = 100f;
            progressBar.SetValue(0f);
        }

        loadingText.text = LoadingTextBase;
        noiseTimeOffset = Time.time;
        targetFakeProgress = 0f;
        StartCoroutine(LoadSceneProcess());
    }

    private IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float dotTimer = 0f;
        int dotCount = 0;
        float minTimeElapsed = 0f;

        float fakeProgress = 0f;
        bool isRealLoadComplete = false;

        while (!op.isDone)
        {
            yield return null;

            dotTimer += Time.deltaTime;
            if (dotTimer >= 0.5f)
            {
                dotTimer = 0f;
                dotCount = (dotCount + 1) % 4;
                loadingText.text = LoadingTextBase + new string('.', dotCount);
            }

            minTimeElapsed += Time.unscaledDeltaTime;

            bool isMinTimeMet = minTimeElapsed >= minLoadTime;

            if (op.progress >= 0.9f)
            {
                isRealLoadComplete = true;
            }

            float linearTarget = Mathf.Clamp01(minTimeElapsed / minLoadTime);

            if (fakeProgress < 0.99f)
            {
                float noise = Mathf.PerlinNoise(noiseTimeOffset + Time.time * noiseScale, 0f);
                float speed = Mathf.Lerp(1.5f, 5.0f, noise);

                targetFakeProgress = Mathf.Lerp(targetFakeProgress, linearTarget, Time.deltaTime * speed);

                fakeProgress = Mathf.Lerp(fakeProgress, targetFakeProgress, Time.deltaTime * speed);

                if (!isRealLoadComplete || !isMinTimeMet)
                {
                    fakeProgress = Mathf.Min(fakeProgress, 0.999f);
                }
            }
            else if (isRealLoadComplete && isMinTimeMet)
            {
                fakeProgress = 1.0f;
            }

            if (progressBar != null)
            {
                progressBar.SetValue(fakeProgress * 100f);
            }

            if (isRealLoadComplete && isMinTimeMet)
            {
                if (progressBar != null)
                {
                    progressBar.SetValue(100f);
                }

                op.allowSceneActivation = true;
                yield break;
            }
        }
    }
}