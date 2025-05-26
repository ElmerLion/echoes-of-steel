using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public static UIManager Instance { get; private set; }

    [SerializeField] private List<BaseUI> baseUIList;
    [SerializeField] private float interactionToggleDelay = 0.05f;
    [SerializeField] private GameObject crosshair;

    public event Action OnAnyUIOpened;
    public event Action OnAllUIClosed;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPausePerformed += GameInput_OnPausePerformed;

        foreach (BaseUI baseUI in baseUIList) {
            baseUI.OnUIOpened += BaseUI_OnUIOpened;
        }



        OnAllUIClosed?.Invoke();

        HideCrosshair();
    }

    private void BaseUI_OnUIOpened() {
        OnAnyUIOpened?.Invoke();
    }

    private void GameInput_OnPausePerformed() {
        List<BaseUI> copyBaseUIList = new List<BaseUI>(baseUIList);

        foreach (BaseUI baseUI in copyBaseUIList) {
            if (baseUI == null) {
                baseUIList.Remove(baseUI);
                continue;
            }

            if (baseUI.isOpen) {
                baseUI.Hide();

                if (!IsAnyUIOpen()) {
                    StartCoroutine(DelayedInvokeAllUIClosed());
                }
                return;
            }
        }

        if (PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.Show();
        }
    }

    public bool IsAnyUIOpen() {
        foreach (BaseUI baseUI in baseUIList) {
            if (baseUI.isOpen) {
                return true;
            }
        }
        return false;
    }

    public void CheckForOpenedUI() {
        if (IsAnyUIOpen()) {
            OnAnyUIOpened?.Invoke();
        } else {
            StartCoroutine(DelayedInvokeAllUIClosed());
        }
    }

    private IEnumerator DelayedInvokeAllUIClosed() {
        yield return new WaitForSeconds(interactionToggleDelay); 
        OnAllUIClosed?.Invoke();
    }

    public void ShowCrosshair() {
        if (crosshair != null) {
            crosshair.SetActive(true);
        }
    }

    public void HideCrosshair() {
        if (crosshair != null) {
            crosshair.SetActive(false);
        }
    }

    private void OnDestroy() {
        GameInput.Instance.OnPausePerformed -= GameInput_OnPausePerformed;
    }
}
