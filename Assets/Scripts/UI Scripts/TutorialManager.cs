using System.Collections.Generic;
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
    private int currentStage;

    public bool IsInTutorial { get { return isInTutorial; } }

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
        CheckIfTutorialShouldStart();
        hasDeckInfoBeenViewed = false;
        hasBeenBackToCombat = false;
        hasAttacked = false;

        SetTutorialHandStage(isInTutorial ? 0 : 2);
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
            SetTutorialHandStage(2);
        }
    }

    public void EndTutorial()
    {
        HideAllTutorialUI();
        SetTutorialHandStage(2);
        isInTutorial = false;
    }

    public void SetTutorialHandStage(int stage)
    {
        if(isInTutorial)
        {
            currentStage = stage;
            SetTutorialHandInteractabilityWithCurrentStage();
        }
        else
        {
            currentStage = 2;
            SetTutorialHandInteractabilityWithCurrentStage();
        }
    }

    public void SetTutorialHandInteractabilityWithCurrentStage()
    {
        if(IsInTutorial)
        {
            DeckManager.instance.SetTutorialHandInteractability(currentStage);
        }
        else
        {
            DeckManager.instance.SetTutorialHandInteractability(2);
        }
    }
}
