using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class EscapeDoor : MonoBehaviour, IInteractable {

    [SerializeField] private LocalizedString escapeText;

    public string GetInteractText() {
        return escapeText.GetLocalizedString();
    }

    public void Interact() {
        EscapeManager.Instance.Escape();
    }
}
