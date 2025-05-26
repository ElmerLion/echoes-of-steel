using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsUI : MonoBehaviour {

    public static CreditsUI Instance { get; private set; }

    private void Awake() {
        Instance = this;

        Hide();
    }

    private void Start() {
        if (SceneManager.GetActiveScene().name == "MainMenuScene") {
            GameInput.Instance.OnAnyAssignedKeybindPerformed += Hide;
        }
    }

    public void Show() {
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
        MainMenuUI.Instance.Show();
    }

    private void OnDestroy() {
        if (GameInput.Instance != null) {
            GameInput.Instance.OnAnyAssignedKeybindPerformed -= Hide;
        }
    }

}
