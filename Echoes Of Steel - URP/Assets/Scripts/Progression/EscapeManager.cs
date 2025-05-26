using Cinemachine;
using QFSW.QC;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Localization;

public class EscapeManager : MonoBehaviour {

    public static EscapeManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private List<GameObject> gameObjectsToDisableOnEscaping;
    [SerializeField] private List<GameObject> gameObjectsToEnableOnEscaping;
    [SerializeField] private TransitionAnimator endTransitionAnimator;
    [SerializeField] private CinemachineVirtualCamera escapedCamera;

    [Header("Localization")]
    [SerializeField] private LocalizedString noPowerString;
    [SerializeField] private LocalizedString escapeFailedString;

    public bool HasEscaped { get; private set; }
    public bool IsEscaping { get; private set; }    
    private bool isPowerOn;
    private bool isTransitioning;

    private float secondsPassed;
    private float minutesPassed;
    private float hoursPassed;

    private float escapeTimerMax = 30f;
    private float escapeTimer;
    private bool failedEscape;
    private float failedEscapeTimer;
    private float failedEscapeTimerMax = 10f;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        foreach (GameObject go in gameObjectsToEnableOnEscaping) {
            go.SetActive(false);
        }

        foreach (GameObject go in gameObjectsToDisableOnEscaping) {
            go.SetActive(true);
        }
    }

    private void Update() {
        secondsPassed += Time.deltaTime;
        if (secondsPassed >= 60) {
            minutesPassed++;
            secondsPassed = 0;
            if (minutesPassed >= 60) {
                hoursPassed++;
                minutesPassed = 0;
            }
        }

        if (isTransitioning) {
            if (endTransitionAnimator.progress >= 0.9f) {
                escapedCamera.Priority = 100;
            }
        }

        if (IsEscaping && escapeTimer < escapeTimerMax) {
            escapeTimer += Time.deltaTime;
            ObjectiveUI.Instance.UpdateTimerText(escapeTimerMax - escapeTimer);
            if (escapeTimer >= escapeTimerMax) {
                if (ObjectiveUI.Instance.currentObjective != ObjectiveType.NoEscape) {
                    ObjectiveUI.Instance.SetObjective(ObjectiveType.NoEscape);
                    failedEscape = true;

                    HorrorGameManager.TryUnlockAchievement("FAIL_ESCAPE");
                }
                AudioManager.Instance.PlaySound(AudioManager.Sound.FailedEscape, Player.Instance.transform.position);
            }
        }

        if (failedEscape) {
            failedEscapeTimer += Time.deltaTime;
            if (failedEscapeTimer >= failedEscapeTimerMax) {
                AudioManager.Instance.PlaySound(AudioManager.Sound.FailedEscape, Player.Instance.transform.position);
                
            }
        }

    }


    [Command]
    public void TriggerEscape() {
        foreach (GameObject go in gameObjectsToDisableOnEscaping) {
            go.SetActive(false);
        }
        foreach (GameObject go in gameObjectsToEnableOnEscaping) {
            go.SetActive(true);
        }
        
        IsEscaping = true;

        AnimatronicManager.Instance.SetStage(AnimatronicManager.Stage.Ending);

        CameraShake.Instance.ShakeCamera(0.5f, 999f);
        LightManager.Instance.FlashLightsContinuously(LightManager.LightType.Factory);

        AudioManager.Instance.PlayMusic(AudioManager.Music.EscapeMusic);
        AudioManager.Instance.PlaySound(AudioManager.Sound.Earthquake, Player.Instance.transform.position);
        AudioManager.Instance.PlaySound(AudioManager.Sound.RocksExploding, gameObjectsToDisableOnEscaping[0].transform.GetChild(0).position, 2f);

        ObjectiveUI.Instance.SetObjective(ObjectiveType.FindExit);
    }

    public void Escape() {
        if (failedEscape) {
            MessageUI.Instance.ShowMessage(escapeFailedString.GetLocalizedString());
            return;
        }

        if (!isPowerOn) {
            MessageUI.Instance.ShowMessage(noPowerString.GetLocalizedString());
            return;
        }

        endTransitionAnimator.onTransitionEnd.AddListener(TransitionEnd);
        endTransitionAnimator.Play();
        isTransitioning = true;
        HasEscaped = true;
        InventoryUI.Instance.Hide();
    }

    private void TransitionEnd() {
        EscapeUI.Instance.Show(secondsPassed, minutesPassed, hoursPassed);

        isTransitioning = false;
        endTransitionAnimator.SetProgress(0);

        endTransitionAnimator.onTransitionEnd.RemoveAllListeners();

        HorrorGameManager.TryUnlockAchievement("SUCCESSFULLY_ESCAPE");

        if (!Animatronic.hasBeenSpotted) {
            HorrorGameManager.TryUnlockAchievement("COMPLETE_GAME_NOT_SPOTTED");
        }
    }

    public void PowerOn() {
        isPowerOn = true;
    }

    public Dictionary<string, float> GetTimePassed() {
        return new Dictionary<string, float> {
            { "Seconds", secondsPassed },
            { "Minutes", minutesPassed },
            { "Hours", hoursPassed }
        };
    }

}
