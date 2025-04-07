using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private Image quotaFill;
    [SerializeField] private GameObject coinImage;

    [Header("EOD UI")]
    [SerializeField] private TextMeshProUGUI eodTitle;
    [SerializeField] private TextMeshProUGUI eodScoreText;

    [Header("EOD Win Lose Sprite")]
    [SerializeField] private Image eodWinLoseSprite;
    [SerializeField] private Sprite winSprite;
    [SerializeField] private Sprite loseSprite;

    [Header("Score Amounts")]
    [SerializeField] private int[] scorePerLevel;

    // keeps track of current day / day score
    private int _score;
    private int _currentDay;

    private void Start()
    {
        quotaFill.fillAmount = 0;
        //coinParticles.Stop();
    }

    #region OnEnable / OnDisable / OnDestroy Events
    private void OnEnable()
    {
        Actions.OnCustomerServed += UpdateScore;
        Actions.OnEndDay += UpdateEodText;
        Actions.OnResetValues += ResetValues;
        Actions.OnSetDay += SetCurrentDay;
    }

    private void OnDisable()
    {
        Actions.OnCustomerServed -= UpdateScore;
        Actions.OnEndDay -= UpdateEodText;
        Actions.OnResetValues -= ResetValues;
        Actions.OnSetDay -= SetCurrentDay;
    }

    private void OnDestroy()
    {
        Actions.OnCustomerServed -= UpdateScore;
        Actions.OnEndDay -= UpdateEodText;
        Actions.OnResetValues -= ResetValues;
        Actions.OnSetDay -= SetCurrentDay;
    }
    #endregion

    private void SetCurrentDay(int day)
    {
        _currentDay = day;
    }

    private void UpdateScore(int regularScore)
    {
        _score += regularScore;

        if (quotaFill.fillAmount == 1)
        {
            coinImage.transform.DOScale(Vector3.one * 1.5f, 1f).OnComplete(ResetCoinSize);
            return;
        }

        quotaFill.fillAmount = (float)_score / (float)scorePerLevel[_currentDay];
    }

    private void ResetCoinSize()
    {
        coinImage.transform.localScale = Vector3.one;
    }

    private void UpdateEodText()
    {
        bool increaseDayCount;

        // Check if the player has reached the score for the current day
        if (_score < scorePerLevel[_currentDay])
            increaseDayCount = false;
        else
            increaseDayCount = true;

        // Save the current day's score and people served
        Actions.OnSaveDay(_currentDay, _score, increaseDayCount);

        // Sets the EOD text
        eodTitle.text = $"End of Day {_currentDay + 1}";

        // Sets the EOD win/lose sprite and score text
        if (!increaseDayCount)
        {
            eodWinLoseSprite.sprite = loseSprite;
            eodScoreText.text = $"Score: {_score}\nTry Level Again?";
        }
        else
        {
            eodWinLoseSprite.sprite = winSprite;
            eodScoreText.text = $"Score: {_score}\nYOU WIN!";
        }

        _score = 0;
    }


    private void ResetValues()
    {
        //coinParticles.Stop();
        quotaFill.fillAmount = 0;
        _score = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other);
        coinImage.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f, 1, 0.5f);
    }
}
