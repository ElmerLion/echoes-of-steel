using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using static AudioManager;
using Steamworks;

public class Animatronic : MonoBehaviour {

    public static float NotSpottedTimer { get; set; }
    public static bool hasBeenSpotted;

    public enum State {
        Sleeping,
        Idle,
        Chasing,
        Patrolling,
        Attacking,
        Stunned,
    }

    public event Action OnJumpscare;
    public event Action OnSleep;
    public event Action OnWokeUp;
    public event Action OnStunned;
    public event Action OnStunnedEnd;

    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera jumpscareCamera;
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private TransitionAnimator transitionAnimator;

    [Header("Settings")]
    [SerializeField] private float hearingRange = 10f; 
    [SerializeField] private float defaultMoveSpeed = 3f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private GameObject[] visuals;
    [SerializeField] private float wakeUpTimerMax = 10f;

    private Vector3 targetPosition; 
    private NavMeshAgent navMeshAgent; 
    [SerializeField] private State state;

    private float timeSinceLastSoundHeard;
    private float idleTimer;
    private float idleTimerMax = 8f;
    private float speedBoost = 1.5f;
    public float CurrentMoveSpeed { get; private set; }
    private bool isAttacking = false;
    public float RunSpeed { get; private set; }

    private float stunTimer;
    private float wakeUpTimer;
    private Vector3 previousPos;
    private float distanceMoved;
    private bool isWakingUp;

    private float fixedYPosition = 10.169f;
    private int timesStunned;

    private void Start() {
        RunSpeed = defaultMoveSpeed + speedBoost;
        jumpscareCamera.Priority = 0;
        HorrorGameManager.Instance.AddAnimatronic(this);

        navMeshAgent = GetComponent<NavMeshAgent>();
        CurrentMoveSpeed = defaultMoveSpeed;
        navMeshAgent.speed = CurrentMoveSpeed;

        state = State.Sleeping;
        OnSleep?.Invoke();
    }

    private void Update() {
        if (state == State.Sleeping && isWakingUp) {
            wakeUpTimer += Time.deltaTime;

            if (wakeUpTimer >= wakeUpTimerMax) {
                StartCoroutine(WakeUp());
                wakeUpTimer = 0;
            }
            return;
        }

        timeSinceLastSoundHeard += Time.deltaTime;

        switch (state) {
            case State.Idle:
                HandleIdling();
                break;
            case State.Chasing:
                HandleChasing();
                break;
            case State.Patrolling:
                HandlePatrolling();
                break;
            case State.Attacking:
                HandleAttacking();
                break;
            case State.Stunned:
                HandleStunned();
                break;
        }

        if (state == State.Stunned) return;

        if (timeSinceLastSoundHeard > 10f && (state == State.Idle || state == State.Chasing)) {
            state = State.Patrolling;
        }

        MaintainFixedYPosition();
    }

    public IEnumerator WakeUp() {
        OnWokeUp?.Invoke();

        AudioManager.Instance.PlaySound(AudioManager.Sound.AnimatronicWakeUp, 1.5f);

        yield return new WaitForSeconds(2f);

        state = State.Patrolling;
    }

    private void MaintainFixedYPosition() {
        Vector3 position = transform.position;
        position.y = fixedYPosition;
        transform.position = position;
    }

    private void HandleIdling() {
        if (CurrentMoveSpeed > 0) {
            SetSpeed(0);
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimerMax) {
            state = State.Patrolling;
            idleTimer = 0f;
        }
    }

    private void HandleChasing() {
        if (CurrentMoveSpeed < RunSpeed) {
            SetSpeed(RunSpeed);
        }

        navMeshAgent.SetDestination(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 1f) {
            if (Vector3.Distance(Player.Instance.transform.position, targetPosition) < 2f) {
                if (Player.Instance.IsBeingScared) return;
                state = State.Attacking;
            } else {
                state = State.Idle;
                StopChasingEffects();
            }
           

        }

        distanceMoved += Vector3.Distance(transform.position, previousPos);
        previousPos = transform.position;

        if (distanceMoved > 2.5f) {
            AudioManager.Instance.PlaySound(AudioManager.Sound.AnimatronicFootstep, transform.position, 1.5f);

            distanceMoved = 0;
        }
    }

    private void HandlePatrolling() {
        if (CurrentMoveSpeed == 0) {
            SetSpeed(defaultMoveSpeed);
        }

        if (!navMeshAgent.hasPath || navMeshAgent.remainingDistance < 0.1f) {
            if (UnityEngine.Random.Range(0, 5) == 0) {
                state = State.Idle;
                return;
            }

            navMeshAgent.SetDestination(patrolPoints[UnityEngine.Random.Range(0, patrolPoints.Length)].position);
            StopChasingEffects();
        }

        distanceMoved += Vector3.Distance(transform.position, previousPos);
        previousPos = transform.position;

        if (distanceMoved > 2.5f) {
            AudioManager.Instance.PlaySound(AudioManager.Sound.AnimatronicFootstep, transform.position);

            distanceMoved = 0;
        }

    }

    private void HandleAttacking() {
        if (!isAttacking) {
            JumpscarePlayer();
        }

        Transform cameraTransform = jumpscareCamera.transform;
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * 1.75f;
        transform.position = targetPosition;
        transform.LookAt(cameraTransform.position);
    }

    private void JumpscarePlayer() {
        Player.Instance.IsBeingScared = true;
        isAttacking = true;

        Player.Instance.SetAllowedToMove(false);
        PlayerLook.Instance.SetAllowedToMove(false);
        InventoryManager.Instance.RemoveItem(InventoryManager.Instance.GetSelectedSlot(), false);
        InventoryUI.Instance.Hide();

        navMeshAgent.isStopped = true;
        navMeshAgent.SetDestination(transform.position);

        Transform cameraTransform = jumpscareCamera.transform;
        Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * 1.75f; 
        transform.position = targetPosition;
        transform.LookAt(cameraTransform.position);

        jumpscareCamera.LookAt = lookAtTarget;

        jumpscareCamera.Priority = playerCamera.Priority + 1;

        AudioManager.Instance.PlaySound(AudioManager.Sound.Jumpscare);
        AudioManager.Instance.PlaySound(AudioManager.Sound.JumpscareBackground);

        OnJumpscare?.Invoke();
        StopChasingEffects();

        transitionAnimator.Play();
    }




    public void OnSoundEmitted(SoundManager.SoundEmitter soundEmitter) {
        if (state == State.Stunned || state == State.Sleeping || state == State.Attacking) return;

        Vector3 haunterPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 soundPosition = new Vector3(soundEmitter.Position.x, 0, soundEmitter.Position.z);

        float horizontalDistance = Vector3.Distance(haunterPosition, soundPosition);

        float yDifferenceThreshold = 0.5f; 
        if (horizontalDistance <= hearingRange + soundEmitter.Loudness &&
            Mathf.Abs(transform.position.y - soundEmitter.Position.y) <= yDifferenceThreshold) {

            if (state != State.Chasing && state != State.Attacking) {
                AudioManager.Instance.PlaySound(AudioManager.Sound.PlayerHeard);
                StartChasing();
            }

            targetPosition = soundEmitter.Position;
            timeSinceLastSoundHeard = 0f;
            state = State.Chasing;
        }
    }

    public void PowerDown() {
        OnSleep?.Invoke();
        state = State.Sleeping;
        isWakingUp = false;

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();

        // Power down animation
    }

    public void Stun(float duration) {
        if (state == State.Sleeping) return;

        state = State.Stunned;
        navMeshAgent.isStopped = true;
        stunTimer = duration;
        StopChasingEffects();
        OnStunned?.Invoke();
        Debug.Log("Stunned");

        timesStunned++;
        if (timesStunned >= 3) {
            HorrorGameManager.TryUnlockAchievement("STUN_ANIMATRONIC");
            
        }
    }

    private void HandleStunned() {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f) {
            OnStunnedEnd?.Invoke();
            state = State.Patrolling;
            navMeshAgent.isStopped = false;

        }
    }

    private void StartChasing() {
        NotSpottedTimer = 0;
        hasBeenSpotted = true;

        ChromaticAbberation.Instance.SetChromaticAberration(1f);
        CameraShake.Instance.StartShaking(0.5f, 1f);
        
        if (!EscapeManager.Instance.IsEscaping) {
            AudioManager.Instance.PlayMusic(AudioManager.Music.ChasingMusic);
        }
    }

    private void StopChasingEffects() {
        ChromaticAbberation.Instance.SetChromaticAberration(0f); 
        CameraShake.Instance.StopShaking();

        AudioManager.Instance.CheckMusic();
    }

    private void SetSpeed(float speed) {
        CurrentMoveSpeed = speed;
        navMeshAgent.speed = CurrentMoveSpeed;
    }

    public void SetState(State state) {
        this.state = state;
    }

    public void SetIsWakingUp(bool isWakingUp) {
        this.isWakingUp = isWakingUp;
    }

}
