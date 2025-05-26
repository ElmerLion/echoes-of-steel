using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFSW.QC;
using UnityEngine.Localization;

public class FactoryRitual : MonoBehaviour, IInteractable {

    public static FactoryRitual Instance { get; private set; }

    public enum Stage {
        PlaceCandles,
        LightCandles,
        DrawCircle,
        UseCross,
        Completed
    }

    [Header("References")]
    [SerializeField] private List<GameObject> candlesToActivate;
    [SerializeField] private List<GameObject> allCandleToLight;
    [SerializeField] private GameObject ritualLight;
    [SerializeField] private GameObject circle;
    [SerializeField] private Transform photoPos;
    [SerializeField] private Transform completionAura;
    [SerializeField] private Transform completionAuraPos;

    [Header("Localization")]
    [SerializeField] private LocalizedString placeCandlesString;
    [SerializeField] private LocalizedString lightCandlesString;
    [SerializeField] private LocalizedString drawCircleString;
    [SerializeField] private LocalizedString useCrossString;

    [Header("Items")]
    [SerializeField] private ItemSO candlesSO;
    [SerializeField] private ItemSO lighterSO;
    [SerializeField] private ItemSO highlighterSO;
    [SerializeField] private ItemSO crossSO;

    [Header("Ancient Mine")]
    [SerializeField] private List<GameObject> ancientMineOpenObjects;
    [SerializeField] private List<GameObject> ancientMineClosedObjects;


    private Stage currentStage = Stage.PlaceCandles;
    private bool moveAura;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        foreach (GameObject candle in candlesToActivate) {
            candle.SetActive(false);
        }
        foreach (GameObject candleLight in allCandleToLight) {
            for (int i = 0; i < 5; i++) {
                candleLight.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        ritualLight.SetActive(false);
        circle.SetActive(false);

        SetStateAncientMine(false);
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

    public void Interact() {
        InventoryManager.InventorySlot selectedSlot = InventoryManager.Instance.GetSelectedSlot();
        bool ritualProgressed = false;

        if (ObjectiveUI.Instance.currentObjective == ObjectiveType.FindWayOut) {
            ObjectiveUI.Instance.SetObjective(ObjectiveType.PerformFirstRitual);
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
                    currentStage = Stage.DrawCircle;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);
                }
                break;
            case Stage.DrawCircle:
                if (HasItemEquipped(highlighterSO)) {
                    InventoryManager.Instance.RemoveItem(selectedSlot, false);
                    DrawCircle();
                    currentStage = Stage.UseCross;
                    ritualProgressed = true;
                    AudioManager.Instance.PlaySound(AudioManager.Sound.RitualProgress, 2f);
                }
                break;
            case Stage.UseCross:
                if (HasItemEquipped(crossSO)) {
                    InventoryManager.Instance.RemoveItem(selectedSlot, false);
                    currentStage = Stage.Completed;
                    ritualProgressed = true;
                    SetStateAncientMine(true);
                    AudioManager.Instance.PlaySound(AudioManager.Sound.SupernaturalDamaged, 5f);

                    moveAura = true;
                    completionAura.gameObject.SetActive(true);

                    ObjectiveUI.Instance.SetObjective(ObjectiveType.FindSoundSource);

                    HorrorGameManager.TryUnlockAchievement("PERFORM_FIRST_RITUAL");
                }

                break;
            default:
                break;
        }

        if (ritualProgressed) {
            ChromaticAbberation.Instance.SetChromaticAberration(1f, 1.5f);
        }
    }

    public string GetInteractText() {
        switch (currentStage) {
            case Stage.PlaceCandles:
                return placeCandlesString.GetLocalizedString();
            case Stage.LightCandles:
                return lightCandlesString.GetLocalizedString();
            case Stage.DrawCircle:
                return drawCircleString.GetLocalizedString();
            case Stage.UseCross:
                return useCrossString.GetLocalizedString();
            default:
                return "";
        }
    }



    private void PlaceCandles() {
        foreach (GameObject candle in candlesToActivate) {
            candle.SetActive(true);
        }

        foreach (GameObject candleLight in allCandleToLight) {
            for (int i = 0; i < 4; i++) {
                candleLight.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        ritualLight.SetActive(false);
    }

    private void LightCandles() {
        foreach (GameObject candleLight in allCandleToLight) {
            for (int i = 0; i < 4; i++) {
                candleLight.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

        ritualLight.SetActive(true);
    }

    private void DrawCircle() {
        circle.SetActive(true);
    }

    private void PlacePhoto(GameObject itemObject) {
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

        itemObject.transform.parent = photoPos;
        itemObject.transform.position = photoPos.position;
        itemObject.transform.rotation = photoPos.rotation;
    }


    private bool HasItemEquipped(ItemSO itemSO) {
        if (InventoryManager.Instance.GetSelectedSlot().Item == null) return false;
        if (InventoryManager.Instance.GetSelectedSlot().Item.GetItemSO() != itemSO) return false;

        return true;
    }

    [Command]
    private void SetStateAncientMine(bool isOpen) {
        foreach (GameObject obj in ancientMineOpenObjects) {
            obj.SetActive(isOpen);
        }

        foreach (GameObject obj in ancientMineClosedObjects) {
            obj.SetActive(!isOpen);
        }

        if (isOpen) {
            AudioManager.Instance.PlaySound(AudioManager.Sound.AncientMineOpened, ancientMineOpenObjects[0].transform.position, 5f);
            CameraShake.Instance.ShakeCamera(2.5f, 0.4f);
            AnimatronicManager.Instance.SetStage(AnimatronicManager.Stage.Mine);
        } 
    }
}
