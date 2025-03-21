using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour 
{
    [SerializeField] private LevelSelectionButtons[] levelSelection;

    private int _unlockedDays;
    private int[] _score;


    private void Start()
    {
        _score = new int[levelSelection.Length];
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.UpdateLevelButtons += UpdateButtons;
        Actions.OnSetUnlockedDays += SetUnlockedDays;
        Actions.OnSetScore += SetScore;
        Actions.OnDeleteSaveFile += ResetButtonLabels;
    }

    private void OnDisable()
    {
        Actions.UpdateLevelButtons -= UpdateButtons;
        Actions.OnSetUnlockedDays -= SetUnlockedDays;
        Actions.OnSetScore -= SetScore;
        Actions.OnDeleteSaveFile -= ResetButtonLabels;
    }

    private void OnDestroy()
    {
        Actions.UpdateLevelButtons -= UpdateButtons;
        Actions.OnSetUnlockedDays -= SetUnlockedDays;
        Actions.OnSetScore -= SetScore;
        Actions.OnDeleteSaveFile -= ResetButtonLabels;
    }
    #endregion
    
    private void ResetButtonLabels()
    {
        for(var i = 0; i < levelSelection.Length; i++)
        {
            levelSelection[i].buttonText.text = $"Day {i + 1}";
        }
    }


    private void UpdateButtons()
    {
        for (var i = 0; i < levelSelection.Length; i++)
        {
            if (levelSelection[i].button == null || levelSelection[i].dayImage == null || levelSelection[i].buttonText == null)
            {
                Debug.LogWarning($"LevelSelectionButtons[{i}] has a null reference.");
                continue; // Skip this iteration if any component is missing
            }

            if (i < _unlockedDays)
            {
                levelSelection[i].button.interactable = true;

                // Check if dayImage is still valid before enabling it
                if (levelSelection[i].dayImage != null)
                {
                    levelSelection[i].dayImage.enabled = true;
                }

                if (_score == null) break;
                if (GameManager.Instance.IsDebugging()) continue;
                if (_score[i] == 0) continue;

                levelSelection[i].buttonText.text = $"Day {i + 1}\nScore: {_score[i]}";
            }
            else
            {
                levelSelection[i].button.interactable = false;

                // Check if dayImage is still valid before disabling it
                if (levelSelection[i].dayImage != null)
                {
                    levelSelection[i].dayImage.enabled = false;
                }
            }
        }
    }


    private void SetUnlockedDays(int days)
    {
        _unlockedDays = days;
        UpdateButtons();
    }

    private void SetScore(int[] _score)
    {
        this._score = _score;
    }
}

[Serializable]
public class LevelSelectionButtons
{
    public Button button;
    public Image dayImage;
    public TextMeshProUGUI buttonText;
}
