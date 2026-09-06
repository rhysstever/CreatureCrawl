using UnityEngine;
using UnityEngine.UI;

public class TutorialButtonPanel : MonoBehaviour
{
    [SerializeField]
    private Button previousButton, nextButton;
    [SerializeField]
    private GameObject previousPanel, nextPanel;

    void Start()
    {
        gameObject.SetActive(false);

        if(previousButton != null)
        {
            previousButton.onClick.AddListener(() => {
                if(previousPanel != null)
                {
                    previousPanel.SetActive(true);
                }
                gameObject.SetActive(false);
            });
        }

        if(nextButton != null)
        {
            nextButton.onClick.AddListener(() => {
                if(nextPanel != null)
                {
                    nextPanel.SetActive(true);
                }
                gameObject.SetActive(false);

                if(gameObject.name == "end")
                {
                    TutorialManager.instance.EndTutorial();
                }
            });
        }
    }

    private void OnEnable()
    {
        if(TutorialManager.instance != null)
        {
            TutorialManager.instance.UpdateTutorialIndex(gameObject);
        }
    }
}
