using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Refiller : MonoBehaviour, IInteractable {

    public enum RefillType {
        None,
        Oil,
    }

    [Header("Localization")]
    [SerializeField] private LocalizedString interactText;

    [Header("Settings")]
    [SerializeField] private ItemSO bucketSO;
    [SerializeField] private RefillType refillType;
    [SerializeField] private GameObject filledItemPrefab;

    public string GetInteractText() {
        if (InventoryManager.Instance.HasItem(bucketSO)) {
            return interactText.GetLocalizedString();
        }

        return "";
    }

    public void Interact() {
        if (InventoryManager.Instance.GetSelectedSlot().Item != null) {
            if (InventoryManager.Instance.GetSelectedSlot().Item.GetItemSO() == bucketSO) {
                Bucket bucket = (Bucket)InventoryManager.Instance.GetSelectedSlot().Item;
                bucket.Fill(refillType, filledItemPrefab);
            }
        }
    }

    public RefillType GetRefillType() {
        return refillType;
    }
}
