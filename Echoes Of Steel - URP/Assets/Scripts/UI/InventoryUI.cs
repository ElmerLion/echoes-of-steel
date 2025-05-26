using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InventoryManager;

public class InventoryUI : MonoBehaviour {

    public static InventoryUI Instance { get; private set; }

    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;

    private List<GameObject> uiSlots = new List<GameObject>();  
    private Dictionary<InventorySlot, GameObject> itemSlotMap = new Dictionary<InventorySlot, GameObject>();  // Mapping between ItemSO and their UI slots

    private void Awake() {
        Instance = this;
    }

    public void InitializeInventoryUI() {
        InventoryManager.Instance.OnItemAdded += AddItem;
        InventoryManager.Instance.OnItemRemoved += RemoveItem;
        InventoryManager.Instance.OnSlotSelected += UpdateSelectedSlotUI;

        // Initialize UI slots based on available slots in InventoryManager
        foreach (Transform child in slotContainer) {
            GameObject itemSpriteObject = child.Find("ItemSprite").gameObject;
            GameObject itemHighlightedObject = child.Find("Highlighted").gameObject;
            itemHighlightedObject.SetActive(false);
            itemSpriteObject.SetActive(false);
            uiSlots.Add(child.gameObject);
        }

        foreach (InventorySlot invSlot in InventoryManager.Instance.GetInventory()) {
            AddItem(invSlot, null);
        }
    }

    private void AddItem(InventorySlot invSlot, Sprite itemSprite) {
        int slotIndex = InventoryManager.Instance.GetInventorySlotIndex(invSlot);
        GameObject uiSlot = uiSlots[slotIndex];

        if (invSlot.Item != null) {
            uiSlot.transform.Find("ItemSprite").gameObject.SetActive(true);
            uiSlot.transform.Find("ItemSprite").GetComponent<Image>().sprite = itemSprite;
        }

        itemSlotMap[invSlot] = uiSlot;
    }


    private void RemoveItem(InventorySlot invSlot) {
        if (invSlot== null) {
            return;
        }

        GameObject slot = itemSlotMap[invSlot];
        slot.transform.Find("ItemSprite").gameObject.SetActive(false);
    }

    public void UpdateSelectedSlotUI(InventorySlot selectedSlot) {
        foreach (GameObject slot in uiSlots) {
            slot.transform.Find("Highlighted").gameObject.SetActive(false);  
        }

        if (selectedSlot != null) {
            GameObject selectedSlotUI = itemSlotMap[selectedSlot];
            selectedSlotUI.transform.Find("Highlighted").gameObject.SetActive(true);  
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

}
