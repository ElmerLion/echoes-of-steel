using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractUI : MonoBehaviour {

    public static InteractUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI interactText;

    private void Awake() {
        Instance = this;
    }

    public void SetText(string text) {
        interactText.text = text;
        interactText.gameObject.SetActive(true);
    }

    public void ClearText() {
        interactText.gameObject.SetActive(false);
    }

}
