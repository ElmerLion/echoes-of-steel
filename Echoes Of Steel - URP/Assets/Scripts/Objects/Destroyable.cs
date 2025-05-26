using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Destroyable : MonoBehaviour, IInteractable {

    [SerializeField] private ItemSO neededItem;
    [SerializeField] private List<GameObject> needToBeDestroyedFirst;

    [Header("Localization")]
    [SerializeField] private LocalizedString interactText;
    [SerializeField] private LocalizedString destroyTopObjectText;

    public bool IsDestroyed { get; private set; }

    private List<Destroyable> needsToBeDestroyedFirst = new List<Destroyable>();

    private void Start() {
        IsDestroyed = false;

        foreach (GameObject obj in needToBeDestroyedFirst) {
            if (obj != null) {
                Destroyable destroyable = obj.GetComponent<Destroyable>();
                if (destroyable != null) {
                    needsToBeDestroyedFirst.Add(destroyable);
                }
            }
        }
    }

    public string GetInteractText() {
        if (IsDestroyed) return string.Empty;

        return interactText.GetLocalizedString();
    }

    public void Interact() {
        if (IsDestroyed) return;

        if (InventoryManager.Instance.GetSelectedSlot().Item == null) return;

        if (InventoryManager.Instance.GetSelectedSlot().Item.GetItemSO() == neededItem) {
            foreach (Destroyable destroyable in needsToBeDestroyedFirst) {
                if (!destroyable.IsDestroyed) {
                    MessageUI.Instance.ShowMessage(destroyTopObjectText.GetLocalizedString());
                    return;
                }
            }

            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.AddForce(-Player.Instance.transform.forward * Random.Range(0.5f, 3f), ForceMode.Impulse);
            gameObject.layer = 7; // No Collision Layer

            IsDestroyed = true;
            Destroy(gameObject, 3f);
            // Play destroy sound
        }
    }
}
