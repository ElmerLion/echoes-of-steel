using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class PlayerLocationTracker : MonoBehaviour {

    [SerializeField] private PlayerLocation playerLocation;

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out Player player)) {
            HorrorGameManager.Instance.SetPlayerLocation(playerLocation);

            if (playerLocation == PlayerLocation.MineZone) {
                HorrorGameManager.TryUnlockAchievement("ENTER_MINE");
            }
        }
    }

}
