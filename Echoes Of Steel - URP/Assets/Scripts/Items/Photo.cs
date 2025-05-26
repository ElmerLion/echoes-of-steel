using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Photo : Item {

    [SerializeField] private GameObject photoImage;

    public bool isAnimatronicInPhoto = false;
    private Sprite photoSprite;

    public void Setup(bool isAnimatronicInPhoto) {
        this.isAnimatronicInPhoto = isAnimatronicInPhoto;
    }

    public override void Interact() {
        if (InventoryManager.Instance.TryAddItem(GetItemSO(), null, true, photoSprite, isAnimatronicInPhoto)) {
            Destroy(gameObject);
        } else {
            MessageUI.Instance.ShowMessage("Inventory full");
        }
    }

    public void SetSprite(Sprite sprite) {
        photoSprite = sprite;
        photoImage.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    public Sprite GetSprite() {
        return photoSprite;
    }

}
