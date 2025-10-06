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

    public GameObject optionsModalWindow;

    public float fadeInDuration = 1.0f;
    public float buttonInterval = 0.2f;
    public float transitionDuration = 0.3f;
    public float slideDistance = 60f;

    private Vector2[] finalPositions;

    void Start()
    {
        DOTween.Init();

        SetupGameSelectButtons();

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
        }
    }

    private void SetupGameSelectButtons()
    {
        finalPositions = new Vector2[gameSelectButtons.Length];

        for (int i = 0; i < gameSelectButtons.Length; i++)
        {
            CanvasGroup cg = gameSelectButtons[i];
            RectTransform rt = cg.GetComponent<RectTransform>();

            finalPositions[i] = rt.anchoredPosition;

            cg.alpha = 0;
            cg.interactable = false;
            cg.blocksRaycasts = false;

            rt.anchoredPosition = finalPositions[i] + new Vector2(0, slideDistance);
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

    public void OnBackClicked()
    {
        foreach (CanvasGroup buttonGroup in gameSelectButtons)
        {
            buttonGroup.interactable = false;
            buttonGroup.blocksRaycasts = false;
        }

        StartCoroutine(BackToMainMenu());
    }

    IEnumerator BackToMainMenu()
    {
        foreach (CanvasGroup buttonGroup in gameSelectButtons)
        {
            buttonGroup.DOFade(0, transitionDuration);

            RectTransform rt = buttonGroup.GetComponent<RectTransform>();
            int index = System.Array.IndexOf(gameSelectButtons, buttonGroup);

            rt.DOAnchorPosY(finalPositions[index].y + slideDistance, transitionDuration);
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
        foreach (CanvasGroup buttonGroup in mainOtherButtons)
        {
            buttonGroup.interactable = true;
            buttonGroup.blocksRaycasts = true;
        }
    }
}