using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUpManager : MonoBehaviour {

    [SerializeField] private Transform mainMenuCameraPos;
    [SerializeField] private List<Transform> mainMenuUIItems;
    [SerializeField] private Transform setBrightnessUI;
    [SerializeField] private SliderManager brightnessSlider;
    [SerializeField] private PanelButton continueButton;
    [SerializeField] private Camera mainCamera;

    private bool hasSetBrightness;

    private void Start() {
        hasSetBrightness = PlayerPrefs.HasKey("Brightness");

        if (!hasSetBrightness) {
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            continueButton.onClick.AddListener(OnContinueClicked);

            brightnessSlider.mainSlider.value = PlayerPrefs.GetFloat("Brightness", 50f);

            foreach (Transform item in mainMenuUIItems) {
                item.gameObject.SetActive(false);
            }
        } else {
            OnContinueClicked();
        }
    }

    public void OnBrightnessChanged(float value) {
        float scaledValue = Mathf.Lerp(-5f, 3f, value / 100f); 
        BrightnessController.Instance.SetBrightness(scaledValue);
    }

    public void OnContinueClicked() {
        foreach (Transform item in mainMenuUIItems) {
            item.gameObject.SetActive(true);
        }

        setBrightnessUI.gameObject.SetActive(false);

        mainCamera.transform.position = mainMenuCameraPos.position;
        mainCamera.transform.rotation = mainMenuCameraPos.rotation;
    }

}
