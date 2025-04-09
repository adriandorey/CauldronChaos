using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private Image quotaFill;
    [SerializeField] private GameObject coinImage;
    [SerializeField] private GameObject coinCollection;
    [SerializeField] private GameObject coinCollectionImage;

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

    private Coroutine pulse;

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

    private void UpdateScore(int regularScore, Transform position)
    {
        _score += regularScore;

        MoveCoin(position);
        if (pulse != null)
            StopCoroutine(PulseCoin());

        pulse = StartCoroutine(PulseCoin());

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

    private void MoveCoin(Transform customerPos)
    {
        GameObject coin1 = Instantiate(coinCollectionImage, customerPos.position, Quaternion.Euler(-55.2f, 0f, 0f));
        GameObject coin2 = Instantiate(coinCollectionImage, customerPos.position + new Vector3(-0.5f, 0f, 0f), Quaternion.Euler(-55.2f, 0f, 0f));
        GameObject coin3 = Instantiate(coinCollectionImage, customerPos.position + new Vector3(0.5f, 0f, 0f), Quaternion.Euler(-55.2f, 0f, 0f));

        coin1.transform.DOMove(coinCollection.transform.position, 1f).SetEase(Ease.OutQuad);
        coin2.transform.DOMove(coinCollection.transform.position, 1f).SetEase(Ease.OutQuad);
        coin3.transform.DOMove(coinCollection.transform.position, 1f).SetEase(Ease.OutQuad);
    }


    private void ResetValues()
    {
        //coinParticles.Stop();
        quotaFill.fillAmount = 0;
        _score = 0;
        ResetCoin();
    }

    private IEnumerator PulseCoin()
    {
        yield return new WaitForSeconds(1);

        coinImage.transform.DOKill();
        coinImage.transform.DOScale(1.2f, 0.3f).OnComplete(ResetCoin);
    }



    private void OnTriggerEnter(Collider other)
    {
        other.transform.DOKill();
        Destroy(other.gameObject);
    }

    private void ResetCoin()
    {
        coinImage.transform.DOScale(1.0f, 0.3f);
    }
}
