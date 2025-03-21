using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private bool isLevelSelectButton;
    private GameObject _button;
    private Button _thisButton;

    private void Awake()
    {
        _button = this.gameObject;
        _thisButton = _button.GetComponent<Button>();
        if (_thisButton == null)
            Debug.LogError("Button component missing on " + gameObject.name);
        
    }


    private void OnEnable()
    {
        _thisButton.onClick.AddListener(ResizeButton);
    }

    private void OnDisable()
    {
        _thisButton.onClick.RemoveListener(ResizeButton);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_thisButton == null || _button == null) return; // Prevent null reference
        if (!_thisButton.interactable) return; // Prevent interaction with disabled buttons

        _button.transform.DOKill();
        // Ensure independent updates and smoother scaling.

        if (isLevelSelectButton)
            _button.transform.SetAsLastSibling();

        _button.transform.DOScale(1.3f, 0.2f).SetUpdate(true).SetEase(Ease.OutBack);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_thisButton == null || _button == null) return; // Prevent null reference
        if (!_thisButton.interactable) return; // Prevent interaction with disabled buttons

        if (isLevelSelectButton)
            _button.transform.SetAsFirstSibling();

        _button.transform.DOKill();
        // Ensure independent updates and smoother scaling.
        _button.transform.DOScale(1f, 0.2f).SetUpdate(true).SetEase(Ease.OutBack);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (_thisButton == null || _button == null) return; // Prevent null reference
        if (!_thisButton.interactable) return; // Prevent interaction with disabled buttons
        _button.transform.DOKill();
        // Ensure independent updates and smoother scaling.
        if (isLevelSelectButton)
            _button.transform.SetAsLastSibling();

        _button.transform.DOScale(1.3f, 0.2f).SetUpdate(true).SetEase(Ease.OutBack);
        
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (_thisButton == null || _button == null) return; // Prevent null reference
        if (!_thisButton.interactable) return; // Prevent interaction with disabled buttons

        if (isLevelSelectButton)
            _button.transform.SetAsFirstSibling();

        _button.transform.DOKill();
        // Ensure independent updates and smoother scaling.
        _button.transform.DOScale(1f, 0.2f).SetUpdate(true).SetEase(Ease.OutBack);
    }

    public void ResizeButton()
    {
        if (isLevelSelectButton)
            _button.transform.SetAsFirstSibling();

        _button.transform.DOKill();
        // Ensure independent updates and smoother scaling.
        _button.transform.DOScale(1f, 0.2f).SetUpdate(true).SetEase(Ease.OutBack);
        
        if (EventSystem.current.currentSelectedGameObject == _button)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
