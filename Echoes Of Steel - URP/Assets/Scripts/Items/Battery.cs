using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battery : Item {

    [SerializeField] private ItemSO flashlightSO;

    public override void OnUse() {
        if (!base.CanUse()) return;

        InventoryManager.InventorySlot flashlightSlot = InventoryManager.Instance.GetInventorySlot(flashlightSO);
        if (flashlightSlot != null) {
            Flashlight flashlight = (Flashlight)flashlightSlot.Item;
            flashlight.AddBattery();
            InventoryManager.Instance.RemoveItem(InventoryManager.Instance.GetSelectedSlot(), false);
        }
    }

}
