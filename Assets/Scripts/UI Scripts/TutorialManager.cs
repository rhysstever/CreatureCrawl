using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    // Singleton
    public static TutorialManager instance;

    [SerializeField]
    private Transform tutorialUIParentTrans;
    [SerializeField]
    private GameObject introPanel, deckInfoPanel, backToCombatPanel, playedFirstCardPanel, playedFirstAttackPanel, startOfSecondTurnPanel;

    private bool isInTutorial, hasDeckInfoBeenViewed, hasBeenBackToCombat, hasAttacked;
    private int tutorialStage;

    public bool IsInTutorial { get { return isInTutorial; } }
    public int TutorialStage { get { return tutorialStage; } }

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
        tutorialStage = -1;
        CheckIfTutorialShouldStart();
        hasDeckInfoBeenViewed = false;
        hasBeenBackToCombat = false;
        hasAttacked = false;
    }

    public void UpdateTutorialIndex(int indexChange)
    {
        if(Mathf.Abs(indexChange) != 1)
        {
            tutorialStage = -1;
        }
        else
        {
            tutorialStage += indexChange;
        }
        DeckManager.instance.UpdateHandInteractability(true);
        UIManager.instance.UpdateEndTurnButtonInteractivability(true);
    }

    public void CheckIfTutorialShouldStart()
    {
        isInTutorial = !SaveDataManager.instance.HasSaveData;
    }

    public void HideAllTutorialUI()
    {
        for(int i = 0; i < tutorialUIParentTrans.childCount; i++)
        {
            tutorialUIParentTrans.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void TryStartTutorial()
    {
        if(!SaveDataManager.instance.HasSaveData)
        {
            isInTutorial = true;
            introPanel.SetActive(true);
            tutorialStage = 0;
            DeckManager.instance.UpdateHandInteractability(true);
            UIManager.instance.UpdateEndTurnButtonInteractivability(false);
        }
    }

    public void TryShowDeckInfoPanel()
    {
        if(IsInTutorial && !hasDeckInfoBeenViewed)
        {
            HideAllTutorialUI();
            deckInfoPanel.SetActive(true);
            hasDeckInfoBeenViewed = true;
        }
    }

    public void TryShowBackToCombatPanel()
    {
        if(IsInTutorial && hasDeckInfoBeenViewed && !hasBeenBackToCombat)
        {
            HideAllTutorialUI();
            backToCombatPanel.SetActive(true);
            hasBeenBackToCombat = true;
            tutorialStage = GetIndexOfTutorialPanel(backToCombatPanel);
        }
    }

    public void TryShowPlayedFirstCardPanel()
    {
        if(IsInTutorial)
        {
            HideAllTutorialUI();
            playedFirstCardPanel.SetActive(true);
            tutorialStage = GetIndexOfTutorialPanel(playedFirstCardPanel);
        }
    }

    public void TryShowPlayedFirstAttackPanel()
    {
        if(IsInTutorial && !hasAttacked)
        {
            HideAllTutorialUI();
            playedFirstAttackPanel.SetActive(true);
            hasAttacked = true;
            tutorialStage = GetIndexOfTutorialPanel(playedFirstAttackPanel);
        }
    }

    public void TryShowStartOfSecondTurnPanel()
    {
        if(IsInTutorial)
        {
            startOfSecondTurnPanel.SetActive(true);
            tutorialStage = GetIndexOfTutorialPanel(startOfSecondTurnPanel);
        }
    }

    public void EndTutorial()
    {
        HideAllTutorialUI();
        isInTutorial = false;
        tutorialStage = -1;
        DeckManager.instance.UpdateHandInteractability(true);
        UIManager.instance.UpdateEndTurnButtonInteractivability(true);
    }

    private int GetIndexOfTutorialPanel(GameObject tutorialPanelObject)
    {
        string panelName = tutorialPanelObject.name;
        for(int i = 0; i < tutorialUIParentTrans.childCount; i++)
        {
            if(tutorialUIParentTrans.GetChild(i).gameObject.name == panelName)
            {
                return i;
            }
        }

        Debug.LogWarning("Warning! No tutorial panel found with name: " + panelName);
        return -1;
    }
}
