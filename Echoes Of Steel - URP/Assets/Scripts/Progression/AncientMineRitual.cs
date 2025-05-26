using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class AncientMineRitual : MonoBehaviour, IInteractable {

    public enum Stage {
        PlaceCandles,
        LightCandles,
        PlacePickaxe,
        PlacePhoto,
        SacrificeSkull,
        Completed,
    }

    [Header("References")]
    [SerializeField] private List<GameObject> candlesToActivate;
    [SerializeField] private List<GameObject> allCandleToLight;
    [SerializeField] private GameObject ritualLight;
    [SerializeField] private Transform pickaxePos;
    [SerializeField] private Transform photoPos;
    [SerializeField] private Transform skullPos;
    [SerializeField] private Transform completionAuraPos;
    [SerializeField] private Transform completionAura;

    [Header("Localization")]
    [SerializeField] private LocalizedString placeCandlesString;
    [SerializeField] private LocalizedString lightCandlesString;
    [SerializeField] private LocalizedString placePickaxeString;
    [SerializeField] private LocalizedString placePhotoString;
    [SerializeField] private LocalizedString sacrificeSkullString;
    [SerializeField] private LocalizedString notAnimatronicInPhotoString;

    [Header("Items")]
    [SerializeField] private ItemSO candlesSO;
    [SerializeField] private ItemSO lighterSO;
    [SerializeField] private ItemSO pickaxeSO;
    [SerializeField] private ItemSO skullSO;

    private Stage currentStage;
    private bool moveAura;

    private void Start() {
        foreach (GameObject candle in candlesToActivate) {
            candle.SetActive(false);
        }
        foreach (GameObject candleLight in allCandleToLight) {
            for (int i = 0; i < 4; i++) {
                candleLight.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        ritualLight.SetActive(false);
        completionAura.gameObject.SetActive(false);

    }

    private void Update() {
        if (moveAura) {
            completionAura.position = Vector3.MoveTowards(completionAura.position, completionAuraPos.position, 0.05f);
            if (Vector3.Distance(completionAura.position, completionAuraPos.position) < 0.1f) {
                moveAura = false;
            }
        }
    }

    public string GetInteractText() {
        switch (currentStage) {
            case Stage.PlaceCandles:
                return placeCandlesString.GetLocalizedString();
            case Stage.LightCandles:
                return lightCandlesString.GetLocalizedString();
            case Stage.PlacePickaxe:
                return placePickaxeString.GetLocalizedString();
            case Stage.PlacePhoto:
                return placePhotoString.GetLocalizedString();
            case Stage.SacrificeSkull:
                return sacrificeSkullString.GetLocalizedString();
            default:
                return "";
        }
    }

    private bool HasItemEquipped(ItemSO itemSO) {
        if (InventoryManager.Instance.GetSelectedSlot().Item == null) return false;
        if (InventoryManager.Instance.GetSelectedSlot().Item.GetItemSO() != itemSO) return false;

        return true;
    }

    public void Interact() {
        InventoryManager.InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();
        bool ritualProgressed = false;
        if (ObjectiveUI.Instance.currentObjective == ObjectiveType.ExploreMine) {
            ObjectiveUI.Instance.SetObjective(ObjectiveType.PerformSecondRitual);
        }

        switch (currentStage) {
            case Stage.PlaceCandles:
                if (HasItemEquipped(candlesSO)) {
                    InventoryManager.Instance.RemoveItem(selectedSlot, false);
                    PlaceCandles();
                    currentStage = Stage.LightCandles;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);
                }
                break;
            case Stage.LightCandles:
                if (HasItemEquipped(lighterSO)) {
                    LightCandles();
                    currentStage = Stage.PlacePickaxe;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);
                }
                break;
            case Stage.PlacePickaxe:
                if (HasItemEquipped(pickaxeSO)) {
                    GameObject itemObject = selectedSlot.Item.gameObject;
                    InventoryManager.Instance.RemoveItem(selectedSlot, true);
                    PlaceObject(itemObject, pickaxePos);
                    currentStage = Stage.PlacePhoto;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);

                }
                break;
            case Stage.PlacePhoto:
                if (selectedSlot.Item == null) return;

                if (selectedSlot.Item.TryGetComponent(out Photo photo)) {
                    if (!photo.isAnimatronicInPhoto) {
                        MessageUI.Instance.ShowMessage(notAnimatronicInPhotoString.GetLocalizedString());
                        return;
                    }
                    GameObject itemObject = selectedSlot.Item.gameObject;
                    InventoryManager.Instance.RemoveItem(selectedSlot, true);
                    PlaceObject(itemObject, photoPos);
                    currentStage = Stage.SacrificeSkull;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);
                }
                break;
            case Stage.SacrificeSkull:
                if (HasItemEquipped(skullSO)) {
                    GameObject itemObject = selectedSlot.Item.gameObject;
                    InventoryManager.Instance.RemoveItem(selectedSlot, true);
                    PlaceObject(itemObject, skullPos);
                    currentStage = Stage.Completed;
                    ritualProgressed = true;
                    EscapeManager.Instance.TriggerEscape();
                    AudioManager.Instance.PlaySound(AudioManager.Sound.SupernaturalDamaged, 5f);

                    moveAura = true;
                    completionAura.gameObject.SetActive(true);

                    ObjectiveUI.Instance.SetObjective(ObjectiveType.FindExit);
                }
                break;
        }

        if (ritualProgressed) {
            ChromaticAbberation.Instance.SetChromaticAberration(1f, 1.5f);
        }
    }


    [Command("PlaceCandlesMine")]
    private void PlaceCandles() {
        foreach (GameObject candle in candlesToActivate) {
            candle.SetActive(true);
        }
    }

    [Command("LightCandlesMine")]
    private void LightCandles() {
        foreach (GameObject candleLight in allCandleToLight) {
            for (int i = 0; i < 4; i++) {
                candleLight.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        ritualLight.SetActive(true);
    }

    private void PlaceObject(GameObject itemObject, Transform placeTransform) {
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

        itemObject.transform.parent = placeTransform;
        itemObject.transform.position = placeTransform.position;
        itemObject.transform.rotation = placeTransform.rotation;
    }
}
