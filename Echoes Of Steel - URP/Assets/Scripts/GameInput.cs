using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {

    public static GameInput Instance { get; private set; }

    private const string PLAYER_PREFS_BINDINGS = "PlayerBindings";

    public enum Binding {
        Interact,
        Use,
        Sprint,
        WalkForward,
        WalkBackward,
        WalkRight,
        WalkLeft,
        Pause,
        DropItem,
    }

    public enum ControlType {
        Keyboard,
        Gamepad
    }

    private PlayerInputActions playerInputActions;
    private bool isControllerConnected;
    private ControlType activeControlType;

    public event Action<int> OnInventoryNavigate;
    public event Action OnInteract;
    public event Action OnUse;
    public event Action OnSprintPerformed;
    public event Action OnSprintCanceled;
    public event Action OnPausePerformed;
    public event Action OnDropItem;
    public event Action OnAnyAssignedKeybindPerformed;

    public event EventHandler OnBindingRebind;

    private int mouseSensitivity = 10;

    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();

        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void Start() {

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS)) {
            playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));

        }

        isControllerConnected = IsControllerConnected();

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.Use.performed += Use_performed;
        playerInputActions.Player.Sprint.performed += Sprint_performed;
        playerInputActions.Player.Sprint.canceled += Sprint_canceled;
        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.DropItem.performed += DropItem_performed;
        playerInputActions.Player.InventoryNavigate.performed += InventoryNavigate_performed;

        playerInputActions.Enable();

        mouseSensitivity = PlayerPrefs.GetInt("MouseSensitivity", 50);

        if (isControllerConnected) {
            StartCoroutine(ShowControllerPrompt());
        } else {
            SetControlType(ControlType.Keyboard);
        }

        DontDestroyOnLoad(this);
    }

    private IEnumerator ShowControllerPrompt() {
        // Wait for one frame to ensure all initializations are complete
        yield return null;

        AreYouSureUI.Instance.ShowAreYouSure("A controller was detected. Do you want to use it?", () => {
            SetControlType(ControlType.Gamepad);
        }, () => {
            SetControlType(ControlType.Keyboard);
        });
    }

    private void InventoryNavigate_performed(InputAction.CallbackContext context) {
        float direction = context.ReadValue<Vector2>().y;

        if (Mathf.Abs(direction) > 0.1f) {
            OnInventoryNavigate?.Invoke(direction > 0 ? -1 : 1);
            OnAnyAssignedKeybindPerformed?.Invoke();
        }
    }

    private void DropItem_performed(InputAction.CallbackContext obj) {
        OnDropItem?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void Pause_performed(InputAction.CallbackContext obj) {
        OnPausePerformed?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void Sprint_canceled(InputAction.CallbackContext obj) {
        OnSprintCanceled?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void Sprint_performed(InputAction.CallbackContext obj) {
        OnSprintPerformed?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void Use_performed(InputAction.CallbackContext obj) {
        OnUse?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void Interact_performed(InputAction.CallbackContext obj) {
        OnInteract?.Invoke();
        OnAnyAssignedKeybindPerformed?.Invoke();
    }

    private void OnDestroy() {
        InputSystem.onDeviceChange -= OnDeviceChange;

        StopAllCoroutines();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
        if (device is Gamepad) {
            if (change == InputDeviceChange.Added) {
                isControllerConnected = true; 
                AreYouSureUI.Instance.ShowAreYouSure("A controller was detected. Do you want to use it?", () => {
                    SetControlType(ControlType.Gamepad); 
                }, () => {
                    SetControlType(ControlType.Keyboard); 
                });
            } else if (change == InputDeviceChange.Removed) {
                isControllerConnected = IsControllerConnected();
                if (!isControllerConnected) {
                    SetControlType(ControlType.Keyboard); 
                }
            }

            Debug.Log($"Device Change: {device.displayName}, Change: {change}, IsControllerConnected: {isControllerConnected}");
        }
    }



    private bool IsControllerConnected() {
        foreach (var device in InputSystem.devices) {
            if (device is Gamepad) {
                return true;
            }
        }
        return false;
    }


    public Vector2 GetMovementVectorNormalized() {
        return playerInputActions.Player.Move.ReadValue<Vector2>().normalized;
    }

    public string GetBindingText(Binding binding) {
        int bindingIndex = activeControlType == ControlType.Gamepad ? 1 : 0; 

        switch (binding) {
            default:
            case Binding.WalkForward:
            case Binding.WalkBackward:
            case Binding.WalkLeft:
            case Binding.WalkRight:
                if (activeControlType == ControlType.Gamepad) {
                    return playerInputActions.Player.Move.bindings[5].ToDisplayString(); 
                }
                // For keyboard, show WASD as separate bindings
                else {
                    switch (binding) {
                        case Binding.WalkForward:
                            return playerInputActions.Player.Move.bindings[1].ToDisplayString();
                        case Binding.WalkBackward:
                            return playerInputActions.Player.Move.bindings[2].ToDisplayString();
                        case Binding.WalkLeft:
                            return playerInputActions.Player.Move.bindings[3].ToDisplayString();
                        case Binding.WalkRight:
                            return playerInputActions.Player.Move.bindings[4].ToDisplayString();
                    }
                }
                return "Unbound";
            case Binding.Interact:
                return playerInputActions.Player.Interact.bindings[bindingIndex].ToDisplayString();
            case Binding.Use:
                return playerInputActions.Player.Use.bindings[bindingIndex].ToDisplayString();
            case Binding.Pause:
                return playerInputActions.Player.Pause.bindings[bindingIndex].ToDisplayString();
            case Binding.Sprint:
                return playerInputActions.Player.Sprint.bindings[bindingIndex].ToDisplayString();
            case Binding.DropItem:
                return playerInputActions.Player.DropItem.bindings[bindingIndex].ToDisplayString();
        }
    }


    public void RebindBinding(Binding binding, Action onActionRebound) {
        playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex = activeControlType == ControlType.Gamepad ? 1 : 0;

        switch (binding) {
            default:
            case Binding.WalkForward:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = activeControlType == ControlType.Gamepad ? 5 : 1; 
                break;
            case Binding.WalkBackward:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = activeControlType == ControlType.Gamepad ? 5 : 2; 
                break;
            case Binding.WalkLeft:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = activeControlType == ControlType.Gamepad ? 5 : 3; 
                break;
            case Binding.WalkRight:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = activeControlType == ControlType.Gamepad ? 5 : 4; 
                break;
            case Binding.Interact:
                inputAction = playerInputActions.Player.Interact;
                break;
            case Binding.Use:
                inputAction = playerInputActions.Player.Use;
                break;
            case Binding.Pause:
                inputAction = playerInputActions.Player.Pause;
                break;
            case Binding.Sprint:
                inputAction = playerInputActions.Player.Sprint;
                break;
            case Binding.DropItem:
                inputAction = playerInputActions.Player.DropItem;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callback => {
            callback.Dispose();
            playerInputActions.Player.Enable();
            onActionRebound?.Invoke();

            PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, playerInputActions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();

            OnBindingRebind?.Invoke(this, EventArgs.Empty);
        })
        .Start();
    }

    public void SetControlType(ControlType selectedControlType) {
        activeControlType = selectedControlType;

        OptionsPanelUI.Instance.UpdateGameInputSelector(activeControlType);
        OptionsPanelUI.Instance.UpdateControlsVisual();
    }



    public void SetMouseSensitivity(float value) {
        mouseSensitivity = (int)value;
        PlayerPrefs.SetInt("MouseSensitivity", mouseSensitivity);
    }

    public float GetMouseSensitivityNormalized() {
        return mouseSensitivity / 100f; 
    }

    public Vector2 GetMouseDelta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    public ControlType GetControlType() {
        return activeControlType;
    }

}
