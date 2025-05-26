using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Taser : Item {

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject taserTrail;
    [SerializeField] private ItemSO taserSO;

    [Header("Settings")]
    [SerializeField] private float stunDuration = 3.5f;
    [SerializeField] private float range = 5f;
    [SerializeField] private float cooldown = 15f;

    private float timeSinceLastShot;
    private GameObject taserTrailObject;

    public override void OnUse() {
        if (!CanUse()) return;
        Tase();
    }

    private void Start() {
        timeSinceLastShot = cooldown;

        OnItemInteracted += Taser_OnItemInteracted;
        OnEquipped += Taser_OnEquipped;
    }

    private void Taser_OnEquipped() {
        UIManager.Instance.ShowCrosshair();
    }

    private void Taser_OnItemInteracted() {
        HorrorGameManager.TryUnlockAchievement("FOUND_TASER");

        UIManager.Instance.ShowCrosshair();
    }

    private void Update() {
        if (taserTrailObject == null) return;

        if (Vector3.Distance(firePoint.position, taserTrailObject.transform.position) > range) {
            Destroy(taserTrailObject);
        }
    }

    private void Reload() {
        timeSinceLastShot = cooldown;
    }


    private void Tase() {

        if (timeSinceLastShot < cooldown) return;       
        AudioManager.Instance.PlaySound(AudioManager.Sound.Taser, Player.Instance.transform.position);

        RaycastHit hit;
        taserTrailObject = Instantiate(taserTrail, firePoint.position, Quaternion.identity);
        Rigidbody rb = taserTrailObject.GetComponent<Rigidbody>();
        rb.velocity = Camera.main.transform.forward * 30f;
        timeSinceLastShot = 0f;

        if (Physics.Raycast(firePoint.position, Camera.main.transform.forward, out hit, range)) {
            if (hit.transform.TryGetComponent(out Animatronic animatronic)) {
                animatronic.Stun(stunDuration);
            }
        }

        TimerHandler.StartTimer(cooldown, Reload);
    }

    private void OnDisable() {
        if (InventoryManager.Instance == null || UIManager.Instance == null) return;

        UIManager.Instance.HideCrosshair();
    }

}
