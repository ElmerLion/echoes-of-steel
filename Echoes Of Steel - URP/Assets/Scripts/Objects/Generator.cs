using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Generator : MonoBehaviour, IInteractable {

    [Header("Localization")]
    [SerializeField] private LocalizedString interactText;

    [Header("Settings")]
    [SerializeField] private ItemSO bucketSO;
    [SerializeField] private float oilAmount = 0;
    [SerializeField] private float maxOilAmount = 100;
    [SerializeField] private float oilConsumptionRate = 1;
    [SerializeField] private float oilRefillAmount = 100;

    private bool isRunning = false;

    private void Start() {
        AudioManager.Instance.AssignGenerator(GetComponent<AudioSource>());
    }

    private void Update() {
        if (oilAmount > 0) {
            oilAmount -= oilConsumptionRate * Time.deltaTime;
            if (!isRunning) {
                isRunning = true;
                LightManager.Instance.FlashLightsContinuously(LightManager.Instance.GetLightStageObject(LightManager.LightType.Factory));
                AudioManager.Instance.PlayGeneratorSound();
            }
        } else {
            if (isRunning) {
                isRunning = false;
                LightManager.Instance.StopFlashingLights(LightManager.Instance.GetLightStageObject(LightManager.LightType.Factory));
                AudioManager.Instance.StopGeneratorSound();
            }
        }
    }

    public void AddOil() {
        if (oilAmount < maxOilAmount) {
            oilAmount += oilRefillAmount;
        }
    }

    public void Interact() {
        if (IsValidBucket()) {
            Bucket bucket = (Bucket)InventoryManager.Instance.GetSelectedSlot().Item;
            bucket.Empty();
            AddOil();

            HorrorGameManager.TryUnlockAchievement("GENERATOR_POWERED");
        }
    }

    public string GetInteractText() {
        if (IsValidBucket()) {
            return interactText.GetLocalizedString();
        }

        return "";
    }

    private bool IsValidBucket() {
        if (InventoryManager.Instance.GetSelectedSlot().Item == null) return false;
        if (InventoryManager.Instance.GetSelectedSlot().Item.GetItemSO() != bucketSO) return false;

        Bucket bucket = (Bucket)InventoryManager.Instance.GetSelectedSlot().Item;
        if (!bucket.IsFilled()) return false;
        if (bucket.GetRefillType() != Refiller.RefillType.Oil) return false;

        return true;
    }
}
