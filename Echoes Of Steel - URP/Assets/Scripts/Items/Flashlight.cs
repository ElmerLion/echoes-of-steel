using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : Item {

    [SerializeField] private Light flashlight;
    [SerializeField] private float batteryLife = 120f;

    [Header("LED")]
    [SerializeField] private Color chargedColor;
    [SerializeField] private Color mediumChargedColor;
    [SerializeField] private Color dyingColor;
    [SerializeField] private Color deadColor;

    private float batteryTimer;
    private bool flashlightPickedUp;
    private Coroutine ledCoroutine;

    public override void Interact() {
        if (InventoryManager.Instance.TryAddItem(GetItemSO(), gameObject)) {
            AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerPickedUpItem, 0.5f);

            if (!flashlightPickedUp) {
                AddBattery();
                flashlightPickedUp = true;
            }

            InvokeInteractedEvent();
        } else {
            MessageUI.Instance.ShowMessage("Inventory full");
        }
    }

    private void Update() {
        if (flashlight.enabled && flashlightPickedUp) {
            //Debug.Log("Batterytimer: " + batteryTimer);
            batteryTimer -= Time.deltaTime;

            HandleLEDStatus();

            if (batteryTimer <= 0) {
                flashlight.enabled = false;
                ledMaterial.color = deadColor;

                if (batteryTimer < 3f) return;
                HorrorGameManager.TryUnlockAchievement("FLASHLIGHT_OUT");
            }
        }
    }

    private void HandleLEDStatus() {
        if (batteryTimer <= batteryLife / 8f && batteryTimer > 0) {
            if (ledCoroutine == null) {
                ledCoroutine = StartCoroutine(FlashingLed());
            }
        } else if (batteryTimer <= batteryLife / 4f) {
            StopFlashingIfNeeded();
            ledMaterial.color = dyingColor;
        } else if (batteryTimer <= batteryLife / 2f) {
            StopFlashingIfNeeded();
            ledMaterial.color = mediumChargedColor;
        } else {
            StopFlashingIfNeeded();
            ledMaterial.color = chargedColor;
        }
    }

    private void StopFlashingIfNeeded() {
        if (ledCoroutine != null) {
            StopCoroutine(ledCoroutine);
            ledCoroutine = null;
        }
    }

    public override bool CanUse() {
        if (UIManager.Instance.IsAnyUIOpen()) {
            return false;
        }

        if (batteryTimer <= 0) {
            flashlight.enabled = false;
            return false;
        }

        return true;
    }

    public override void OnUse() {
        if (!CanUse()) return;

        flashlight.enabled = !flashlight.enabled;
        AudioManager.Instance.PlaySound(AudioManager.Sound.FlashlightToggle);
    }

    public void AddBattery() {
        batteryTimer = batteryLife;

        flashlight.enabled = true;

        StopFlashingIfNeeded();
        ledMaterial.color = chargedColor;
    }

    public IEnumerator FlashingLed() {
        while (true) {
            ledMaterial.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            ledMaterial.color = Color.gray;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
