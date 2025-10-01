using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections;
using TMPro; // TextMeshPro 사용을 위한 네임스페이스

public class IntroManager : MonoBehaviour
{
    // === UI 및 Scene 설정 (Inspector에서 연결) ===
    public Image fadePanel;
    public TextMeshProUGUI clickToStartText; // TMP 대응
    public string lobbySceneName = "LobbyScene";
    public float fadeDuration = 1.0f;
    public float minLogoDisplayTime = 3.0f; // 로고의 최소 노출 시간 (초)

    // === VideoPlayer 설정 ===
    public VideoPlayer logoVideoPlayer;
    public VideoPlayer titleVideoPlayer;

    private AsyncOperation asyncLoad;
    private bool isReadyToStart = false;

    void Start()
    {
        // 초기화: 타이틀 비디오와 텍스트는 숨깁니다.
        titleVideoPlayer.gameObject.SetActive(false);
        clickToStartText.gameObject.SetActive(false);

        // 1. 비디오 Prepare 호출 및 로딩 시작
        logoVideoPlayer.Prepare();
        StartCoroutine(LoadLobbySceneAsync());

        // 2. 준비와 최소 시간을 기다리는 핵심 코루틴 실행
        StartCoroutine(WaitForVideoPreparationAndStart());

        // 3. 로고 동영상의 재생 완료 이벤트 구독
        logoVideoPlayer.loopPointReached += OnLogoVideoFinished;

        // **주의: logoVideoPlayer.Play()와 StartCoroutine(FadeIn())은 WaitForVideoPreparationAndStart() 내부에서 호출됩니다.**
    }

    void Update()
    {
        // 로딩 완료 + 클릭 대기 중일 때 입력 감지
        if (isReadyToStart && Input.anyKeyDown)
        {
            StartLobbyTransition();
        }
    }

    // ===================================
    // == 1. 비디오 준비 및 최소 시간 관리 ==
    // ===================================
    IEnumerator WaitForVideoPreparationAndStart()
    {
        float startTime = Time.time;

        // 1. 비디오 준비 대기
        while (!logoVideoPlayer.isPrepared)
        {
            yield return null;
        }

        // 2. 준비 완료 후 재생 및 페이드 인 시작
        logoVideoPlayer.Play();
        StartCoroutine(FadeIn());

        // 3. 최소 노출 시간 강제 대기
        float elapsedTime = Time.time - startTime;
        float remainingTime = minLogoDisplayTime - elapsedTime;

        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }
    }

    // =============================
    // == 2. 로고 종료 및 타이틀 전환 ==
    // =============================
    void OnLogoVideoFinished(VideoPlayer vp)
    {
        logoVideoPlayer.loopPointReached -= OnLogoVideoFinished;
        // 로고 종료 시 1초 대기 후 타이틀로 전환
        StartCoroutine(TransitionToTitleAfterDelay(1.0f));
    }

    IEnumerator TransitionToTitleAfterDelay(float delay)
    {
        // 1초간 대기 (로고 영상의 마지막 프레임 또는 검은 화면 유지)
        yield return new WaitForSeconds(delay);

        // 대기 후 타이틀 비디오로 전환 시작
        logoVideoPlayer.gameObject.SetActive(false);
        titleVideoPlayer.gameObject.SetActive(true);

        // 타이틀 비디오는 무한 반복 설정
        titleVideoPlayer.isLooping = true;
        titleVideoPlayer.Play();
    }


    // ========================
    // == 3. 비동기 로딩 관리 ==
    // ========================
    IEnumerator LoadLobbySceneAsync()
    {
        // 로딩 시작 직전에 GC를 강제 실행하여 로딩 중 랜덤 멈춤 방지 (최적화)
        System.GC.Collect();

        asyncLoad = SceneManager.LoadSceneAsync(lobbySceneName);

        // NullReferenceException 방지
        if (asyncLoad == null)
        {
            Debug.LogError($"씬 로딩 실패: '{lobbySceneName}'을 Build Settings에 추가했는지 확인하세요.");
            yield break;
        }

        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        OnLoadingComplete();
    }

    void OnLoadingComplete()
    {
        // 로딩 완료 후, 타이틀 동영상이 재생 중일 때만 텍스트를 띄웁니다.
        if (titleVideoPlayer.isPlaying)
        {
            isReadyToStart = true;
            clickToStartText.gameObject.SetActive(true);
        }
    }

    // ========================
    // == 4. 페이드 및 전환 ==
    // ========================
    IEnumerator FadeIn()
    {
        // 로고가 밝아지며 등장하는 페이드 인
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0f, 0f, 0f, 0f);
    }

    void StartLobbyTransition()
    {
        // 로비 씬으로 넘어가기 위해 모든 입력 무시
        isReadyToStart = false;

        // 최종 페이드 아웃 시작
        StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        // 화면이 검은색으로 어두워지는 페이드 아웃
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0f, 0f, 0f, 1f);

        // 검은 화면 상태에서 씬 활성화
        asyncLoad.allowSceneActivation = true;
    }
}