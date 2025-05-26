using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PanelButton startButton;
    [SerializeField] private PanelButton optionsButton;
    [SerializeField] private PanelButton creditsButton; 
    [SerializeField] private PanelButton quitButton;
    [SerializeField] private GameObject optionsPanel;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.None;

        startButton.onClick.AddListener(StartGame);
        optionsButton.onClick.AddListener(OpenOptions);
        creditsButton.onClick.AddListener(() => {
            CreditsUI.Instance.Show();
            Hide();
        });
        quitButton.onClick.AddListener(QuitGame);

        AudioManager.Instance.PlayMusic(AudioManager.Music.MainMenuMusic);
    }

    private void StartGame() {
        Loader.Load(Loader.Scene.GameScene);
    }

    private void OpenOptions() {
        OptionsPanelUI.Instance.Show();
        Hide();
    }

    private void QuitGame() {
        Application.Quit();
    }

    private void Hide() {
        startButton.gameObject.SetActive(false);
        optionsButton.gameObject.SetActive(false);
        creditsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    public void Show() {
        startButton.gameObject.SetActive(true);
        optionsButton.gameObject.SetActive(true);
        creditsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

}
