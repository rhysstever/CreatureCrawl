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
                    TutorialManager.instance.UpdateTutorialIndex(-1);
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
                    TutorialManager.instance.UpdateTutorialIndex(1);
                }
                gameObject.SetActive(false);

                if(gameObject.name == "end")
                {
                    TutorialManager.instance.EndTutorial();
                }
            });
        }
    }
}
