using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class FormationSceneUI : MonoBehaviour
{
    [System.Serializable]
    public class SpecialSkillData
    {
        public Button skillButton;
        public Sprite skillImage;
    }

    [System.Serializable]
    public class CharacterData
    {
        public Button characterButton;
        public Image characterImageDisplay;
        public Sprite characterImage;
        public GameObject selectedCharacterPanelPrefab;

        [Header("Skill 4 UI Links (Inside Panel)")]
        public Button skill4Button;
        public Image skill4ImageDisplay;
        public GameObject specialSkillPanel;

        [Header("Character Specific Skills")]
        public SpecialSkillData[] characterSpecialSkills;
    }

    [Header("1. Initial State UI")]
    public Button gameStartButton;
    public Button returnButton;
    public GameObject defaultSelectedPanel;

    [Header("2. Character Pool Data")]
    public CharacterData[] allCharacters;

    [Header("3. Scene Configuration")]
    public string lobbySceneName = "LobbyScene";
    public string scenarioModeSceneName = "ScenarioModeScene";
    public string defenceModeSceneName = "DefenceModeScene";

    private string currentSelectedGameMode;
    private GameObject currentSelectedPanel = null;
    private CharacterData currentCharacterData = null;

    void Start()
    {
        currentSelectedGameMode = LoadingSceneUI.GetNextGameMode();

        if (string.IsNullOrEmpty(currentSelectedGameMode))
        {
            Debug.LogError("로드할 게임 모드 정보가 Lobby Scene에서 전달되지 않았습니다. 로비로 복귀합니다.");
            ReturnToLobby();
            return;
        }

        SetupSceneButtons();

        if (gameStartButton != null)
        {
            gameStartButton.interactable = false;
        }

        if (defaultSelectedPanel != null)
        {
            defaultSelectedPanel.SetActive(true);
        }

        foreach (var data in allCharacters)
        {
            if (data.specialSkillPanel != null)
            {
                data.specialSkillPanel.SetActive(false);
            }
        }

        SetupCharacterButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectCharacter();
        }
    }

    void SetupSceneButtons()
    {
        if (gameStartButton != null)
        {
            gameStartButton.onClick.RemoveAllListeners();
            gameStartButton.onClick.AddListener(StartGameForMode);
        }

        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(ReturnToLobby);
        }
    }

    public void ReturnToLobby()
    {
        //if (string.IsNullOrEmpty(lobbySceneName))
        //{
        //    Debug.LogError("Lobby Scene Name이 설정되지 않았습니다.");
        //    return;
        //}
        //LoadingSceneUI.LoadScene(lobbySceneName);

        if (string.IsNullOrEmpty(lobbySceneName))
        {
            Debug.LogError("Lobby Scene Name이 설정되지 않았습니다.");
            return;
        }
        // 로딩 씬을 건너뛰고 바로 LobbyScene 로드
        SceneManager.LoadScene(lobbySceneName);
    }

    public void StartGameForMode()
    {
        string targetSceneName = "";

        if (currentSelectedGameMode == "ScenarioMode")
        {
            targetSceneName = scenarioModeSceneName;
        }
        else if (currentSelectedGameMode == "DefenceMode")
        {
            targetSceneName = defenceModeSceneName;
        }
        else
        {
            Debug.LogError($"알 수 없는 게임 모드입니다: {currentSelectedGameMode}");
            return;
        }

        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"{currentSelectedGameMode}의 씬 이름이 설정되지 않았습니다.");
            return;
        }

        LoadingSceneUI.LoadScene(targetSceneName);
    }

    void SetupCharacterButtons()
    {
        foreach (var data in allCharacters)
        {
            if (data.characterImageDisplay != null && data.characterImage != null)
            {
                data.characterImageDisplay.sprite = data.characterImage;
            }

            if (data.skill4Button != null)
            {
                data.skill4Button.onClick.RemoveAllListeners();
                data.skill4Button.onClick.AddListener(() => OpenSpecialSkillPanel(data));
            }

            if (data.characterButton != null && data.selectedCharacterPanelPrefab != null)
            {
                GameObject panel = data.selectedCharacterPanelPrefab;
                data.characterButton.onClick.RemoveAllListeners();
                data.characterButton.onClick.AddListener(() => SelectCharacter(panel));
            }
        }
    }

    void SetupSpecialSkillButtons(CharacterData data)
    {
        foreach (var skillData in data.characterSpecialSkills)
        {
            if (skillData.skillButton != null)
            {
                skillData.skillButton.onClick.RemoveAllListeners();

                Image buttonImage = skillData.skillButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = skillData.skillImage;
                }

                skillData.skillButton.onClick.AddListener(() => ChangeSkill4ImageAndClose(skillData.skillImage));
            }
        }
    }

    public void OpenSpecialSkillPanel(CharacterData data)
    {
        if (data != currentCharacterData || data.specialSkillPanel == null) return;

        GameObject panel = data.specialSkillPanel;

        panel.SetActive(!panel.activeSelf);
    }

    public void ChangeSkill4ImageAndClose(Sprite newSkillSprite)
    {
        if (currentCharacterData == null) return;

        if (currentCharacterData.skill4ImageDisplay != null && newSkillSprite != null)
        {
            currentCharacterData.skill4ImageDisplay.sprite = newSkillSprite;
        }

        if (currentCharacterData.specialSkillPanel != null)
        {
            currentCharacterData.specialSkillPanel.SetActive(false);
        }
    }

    private CharacterData FindCharacterDataByPanel(GameObject panel)
    {
        return allCharacters.FirstOrDefault(data => data.selectedCharacterPanelPrefab == panel);
    }

    public void SelectCharacter(GameObject panelToActivate)
    {
        if (defaultSelectedPanel != null && defaultSelectedPanel.activeSelf)
        {
            defaultSelectedPanel.SetActive(false);
        }

        if (currentSelectedPanel != null)
        {
            if (currentCharacterData != null && currentCharacterData.specialSkillPanel != null)
            {
                currentCharacterData.specialSkillPanel.SetActive(false);
            }
            currentSelectedPanel.SetActive(false);
        }

        if (panelToActivate != null)
        {
            panelToActivate.SetActive(true);
            currentSelectedPanel = panelToActivate;

            currentCharacterData = FindCharacterDataByPanel(panelToActivate);

            if (currentCharacterData != null)
            {
                SetupSpecialSkillButtons(currentCharacterData);
            }

            if (gameStartButton != null)
            {
                gameStartButton.interactable = true;
            }
        }
    }

    public void DeselectCharacter()
    {
        if (currentCharacterData != null)
        {
            if (currentCharacterData.specialSkillPanel != null)
            {
                currentCharacterData.specialSkillPanel.SetActive(false);
            }
        }

        if (currentSelectedPanel != null)
        {
            currentSelectedPanel.SetActive(false);
            currentSelectedPanel = null;
        }

        currentCharacterData = null;

        if (gameStartButton != null)
        {
            gameStartButton.interactable = false;
        }

        if (defaultSelectedPanel != null)
        {
            defaultSelectedPanel.SetActive(true);
        }
    }
}