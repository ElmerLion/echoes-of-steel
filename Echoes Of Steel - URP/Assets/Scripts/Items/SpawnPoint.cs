using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour {

    public event Action<Item> OnItemRemoved;

    [SerializeField] private ItemSO.ItemType[] allowedItem;

    public Item SpawnedItem { get; set; }
    public bool IsPointTaken { get; private set; }

    public void SetItem(Item item) {
        SpawnedItem = item;

        SpawnedItem.OnItemInteracted += SpawnedItem_OnItemInteracted;
        IsPointTaken = true;
    }

    private void SpawnedItem_OnItemInteracted() {
        Item item = SpawnedItem;

        SpawnedItem = null;
        IsPointTaken = false;

        OnItemRemoved?.Invoke(item);
    }

    public ItemSO.ItemType[] GetAllowedItems() {
        return allowedItem;
    }

    public bool IsSpawnPointEmpty() {
        if (SpawnedItem == null && !IsPointTaken) {
            return true;
        }

        return false;

    }
}
