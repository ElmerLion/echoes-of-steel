using System.Collections;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance { get; private set; }

    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin perlinNoise;
    private float shakeDuration;
    private float shakeTimer;
    private bool isShaking = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        StopShaking();
    }

    public void ShakeCamera(float intensity, float duration, float frequency = 1f) {
        if (isShaking) return;

        perlinNoise.m_AmplitudeGain = intensity;
        perlinNoise.m_FrequencyGain = frequency;
        shakeDuration = duration; 
        shakeTimer = duration;

        isShaking = true;
    }

    public void StartShaking(float intensity, float frequency = 1f) {
        perlinNoise.m_AmplitudeGain = intensity;
        perlinNoise.m_FrequencyGain = frequency;
        isShaking = true;
    }

    public void StopShaking() {
        if (EscapeManager.Instance.IsEscaping) return;

        perlinNoise.m_AmplitudeGain = 0f;
        isShaking = false;
    }

    private void Update() {
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;

            if (shakeTimer <= 0) {
                perlinNoise.m_AmplitudeGain = 0f;
                isShaking = false;
            }
        }
    }
}
