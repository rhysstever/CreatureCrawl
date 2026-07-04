using System.Collections;
using UnityEngine;

public enum MenuState
{
    MainMenu,
    Stats,
    Credits,
    CharacterSelect,
    Game,
    GameEnd
}

public enum GameState
{
    None,
    Combat,
    CardSelection,
    Well
}

public enum CombatState
{
    None,
    Start,
    PlayerTurn,
    AllyTurn,
    EnemyTurn,
    End
}

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager instance = null;

    // Instantiated in inspector
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform playerPostion;

    // Instantiated in code
    private Player player;
    [SerializeField]
    private MenuState currentMenuState;
    [SerializeField]
    private GameState currentGameState;
    [SerializeField]
    private CombatState currentCombatState;
    private int currentAreaIndex;
    private int currentStageIndex;

    private IEnumerator playerEffectsCoroutine, enemyEffectsCoroutine, allyTurnCoroutine;

    // Properties
    public Player Player { get { return player; } }
    public MenuState CurrentMenuState { get { return currentMenuState; } }
    public GameState CurrentGameState { get { return currentGameState; } }
    public CombatState CurrentCombatState { get { return currentCombatState; } }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ChangeMenuState(MenuState.MainMenu);
    }

    public void ChangeMenuState(MenuState newMenuState)
    {
        currentMenuState = newMenuState;

        switch(newMenuState)
        {
            case MenuState.MainMenu:
                // Destroy the player if it exists
                if(player != null)
                {
                    Destroy(player.gameObject);
                }
                currentAreaIndex = -1;
                currentStageIndex = -1;
                CardManager.instance.Reset();
                EnemyManager.instance.Reset();
                ChangeGameState(GameState.None);
                ChangeCombatState(CombatState.None);
                Camera.main.GetComponent<CameraPan>().ResetCameraPosition();
                // Start playing overworld music
                AudioManager.instance.PlayOverworldMusic();
                break;
            case MenuState.CharacterSelect:
                CharacterManager.instance.ShowCharacterSelectIcons();
                ChangeGameState(GameState.None);
                ChangeCombatState(CombatState.None);
                break;
            case MenuState.GameEnd:
                // Start playing overworld music
                AudioManager.instance.PlayOverworldMusic();
                ChangeGameState(GameState.None);
                ChangeCombatState(CombatState.None);
                break;
        }

        UIManager.instance.UpdateMenuUI(newMenuState);
    }

    public void ChangeGameState(GameState newGameState)
    {
        currentGameState = newGameState;

        switch(newGameState)
        {
            case GameState.Combat:
                // Start playing combat music
                AudioManager.instance.PlayCombatMusic();
                ChangeCombatState(CombatState.Start);
                break;
            case GameState.CardSelection:
                // Start playing overworld music
                AudioManager.instance.PlayOverworldMusic();
                ChangeCombatState(CombatState.None);
                DeckManager.instance.SetupCardSelection();
                break;
            case GameState.Well:
                ChangeCombatState(CombatState.None);
                break;
            case GameState.None:
                ChangeCombatState(CombatState.None);
                break;
        }

        UIManager.instance.UpdateGameUI(newGameState);
    }

    public void ChangeCombatState(CombatState newCombatState)
    {
        currentCombatState = newCombatState;

        switch(newCombatState)
        {
            case CombatState.Start:
                EnemyManager.instance.SpawnNextWave();
                DeckManager.instance.SetupForNewCombat();
                break;
            case CombatState.PlayerTurn:
                DeckManager.instance.IncrementRound();
                playerEffectsCoroutine = player.ProcessEffects();
                StartCoroutine(playerEffectsCoroutine);
                break;
            case CombatState.AllyTurn:
                DeckManager.instance.ClearHand();
                if(CharacterManager.instance.Ally != null)
                {
                    allyTurnCoroutine = CharacterManager.instance.ProcessAllyTurn();
                    StartCoroutine(allyTurnCoroutine);
                }
                else
                {
                    ChangeCombatState(CombatState.EnemyTurn);
                }
                break;
            case CombatState.EnemyTurn:
                enemyEffectsCoroutine = EnemyManager.instance.ProcessEffectsOnEnemies();
                StartCoroutine(enemyEffectsCoroutine);
                break;
            case CombatState.End:
                player.PostCombatReset();

                if(EnemyManager.instance.IsLastWave())
                {
                    ChangeMenuState(MenuState.GameEnd);
                }
                else
                {
                    ChangeGameState(GameState.CardSelection);
                }
                break;
            case CombatState.None:
                DeckManager.instance.ClearHand();
                break;
        }
    }

    public void StartGame()
    {
        CharacterManager.instance.ResetSummons();
        ChangeMenuState(MenuState.Game);

        // Create Player
        GameObject playerObj = Instantiate(playerPrefab, playerPostion.position, Quaternion.identity, transform);
        player = playerObj.GetComponent<Player>();

        EnterArea();
    }

    private void EnterArea()
    {
        currentAreaIndex++;
        currentStageIndex = -1;
        GoToNextStage();
    }

    public void GoToNextStage()
    {
        currentStageIndex++;
        // Location Order:
        // 0) Tutorial Combat, Wave 0
        // 1) Tutorial Combat, Wave 1
        // 2) Combat, Wave 2
        // 3) Well
        // 4) Combat, Wave 3
        // 5) Combat, Wave 4 (Mini Boss)
        // 6) Well
        // 7) Combat, Wave 5 (Boss)
        // 8) Well
        switch(currentStageIndex)
        {
            case 3:
            case 6:
            case 8:
                ChangeGameState(GameState.Well);
                break;
            default:
                // Move right to card selection if the player has played a game before
                if(currentStageIndex == 0 && !TutorialManager.instance.IsInTutorial)
                {
                    EnemyManager.instance.IncrementWaveNum();
                    ChangeGameState(GameState.CardSelection);
                }
                else
                {
                    ChangeGameState(GameState.Combat);
                }
                break;
        }

        UIManager.instance.UpdateStageText();
    }

    public string GetCurrentStageText()
    {
        int area = currentAreaIndex + 1;

        string stageText = currentStageIndex.ToString();
        if(currentStageIndex == 0)
        {
            stageText = "T";
        }
        else if(currentStageIndex == 3 || currentStageIndex == 6 || currentStageIndex == 8)
        {
            stageText = "W";
        }

        return string.Format("{0}-{1}", area, stageText);
    }
}
