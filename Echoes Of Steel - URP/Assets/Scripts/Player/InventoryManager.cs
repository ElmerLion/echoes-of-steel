using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class InventoryManager : MonoBehaviour {

    public static InventoryManager Instance { get; private set; }

    [SerializeField] private List<InventorySlot> inventory = new List<InventorySlot>();
    [SerializeField] private int maxInventoryItems = 5;
    [SerializeField] private Transform itemHoldLoc;

    [Header("Item Sway Settings")]
    [SerializeField] private float swayAmount = 0.05f; 
    [SerializeField] private float swaySpeed = 5f; 
    [SerializeField] private float swayRotationAmount = 2f;
    [SerializeField] private float swayRotationSpeed = 5f;

    private int selectedSlotIndex;
    private InventorySlot selectedSlot;

    private Vector3 initialItemLocalPosition; 
    private Quaternion initialItemLocalRotation;
    private Transform currentItemTransform;

    private bool canUseInventory;

    public event Action<InventorySlot> OnItemRemoved;
    public event Action<InventorySlot, Sprite> OnItemAdded;
    public event Action<InventorySlot> OnSlotSelected;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        for (int i = 0; i < maxInventoryItems; i++) {
            inventory.Add(new InventorySlot());
        }

        InventoryUI.Instance.InitializeInventoryUI();

        if (inventory.Count > 0) {
            SelectSlot(inventory[0]);
        }

        GameInput.Instance.OnUse += () => {
            if (selectedSlot.Item != null) {
                selectedSlot.Item.OnUse();
            }
        };

        GameInput.Instance.OnDropItem += () => {
            if (inventory.Count > 0) {
                if (selectedSlot.Item != null) {
                    RemoveItem(selectedSlot);
                }
            }
        };

        UIManager.Instance.OnAnyUIOpened += () => {
            canUseInventory = false;
        };

        UIManager.Instance.OnAllUIClosed += () => {
            canUseInventory = true;
        };

        GameInput.Instance.OnInventoryNavigate += GameInput_OnInventoryNavigate;
    }

    private void GameInput_OnInventoryNavigate(int direction) {
        if (UIManager.Instance.IsAnyUIOpen()) return;

        selectedSlotIndex += direction;

        if (selectedSlotIndex >= inventory.Count) {
            selectedSlotIndex = 0;
        } else if (selectedSlotIndex < 0) {
            selectedSlotIndex = inventory.Count - 1;
        }

        SelectSlot(inventory[selectedSlotIndex]);
    }

    private void Update() {
        if (!canUseInventory) return;

        HandleItemSway();
    }

    private void HandleItemSway() {
        if (selectedSlot?.Item == null || currentItemTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = Input.GetAxis("Mouse Y") * swayAmount;

        Vector3 targetPosition = new Vector3(-mouseX, -mouseY, 0f) + initialItemLocalPosition;
        currentItemTransform.localPosition = Vector3.Lerp(currentItemTransform.localPosition, targetPosition, Time.deltaTime * swaySpeed);

        Quaternion targetRotation = Quaternion.Euler(mouseY * swayRotationAmount, mouseX * swayRotationAmount, 0f) * initialItemLocalRotation;
        currentItemTransform.localRotation = Quaternion.Slerp(currentItemTransform.localRotation, targetRotation, Time.deltaTime * swayRotationSpeed);
    }


    public bool TryAddItem(ItemSO itemSO, GameObject itemObject = null, bool isPhoto = false, Sprite photoSprite = null, bool isAnimatronicInPhoto = false) {
        if (inventory.Count > 0) {
            bool inventoryFull = true;
            foreach (InventorySlot invSlot in inventory) {
                if (invSlot.Item == null) {
                    inventoryFull = false;
                    break;
                }
            }

            if (inventoryFull) {
                Debug.Log("Inventory is full");
                return false;
            }
        }

        if (itemObject == null) {
            itemObject = Instantiate(itemSO.prefab, itemHoldLoc);
        } else {
            itemObject.transform.SetParent(itemHoldLoc);
            itemObject.transform.localPosition = Vector3.zero; 
            itemObject.transform.localRotation = Quaternion.identity;
        }
        Item item = itemObject.GetComponent<Item>();

        itemObject.transform.localPosition += item.GetHoldOffset();
        itemObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        Sprite itemSprite = photoSprite != null ? photoSprite : itemSO.itemSprite;

        if (isPhoto) {
            Photo photoItem = itemObject.gameObject.GetComponent<Photo>();
            photoItem.Setup(isAnimatronicInPhoto);
            photoItem.SetSprite(photoSprite);
        }

        if (itemObject.TryGetComponent(out Rigidbody rb)) {
            rb.isKinematic = true;
        } else {
            rb = itemObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        Collider[] colliders = itemObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) {
            collider.enabled = false;
        }

        currentItemTransform = itemObject.transform;
        initialItemLocalPosition = currentItemTransform.localPosition;
        initialItemLocalRotation = currentItemTransform.localRotation;

        if (selectedSlot.Item == null) {
            selectedSlot.SetItem(item);
            SelectSlot(selectedSlot);
            OnItemAdded?.Invoke(selectedSlot, itemSprite);
            return true;
        }

        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item == null) {
                invSlot.SetItem(item);
                OnItemAdded?.Invoke(invSlot, itemSprite);

                SelectSlot(invSlot);
                break;
            }
        }
        return true;
    }

    public void RemoveItem(InventorySlot invSlot, bool dropItem = true) {
        if (invSlot.Item == null) return;

        GameObject itemObject = invSlot.Item.gameObject;

        if (dropItem) {
            invSlot.Item.gameObject.transform.parent = null;
            invSlot.Item.gameObject.layer = LayerMask.NameToLayer("Default");

            if (itemObject.TryGetComponent(out Rigidbody rb)) {
                rb.isKinematic = false;
            } else {
                rb = itemObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
            }

            Collider[] colliders = itemObject.GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders) {
                collider.enabled = true;
            }
        } else {
            Destroy(itemObject);
        }


        invSlot.SetItem(null);
        currentItemTransform = null;

        OnItemRemoved?.Invoke(invSlot);
    }



    private void SelectSlot(InventorySlot inventorySlot) {
        foreach (InventorySlot invSlot in inventory) {
            invSlot.Item?.gameObject.SetActive(false);
        }

        selectedSlot = inventorySlot;
        if (selectedSlot.Item != null) {
            Item item = selectedSlot.Item;

            item.gameObject.SetActive(true);
            item.transform.position = itemHoldLoc.position + item.GetHoldOffset();

            currentItemTransform = item.transform;
            initialItemLocalPosition = currentItemTransform.localPosition;
            initialItemLocalRotation = currentItemTransform.localRotation;

            item.InvokeOnEquipped();
        }
        selectedSlotIndex = inventory.IndexOf(selectedSlot);

        OnSlotSelected?.Invoke(selectedSlot);
    }

    public bool HasItemEquipped(ItemSO itemSO) {
        return selectedSlot.Item.GetItemSO() == itemSO;
    }

    private void DebugPrintInventoryToConsole() {
        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item != null) {
                Debug.Log(inventory.IndexOf(invSlot) + " " + invSlot.Item.GetItemSO().itemName);
            }
        }
    }

    public InventorySlot GetInventorySlot(ItemSO itemSO) {
        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item == null) continue;

            if (invSlot.Item.GetItemSO() == itemSO) {
                return invSlot;
            }
        }

        return null;
    }

    public InventorySlot GetEmptyInventorySlot() {
        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item == null) {
                return invSlot;
            }
        }

        return null;
    }


    public bool HasItem(ItemSO itemSO) {
        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item == null) continue;

            if (invSlot.Item.GetItemSO() == itemSO) {
                return true;
            }
        }

        return false;
    }

    public int GetInventorySlotIndex(InventorySlot inventorySlot) {
        return inventory.IndexOf(inventorySlot);
    }

    public InventorySlot GetSelectedSlot() {
        return selectedSlot;
    }

    public List<InventorySlot> GetInventory() {
        return inventory;
    }

    public bool IsInventoryFull() {
        foreach (InventorySlot invSlot in inventory) {
            if (invSlot.Item == null) {
                return false;
            }
        }

        return true;
    }

    [Serializable]
    public class InventorySlot {
        public Item Item { get; private set; }

        public void SetItem(Item item) {
            Item = item;
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnUse -= () => {
            if (selectedSlot.Item != null) {
                selectedSlot.Item.OnUse();
            }
        };

        GameInput.Instance.OnDropItem -= () => {
            if (inventory.Count > 0) {
                if (selectedSlot.Item != null) {
                    RemoveItem(selectedSlot);
                }
            }
        };

        GameInput.Instance.OnInventoryNavigate -= GameInput_OnInventoryNavigate;

        UIManager.Instance.OnAnyUIOpened -= () => {
            canUseInventory = false;
        };

        UIManager.Instance.OnAllUIClosed -= () => {
            canUseInventory = true;
        };
    }
}
