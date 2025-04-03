using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour
{
    // Order details
    [Header("Order Details")]
    public string customerName;
    public Transform customerHands;
    internal RecipeSO RequestedOrder;

    [Header("UI for Order")]
    [SerializeField] private GameObject orderUiPrefab;
    [SerializeField] private ParticleSystem coin;
    private Image _orderIcon;
    private Transform _orderUiParent;
    private bool _hasShownOrder;
    private bool _hasReceivedPotion;
    
    private GameObject _orderUiInstance;
    
    [Header("SFX")]
    [SerializeField] private AudioClip sfxOrderIn;

    internal void Assign(RecipeSO recipe, Transform parent)
    {
        // Sets requested order
        RequestedOrder = recipe;
        // Assigns parent for order Ui
        _orderUiParent = parent;
    }

    internal void DisplayOrderUI()
    {
        // if the order has already been shown don't show it again
        if(_hasShownOrder) return;
        
        // creates an instance of the order ui and that the customer keeps track of.
        _orderUiInstance = Instantiate(orderUiPrefab, _orderUiParent);
        var child = _orderUiInstance.transform.GetChild(0);
        _orderIcon = child.GetComponent<Image>();
        _orderIcon.sprite = RequestedOrder.potionIcon;
        
        AudioManager.instance.sfxManager.PlaySFX(SFX_Type.ShopSounds, sfxOrderIn, true);
        _hasShownOrder = true;
    }
    
    internal void RemoveOrderUI() => Destroy(_orderUiInstance);
    
    internal bool HasReceivedPotion() => _hasReceivedPotion;

    internal void Complete()
    {
        _hasReceivedPotion = true;
        Actions.OnCustomerServed?.Invoke(RequestedOrder.sellAmount);
        coin.Play();
        Destroy(_orderUiInstance);
    }
}
