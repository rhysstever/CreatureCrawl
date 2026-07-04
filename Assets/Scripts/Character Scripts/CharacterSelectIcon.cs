using UnityEngine;

public class CharacterSelectIcon : MonoBehaviour
{
    // Set in inspector
    [SerializeField]
    private GameObject characterIconSelectedObj;
    [SerializeField]
    private SpriteRenderer characterSpriteRenderer;
    [SerializeField]
    private Character character;

    private bool isUnlocked;

    void Start()
    {
        characterIconSelectedObj.SetActive(false);
        UpdatedLockedState();
    }

    public void UpdatedLockedState()
    {
        // Determine head sprite used
        isUnlocked = CharacterManager.instance.IsCharacterUnlocked(character);
        characterSpriteRenderer.sprite = CharacterManager.instance.GetCharacterHeadSprite(isUnlocked ? character : Character.Locked);
    }

    private void OnMouseUpAsButton()
    {
        if(isUnlocked)
        {
            characterIconSelectedObj.SetActive(false);
            CharacterManager.instance.ChooseCharacter(character);
        }
    }

    private void OnMouseEnter()
    {
        characterIconSelectedObj.SetActive(true);
        CharacterManager.instance.SetCharacterSelectInfo(isUnlocked ? character : Character.Locked);
    }

    private void OnMouseExit()
    {
        characterIconSelectedObj.SetActive(false);
        CharacterManager.instance.ClearCharacterSelectInfo();
    }
}
