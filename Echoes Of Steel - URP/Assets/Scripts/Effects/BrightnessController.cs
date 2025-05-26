using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class BrightnessController : MonoBehaviour {

    public static BrightnessController Instance { get; private set; }

    [SerializeField] private Volume postProcessingVolume;

    private ColorAdjustments colorAdjustments;
    private float brightness;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (postProcessingVolume.profile.TryGet(out ColorAdjustments adjustments)) {
            colorAdjustments = adjustments;
        }

        brightness = PlayerPrefs.GetFloat("Brightness", 0f);
        SetBrightness(brightness);

    }

    public void SetBrightness(float brightness) {
        PlayerPrefs.SetFloat("Brightness", brightness);
        this.brightness = brightness;
        if (colorAdjustments != null) {
            colorAdjustments.postExposure.value = brightness;
        }
    }

    private void OnApplicationQuit() {
        PlayerPrefs.SetFloat("Brightness", brightness);
    }
}
