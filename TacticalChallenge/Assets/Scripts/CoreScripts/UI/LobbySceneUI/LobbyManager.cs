using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class OptionCategory
{
    public Button categoryButton;
    public GameObject[] contentPanels;
}

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

    public Slider masterVolumeSlider;
    public AudioMixer mainAudioMixer;
    private const string MASTER_VOLUME_PARAM = "MasterVolume";

    public string scenarioModeSceneName = "ScenarioModeScene";
    public string defenceModeSceneName = "DefenceModeScene";

    private Vector2[] finalPositions;
    private Vector2[] gameSelectOriginalPositions;

    private bool isSinglePlayExpanded = false;

    public List<OptionCategory> optionCategories;
    private OptionCategory currentActiveCategory;

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

        if (masterVolumeSlider != null && mainAudioMixer != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

            float savedVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_PARAM, 1f);
            masterVolumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);
        }

        if (singlePlaySubButtons != null && singlePlaySubButtons.Length > 1)
        {
            Button scenarioButton = singlePlaySubButtons[0].GetComponent<Button>();
            if (scenarioButton != null)
            {
                scenarioButton.onClick.RemoveAllListeners();
                scenarioButton.onClick.AddListener(StartScenarioMode);
            }

            Button defenceButton = singlePlaySubButtons[1].GetComponent<Button>();
            if (defenceButton != null)
            {
                defenceButton.onClick.RemoveAllListeners();
                defenceButton.onClick.AddListener(StartDefenceMode);
            }
        }

        bool wasOptionsActive = false;

        if (optionsModalWindow != null && !optionsModalWindow.activeSelf)
        {
            optionsModalWindow.SetActive(true);
            wasOptionsActive = true;
        }

        SetupOptionCategoryButtons();

        if (optionsModalWindow != null && wasOptionsActive)
        {
            optionsModalWindow.SetActive(false);
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

    private void SetupOptionCategoryButtons()
    {
        if (optionCategories == null || optionCategories.Count == 0)
        {
            Debug.LogError("Option Categories 리스트가 설정되지 않았습니다. 인스펙터 설정을 확인하세요.");
            return;
        }

        for (int i = 0; i < optionCategories.Count; i++)
        {
            OptionCategory category = optionCategories[i];

            if (category.categoryButton == null) continue;

            int index = i;

            category.categoryButton.onClick.RemoveAllListeners();
            category.categoryButton.onClick.AddListener(() => OnCategoryButtonClicked(index));

            if (i == 0)
            {
                currentActiveCategory = category;

                foreach (GameObject panel in category.contentPanels)
                {
                    if (panel != null) panel.SetActive(true);
                }
            }
            else
            {
                foreach (GameObject panel in category.contentPanels)
                {
                    if (panel != null) panel.SetActive(false);
                }
            }
        }
    }

    public void OnCategoryButtonClicked(int categoryIndex)
    {
        OptionCategory newCategory = optionCategories[categoryIndex];

        if (newCategory == currentActiveCategory)
        {
            return;
        }

        if (currentActiveCategory != null)
        {
            foreach (GameObject panel in currentActiveCategory.contentPanels)
            {
                if (panel != null) panel.SetActive(false);
            }
        }

        foreach (GameObject panel in newCategory.contentPanels)
        {
            if (panel != null) panel.SetActive(true);
        }

        currentActiveCategory = newCategory;
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

        if (optionCategories != null && optionCategories.Count > 0)
        {
            if (currentActiveCategory != null)
            {
                foreach (GameObject panel in currentActiveCategory.contentPanels)
                {
                    if (panel != null) panel.SetActive(false);
                }
            }

            OptionCategory firstCategory = optionCategories[0];
            currentActiveCategory = firstCategory;

            foreach (GameObject panel in firstCategory.contentPanels)
            {
                if (panel != null) panel.SetActive(true);
            }
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

    public void SetMasterVolume(float value)
    {
        if (mainAudioMixer == null) return;

        float dB = Mathf.Log10(value) * 20;

        mainAudioMixer.SetFloat(MASTER_VOLUME_PARAM, dB);

        PlayerPrefs.SetFloat(MASTER_VOLUME_PARAM, value);
        PlayerPrefs.Save();
    }

    public void StartScenarioMode()
    {
        if (string.IsNullOrEmpty(scenarioModeSceneName))
        {
            Debug.LogError("Scenario Mode Scene Name이 LobbyManager 컴포넌트에 설정되지 않았습니다.");
            return;
        }
        LoadingSceneUI.LoadScene(scenarioModeSceneName);
    }

    public void StartDefenceMode()
    {
        if (string.IsNullOrEmpty(defenceModeSceneName))
        {
            Debug.LogError("Defence Mode Scene Name이 LobbyManager 컴포넌트에 설정되지 않았습니다.");
            return;
        }

        LoadingSceneUI.LoadScene(defenceModeSceneName);
    }
}