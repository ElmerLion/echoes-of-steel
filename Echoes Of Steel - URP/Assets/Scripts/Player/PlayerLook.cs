using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour {
    public static PlayerLook Instance { get; private set; }

    [SerializeField] private Transform playerCamera;
    [SerializeField] private float bobFrequency = 1.5f;  
    [SerializeField] private float bobAmplitude = 0.05f;  
    [SerializeField] private float bobSmoothing = 5f;  

    private bool isPaused = false;
    private float xRotation = 0f;
    private Vector3 defaultCameraPosition;  
    private float bobbingTimer = 0f;  

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;  
        defaultCameraPosition = playerCamera.localPosition;  
    }

    private void Update() {
        if (isPaused) return;

        HandleRotation();
        HandleViewBobbing();
    }

    private void HandleRotation() {
        Vector2 mouseDelta = GameInput.Instance.GetMouseDelta();

        float mouseX = mouseDelta.x * GameInput.Instance.GetMouseSensitivityNormalized();
        float mouseY = mouseDelta.y * GameInput.Instance.GetMouseSensitivityNormalized();

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleViewBobbing() {
        if (Player.Instance.IsMoving()) {
            bobbingTimer += Time.deltaTime * Player.Instance.CurrentSpeed * bobFrequency;

            float bobOffset = Mathf.Sin(bobbingTimer) * bobAmplitude;

            Vector3 newCameraPosition = defaultCameraPosition + new Vector3(0f, bobOffset, 0f);
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, newCameraPosition, Time.deltaTime * bobSmoothing);
        } else {
            bobbingTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, defaultCameraPosition, Time.deltaTime * bobSmoothing);
        }
    }

    public void TogglePause() {
        isPaused = !isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void SetAllowedToMove(bool allowedToMove) {
        isPaused = !allowedToMove;
    }
}
