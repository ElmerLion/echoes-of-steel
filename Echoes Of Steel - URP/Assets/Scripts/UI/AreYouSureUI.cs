using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AreYouSureUI : BaseUI {

    public static AreYouSureUI Instance { get; private set; }

    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI areYouSureText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private GameObject selectedObjectOnExit;

    private void Awake() {
        Instance = this;
    }

    public void ShowAreYouSure(string text, Action onYesPressed, Action onNoPressed = null) {
        areYouSureText.text = text;

        Show();

        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => {
            Hide();
            if (selectedObjectOnExit != null) {
                EventSystem.current.SetSelectedGameObject(selectedObjectOnExit);
            }
            onYesPressed?.Invoke();
        });

        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() => {
            Hide();
            if (selectedObjectOnExit != null) { 
                EventSystem.current.SetSelectedGameObject(selectedObjectOnExit);
            }
            if (onNoPressed != null) {
                onNoPressed?.Invoke();
            }
        });

        EventSystem.current.SetSelectedGameObject(noButton.gameObject);
    }

    private void OnDestroy() {
        Instance = null;
    }

    public override void Hide() {
        gameObject.SetActive(false);
        isOpen = false;
        if (PlayerLook.Instance != null) {
            PlayerLook.Instance.SetAllowedToMove(true);
        }

        if (UIManager.Instance != null) {
            UIManager.Instance.CheckForOpenedUI();
        }

        yesButton.onClick.RemoveAllListeners();
        noButton.onClick.RemoveAllListeners();
    }

}
