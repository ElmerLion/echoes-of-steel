using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUI : MonoBehaviour {

    public event Action OnUIOpened;

    public bool isOpen;

    public virtual void Start() {
        Hide();
    }

    public virtual void Show() {
        Cursor.lockState = CursorLockMode.None;
        gameObject.SetActive(true);
        isOpen = true;
        if (PlayerLook.Instance != null) {
            PlayerLook.Instance.SetAllowedToMove(false);
        }

        OnUIOpened?.Invoke();
    }

    public virtual void Hide() {
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.SetActive(false);
        isOpen = false;
        if (PlayerLook.Instance != null) {
            PlayerLook.Instance.SetAllowedToMove(true);
        }

        if (UIManager.Instance != null) {
            UIManager.Instance.CheckForOpenedUI();
        }
    }

    public virtual void Toggle() {
        if (isOpen) {
            Hide();
        } else {
            Show();
        }
    }
    
}
