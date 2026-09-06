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

    public void UpdateTutorialIndex(GameObject currentTutorialPanelBeingShown)
    {
        if(currentTutorialPanelBeingShown == null)
        {
            tutorialStage = -1;
            DeckManager.instance.UpdateHandInteractability(true);
            return;
        }

        for(int i = 0; i < tutorialUIParentTrans.childCount; i++)
        {
            if(tutorialUIParentTrans.GetChild(i).gameObject.activeSelf)
            {
                tutorialStage = i;
                DeckManager.instance.UpdateHandInteractability(true);
                return;
            }
        }

        tutorialStage = -1;
        DeckManager.instance.UpdateHandInteractability(true);
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
        }
    }

    public void TryShowPlayedFirstCardPanel()
    {
        if(IsInTutorial)
        {
            HideAllTutorialUI();
            playedFirstCardPanel.SetActive(true);
        }
    }

    public void TryShowPlayedFirstAttackPanel()
    {
        if(IsInTutorial && !hasAttacked)
        {
            HideAllTutorialUI();
            playedFirstAttackPanel.SetActive(true);
            hasAttacked = true;
        }
    }

    public void TryShowStartOfSecondTurnPanel()
    {
        if(IsInTutorial)
        {
            startOfSecondTurnPanel.SetActive(true);
            DeckManager.instance.UpdateHandInteractability(true);
        }
    }

    public void EndTutorial()
    {
        HideAllTutorialUI();
        isInTutorial = false;
        DeckManager.instance.UpdateHandInteractability(true);
    }
}
