using UnityEngine;

public interface IPickupable
{
    void OnPickup(PickupInteractionController controller);
    void OnDrop();

    Sprite GetSprite();
}
