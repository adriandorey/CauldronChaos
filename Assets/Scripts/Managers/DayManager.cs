using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;

public class DayManager : MonoBehaviour
{
    [Header("Day Timer Settings")]
    [Tooltip("In Minutes")]
    [SerializeField] private float dayLength = 3f;

    private CustomTimer _gameplayTimer;
    private bool _gameplayTimerStarted;
    private float _lastPulseTime = -1f; // Track last pulse time

    [Header("Analog Timer")]
    [SerializeField] private RectTransform timerHand;

    [SerializeField] private Image timerFill;

    [Header("Digital Timer")]
    [SerializeField] private TextMeshProUGUI clockText;

    [SerializeField] private GameObject digitalText;

    private int _currentDay;

    [Header("Day Start Overlay")]
    [SerializeField] private GameObject dayStartOverlay;

    [SerializeField] private TextMeshProUGUI panelExplanation;

    [TextArea]
    [SerializeField] private string[] dayExplanation;
    [SerializeField] private Color timerColour;

    [Header("SFX")]
    [SerializeField] private AudioClip startDaySfx;

    [SerializeField] private AudioClip endDaySfx;

    private Coroutine _coroutine;

    // Start is called before the first frame update
    private void Start()
    {
        _gameplayTimer = new CustomTimer(dayLength, true);
    }

    #region OnEnable / OnDisable / OnDestroy Events

    private void OnEnable()
    {
        Actions.OnResetValues += ResetValues;
        Actions.OnStartDay += StartDayTimer;
    }

    private void OnDisable()
    {
        Actions.OnResetValues -= ResetValues;
        Actions.OnStartDay -= StartDayTimer;
    }

    private void OnDestroy()
    {
        Actions.OnResetValues -= ResetValues;
        Actions.OnStartDay -= StartDayTimer;
    }

    #endregion

    private void Update()
    {
        if (!_gameplayTimerStarted) return;

        if (_gameplayTimer.UpdateTimer())
        {
            Actions.OnEndDay?.Invoke();
            Actions.OnStateChange("EndOfDay");

            _gameplayTimerStarted = false;

            //playing end of day SFX
            AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, endDaySfx, true);
            return;
        }

        SetTimerRotation();
        SetDigitalClock();
    }

    private void StartDayTimer()
    {
        _gameplayTimerStarted = true;
        _gameplayTimer.StartTimer();
    }


    public void SetDay(int day)
    {
        _currentDay = day;
        Actions.OnSetDay?.Invoke(_currentDay - 1);
    }

    private void SetTimerRotation()
    {
        var elapsedTime = _gameplayTimer.elapsedTime;

        timerFill.fillAmount = elapsedTime / (dayLength * 60);

        // Calculate how far along the rotation should be (0� to -360�)
        var rotationAngle = Mathf.Lerp(0, -360, elapsedTime / (dayLength * 60));

        // Apply rotation directly without animation
        timerHand.rotation = Quaternion.Euler(0, 0, rotationAngle);
        Pulse();
    }

    private void SetDigitalClock()
    {
        var remainingTime = _gameplayTimer.GetRemainingTime();
        var minutes = Mathf.FloorToInt(remainingTime / 60);
        var seconds = Mathf.FloorToInt(remainingTime % 60);
        clockText.text = $"{minutes:00}:{seconds:00}";
    }

    private void Pulse()
    {
        var remainingSeconds = Mathf.FloorToInt(_gameplayTimer.GetRemainingTime());

        if (remainingSeconds > 60) return;
        
        // pulse every 15 seconds before 30s, then every 5 seconds afterwards
        var pulseInterval = remainingSeconds > 30 ? 15 : 5;


        if(remainingSeconds <= 30)
        {
            clockText.color = timerColour;
        }

        // Check if it's time to pulse and ensure it doesn't repeat in the same second
        if (remainingSeconds % pulseInterval != 0 || Mathf.Approximately(_lastPulseTime, remainingSeconds)) return;
        
        _lastPulseTime = remainingSeconds; // update last pulse time
        timerHand.DOScale(1.5f, 0.3f).SetLoops(2, LoopType.Yoyo);
        digitalText.transform.DOScale(1.5f,0.3f).SetLoops(2, LoopType.Yoyo);
    }
    
    

    internal void ShowStartDayPanel()
    {
        //Debug.Log("Day Countdown Started");
        if (_currentDay % 2 == 0)
        {
            Actions.OnStartChallenge?.Invoke(_currentDay / 2);
        }
        else
        {
            AudioManager.instance.environmentManager.StartEnvironmentSFX(EnvironmentSound.regular);
        }

        if (_currentDay > 6)
            Actions.OnStartGoblin?.Invoke(false);

        if (dayExplanation.Length == 0)
            Debug.LogError("Day Explanation is empty");

        panelExplanation.text = dayExplanation[_currentDay - 1];
        dayStartOverlay.SetActive(true);

        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(1);
        yield return WaitForAnyKeyPress();
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, startDaySfx, true);
        dayStartOverlay.SetActive(false);
       
        if (_currentDay == 1)
        {
            GameManager.Instance.SetTutorialMode(true);
        }
        else
        {
            Actions.OnStartDay?.Invoke();
            StartDayTimer();
        }
    }


    private void ResetValues()
    {
        timerFill.fillAmount = 0;
        _gameplayTimerStarted = false;
        _gameplayTimer.ResetTimer();
        SetDigitalClock();
        SetTimerRotation();
    }

    // Used for loading screen between level selection or main menu and gameplay
    private IEnumerator WaitForAnyKeyPress()
    {
        // Cache the keyboard and gamepad references
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;

        while (true)
        {
            // Check for any key press on the keyboard
            if (keyboard?.anyKey.wasPressedThisFrame == true)
            {
                yield break; // Exit the loop and the coroutine
            }

            // Check for any button press on the gamepad
            if (gamepad != null)
            {
                if (gamepad.buttonSouth.wasPressedThisFrame ||
                    gamepad.buttonNorth.wasPressedThisFrame ||
                    gamepad.buttonEast.wasPressedThisFrame ||
                    gamepad.buttonWest.wasPressedThisFrame ||
                    gamepad.leftStick.ReadValue().magnitude > 0.1f ||
                    gamepad.rightStick.ReadValue().magnitude > 0.1f ||
                    gamepad.dpad.ReadValue().magnitude > 0.1f)
                {
                    yield break; // Exit the loop and the coroutine
                }
            }

            yield return null;
        }
    }
}