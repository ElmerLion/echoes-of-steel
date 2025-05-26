using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsPanelUI : BaseUI {

    public static OptionsPanelUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject defaultSelectedOnExit;
    [SerializeField] private GameObject defaultSelectedOnEnter;

    [Header("UI Settings")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Michsky.UI.Heat.Dropdown languageDropdown;
    [SerializeField] private SwitchManager showHudSwitch;
    [SerializeField] private SliderManager mouseSensitivitySlider;
    [SerializeField] private SliderManager brightnessSlider;

    [Header("Audio Settings")]
    [SerializeField] private SliderManager masterVolumeSlider;
    [SerializeField] private SliderManager musicVolumeSlider;
    [SerializeField] private SliderManager sfxVolumeSlider;
    [SerializeField] private SliderManager environmentVolumeSlider;
    [SerializeField] private SliderManager jumpscareVolumeSlider;

    [Header("Controls")]
    [SerializeField] private LocalizedString keyboardText;
    [SerializeField] private LocalizedString gamepadText;
    [SerializeField] private HorizontalSelector gameInputTypeSelector;
    [SerializeField] private LocalizeStringEvent selectedInputText;
    [SerializeField] private ButtonManager interactBinding;
    [SerializeField] private ButtonManager useBinding;
    [SerializeField] private ButtonManager sprintBinding;
    [SerializeField] private ButtonManager walkForwardBinding;
    [SerializeField] private ButtonManager walkBackwardBinding;
    [SerializeField] private ButtonManager walkRightBinding;
    [SerializeField] private ButtonManager walkLeftBinding;
    [SerializeField] private ButtonManager pauseBinding;
    [SerializeField] private ButtonManager dropItemBinding;
    [SerializeField] private Transform pressToRebindKeyTransform;

    public bool ShowStaminaBar { get; private set; }

    private void Awake() {
        Instance = this;
    }


    public override void Start() {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        environmentVolumeSlider.onValueChanged.AddListener(OnEnvironmentVolumeChanged);
        jumpscareVolumeSlider.onValueChanged.AddListener(OnJumpscareVolumeChanged);
        mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);

        gameInputTypeSelector.onValueChanged.AddListener((int index) => {
            GameInput.ControlType controlType = index == 0 ? GameInput.ControlType.Keyboard : GameInput.ControlType.Gamepad;

            if (controlType == GameInput.ControlType.Keyboard) {
                selectedInputText.StringReference = keyboardText;
            } else {
                selectedInputText.StringReference = gamepadText;
            }

            GameInput.Instance.SetControlType(controlType);
            UpdateControlsVisual();
        });

        closeButton.onClick.AddListener(Hide);

        if (SceneManager.GetActiveScene().name == "MainMenuScene") {
            GameInput.Instance.OnPausePerformed += Hide;
        }

        UpdateControlsVisual();
        HidePressToRebindKey();

        interactBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.Interact);
        });

        useBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.Use);
        });

        sprintBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.Sprint);
        });

        walkForwardBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.WalkForward);
        });

        walkBackwardBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.WalkBackward);
        });

        walkRightBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.WalkRight);
        });

        walkLeftBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.WalkLeft);
        });

        pauseBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.Pause);
        });

        dropItemBinding.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.DropItem);
        });

        showHudSwitch.offEvents.AddListener(() => {
            ShowStaminaBar = false;

            if (StaminaUI.Instance != null) {
                StaminaUI.Instance.SetStaminaBarVisibility(false);
            }
            PlayerPrefs.SetInt("ShowStaminaBar", 0);
        });

        showHudSwitch.onEvents.AddListener(() => {
            ShowStaminaBar = true;

            if (StaminaUI.Instance != null) {
                StaminaUI.Instance.SetStaminaBarVisibility(true);
            }
            PlayerPrefs.SetInt("ShowStaminaBar", 1);
        });

        

        GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;

        if (languageDropdown != null) {
            AddLanguagesToDropdown();
            SetCurrentLanguage();
        }

        Hide();
    }


    private void AddLanguagesToDropdown() {
        List<Locale> locales = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;

        int languageIndex = 0;
        foreach (Locale locale in locales) {
            languageDropdown.CreateNewItem(locale.LocaleName, false);
            languageIndex++;
        }

        foreach (Michsky.UI.Heat.Dropdown.Item item in languageDropdown.items) {
            item.onItemSelection.AddListener(() => {
                ChangeLanguage(locales[item.itemIndex]);
            });
        }

        if (languageDropdown.items.Count == 0) {
            Debug.LogError("No languages found for the dropdown!");
        }

        languageDropdown.Initialize();
        UpdateNavigation();
    }

    private void SetCurrentLanguage() {
        string savedLanguage = PlayerPrefs.GetString("Language", UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale.Identifier.Code);

        List<Locale> locales = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;

        foreach (Locale locale in locales) {
            if (locale.Identifier.Code == savedLanguage) {
                UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locale;
                return;
            }
        }

        languageDropdown.SetDropdownIndex(1);
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locales[1];
    }

    private void UpdateNavigation() {
        if (languageDropdown.items.Count == 0) return;

        for (int i = 0; i < languageDropdown.items.Count; i++) {
            Selectable currentButton = languageDropdown.items[i].itemButton.GetComponent<Selectable>();
            ButtonManager buttonManager = languageDropdown.items[i].itemButton.GetComponent<ButtonManager>();
            buttonManager.useUINavigation = true;

            Navigation navigation = currentButton.navigation;
            navigation.mode = Navigation.Mode.Explicit;

            // Set navigation links for up and down
            if (i > 0) {
                Selectable previousButton = languageDropdown.items[i - 1].itemButton.GetComponent<Selectable>();
                navigation.selectOnUp = previousButton;
            }

            if (i < languageDropdown.items.Count - 1) {
                Selectable nextButton = languageDropdown.items[i + 1].itemButton.GetComponent<Selectable>();
                navigation.selectOnDown = nextButton;
            }

            // Apply the navigation to the button
            currentButton.navigation = navigation;

        }
    }





    private void ChangeLanguage(Locale locale) {
        UnityEngine.Localization.Settings.LocalizationSettings.SelectedLocale = locale;

        PlayerPrefs.SetString("Language", locale.Identifier.Code);
        PlayerPrefs.Save();
    }

    private void GameInput_OnBindingRebind(object sender, System.EventArgs e) {
        UpdateControlsVisual();
    }

    public override void Hide() {
        base.Hide();
        Time.timeScale = 1f;

        if (MainMenuUI.Instance != null) {
            MainMenuUI.Instance.Show();
            Cursor.lockState = CursorLockMode.None;
        }
        if (PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.Show();
        }
        EventSystem.current.SetSelectedGameObject(defaultSelectedOnExit);
    }

    public override void Show() {
        base.Show();
        Time.timeScale = 0f;

        EventSystem.current.SetSelectedGameObject(defaultSelectedOnEnter);
        UpdateControlsVisual();
    }

    private void OnBrightnessChanged(float value) {
        float scaledValue = Mathf.Lerp(-5f, 3f, value / 100f);
        BrightnessController.Instance.SetBrightness(scaledValue);
    }

    private void OnMouseSensitivityChanged(float value) {
        GameInput.Instance.SetMouseSensitivity(value);
    }

    private void OnMasterVolumeChanged(float value) {
        AudioManager.Instance.SetVolume(Normalize(value), AudioManager.AudioType.Master);
    }

    private void OnMusicVolumeChanged(float value) {
        AudioManager.Instance.SetVolume(Normalize(value), AudioManager.AudioType.Music);
    }

    private void OnSFXVolumeChanged(float value) {
        AudioManager.Instance.SetVolume(Normalize(value), AudioManager.AudioType.SFX);
    }

    private void OnEnvironmentVolumeChanged(float value) {
        AudioManager.Instance.SetVolume(Normalize(value), AudioManager.AudioType.Environment);
    }

    private void OnJumpscareVolumeChanged(float value) {
        AudioManager.Instance.SetVolume(Normalize(value), AudioManager.AudioType.Jumpscare);
    }

    private float Normalize(float value) {
        return value = value / 100;
    }

    public void UpdateControlsVisual() {
        interactBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.Interact));
        useBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.Use));
        sprintBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.Sprint));
        walkForwardBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.WalkForward));
        walkBackwardBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.WalkBackward));
        walkRightBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.WalkRight));
        walkLeftBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.WalkLeft));
        pauseBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.Pause));
        dropItemBinding.SetText(GameInput.Instance.GetBindingText(GameInput.Binding.DropItem));
    }

    public void UpdateGameInputSelector(GameInput.ControlType controlType) {
        int newIndex = controlType == GameInput.ControlType.Keyboard ? 0 : 1;
        gameInputTypeSelector.index = newIndex;

        if (controlType == GameInput.ControlType.Keyboard) {
            selectedInputText.StringReference = keyboardText;
        } else {
            selectedInputText.StringReference = gamepadText;
        }

        gameInputTypeSelector.UpdateUI();

    }

    private void ShowPressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(true);
    }
    private void HidePressToRebindKey() {
        pressToRebindKeyTransform.gameObject.SetActive(false);
    }

    private void RebindBinding(GameInput.Binding binding) {
        ShowPressToRebindKey();
        GameInput.Instance.RebindBinding(binding, () => {
            HidePressToRebindKey();
            UpdateControlsVisual();
        });
    }

    private void OnDestroy() {
        GameInput.Instance.OnPausePerformed -= Hide;

        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        environmentVolumeSlider.onValueChanged.RemoveListener(OnEnvironmentVolumeChanged);
        jumpscareVolumeSlider.onValueChanged.RemoveListener(OnJumpscareVolumeChanged);
        mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSensitivityChanged);
        brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

        gameInputTypeSelector.onValueChanged.RemoveAllListeners();
        interactBinding.onClick.RemoveAllListeners();
        useBinding.onClick.RemoveAllListeners();
        sprintBinding.onClick.RemoveAllListeners();
        walkForwardBinding.onClick.RemoveAllListeners();
        walkBackwardBinding.onClick.RemoveAllListeners();
        walkRightBinding.onClick.RemoveAllListeners();
        walkLeftBinding.onClick.RemoveAllListeners();
        pauseBinding.onClick.RemoveAllListeners();
        dropItemBinding.onClick.RemoveAllListeners();
    }


}
