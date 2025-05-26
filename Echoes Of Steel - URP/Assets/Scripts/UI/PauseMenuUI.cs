using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class PauseMenuUI : BaseUI {

    public static PauseMenuUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PanelButton continueButton;
    [SerializeField] private PanelButton optionsButton;
    [SerializeField] private PanelButton mainMenuButton;
    [SerializeField] private PanelButton quitButton;

    [Header("Localization")]
    [SerializeField] private LocalizedString wantToQuitText;
    [SerializeField] private LocalizedString wantToGoToMainMenuText;

    public bool IsGamePaused => isOpen;

    private void Awake() {
        Instance = this;
    }

    public override void Start() {
        base.Start();

        continueButton.onClick.AddListener(OnContinueButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }

    public override void Show() {
        if (EscapeManager.Instance.HasEscaped) {
            Hide();
            return;
        }
        base.Show();
        Time.timeScale = 0f;

        EventSystem.current.SetSelectedGameObject(continueButton.gameObject);
    }

    public override void Hide() {
        base.Hide();
        Time.timeScale = 1f;
    }

    private void OnContinueButtonClicked() {
        Hide();
    }

    private void OnOptionsButtonClicked() {
        Hide();
        OptionsPanelUI.Instance.Show();
    }

    private void OnQuitButtonClicked() {
        AreYouSureUI.Instance.ShowAreYouSure(wantToQuitText.GetLocalizedString(), () => {
            Application.Quit();
        }, Show);
    }

    private void OnMainMenuButtonClicked() {
        AreYouSureUI.Instance.ShowAreYouSure(wantToGoToMainMenuText.GetLocalizedString(), () => {
            SceneManager.LoadScene("MainMenuScene");
        }, Show);
    }

}
