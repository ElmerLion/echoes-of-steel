using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Item : MonoBehaviour, IInteractable {

    public event Action OnItemInteracted;
    public event Action OnEquipped;

    [Header("Localization")]
    [SerializeField] private LocalizedString interactText;
    [SerializeField] private LocalizedString inventoryFullText;

    [Header("Item Settings")]
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private Vector3 holdOffset = Vector3.zero;

    [Header("Flashlight")]
    [SerializeField] public GameObject flashlightPowerLed;

    private Color glowColor = Color.blue; 
    private float glowIntensity = 0.015f;       
    private List<Material> materials = new List<Material>();
    private Dictionary<Material, Color> originalEmissionColors = new Dictionary<Material, Color>();
    private bool isGlowing = false;
    public Material ledMaterial { get; private set; }

    public ItemSO GetItemSO() {
        return itemSO;
    }

    public Vector3 GetHoldOffset() {
        return holdOffset;
    }

    private void Start() {
        if (flashlightPowerLed != null) {
            ledMaterial = flashlightPowerLed.GetComponent<Renderer>().material;
        }

        interactText.TableEntryReference = "Interact_Pickup";

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            foreach (Material material in renderer.materials) {
                if (material.HasProperty("_EmissionColor")) {

                    if (material == ledMaterial) continue;

                    materials.Add(material);
                    originalEmissionColors[material] = material.GetColor("_EmissionColor");
                }
            }
        }
    }

    public virtual bool CanUse() {
        if (UIManager.Instance.IsAnyUIOpen()) {
            Debug.Log("Can't use item while UI is open");
            return false;
        }

        return true;
    }

    public string GetInteractText() {
        string itemName = GetItemSO().itemName.GetLocalizedString();
        return string.Format(interactText.GetLocalizedString(), itemName);
    }

    public virtual void Interact() {
        if (InventoryManager.Instance.TryAddItem(GetItemSO())) {
            Destroy(gameObject);
            AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerPickedUpItem, 2f);

            OnItemInteracted?.Invoke();
        } else {
            MessageUI.Instance.ShowMessage(inventoryFullText.GetLocalizedString());
        }
    }

    public virtual void OnUse() {
        if (!CanUse()) return;
        // OnUse
    }

    public void InvokeInteractedEvent() {
        OnItemInteracted?.Invoke();
    }

    public void EnableGlow() {
        if (isGlowing) return;
        isGlowing = true;

        foreach (Material material in materials) {
            if (material == ledMaterial) continue;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    public void DisableGlow() {
        if (!isGlowing) return;
        isGlowing = false;

        foreach (Material material in materials) {
            if (material == ledMaterial) continue;
            if (originalEmissionColors.TryGetValue(material, out Color originalColor)) {
                material.SetColor("_EmissionColor", originalColor);
            }
            material.DisableKeyword("_EMISSION");
        }
    }

    public void InvokeOnEquipped() {
        OnEquipped?.Invoke();
    }
} 
