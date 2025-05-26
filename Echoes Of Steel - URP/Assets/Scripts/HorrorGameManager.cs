using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class HorrorGameManager : MonoBehaviour {
    public static HorrorGameManager Instance { get; private set; }

    public List<Animatronic> AllAnimatronics { get; private set; } = new List<Animatronic>();

    public event Action<PlayerLocation> OnPlayerLocationChanged;

    public PlayerLocation CurrentPlayerLocation { get; private set; } = PlayerLocation.FactoryZone;

    public static void TryUnlockAchievement(string achievementName) {
        if (SteamManager.Initialized) {
            SteamUserStats.GetAchievement(achievementName, out bool isAchieved);

            if (isAchieved) return;

            SteamUserStats.SetAchievement(achievementName);
            SteamUserStats.StoreStats();
        }
    }

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        AudioManager.Instance.ResetMusic();
        ObjectiveUI.Instance.SetObjective(ObjectiveType.FindWayOut);
        SetPlayerLocation(PlayerLocation.FactoryZone);
    }


    public void AddAnimatronic(Animatronic animatronic) {
        AllAnimatronics.Add(animatronic);
    }

    public void SetPlayerLocation(PlayerLocation location) {
        CurrentPlayerLocation = location;
        OnPlayerLocationChanged?.Invoke(location);
    }

}

public enum PlayerLocation {
    FactoryZone,
    MineZone,
}
