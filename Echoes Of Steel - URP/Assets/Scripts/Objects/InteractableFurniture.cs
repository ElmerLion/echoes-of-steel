using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractableFurniture : MonoBehaviour, IInteractable {

    public enum InteractAction {
        Move,
        Rotate,
    }

    [SerializeField] private string interactText = "Open";
    [SerializeField] private InteractAction interactAction = InteractAction.Move;
    [SerializeField] private Vector3 openRotation;
    [SerializeField] private Vector3 moveDirection;

    private Quaternion initialRotation;  
    private Quaternion targetRotation;


    private float moveSpeed = 1f;  // Movement speed in units per second
    private Vector3 initialPosition;  // Store the initial position
    private Vector3 targetPosition;   // Store the target position when moved
    private Vector3 currentTargetPosition;
    private bool isMoving;  // Flag to check if the object is moving

    public bool isInInventory { get; set; }
    public bool isOpen;

    private void Start() {
        moveDirection = transform.forward / 3;
        isOpen = false;

        initialRotation = transform.rotation;
        targetRotation = Quaternion.Euler(openRotation);

        initialPosition = transform.position;
        targetPosition = initialPosition + moveDirection;
        currentTargetPosition = initialPosition;
    }

    private void Update() {
        if (isMoving) {
            SmoothMove();
        }
    }

    public string GetInteractText() {
        return interactText;
    }

    public void Interact() {
        if (isOpen) {
            Close();
        } else {
            Open();
        }
    }

    private void Open() {

        if (interactAction == InteractAction.Move) {
            Move(true);
        } else if (interactAction == InteractAction.Rotate) {
            Rotate(true);
        }

        isOpen = true;
        interactText = "Close";
        SoundManager.Instance.EmitSound(transform.position, 0.1f, 3);
    }

    private void Close() {

        if (interactAction == InteractAction.Move) {
            Move(false);
        } else if (interactAction == InteractAction.Rotate) {
            Rotate(false);
        }

        isOpen = false;
        interactText = "Open";
        SoundManager.Instance.EmitSound(transform.position, 0.1f, 3);
    }

    private void Move(bool open) {
        // Set the target position and enable smooth movement
        currentTargetPosition = open ? targetPosition : initialPosition;
        isMoving = true;  // Set the flag to start moving in Update
    }

    private void Rotate(bool open) {
        // Stop any ongoing rotation coroutine to avoid conflicts
        StopAllCoroutines();

        // Start a new coroutine to smoothly rotate
        StartCoroutine(RotateCoroutine(open ? targetRotation : initialRotation));
    }

    private IEnumerator RotateCoroutine(Quaternion targetRotation) {
        float rotationSpeed = 180f;  // Rotation speed in degrees per second
        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f) {  // Check if the rotation is close enough
            // Rotate towards the target rotation smoothly
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;  // Snap to the final rotation to avoid precision issues
    }

    private void SmoothMove() {
        // Move towards the target position smoothly
        transform.position = Vector3.MoveTowards(transform.position, currentTargetPosition, moveSpeed * Time.deltaTime);

        // Check if the position is close enough to the target
        if (Vector3.Distance(transform.position, currentTargetPosition) < 0.01f) {
            transform.position = currentTargetPosition;  // Snap to the final position to avoid precision issues
            isMoving = false;  // Stop moving
        }
    }

    public bool IsInInventory() {
        throw new System.NotImplementedException();
    }
}
