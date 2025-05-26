using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization;

public class EscapeUI : BaseUI {

    public static EscapeUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI timePassedText;
    [SerializeField] private PanelButton playAgainButton;
    [SerializeField] private PanelButton mainMenuButton;
    [SerializeField] private PanelButton quitButton;

    [Header("Localization")]
    [SerializeField] private LocalizedString timePassedStringMinutes;
    [SerializeField] private LocalizedString timePassedStringHours;

    private void Awake() {
        Instance = this;
    }

    public override void Start() {
        base.Start();
        playAgainButton.onClick.AddListener(PlayAgain);
        mainMenuButton.onClick.AddListener(MainMenu);
        quitButton.onClick.AddListener(Quit);
    }

    public void Show(float secondsPassed, float minutesPassed, float hoursPassed) {
        base.Show();

        secondsPassed = Mathf.Floor(secondsPassed);
        minutesPassed = Mathf.Floor(minutesPassed);
        hoursPassed = Mathf.Floor(hoursPassed);

        if (hoursPassed > 0) {
            timePassedText.text = string.Format(timePassedStringHours.GetLocalizedString(), hoursPassed, minutesPassed, secondsPassed);
            return;
        }
        timePassedText.text = string.Format(timePassedStringHours.GetLocalizedString(), minutesPassed, secondsPassed);
    }

    private void PlayAgain() {
        Loader.Load(Loader.Scene.GameScene);
    }

    private void MainMenu() {
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    private void Quit() {
        Application.Quit();
    }

}
