using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimatronicAnimator : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private Animatronic animatronic;
    [SerializeField] private NavMeshAgent navMeshAgent;

    private float targetSpeed;
    private float currentSpeed;

    private void Start() {
        animatronic.OnJumpscare += () => animator.SetTrigger("Scream");
        animatronic.OnSleep += () => animator.SetBool("Initiate", false);
        animatronic.OnWokeUp += () => animator.SetBool("Initiate", true);
        animatronic.OnStunned += () => animator.SetBool("Agony", true);
        animatronic.OnStunnedEnd += () => animator.SetBool("Agony", false);
    }

    private void Update() {
        UpdateSpeedParameter();
    }

    private void UpdateSpeedParameter() {
        targetSpeed = Mathf.Clamp01(animatronic.CurrentMoveSpeed / animatronic.RunSpeed); 
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 5f);  
        animator.SetFloat("Speed", currentSpeed);
    }

}
