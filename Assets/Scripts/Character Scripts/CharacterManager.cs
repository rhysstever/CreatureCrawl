using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum Character
{
    Locked,
    Badger,
    Beaver,
    Fox,
    Opossum,
    Otter,
    Skunk
}

public class CharacterManager : MonoBehaviour
{
    // Singleton
    public static CharacterManager instance = null;

    // Set in inspector
    [SerializeField]
    private Transform characterSelectIconParent;
    [SerializeField]
    private SpriteRenderer characterSelectSprite;
    [SerializeField]
    private Transform allySpawnTrans, spiritSpawnTrans;
    [SerializeField]    // Character Sprites
    private Sprite lockedSprite, badgerSprite, beaverSprite, foxSprite, opossumSprite, otterSprite, skunkSprite;
    [SerializeField]    // Character Head Sprites
    private Sprite lockedHeadSprite, badgerHeadSprite, beaverHeadSprite, foxHeadSprite, opossumHeadSprite, otterHeadSprite, skunkHeadSprite;

    // Set at Start
    private bool isFreePlayOn;
    private List<Character> charactersWonWith;
    private Dictionary<string, GameObject> allyPrefabs;
    private Dictionary<string, GameObject> spiritPrefabs;
    private Character chosenCharacter;
    private Ally ally;
    private List<GameObject> summonedSpirits;

    public bool IsFreePlayOn { get { return isFreePlayOn; } }
    public Character ChosenCharacter { get { return chosenCharacter; } }
    public Ally Ally { get { return ally; } }

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

        isFreePlayOn = false;
        charactersWonWith = new List<Character>();
        allyPrefabs = LoadAllyPrefabs();
        spiritPrefabs = LoadSpiritPrefabs();
    }

    void Start()
    {
        summonedSpirits = new List<GameObject>();
        HideCharacterSelectIcons();
    }

    #region Prefab File Loading
    private Dictionary<string, GameObject> LoadAllyPrefabs()
    {
        // Ally Sprites
        Dictionary<string, GameObject> newAllyPrefabs = new Dictionary<string, GameObject>();

        string allyPrefabResDirPath = "Prefabs/Units/Allies";
        UnityEngine.Object[] allyPrefabObjs = Resources.LoadAll(allyPrefabResDirPath, typeof(GameObject));

        foreach(var allyPrefabObj in allyPrefabObjs)
        {
            GameObject allyPrefab = (GameObject)allyPrefabObj;

            if(allyPrefab != null)
            {
                newAllyPrefabs.Add(allyPrefab.name, allyPrefab);
            }
            else
            {
                Debug.Log("Error! Ally Prefab not loaded");
            }
        }

        return newAllyPrefabs;
    }

    private Dictionary<string, GameObject> LoadSpiritPrefabs()
    {
        Dictionary<string, GameObject> newSpiritPrefabs = new Dictionary<string, GameObject>();

        string spiritPrefabResDirPath = "Prefabs/Units/Spirits";
        UnityEngine.Object[] spiritPrefabObjs = Resources.LoadAll(spiritPrefabResDirPath, typeof(GameObject));

        foreach(var spiritPrefabObj in spiritPrefabObjs)
        {
            GameObject spiritPrefab = (GameObject)spiritPrefabObj;

            if(spiritPrefab != null)
            {
                newSpiritPrefabs.Add(spiritPrefab.name, spiritPrefab);
            }
            else
            {
                Debug.Log("Error! Ally Prefab not loaded");
            }
        }

        return newSpiritPrefabs;
    }
    #endregion Prefab File Loading

    public void ShowCharacterSelectIcons()
    {
        charactersWonWith = GetCharactersWonWith();
        for(int i = 0; i < characterSelectIconParent.childCount; i++) 
        {
            characterSelectIconParent.transform.GetChild(i).GetComponent<CharacterSelectIcon>().UpdatedLockedState();
        }
        characterSelectIconParent.gameObject.SetActive(true);
    }

    public void ToggleFreePlay(bool newValue)
    {
        isFreePlayOn = newValue;
    }

    private List<Character> GetCharactersWonWith()
    {
        List<SaveDataObject> runHistoryList = SaveDataManager.instance.LoadRunInfo();
        return runHistoryList
            .Where(run => run.progress == "WIN")
            .Select(run => {
                if(run.character.Contains("*"))
                {
                    Character.TryParse(run.character, out Character freePlayRunCharacter);
                    return freePlayRunCharacter;
                }
                else
                {
                    Character.TryParse(run.character, out Character runCharacter);
                    return runCharacter;
                }
            }).ToList();
    }

    public void ChooseCharacter(Character character)
    {
        chosenCharacter = character;
        HideCharacterSelectIcons();
        ClearCharacterSelectInfo();
        GameManager.instance.StartGame();
    }

    public void HideCharacterSelectIcons()
    {
        characterSelectIconParent.gameObject.SetActive(false);
    }

    public void ClearCharacterSelectInfo()
    {
        characterSelectSprite.gameObject.SetActive(false);
        UIManager.instance.UpdateCharacterSelectInfo();
    }

    public void SetCharacterSelectInfo(Character character)
    {
        // Update character sprite
        characterSelectSprite.gameObject.SetActive(true);
        characterSelectSprite.sprite = GetCharacterSprite(character);
        UIManager.instance.UpdateCharacterSelectInfo(character);
    }

    public Sprite GetCharacterSprite(Character character)
    {
        return character switch
        {
            Character.Badger => badgerSprite,
            Character.Beaver => beaverSprite,
            Character.Fox => foxSprite,
            Character.Opossum => opossumSprite,
            Character.Otter => otterSprite,
            Character.Skunk => skunkSprite,
            _ => lockedSprite,
        };
    }

    public Sprite GetCharacterHeadSprite(Character character)
    {
        return character switch
        {
            Character.Badger => badgerHeadSprite,
            Character.Beaver => beaverHeadSprite,
            Character.Fox => foxHeadSprite,
            Character.Opossum => opossumHeadSprite,
            Character.Otter => otterHeadSprite,
            Character.Skunk => skunkHeadSprite,
            _ => lockedHeadSprite,
        };
    }

    public string GetCharacterDeckDescription(Character character)
    {
        return character switch
        {
            Character.Badger => "Physical attacks deal more damage.",
            Character.Beaver => "Starts with higher max health.",
            Character.Fox => "Spells deal more damage.",
            Character.Opossum => "Summons are tougher.",
            Character.Otter => "Cards draw with an equal chance.",
            Character.Skunk => "Damaging effects linger on enemies longer.",
            _ => "Claim victory with the previous character to unlock."
        };
    }

    public bool IsCharacterUnlocked(Character character)
    {
        if(isFreePlayOn)
        {
            return true;
        }

        return character switch
        {
            Character.Beaver => true,
            Character.Badger => HasWonWithCharacter(Character.Beaver),
            Character.Fox => HasWonWithCharacter(Character.Badger),
            Character.Skunk => HasWonWithCharacter(Character.Fox),
            Character.Opossum => HasWonWithCharacter(Character.Skunk),
            Character.Otter => HasWonWithCharacter(Character.Opossum),
            _ => false,
        };
    }

    private bool HasWonWithCharacter(Character character)
    { 
        if(!SaveDataManager.instance.HasSaveData || character == Character.Locked)
        {
            return false;
        }

        return charactersWonWith.Contains(character);
    }

    public void SummonAlly(Summon summonAction)
    {
        int amount = summonAction.Amount + GameManager.instance.Player.UnitEffects.GetEffectAmount(ActionType.Summon, true);

        AudioManager.instance.PlayAllyAudio(summonAction.SummonName);
        if(ally != null)
        {
            ally.GetComponent<Ally>().Buff(amount);
        }
        else
        {
            Ally newAlly = Instantiate(
                allyPrefabs[summonAction.SummonName.Replace(" ", "")],
                allySpawnTrans.position,
                Quaternion.identity,
                allySpawnTrans
            ).GetComponent<Ally>();

            newAlly.SetHealth(amount);
            newAlly.SetActions(summonAction.SummonActions);
            ally = newAlly;
        }
    }

    public void SummonSpirit(string spiritTypeToSummon)
    {
        GameObject newSpirit = Instantiate(
            spiritPrefabs[spiritTypeToSummon.Replace(" ", "")],
            spiritSpawnTrans.position,
            Quaternion.identity,
            spiritSpawnTrans
        );
        summonedSpirits.Add(newSpirit);
    }

    public IEnumerator ProcessAllyTurn()
    {
        WaitForSeconds allyActionDelayWait = new WaitForSeconds(0.5f);

        yield return allyActionDelayWait;
        CardData allyCardToPlay = DeckManager.instance.GetCardDataBySlot(Slot.Ally);

        if(allyCardToPlay != null)
        {
            AudioManager.instance.PlayAllyAudio(allyCardToPlay.Name);
            yield return allyActionDelayWait;
            ActionManager.instance.PerformActions(ally.GetComponent<Ally>().Actions, ally, null);
            yield return allyActionDelayWait;
            yield return allyActionDelayWait;
        }

        if(!EnemyManager.instance.IsWaveOver())
        {
            GameManager.instance.ChangeCombatState(CombatState.EnemyTurn);
        }
    }

    public void ResetSummons()
    {
        if(ally != null)
        {
            Destroy(ally.gameObject);
            ally = null;
        }

        for(int i = summonedSpirits.Count - 1; i >= 0; i--)
        {
            Destroy(summonedSpirits[i]);
        }
        summonedSpirits.Clear();
    }
}
