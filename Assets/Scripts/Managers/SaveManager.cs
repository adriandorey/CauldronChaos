using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SaveManager : MonoBehaviour
{
    private string _savePath;
    private const string SaveFileName = "cauldronChaos.json";
    [SerializeField] private TextMeshProUGUI deleteFileConfirmation;

    public GameData gameData;

    private Coroutine _deleteConfirm;

    public void Start()
    {
       deleteFileConfirmation.enabled = false;
        _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
        LoadGame();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnSaveDay += SaveDayScore;
        Actions.OnDeleteSaveFile += DeleteSave;
    }

    private void OnDisable()
    {
        Actions.OnSaveDay -= SaveDayScore;
        Actions.OnDeleteSaveFile -= DeleteSave;
    }

    private void OnDestroy()
    {
        Actions.OnSaveDay -= SaveDayScore;
        Actions.OnDeleteSaveFile -= DeleteSave;
    }
    #endregion

    public void CheckSaveFile()
    {
        if (gameData.isValidSave && !GameManager.Instance.IsDebugging())
        {
            Actions.OnSetUnlockedDays?.Invoke(GetUnlockedDaysCount());
            Actions.OnSetScore?.Invoke(GetAllScores());
            Actions.OnSaveExist?.Invoke(true);
        }
        else
        {
            if(GameManager.Instance.IsDebugging())
            {
                Actions.OnSaveExist?.Invoke(true);
                Actions.OnSetUnlockedDays?.Invoke(10);
                return;
            }
            else
            {
                Actions.OnSaveExist?.Invoke(false);
                Actions.OnSetUnlockedDays?.Invoke(1);
                Actions.OnSetDay(1);
                gameData = new GameData();
                gameData.CreateNewSave();
            }
        }
    }


    private void SaveGame()
    {
        if(!GameManager.Instance.IsDebugging())
        {
            gameData.isValidSave = true;

            Actions.OnSetUnlockedDays?.Invoke(GetUnlockedDaysCount());
            Actions.OnSetScore?.Invoke(GetAllScores());

            var json = JsonUtility.ToJson(gameData);
            File.WriteAllText(_savePath, json);
        }
        else
        {
            Debug.Log("Save file not created in debug mode");
        }
    }

    private void LoadGame()
    {
        if (!File.Exists(_savePath)) return;
        
        var json = File.ReadAllText(_savePath);
        gameData = JsonUtility.FromJson<GameData>(json);
        gameData.days[0].isUnlocked = true;
    }

    public void SaveAbovePlayerUI(Toggle abovePlayer)
    {
        if (!GameManager.Instance.IsDebugging())
        {
            gameData.abovePlayerUI = abovePlayer.isOn;

            var json = JsonUtility.ToJson(abovePlayer);
            File.WriteAllText(_savePath, json);
        }
        else
        {
            Debug.Log("Save file not created in debug mode");
        }
    }

    private void SaveDayScore(int day, int score, bool unlockNext)
    {
        if (!GameManager.Instance.IsDebugging())
        {
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
        else
        {
            Debug.Log("Save file not created in debug mode");
        }
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
        if (File.Exists(_savePath))
        {
            File.Delete(_savePath);

            // Will reset the game data and refresh the level select screen
            if (File.Exists(_savePath)) return;
            
            gameData = new GameData();
            gameData.CreateNewSave();
            _deleteConfirm = StartCoroutine(ShowDeleteConfirmation("Save File Deleted"));
            return;
        }
        
        _deleteConfirm = StartCoroutine(ShowDeleteConfirmation("No Save File Found"));
    }

    private IEnumerator ShowDeleteConfirmation(string text)
    {
        deleteFileConfirmation.enabled = true;
        deleteFileConfirmation.text = text;
        yield return new WaitForSeconds(2);
        deleteFileConfirmation.enabled = false;
    }
}
