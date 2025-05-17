using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Networking.Transport.Relay;

public class MainMenuController : MonoBehaviour
{
    enum MenuStage
    {
        MainMenu,
        ModesMenu,
        LevelSelection,
        DifficultySelection,
        Settings,
        Multiplayer,
        MultiPlayerLobby,
        MulitplayerJoin,
        QuitGame
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CanvasGroup MainMenuButtons;
    public CanvasGroup ModesMenu;
    public CanvasGroup LevelSelector;
    public CanvasGroup LevelsMenu;
    public CanvasGroup DiffucultyMenu;
    public CanvasGroup SettingMenu;
    public CanvasGroup MultiPlayerMenu;
    public CanvasGroup MultiPlayerLobby;
    public CanvasGroup MultiPlayerJoin;
    public CanvasGroup QuitMenu;

    public CanvasGroup TestGroup;

    private Dictionary<MenuStage, CanvasGroup> menuDictionary;

    private MenuStage menuStage;

    private string LevelName;

    public MultiPlayerLobby lobby;

    public CanvasGroup errorMessage;

    public TMP_InputField joinCodeInputField;
    public RectTransform SpawnPos;
    public Transform playerIconHolderTransform;
    public GameObject playerIconPrefab;

    public List<GameObject> playerIcons = new List<GameObject>();

    int playerIconSpacing = 100;

    private int levelSelected = 0;

    bool isMultiplayer = false;

    public static MainMenuController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        menuDictionary = new Dictionary<MenuStage, CanvasGroup>
        {
            { MenuStage.MainMenu, MainMenuButtons },
            { MenuStage.ModesMenu, ModesMenu },
            { MenuStage.LevelSelection, LevelSelector },
            { MenuStage.DifficultySelection, DiffucultyMenu },
            { MenuStage.Settings, SettingMenu },
            { MenuStage.Multiplayer, MultiPlayerMenu },
            { MenuStage.MultiPlayerLobby, MultiPlayerLobby },
            { MenuStage.MulitplayerJoin, MultiPlayerJoin },
            { MenuStage.QuitGame, QuitMenu }
        };

        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.ModesMenu]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.DifficultySelection]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.LevelSelection]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.Settings]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.QuitGame]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.DifficultySelection]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.Multiplayer]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.MultiPlayerLobby]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.MulitplayerJoin]);
        CanvasGroupController.DisableGroup(errorMessage);

        CanvasGroupController.DisableGroup(TestGroup);

        menuStage = MenuStage.MainMenu;
    }

    public void OpenMultiplayer()
    {
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.ModesMenu]);
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.Multiplayer]);
        menuStage = MenuStage.Multiplayer;

        isMultiplayer = true;
    }
    public void OpenSettingsMenu()
    {
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.MainMenu]);
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.Settings]);

        menuStage = MenuStage.Settings;
    }

    public void OpenLevelSelector()
    {
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.MainMenu]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.ModesMenu]);
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.Multiplayer]);
        CanvasGroupController.EnableGroup(LevelsMenu);

        UIGeneralController.ToggleUI(menuDictionary[MenuStage.LevelSelection]);

        menuStage = MenuStage.LevelSelection;
    }

    public void OpenDifficultySelector(string _level)
    {
        LevelName = _level;

        CanvasGroupController.DisableGroup(LevelsMenu);
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.DifficultySelection]);

        menuStage = MenuStage.DifficultySelection;
    }

    public void OpenLobby(string _code)
    {
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.Multiplayer]);
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.MultiPlayerLobby]);

        MultiPlayerLobby.transform.Find("LobbyCode").GetComponent<TextMeshProUGUI>().text = "Code : " + _code;

        if (Unity.Netcode.NetworkManager.Singleton.IsHost)
        {
            MultiPlayerLobby.transform.Find("StartButton").gameObject.SetActive(true);
        }
        else
        {
            MultiPlayerLobby.transform.Find("StartButton").gameObject.SetActive(false);
        }


        menuStage = MenuStage.MultiPlayerLobby;
    }

    public void OpenJoinLobby()
    {
        CanvasGroupController.DisableGroup(menuDictionary[MenuStage.Multiplayer]);
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.MulitplayerJoin]);
        menuStage = MenuStage.MulitplayerJoin;
    }

    public async void OpenLobbyJoined(string _code)
    {
        await UIGeneralController.CloseUI(menuDictionary[MenuStage.MulitplayerJoin]);
        CanvasGroupController.DisableGroup(errorMessage);
        OpenLobby(_code);
        menuStage = MenuStage.MultiPlayerLobby;
    }

    public void DisplayLobbyError()
    {
        CanvasGroupController.EnableGroup(errorMessage);
    }
    public void JoinLobby()
    {
        string joinCode = joinCodeInputField.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.LogWarning("Join code is empty!");
            return;
        }
        
        lobby.JoinGame(joinCode);
    }

    public void AddNewPlayer(int index)
    {
        Debug.Log($"AddNewPlayer called. Player index: {index}");

        GameObject playerIcon = Instantiate(playerIconPrefab, playerIconHolderTransform);

        playerIcon.GetComponent<RectTransform>().localPosition =
            new Vector3(SpawnPos.localPosition.x + (index * playerIconSpacing),
                        SpawnPos.localPosition.y,
                        SpawnPos.localPosition.z);

        playerIcons.Add(playerIcon);
    }


    public void RemovePlayerIcon(int playerIndex)
    {
        if (playerIndex >= 0 && playerIndex < playerIcons.Count)
        {
            GameObject iconToRemove = playerIcons[playerIndex];

            // Destroy the icon
            Destroy(iconToRemove);

            // Remove the icon from the list
            playerIcons.RemoveAt(playerIndex);

            Debug.Log($"Player icon removed at index: {playerIndex}");
        }
        else
        {
            Debug.LogWarning("Invalid player index, cannot remove icon.");
        }
    }

    public void OpenExitGameUI()
    {
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.QuitGame]);

        menuStage = MenuStage.QuitGame;
    }
    public void OpenModesMenu()
    {
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.ModesMenu]);
        menuStage = MenuStage.ModesMenu;
    }

    public void OpenQuitMenu()
    {
        UIGeneralController.ToggleUI(menuDictionary[MenuStage.QuitGame]);
        menuStage = MenuStage.QuitGame;
    }

    public async void StartGame(int t_difficulty)
    {
        levelSelected = t_difficulty;
        if (isMultiplayer)
        {
            await UIGeneralController.CloseUI(menuDictionary[MenuStage.LevelSelection]);
            lobby.HostGame();
            await UIGeneralController.CloseUI(menuDictionary[MenuStage.DifficultySelection]);
        }
        else
        {
            LaunchIntoLevel();
        }
        
    }


    public void LaunchIntoLevel()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            GameManager.Instance.difficulty = (Difficulty)levelSelected;


            GameManager.Instance.StartGame();

        }
    }

    public async void BackButton()
    {
        switch (menuStage)
        {
            case MenuStage.DifficultySelection:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.DifficultySelection]);
                CanvasGroupController.EnableGroup(LevelsMenu);
                menuStage = MenuStage.LevelSelection;
                break;

            case MenuStage.LevelSelection:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.LevelSelection]);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.ModesMenu]);
                menuStage = MenuStage.ModesMenu;
                break;

            case MenuStage.ModesMenu:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.ModesMenu]);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.MainMenu]);
                menuStage = MenuStage.MainMenu;
                break;

            case MenuStage.Settings:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.Settings]);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.MainMenu]);
                menuStage = MenuStage.MainMenu;
                break;

            case MenuStage.QuitGame:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.QuitGame]);
                break;

            case MenuStage.Multiplayer:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.Multiplayer]);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.ModesMenu]);
                isMultiplayer = false;
                menuStage = MenuStage.ModesMenu;
                break;

            case MenuStage.MultiPlayerLobby:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.MultiPlayerLobby]);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.Multiplayer]);
                foreach (GameObject icon in playerIcons)
                {
                    if (icon != null)
                        Destroy(icon);
                }
                playerIcons.Clear();

                lobby.DisconnectClient();
                menuStage = MenuStage.Multiplayer;
                break;

            case MenuStage.MulitplayerJoin:
                await UIGeneralController.CloseUI(menuDictionary[MenuStage.MulitplayerJoin]);
                CanvasGroupController.DisableGroup(errorMessage);
                CanvasGroupController.EnableGroup(menuDictionary[MenuStage.Multiplayer]);
                lobby.DisconnectClient();
                menuStage = MenuStage.Multiplayer;
                break;
            default:
                break;
        }
    }
}

