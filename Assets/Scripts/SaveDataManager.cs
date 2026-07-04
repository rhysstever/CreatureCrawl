using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    // Singleton
    public static SaveDataManager instance;

    private string dirParentPath, dirName, saveFileName, dirFullPath, saveFileFullPath;
    private bool hasSaveData;

    public bool HasSaveData { get { return hasSaveData; } } 

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

        dirParentPath = "Assets";
        dirName = "Saves";
        saveFileName = "saveData.txt";
        dirFullPath = Path.Combine(dirParentPath, dirName);
        saveFileFullPath = Path.Combine(dirParentPath, dirName, saveFileName);
        hasSaveData = CheckForSaveData();
    }

    private bool CheckForSaveData()
    {
        try
        {
            return Directory.Exists(dirFullPath) && File.Exists(saveFileFullPath);
        } 
        catch(Exception e)
        {
            Debug.Log(string.Format("No save data found! Failed to find directory: {0}\n {1}", dirFullPath, e));
            return false;
        }
    }

    public void SaveGame(string progress)
    {
        // JSON structure
        // list of runs
        // [
        //  {
        //      "date": {YYYY/MM/DD HH:MM},
        //      "progress": #-# or WIN (if won run)
        //      "character": {NAME OF CHARACTER},
        //      "mainHand": {NAME OF ATTACK CARD},
        //      "offHand": {NAME OF DEFEND CARD},
        //      "ally": {NAME OF ALLY CARD},
        //      "spell": {NAME OF SPELL CARD},
        //      "spirit": {NAME OF SPIRIT CARD},
        //      "drink": {NAME OF DRINK CARD},
        //  } 
        // ]
        SaveDataObject saveData = new SaveDataObject();

        // Current date and time
        DateTime today = DateTime.Now;
        saveData.date = string.Format("{0}/{1}/{2} {3}:{4}", today.Year, today.Month, today.Day, today.Hour, today.Minute);
        // Current progress
        saveData.progress = progress;
        // Character info
        string freePlaySymbol = CharacterManager.instance.IsFreePlayOn ? "*" : "";
        saveData.character = string.Format("{0}{1}", CharacterManager.instance.ChosenCharacter, freePlaySymbol);
        // Deck info
        saveData.mainHand = DeckManager.instance.GetCardDataBySlot(Slot.MainHand).Name;
        saveData.offHand = DeckManager.instance.GetCardDataBySlot(Slot.OffHand).Name;
        saveData.ally = DeckManager.instance.GetCardDataBySlot(Slot.Ally).Name;
        saveData.spell = DeckManager.instance.GetCardDataBySlot(Slot.Spell).Name;
        saveData.spirit = DeckManager.instance.GetCardDataBySlot(Slot.Spirit).Name;
        saveData.drink = DeckManager.instance.GetCardDataBySlot(Slot.Drink).Name;

        CreateSave(saveData);
    }

    private void CreateSave(SaveDataObject saveData)
    {
        try
        {
            string saveDataJSON = JsonUtility.ToJson(saveData);

            FileMode mode = FileMode.Append;
            if(!hasSaveData)
            {
                Directory.CreateDirectory(dirFullPath);
                mode = FileMode.Create;
            }

            using(var stream = new FileStream(saveFileFullPath, mode))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine(saveDataJSON);
                }
            }

            hasSaveData = true;
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format("Error! Failed to save data to file: {0}\n {1}", saveFileFullPath, e));
        }
    }

    public List<SaveDataObject> LoadRunInfo()
    {
        // Return an empty list if there is no save directory or file
        if(!CheckForSaveData())
        {
            return new List<SaveDataObject>();
        }

        List<SaveDataObject> saveData = new List<SaveDataObject>();
        try
        {
            string readLine = "";
            using(var stream = new FileStream(saveFileFullPath, FileMode.Open))
            {
                using(StreamReader reader = new StreamReader(stream))
                {
                    while((readLine = reader.ReadLine()) != null)
                    {
                        saveData.Add(JsonUtility.FromJson<SaveDataObject>(readLine));
                    }
                }
            }

        } 
        catch(Exception e)
        {
            Debug.LogWarning(string.Format("Warning! Failed to load data from file: {0}\n {1}", saveFileFullPath, e));
            return new List<SaveDataObject>();
        }

        return saveData;
    }

    public void ClearSaveData()
    {
        if(hasSaveData)
        {
            File.Delete(saveFileFullPath);
            hasSaveData = CheckForSaveData();
            TutorialManager.instance.CheckIfTutorialShouldStart();
        }
    }
}
