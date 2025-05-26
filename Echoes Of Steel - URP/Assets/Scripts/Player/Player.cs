using System;
using UnityEngine;

public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    public float CurrentSpeed => currentSpeed;
    public float Stamina => stamina;
    public float MaxStamina => maxStamina;
    public bool IsBeingScared { get; set; }

    public event Action OnStaminaChanged;

    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintAdd = 3f;
    [SerializeField] private float maxInteractionDistance = 2f;
    [SerializeField] private LayerMask collisionsLayerMask;

    private float breathTimer = 0f;
    private float breathTimerMax = 1f;
    private bool allowedToMove = true;
    private bool isSprinting = false;
    private float currentSpeed;
    private bool canInteract;

    private float stamina;
    private float staminaRegenRate = 4.5f;
    private float staminaDepletionRate = 10f;

    private Vector3 lastPosition;
    private float distanceMoved = 0f;
    private float footstepThreshold = 2.5f;
    private Item hoveringItem;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnInteract += HandleInteraction;
        GameInput.Instance.OnSprintPerformed += GameInput_OnSprintPerformed;
        GameInput.Instance.OnSprintCanceled += GameInput_OnSprintCanceled;

        currentSpeed = moveSpeed;
        stamina = maxStamina;
        OnStaminaChanged?.Invoke();

        canInteract = true;

        UIManager.Instance.OnAllUIClosed += (() => {
            allowedToMove = true;
            canInteract = true;
        });

        UIManager.Instance.OnAnyUIOpened += (() => {
            allowedToMove = false;
            canInteract = false;
        });

        AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerRespawn);
    }
    //
    private void Update() {
        HandleMovement();
        CheckForInteractable();
        
        breathTimer += Time.deltaTime;

        if (breathTimer >= breathTimerMax) {
            PlayBreathingSound();
            breathTimer = 0f;
        }

        if (isSprinting) {
            stamina -= Time.deltaTime * staminaDepletionRate;
            OnStaminaChanged?.Invoke();
            if (stamina <= 0) {
                stamina = 0;
                GameInput_OnSprintCanceled();
            }
        } 

        if (!isSprinting && stamina < maxStamina) {
            stamina += Time.deltaTime * staminaRegenRate;
            OnStaminaChanged?.Invoke();
            if (stamina > maxStamina) {
                stamina = maxStamina;
            }
        }
    }


    private void GameInput_OnSprintCanceled() {
        if (currentSpeed <= moveSpeed) return;
        currentSpeed -= sprintAdd;
        isSprinting = false;
    }

    private void GameInput_OnSprintPerformed() {
        if (stamina <= 0) return;
        currentSpeed += sprintAdd;
        isSprinting = true;
    }

    private void HandleMovement() {
        if (!allowedToMove) return;

        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        moveDir = transform.TransformDirection(moveDir);

        float moveDistance = currentSpeed * Time.deltaTime;
        float playerHeight = 1.8f;
        float playerRadius = 0.4f;

        Vector3 capsuleBottom = transform.position + Vector3.up * playerRadius;  // Bottom sphere of the capsule
        Vector3 capsuleTop = transform.position + Vector3.up * (playerHeight - playerRadius);  // Top sphere of the capsule

        bool canMove = !Physics.CapsuleCast(capsuleBottom, capsuleTop, playerRadius, moveDir, moveDistance, collisionsLayerMask);

        if (!canMove) {
            // Cannot move towards moveDir, so try movement in X and Z directions separately

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -0.5f || moveDir.x > 0.5f) && !Physics.CapsuleCast(capsuleBottom, capsuleTop, playerRadius, moveDirX, moveDistance, collisionsLayerMask);

            if (canMove) {
                // Can only move on the X axis
                moveDir = moveDirX;
            } else {
                // Cannot move only on the X axis, attempt Z movement

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -0.5f || moveDir.z > 0.5f) && !Physics.CapsuleCast(capsuleBottom, capsuleTop, playerRadius, moveDirZ, moveDistance, collisionsLayerMask);

                if (canMove) {
                    // Can only move on the Z axis
                    moveDir = moveDirZ;
                } else {
                    // Cannot move in any direction
                    return;  // Exit without moving
                }
            }
        }

        if (canMove) {
            Vector3 previousPos = transform.position;

            // Check if moving up a slope
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, playerHeight / 2 + 0.1f, collisionsLayerMask)) {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                if (slopeAngle <= 45f) {  // Adjust the value to your slope tolerance
                    moveDir = Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
                }
            }

            // Apply movement
            transform.position += moveDir * currentSpeed * Time.deltaTime;

            // Accumulate distance moved and play footstep sound when threshold is reached
            distanceMoved += Vector3.Distance(transform.position, lastPosition);
            lastPosition = transform.position;

            if (distanceMoved >= footstepThreshold) {
                if (isSprinting) {
                    AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerSprinting, transform.position, 0.5f);
                } else {
                    AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerFootstep, transform.position, 0.5f);
                }
                
                distanceMoved = 0f; 
            }
        }
    }

    public bool IsMoving() {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();
        return inputVector.magnitude > 0.1f;
    }
    private void HandleInteraction() {
        if (!canInteract) return;

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), maxInteractionDistance);

        foreach (RaycastHit hit in hits) {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                interactable.Interact();
                return;
            }
        }
    }

    private void CheckForInteractable() {
        if (!canInteract) {
            InteractUI.Instance.ClearText();
            HotkeyTooltipUI.Instance.HideHotkey();
            return;
        }

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), maxInteractionDistance);

        foreach (RaycastHit hit in hits) {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) {
                InteractUI.Instance.SetText(interactable.GetInteractText());
                HotkeyTooltipUI.Instance.ShowHotkey(GameInput.Binding.Interact);
                if (hit.transform.TryGetComponent(out Item item)) {
                    item.EnableGlow();
                    hoveringItem = item;
                }
                return;
            } else {
                InteractUI.Instance.ClearText();
                HotkeyTooltipUI.Instance.HideHotkey();
                if (hoveringItem != null) {
                    hoveringItem.DisableGlow();
                    hoveringItem = null;
                }
            }
        }

        if (hits.Length == 0) {
            InteractUI.Instance.ClearText();
            HotkeyTooltipUI.Instance.HideHotkey();
            if (hoveringItem != null) {
                hoveringItem.DisableGlow();
                hoveringItem = null;
            }
        }

    }

    private void PlayBreathingSound() {
        AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerBreathing, transform.position, 2f);
    }

    public void SetAllowedToMove(bool value) {
        allowedToMove = value;
    }

    private void OnDestroy() {
        GameInput.Instance.OnInteract -= HandleInteraction;
        GameInput.Instance.OnSprintPerformed -= GameInput_OnSprintPerformed;
        GameInput.Instance.OnSprintCanceled -= GameInput_OnSprintCanceled;
    }

}
