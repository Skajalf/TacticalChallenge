using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{

    public CanvasGroup[] menuButtons;

    public Button gameStartButton;
    public CanvasGroup[] mainOtherButtons;

    public CanvasGroup[] gameSelectButtons;

    public CanvasGroup[] singlePlaySubButtons;

    public GameObject optionsModalWindow;
    public GameObject creditsModalWindow;

    public float fadeInDuration = 1.0f;
    public float buttonInterval = 0.2f;
    public float transitionDuration = 0.3f;
    public float slideDistance = 60f;

    public float pushDistance = 120f;

    private Vector2[] finalPositions;
    private Vector2[] gameSelectOriginalPositions;

    private bool isSinglePlayExpanded = false;

    void Start()
    {
        DOTween.Init();

        SetupGameSelectButtons();

        SetupSinglePlaySubButtons();

        StartCoroutine(AnimateButtonsSequentially());

        if (gameStartButton == null)
        {
            if (menuButtons != null && menuButtons.Length > 0)
            {
                gameStartButton = menuButtons[0].GetComponent<Button>();
            }
        }

        if (gameStartButton != null)
        {
            gameStartButton.onClick.RemoveAllListeners();
            gameStartButton.onClick.AddListener(OnGameStartClicked);
        }

        if (gameSelectButtons != null && gameSelectButtons.Length > 0)
        {
            Button backButton = gameSelectButtons[gameSelectButtons.Length - 1].GetComponent<Button>();
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
            }

            Button singlePlayButton = gameSelectButtons[0].GetComponent<Button>();
            if (singlePlayButton != null)
            {
                singlePlayButton.onClick.RemoveAllListeners();
                singlePlayButton.onClick.AddListener(OnSinglePlayClicked);
            }
        }

        if (mainOtherButtons != null && mainOtherButtons.Length > 0)
        {
            Button exitButton = mainOtherButtons[mainOtherButtons.Length - 1].GetComponent<Button>();
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(OnExitGame);
            }

            Button optionsButton = mainOtherButtons[0].GetComponent<Button>();
            if (optionsButton != null)
            {
                optionsButton.onClick.RemoveAllListeners();
                optionsButton.onClick.AddListener(OnOptionsClicked);
            }

            if (mainOtherButtons.Length > 1)
            {
                Button creditsButton = mainOtherButtons[1].GetComponent<Button>();
                if (creditsButton != null)
                {
                    creditsButton.onClick.RemoveAllListeners();
                    creditsButton.onClick.AddListener(OnCreditsClicked);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsModalWindow != null && optionsModalWindow.activeSelf)
            {
                CloseOptionsPanel();
            }
            else if (creditsModalWindow != null && creditsModalWindow.activeSelf)
            {
                CloseCreditsPanel();
            }
            else if (gameSelectButtons != null && gameSelectButtons.Length > 0 && isSinglePlayExpanded)
            {
                StartCoroutine(CloseSinglePlayMenu());
            }
            else if (gameSelectButtons != null && gameSelectButtons.Length > 0 && gameSelectButtons[0].alpha > 0)
            {
                OnBackClicked();
            }
        }
    }

    private void SetupGameSelectButtons()
    {
        finalPositions = new Vector2[gameSelectButtons.Length];
        gameSelectOriginalPositions = new Vector2[gameSelectButtons.Length];

        for (int i = 0; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup cg = gameSelectButtons[i];
            RectTransform rt = cg.GetComponent<RectTransform>();

            finalPositions[i] = rt.anchoredPosition;
            gameSelectOriginalPositions[i] = rt.anchoredPosition;

            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            rt.anchoredPosition = finalPositions[i] + new Vector2(0, slideDistance);
        }
    }

    private void SetupSinglePlaySubButtons()
    {
        if (singlePlaySubButtons == null) return;

        for (int i = 0; i < singlePlaySubButtons.Length; i++)
        {
            CanvasGroup cg = singlePlaySubButtons[i];

            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    IEnumerator AnimateButtonsSequentially()
    {
        if (menuButtons == null || menuButtons.Length == 0) yield break;

        foreach (CanvasGroup buttonGroup in menuButtons)
        {
            buttonGroup.alpha = 0;

            buttonGroup.DOFade(1, fadeInDuration);

            yield return new WaitForSeconds(buttonInterval);
        }
    }

    public void OnGameStartClicked()
    {
        if (!gameStartButton.interactable) return;

        gameStartButton.interactable = false;
        CanvasGroup gameStartCG = gameStartButton.GetComponent<CanvasGroup>();
        if (gameStartCG != null)
        {
            gameStartCG.blocksRaycasts = false;
        }

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        StartCoroutine(GameMenuTransition());
    }

    IEnumerator GameMenuTransition()
    {
        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.DOFade(0, transitionDuration);
        }

        yield return new WaitForSeconds(transitionDuration);

        for (int i = 0; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup buttonGroup = gameSelectButtons[i];
            RectTransform rt = buttonGroup.GetComponent<RectTransform>();

            buttonGroup.DOFade(1, transitionDuration);

            rt.DOAnchorPosY(finalPositions[i].y, transitionDuration).SetEase(Ease.OutBack);

            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;

            yield return new WaitForSeconds(buttonInterval);
        }
    }

    public void OnSinglePlayClicked()
    {
        CanvasGroup singlePlayCG = gameSelectButtons[0];
        if (!singlePlayCG.interactable) return;

        if (isSinglePlayExpanded)
        {
            StartCoroutine(CloseSinglePlayMenu());
        }
        else
        {
            StartCoroutine(OpenSinglePlayMenu());
        }
    }

    IEnumerator OpenSinglePlayMenu()
    {
        isSinglePlayExpanded = true;

        CanvasGroup singlePlayCG = gameSelectButtons[0];
        RectTransform singlePlayRT = singlePlayCG.GetComponent<RectTransform>();

        singlePlayCG.interactable = false;
        singlePlayCG.blocksRaycasts = false;

        singlePlayRT.DOAnchorPosY(gameSelectOriginalPositions[0].y - 5f, transitionDuration * 0.5f);

        for (int i = 1; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup buttonGroup = gameSelectButtons[i];
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        for (int i = 1; i < gameSelectButtons.Length; i++)
        {
            RectTransform rt = gameSelectButtons[i].GetComponent<RectTransform>();
            float targetY = gameSelectOriginalPositions[i].y - pushDistance;

            rt.DOAnchorPosY(targetY, transitionDuration).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(transitionDuration);

        for (int i = 0; i < singlePlaySubButtons.Length; i++)
        {
            CanvasGroup subButtonCG = singlePlaySubButtons[i];

            subButtonCG.DOFade(1, buttonInterval);

            subButtonCG.interactable = true;
            subButtonCG.blocksRaycasts = true;

            yield return new WaitForSeconds(buttonInterval);
        }

        singlePlayCG.interactable = true;
        singlePlayCG.blocksRaycasts = true;

        for (int i = 1; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup buttonGroup = gameSelectButtons[i];
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }

    IEnumerator CloseSinglePlayMenu()
    {
        isSinglePlayExpanded = false;

        CanvasGroup singlePlayCG = gameSelectButtons[0];
        RectTransform singlePlayRT = singlePlayCG.GetComponent<RectTransform>();

        singlePlayCG.interactable = false;
        singlePlayCG.blocksRaycasts = false;

        foreach (CanvasGroup buttonGroup in gameSelectButtons)
        {
            if (buttonGroup != singlePlayCG)
            {
                buttonGroup.interactable = false;
                buttonGroup.blocksRaycasts = false;
            }
        }
        foreach (CanvasGroup subButtonCG in singlePlaySubButtons)
        {
            subButtonCG.interactable = false;
            subButtonCG.blocksRaycasts = false;
        }

        foreach (CanvasGroup subButtonCG in singlePlaySubButtons)
        {
            subButtonCG.DOFade(0, transitionDuration);
        }

        singlePlayRT.DOAnchorPosY(gameSelectOriginalPositions[0].y, transitionDuration * 0.5f);

        for (int i = 1; i < gameSelectButtons.Length; i++)
        {
            RectTransform rt = gameSelectButtons[i].GetComponent<RectTransform>();
            float targetY = gameSelectOriginalPositions[i].y;

            rt.DOAnchorPosY(targetY, transitionDuration).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(transitionDuration);

        foreach (CanvasGroup buttonGroup in gameSelectButtons)
        {
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }

    public void OnBackClicked()
    {
        foreach (CanvasGroup buttonGroup in gameSelectButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }
        foreach (CanvasGroup buttonGroup in singlePlaySubButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        if (isSinglePlayExpanded)
        {
            isSinglePlayExpanded = false;
        }

        StartCoroutine(BackToMainMenu());
    }

    IEnumerator BackToMainMenu()
    {
        for (int i = 0; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup buttonGroup = gameSelectButtons[i];
            buttonGroup.DOFade(0, transitionDuration);

            RectTransform rt = buttonGroup.GetComponent<RectTransform>();

            float originalY = gameSelectOriginalPositions[i].y;

            rt.DOKill();
            rt.DOAnchorPosY(originalY + slideDistance, transitionDuration);
        }

        foreach (CanvasGroup subButtonCG in singlePlaySubButtons)
        {
            subButtonCG.DOFade(0, transitionDuration);
        }

        yield return new WaitForSeconds(transitionDuration);

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.DOFade(1, transitionDuration);

            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;

            yield return new WaitForSeconds(buttonInterval);
        }

        if (gameStartButton != null)
        {
            gameStartButton.interactable = true;
            gameStartButton.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
    }

    public void OnExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }

    public void OnOptionsClicked()
    {
        if (optionsModalWindow == null) return;

        gameStartButton.interactable = false;
        gameStartButton.GetComponent<CanvasGroup>().blocksRaycasts = false;

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        optionsModalWindow.SetActive(true);
    }

    public void CloseOptionsPanel()
    {
        if (optionsModalWindow == null) return;

        optionsModalWindow.SetActive(false);

        gameStartButton.interactable = true;
        gameStartButton.GetComponent<CanvasGroup>().blocksRaycasts = true;

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }

    public void OnCreditsClicked()
    {
        if (creditsModalWindow == null) return;

        gameStartButton.interactable = false;
        gameStartButton.GetComponent<CanvasGroup>().blocksRaycasts = false;

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        creditsModalWindow.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        if (creditsModalWindow == null) return;

        creditsModalWindow.SetActive(false);

        gameStartButton.interactable = true;
        gameStartButton.GetComponent<CanvasGroup>().blocksRaycasts = true;

        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }
}