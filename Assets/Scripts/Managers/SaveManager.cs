using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;


public class SaveManager : MonoBehaviour
{
    private string _savePath;
    private string _saveFileName = "cauldronChaos.json";
    [SerializeField] private TextMeshProUGUI deleteFileConfirmation;

    public GameData gameData;

    private Coroutine _deleteConfirm;
    
    [SerializeField] private GraphicsManager graphicsManager;
    
    

    public void Start()
    {
       deleteFileConfirmation.enabled = false;
        _savePath = Path.Combine(Application.persistentDataPath, _saveFileName);
        LoadGame();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnSaveDay += SaveDayScore;
    }

    private void OnDisable()
    {
        Actions.OnSaveDay -= SaveDayScore;
    }

    private void OnDestroy()
    {
        Actions.OnSaveDay -= SaveDayScore;
    }
    #endregion

    public void CheckSaveFile()
    {
        if (gameData.isValidSave && !GameManager.Instance.IsDebugging())
        {
            Actions.OnSetUnlockedDays?.Invoke(GetUnlockedDaysCount());
            Actions.OnSetScore?.Invoke(GetAllScores());
            graphicsManager.SetAbovePlayerUi(gameData.abovePlayerUI);                    
            Actions.OnSaveExist?.Invoke(true);
        }
        else
        {
            if(GameManager.Instance.IsDebugging())
            {
                Actions.OnSaveExist?.Invoke(true);
                Actions.OnSetUnlockedDays?.Invoke(10);
            }
            else
            {
                Actions.OnSaveExist?.Invoke(false);
                Actions.OnSetUnlockedDays?.Invoke(1);
                Actions.OnSetDay(1);
                graphicsManager.SetAbovePlayerUi(true);
                gameData = new GameData();
                gameData.CreateNewSave();
            }
        }
    }


    private void SaveGame()
    {
        if(GameManager.Instance.IsDebugging()) return;

        gameData.isValidSave = true;
        gameData.abovePlayerUI = graphicsManager.GetAbovePlayerUi();
        Actions.OnSetUnlockedDays?.Invoke(GetUnlockedDaysCount());
        Actions.OnSetScore?.Invoke(GetAllScores());
        
        var json = JsonUtility.ToJson(gameData);
        File.WriteAllText(_savePath, json);
    }

    private void LoadGame()
    {
        if (!File.Exists(_savePath)) return;
        
        var json = File.ReadAllText(_savePath);
        gameData = JsonUtility.FromJson<GameData>(json);
        gameData.days[0].isUnlocked = true;
    }

    private void SaveDayScore(int day, int score, bool unlockNext)
    {
        if (GameManager.Instance.IsDebugging()) return;
        
        // Update the day data if the day is valid
        if (day < gameData.days.Count)
        {
            var dayData = gameData.days[day];

            // Update the best score and people served for the day
            if (score > dayData.bestScore)
                dayData.bestScore = score;

            // Day 0 will always be unlocked
            gameData.days[0].isUnlocked = true;
            // Unlock the next day if the current day is completed
            if (unlockNext)
                UnlockDay(day + 1);
        }

        SaveGame();
    }

    private void UnlockDay(int day)
    {
        if (day >= gameData.days.Count) return;
        
        gameData.days[day].isUnlocked = true;
        Actions.OnSetUnlockedDays?.Invoke(GetUnlockedDaysCount());
    }


    private int GetUnlockedDaysCount()
    {
        var count = 0;
        
        foreach (DayData day in gameData.days)
        {
            if (day.isUnlocked)
                count++;
        }
        
        return count;
    }

    private int[] GetAllScores()
    {
        var scores = new int[gameData.days.Count];
        for (var i = 0; i < gameData.days.Count; i++)
        {
            scores[i] = gameData.days[i].bestScore;
        }
        return scores;
    }

    private void DeleteSave()
    {
        Debug.Log("Deleting save");
        // check if there isn't a save first then check to see if there is. 
        if (!File.Exists(_savePath))
        {
            _deleteConfirm = StartCoroutine(ShowDeleteConfirmation("No Save File Found"));
        }
        else
        {
            File.Delete(_savePath);

            // Will reset the game data and refresh the level select screen
            if (File.Exists(_savePath)) return;
            
            gameData = new GameData();
            gameData.CreateNewSave();
            _deleteConfirm = StartCoroutine(ShowDeleteConfirmation("Save File Deleted"));
        }
    }

    private IEnumerator ShowDeleteConfirmation(string text)
    {
        deleteFileConfirmation.enabled = true;
        deleteFileConfirmation.text = text;
        yield return new WaitForSeconds(2);
        deleteFileConfirmation.enabled = false;
    }
}
