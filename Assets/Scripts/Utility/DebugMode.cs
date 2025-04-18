using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugMode : MonoBehaviour
{
    [SerializeField] private string debugPassword = "Studio_CLAMS";
    
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Toggle debugToggle;
    [SerializeField] private Toggle debugMenuToggle;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI debugTitle;
    
    private bool _isManuallyToggling; // Prevents unwanted function calls

    private Coroutine _textCoroutine;

    private void Start()
    {
        passwordInput.onEndEdit.AddListener(delegate { CheckPassword(); });
        backButton.onClick.AddListener(BackButton);


        debugToggle.isOn = false;
        debugMenuToggle.isOn = false;
        debugToggle.gameObject.SetActive(false);
        debugText.enabled = false;

        debugMenuToggle.onValueChanged.AddListener(delegate { ToggleMenuDebugMode(); });
        debugToggle.onValueChanged.AddListener(delegate { ToggleDebugMode(); });
    }

    private void CheckPassword()
    {
        if (passwordInput.text == debugPassword)
        {
            if(_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            debugTitle.text = "Enter/Exit Debug Mode";
            _textCoroutine = StartCoroutine(VisualText("Debug Mode Unlocked"));

            debugToggle.gameObject.SetActive(true);
            // Clear password input field if the password is incorrect
            passwordInput.text = "";
            passwordInput.gameObject.SetActive(false);
        }
        else
        {
            if (_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            _textCoroutine = StartCoroutine(VisualText("Incorrect Password"));
            // Clear password input field if the password is incorrect
            passwordInput.text = "";
        }
    }

    private void BackButton()
    {
        debugToggle.gameObject.SetActive(false);
        passwordInput.gameObject.SetActive(true);
        debugTitle.text = "Enter Debug Password";
        Actions.OnOpenSettingsAction?.Invoke();
    }

    public void ToggleDebugMode()
    {
        if(_isManuallyToggling) return; // Prevent function from being triggered by value change

        _isManuallyToggling = true;
        debugMenuToggle.isOn = debugToggle.isOn; // Update UI without triggering function
        _isManuallyToggling = false;

        if (debugToggle.isOn)
        {
            if (_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            _textCoroutine = StartCoroutine(VisualText("Debug Mode Enabled"));
            BackButton();
            GameManager.Instance.SetDebugMode(true);
        }
        else
        {
            debugMenuToggle.isOn = false;
            if (_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            _textCoroutine = StartCoroutine(VisualText("Debug Mode Disabled"));
            BackButton();
            GameManager.Instance.SetDebugMode(false);
        }
    }

    public void ToggleMenuDebugMode()
    {
        if (_isManuallyToggling) return; // Prevent execution when toggling programmatically

        _isManuallyToggling = true;
        debugToggle.isOn = debugMenuToggle.isOn; // Update the other toggle
        _isManuallyToggling = false;

        if (debugMenuToggle.isOn)
        {
            if (_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            _textCoroutine = StartCoroutine(VisualText("Debug Mode Enabled"));
            GameManager.Instance.SetDebugMode(true);

        }
        else
        {
            debugToggle.isOn = false;
            if (_textCoroutine != null)
                StopCoroutine(_textCoroutine);

            _textCoroutine = StartCoroutine(VisualText("Debug Mode Disabled"));
            GameManager.Instance.SetDebugMode(false);
        }
    }

    private IEnumerator VisualText(string text)
    {
        debugText.enabled = true;
        debugText.text = text;

        yield return new WaitForSeconds(2f);
        debugText.enabled = false;
    }
}
