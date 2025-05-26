using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HotkeyTooltipUI : MonoBehaviour {

    public static HotkeyTooltipUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI hotkeyText;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        HideHotkey();
    }

    public void ShowHotkey(GameInput.Binding binding) {
        hotkeyText.text = GameInput.Instance.GetBindingText(binding);
        gameObject.SetActive(true);
    }

    public void HideHotkey() {
        gameObject.SetActive(false);
    }

}
